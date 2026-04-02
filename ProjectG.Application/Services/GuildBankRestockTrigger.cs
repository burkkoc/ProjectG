using ProjectG.DomainLayer.Entities.Concrete;
using System.Collections.Concurrent;

namespace ProjectG.ApplicationLayer.Services
{
    /// <summary>
    /// Guild bank tetikleme: min. zaman aralığı VEYA kısa posting (önceki cycle ilk post × RestockThreshold% veya saniye farkı).
    /// <see cref="AppSettings.DualClient"/> açıkken her WoW ana penceresi (<see cref="IntPtr"/> HWND) için ayrı durum.
    /// </summary>
    public static class GuildBankRestockTrigger
    {
        sealed class ClientState
        {
            public int? PriorCycleFirstRunPostSec;
            public int? CurrentCycleFirstRunPostSec;
            public int RestockNotifySentCount;
            public bool AwaitPostAfterGuildBankForShortNotify;
            public DateTime? GuildBankEligibleAfterUtc;
        }

        static readonly object _gate = new();
        static readonly ConcurrentDictionary<IntPtr, ClientState> ByClient = new();

        static ClientState State(IntPtr key) => ByClient.GetOrAdd(key, _ => new ClientState());

        /// <summary>Çift istemicide ana HWND; tek istemicide <see cref="IntPtr.Zero"/>.</summary>
        public static IntPtr ResolveRestockClientKey()
        {
            if (!AppSettings.DualClient)
                return IntPtr.Zero;

            IntPtr h = WowDualClientWindowSwitcher.TryGetForegroundWowMainWindowHandle();
            if (h != IntPtr.Zero)
                return h;

            return DualClientLayoutStore.LastAppliedWowMainWindowHandle;
        }

        public static void ResetSession(NtfyUserSettings settings)
        {
            lock (_gate)
            {
                ByClient.Clear();
                if (AppSettings.DualClient)
                {
                    var handles = WowDualClientWindowSwitcher.GetSortedWowMainWindowHandles();
                    if (handles.Count > 0)
                    {
                        foreach (var h in handles)
                            ScheduleNextGuildBankWindow(State(h), settings);
                    }
                    else
                        ScheduleNextGuildBankWindow(State(IntPtr.Zero), settings);
                }
                else
                    ScheduleNextGuildBankWindow(State(IntPtr.Zero), settings);
            }
        }

        /// <summary>Kurtarma / yarım cycle: sadece bu cycle’a ait ilk post ölçümünü sıfırla.</summary>
        public static void ClearCurrentCycleFirstPost(IntPtr clientKey)
        {
            lock (_gate)
            {
                if (!ByClient.TryGetValue(clientKey, out var s))
                    return;
                s.CurrentCycleFirstRunPostSec = null;
            }
        }

        /// <summary>
        /// MailBox → downtime sonunda guild kontrolünden hemen sonra: bu istemcinin cycle ilk postunu bir sonraki karşılaştırma için sakla.
        /// </summary>
        public static void RollCycleBaselinesAfterDowntimeGuildCheck(IntPtr clientKey)
        {
            lock (_gate)
            {
                var s = State(clientKey);
                if (s.CurrentCycleFirstRunPostSec.HasValue)
                    s.PriorCycleFirstRunPostSec = s.CurrentCycleFirstRunPostSec;
                s.CurrentCycleFirstRunPostSec = null;
            }
        }

        /// <summary>
        /// Her posting fazı biter; yalnızca cycle içindeki <b>ilk</b> faz süresi guild kısa-posting mantığına yazılır.
        /// </summary>
        public static void RecordPostingPhaseCompleted(int runPostSec, bool isFirstPostingPhaseInCycle, IntPtr clientKey)
        {
            lock (_gate)
            {
                if (!isFirstPostingPhaseInCycle)
                    return;
                State(clientKey).CurrentCycleFirstRunPostSec = runPostSec;
            }
        }

        public static void RecordRestockNotificationSent(IntPtr clientKey)
        {
            lock (_gate)
            {
                State(clientKey).RestockNotifySentCount++;
            }
        }

        public static void RecordGuildBankFlowCompleted(NtfyUserSettings settings, IntPtr clientKey)
        {
            lock (_gate)
            {
                var s = State(clientKey);
                s.AwaitPostAfterGuildBankForShortNotify = true;
                ScheduleNextGuildBankWindow(s, settings);
            }
        }

        static void ScheduleNextGuildBankWindow(ClientState s, NtfyUserSettings settings)
        {
            double minutes = SampleIntervalMinutes(settings);
            s.GuildBankEligibleAfterUtc = DateTime.UtcNow + TimeSpan.FromMinutes(minutes);
        }

        static double SampleIntervalMinutes(NtfyUserSettings settings)
        {
            double lo = Math.Max(0.5, settings.GuildBankMinIntervalMinutes);
            double hiRaw = settings.GuildBankMaxIntervalMinutes <= 0
                ? lo
                : Math.Max(0.5, settings.GuildBankMaxIntervalMinutes);
            double hi = Math.Max(lo, hiRaw);
            if (hi <= lo)
                return lo;
            return Random.Shared.NextDouble() * (hi - lo) + lo;
        }

        /// <summary>
        /// Guild bank akışı bittiyse bu istemci için bir sonraki RunPost çıkışında tek seferlik true döner.
        /// </summary>
        public static bool TryConsumeAwaitingPostAfterGuildBank(IntPtr clientKey)
        {
            lock (_gate)
            {
                var s = State(clientKey);
                if (!s.AwaitPostAfterGuildBankForShortNotify)
                    return false;
                s.AwaitPostAfterGuildBankForShortNotify = false;
                return true;
            }
        }

        /// <summary>
        /// Zaman aralığı VEYA kısa posting: bu istemcinin cycle ilk postu, önceki cycle ilk postuna göre kısa mı?
        /// </summary>
        public static bool ShouldRunGuildBank(NtfyUserSettings settings, IntPtr clientKey)
        {
            int minDelta = Math.Max(0, settings.GuildBankAfterNotifyMinSecondsShorterThanFirst);
            double ratio = Math.Clamp(settings.RestockThresholdPercent / 100.0, 0.01, 1.0);

            lock (_gate)
            {
                var s = State(clientKey);

                if (s.GuildBankEligibleAfterUtc.HasValue
                    && DateTime.UtcNow >= s.GuildBankEligibleAfterUtc.Value)
                    return true;

                if (s.PriorCycleFirstRunPostSec is int prior && prior > 0
                    && s.CurrentCycleFirstRunPostSec is int cur && cur > 0)
                {
                    int shortCapSec = (int)Math.Floor(prior * ratio);
                    bool byRatio = cur <= shortCapSec;
                    bool byDelta = minDelta > 0 && (prior - cur) >= minDelta;
                    if (byRatio || byDelta)
                        return true;
                }

                return false;
            }
        }

        /// <summary>Makro bildirim tavanı için bu istemcide kaç bildirim gitti.</summary>
        public static int GetRestockNotifySentCount(IntPtr clientKey)
        {
            lock (_gate)
            {
                return ByClient.TryGetValue(clientKey, out var s) ? s.RestockNotifySentCount : 0;
            }
        }
    }
}
