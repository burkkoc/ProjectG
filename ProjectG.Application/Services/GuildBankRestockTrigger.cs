namespace ProjectG.ApplicationLayer.Services
{
    /// <summary>
    /// Guild bank tetikleme: min. zaman aralığı VEYA kısa posting (ilk süre × RestockThreshold% veya min. saniye farkı).
    /// </summary>
    public static class GuildBankRestockTrigger
    {
        static readonly object _gate = new();
        static int? _baselineRunPostSec;
        static int _lastRunPostDurationSec;
        static int _restockNotifySentCount;
        static bool _awaitPostAfterGuildBankForShortNotify;
        static DateTime? _guildBankEligibleAfterUtc;

        public static void ResetSession(NtfyUserSettings settings)
        {
            lock (_gate)
            {
                _baselineRunPostSec = null;
                _lastRunPostDurationSec = 0;
                _restockNotifySentCount = 0;
                _awaitPostAfterGuildBankForShortNotify = false;
                ScheduleNextGuildBankWindow(settings);
            }
        }

        public static void RecordBaseline(int firstRunPostSec)
        {
            lock (_gate) { _baselineRunPostSec = firstRunPostSec; }
        }

        public static void RecordRunPostCompleted(int runPostSec)
        {
            lock (_gate) { _lastRunPostDurationSec = runPostSec; }
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
        /// Zaman aralığı (örneklenmiş pencere) VEYA kısa posting: son RunPost ≤ ilk×(RestockThreshold%) veya (ilk−son) ≥ ayarlanan saniye.
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

                if (_baselineRunPostSec is int b && b > 0 && _lastRunPostDurationSec > 0)
                {
                    int shortCapSec = (int)Math.Floor(b * ratio);
                    bool byRatio = _lastRunPostDurationSec <= shortCapSec;
                    bool byDelta = minDelta > 0 && (b - _lastRunPostDurationSec) >= minDelta;
                    if (byRatio || byDelta)
                        return true;
                }

                return false;
            }
        }
    }
}
