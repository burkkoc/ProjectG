namespace ProjectG.ApplicationLayer.Services
{
    /// <summary>
    /// Guild bank tetikleme: min. zaman aralığı VEYA kısa posting (önceki cycle ilk post × RestockThreshold% veya saniye farkı).
    /// Dinamik AH’ta ikinci posting fazı bilinçli kısa olduğundan yalnızca her cycle’ın <b>ilk</b> posting süresi kaydedilir.
    /// </summary>
    public static class GuildBankRestockTrigger
    {
        static readonly object _gate = new();
        static int? _priorCycleFirstRunPostSec;
        static int? _currentCycleFirstRunPostSec;
        static int _restockNotifySentCount;
        static bool _awaitPostAfterGuildBankForShortNotify;
        static DateTime? _guildBankEligibleAfterUtc;

        public static void ResetSession(NtfyUserSettings settings)
        {
            lock (_gate)
            {
                _priorCycleFirstRunPostSec = null;
                _currentCycleFirstRunPostSec = null;
                _restockNotifySentCount = 0;
                _awaitPostAfterGuildBankForShortNotify = false;
                ScheduleNextGuildBankWindow(settings);
            }
        }

        /// <summary>Kurtarma / yarım cycle: sadece bu cycle’a ait ilk post ölçümünü sıfırla (önceki cycle referansına dokunma).</summary>
        public static void ClearCurrentCycleFirstPost()
        {
            lock (_gate) { _currentCycleFirstRunPostSec = null; }
        }

        /// <summary>
        /// MailBox → downtime sonunda guild kontrolünden hemen sonra: bu cycle’ın ilk postunu bir sonraki karşılaştırma için sakla.
        /// </summary>
        public static void RollCycleBaselinesAfterDowntimeGuildCheck()
        {
            lock (_gate)
            {
                if (_currentCycleFirstRunPostSec.HasValue)
                    _priorCycleFirstRunPostSec = _currentCycleFirstRunPostSec;
                _currentCycleFirstRunPostSec = null;
            }
        }

        /// <summary>
        /// Her posting fazı biter; yalnızca cycle içindeki <b>ilk</b> faz süresi guild kısa-posting mantığına yazılır.
        /// </summary>
        public static void RecordPostingPhaseCompleted(int runPostSec, bool isFirstPostingPhaseInCycle)
        {
            lock (_gate)
            {
                if (isFirstPostingPhaseInCycle)
                    _currentCycleFirstRunPostSec = runPostSec;
            }
        }

        public static void RecordRestockNotificationSent()
        {
            lock (_gate) { _restockNotifySentCount++; }
        }

        public static void RecordGuildBankFlowCompleted(NtfyUserSettings settings)
        {
            lock (_gate)
            {
                _awaitPostAfterGuildBankForShortNotify = true;
                ScheduleNextGuildBankWindow(settings);
            }
        }

        static void ScheduleNextGuildBankWindow(NtfyUserSettings settings)
        {
            double minutes = SampleIntervalMinutes(settings);
            _guildBankEligibleAfterUtc = DateTime.UtcNow + TimeSpan.FromMinutes(minutes);
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
        /// Guild bank akışı bittiyse bir sonraki <see cref="State.RunPostButtonClicked"/> çıkışında tek seferlik true döner (kısa/normal kararı için).
        /// </summary>
        public static bool TryConsumeAwaitingPostAfterGuildBank()
        {
            lock (_gate)
            {
                if (!_awaitPostAfterGuildBankForShortNotify)
                    return false;
                _awaitPostAfterGuildBankForShortNotify = false;
                return true;
            }
        }

        /// <summary>
        /// Zaman aralığı (örneklenmiş pencere) VEYA kısa posting: bu cycle’ın ilk postu, önceki cycle ilk postuna göre kısa mı?
        /// </summary>
        public static bool ShouldRunGuildBank(NtfyUserSettings settings)
        {
            int minDelta = Math.Max(0, settings.GuildBankAfterNotifyMinSecondsShorterThanFirst);
            double ratio = Math.Clamp(settings.RestockThresholdPercent / 100.0, 0.01, 1.0);

            lock (_gate)
            {
                if (_guildBankEligibleAfterUtc.HasValue
                    && DateTime.UtcNow >= _guildBankEligibleAfterUtc.Value)
                    return true;

                if (_priorCycleFirstRunPostSec is int prior && prior > 0
                    && _currentCycleFirstRunPostSec is int cur && cur > 0)
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
    }
}
