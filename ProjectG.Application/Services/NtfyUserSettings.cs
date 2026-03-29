namespace ProjectG.ApplicationLayer.Services
{
    public sealed class NtfyUserSettings
    {
        public string NtfyNotifyTopicUrl { get; set; } = "https://ntfy.sh/bk-pc-9f3k2m7x";
        public string MailboxLocateHotkey { get; set; } = "Z";
        public int RestockMaxNotificationCount { get; set; } = 3;
        public int RestockThresholdPercent { get; set; } = 90;
        public int CancelingLoadedExtraThresholdSeconds { get; set; } = 5;

        /// <summary>Short cycle dinamik bekleme: min saniye ≈ T × bu çarpan (T = RunCancel → CancelingDone).</summary>
        public double DynamicShortAfterCancelMinTMultiplier { get; set; } = 2;

        /// <summary>Short cycle dinamik bekleme: max saniye ≈ T × bu çarpan + ek saniye.</summary>
        public double DynamicShortAfterCancelMaxTMultiplier { get; set; } = 2;

        /// <summary>Short cycle dinamik bekleme üst uca eklenecek sabit saniye (varsayılan 10 → [2T, 2T+10]).</summary>
        public double DynamicShortAfterCancelMaxExtraSeconds { get; set; } = 10;

        public int CustomDowntimeMinSeconds { get; set; } = 120;
        public int CustomDowntimeMaxSeconds { get; set; } = 180;
        public string SelectedCycleDowntime { get; set; } = "Short";
    }
}
