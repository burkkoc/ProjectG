namespace ProjectG.ApplicationLayer.Services
{
    public sealed class NtfyUserSettings
    {
        public string NtfyNotifyTopicUrl { get; set; } = "https://ntfy.sh/bk-pc-9f3k2m7x";
        public string MailboxLocateHotkey { get; set; } = "Z";
        public int RestockMaxNotificationCount { get; set; } = 3;
        public int RestockThresholdPercent { get; set; } = 90;
        public int CancelingLoadedExtraThresholdSeconds { get; set; } = 5;
        public int CustomDowntimeMinSeconds { get; set; } = 120;
        public int CustomDowntimeMaxSeconds { get; set; } = 180;
        public string SelectedCycleDowntime { get; set; } = "Short";
    }
}
