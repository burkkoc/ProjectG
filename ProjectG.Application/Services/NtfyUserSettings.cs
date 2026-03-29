namespace ProjectG.ApplicationLayer.Services
{
    public sealed class NtfyUserSettings
    {
        public string NtfyNotifyTopicUrl { get; set; } = "https://ntfy.sh/bk-pc-9f3k2m7x";
        public string MailboxLocateHotkey { get; set; } = "Z";
        public string GuildBankLocateHotkey { get; set; } = "X";
        public int RestockMaxNotificationCount { get; set; } = 3;
        public int RestockThresholdPercent { get; set; } = 90;

        /// <summary>Guild bank zaman OR eşiğinin alt ucu (dk); üst uç <see cref="GuildBankMaxIntervalMinutes"/> ile rastgele seçilir.</summary>
        public double GuildBankMinIntervalMinutes { get; set; } = 2;

        /// <summary>Guild bank zaman OR eşiğinin üst ucu (dk). 0 veya ayar yoksa min ile aynı kabul edilir (eski tek alan davranışı).</summary>
        public double GuildBankMaxIntervalMinutes { get; set; } = 0;

        /// <summary>
        /// OR koşulu: son RunPost süresi, ilk posting süresinden en az bu kadar saniye kısa ise guild bank tetiklenir
        /// (Restock Threshold % ile birlikte; 0 ise yalnızca yüzde eşiği kullanılır).
        /// </summary>
        public int GuildBankAfterNotifyMinSecondsShorterThanFirst { get; set; } = 60;
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
