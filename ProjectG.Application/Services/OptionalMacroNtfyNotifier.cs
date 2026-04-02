using System.Text;
using ProjectG.DomainLayer.Entities.Concrete;
using ProjectG.DomainLayer.Entities.Enums;
using System;
using System.Threading.Tasks;

namespace ProjectG.ApplicationLayer.Services
{
    /// <summary>
    /// Stall kurtarma, ExitTime oncesi uyari ve kose referansi tiklama hatalari icin istege bagli ntfy metin bildirimleri.
    /// </summary>
    public static class OptionalMacroNtfyNotifier
    {
        static DateTime _lastStallNotifyUtc = DateTime.MinValue;
        static DateTime _lastCornerFailNotifyUtc = DateTime.MinValue;
        static bool _exitTimeWarningSent;

        static readonly TimeSpan StallNotifyCooldown = TimeSpan.FromMinutes(2);
        static readonly TimeSpan CornerFailNotifyCooldown = TimeSpan.FromMinutes(5);

        public static void ResetSessionFlags() => _exitTimeWarningSent = false;

        public static async Task NotifyStallRecoveryIfEnabledAsync(State stalledInState)
        {
            if (!AppSettings.Working)
                return;

            var settings = NtfySettingsStore.Load();
            if (!settings.NotifyOnStallRecovery)
                return;

            var now = DateTime.UtcNow;
            if (now - _lastStallNotifyUtc < StallNotifyCooldown)
                return;

            _lastStallNotifyUtc = now;

            string title = $"Makro kurtarma | {SanitizeAsciiMachineName()}";
            string msg =
                $"Takilan state: {stalledInState}. ~120 sn watchdog: AH aciksa ESC, ardindan OnCycleDowntime.";
            await InternetConnectivityMonitor.SendTextNotificationAsync(title, msg).ConfigureAwait(false);
        }

        /// <summary> ExitTime dakikasi dolmadan once (ayar &gt; 0) bir kez uyari gonderir. </summary>
        public static void TryNotifyExitTimeApproachingIfEnabled()
        {
            if (!AppSettings.Working || _exitTimeWarningSent)
                return;
            if (AppSettings.ExitTime <= 0 || AppSettings.MacroSessionStartedUtc == default)
                return;

            var settings = NtfySettingsStore.Load();
            int warnBeforeMin = settings.ExitTimeNotifyMinutesBefore;
            if (warnBeforeMin <= 0)
                return;

            double elapsedMin = (DateTime.UtcNow - AppSettings.MacroSessionStartedUtc).TotalMinutes;
            double remainingMin = AppSettings.ExitTime - elapsedMin;
            if (remainingMin <= 0 || remainingMin > warnBeforeMin)
                return;

            _exitTimeWarningSent = true;

            string title = $"Makro bitiyor | {SanitizeAsciiMachineName()}";
            string msg =
                $"Yaklasik {remainingMin:F1} dk sonra ExitTime dolacak (oturum {AppSettings.ExitTime} dk). WoW ve uygulama kapanacak.";

            _ = Task.Run(async () =>
            {
                try
                {
                    await InternetConnectivityMonitor.SendTextNotificationAsync(title, msg).ConfigureAwait(false);
                }
                catch
                {
                    // ignored
                }
            });
        }

        public static async Task NotifyCornerReferenceFailureIfEnabledAsync(bool isGuildBank)
        {
            if (!AppSettings.Working)
                return;

            var settings = NtfySettingsStore.Load();
            if (!settings.NotifyOnCriticalClickFailure)
                return;

            var now = DateTime.UtcNow;
            if (now - _lastCornerFailNotifyUtc < CornerFailNotifyCooldown)
                return;

            _lastCornerFailNotifyUtc = now;

            string target = isGuildBank ? "Guild bank" : "Mailbox";
            string title = $"Kose eslesme basarisiz | {SanitizeAsciiMachineName()}";
            string msg =
                $"{target}: kayitli kose referansi ile ekran eslesmedi; tiklama iptal, OnCycleDowntime. Kalibrasyonu (Z/X) kontrol edin.";
            await InternetConnectivityMonitor.SendTextNotificationAsync(title, msg).ConfigureAwait(false);
        }

        static string SanitizeAsciiMachineName()
        {
            string m = Environment.MachineName;
            if (string.IsNullOrEmpty(m))
                return "?";
            var sb = new StringBuilder(m.Length);
            foreach (char c in m)
                sb.Append(c <= 0x7f ? c : '?');
            return sb.ToString();
        }
    }
}
