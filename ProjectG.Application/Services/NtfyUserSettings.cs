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

        /// <summary>CancelingLoaded: kalis suresi (sn) alt sinir; her giriste Max ile rastgele ust sinir secilir, dolunca CancelingDone.</summary>
        public int CancelingLoadedMaxStayMinSeconds { get; set; } = 30;

        public int CancelingLoadedMaxStayMaxSeconds { get; set; } = 40;

        /// <summary>Short cycle dinamik bekleme: min saniye ≈ T × bu çarpan (T = RunCancel → CancelingDone).</summary>
        public double DynamicShortAfterCancelMinTMultiplier { get; set; } = 2;

        /// <summary>Short cycle dinamik bekleme: max saniye ≈ T × bu çarpan + ek saniye.</summary>
        public double DynamicShortAfterCancelMaxTMultiplier { get; set; } = 2;

        /// <summary>Short cycle dinamik bekleme üst uca eklenecek sabit saniye (varsayılan 10 → [2T, 2T+10]).</summary>
        public double DynamicShortAfterCancelMaxExtraSeconds { get; set; } = 10;

        public int CustomDowntimeMinSeconds { get; set; } = 120;
        public int CustomDowntimeMaxSeconds { get; set; } = 180;
        public string SelectedCycleDowntime { get; set; } = "Short";

        /// <summary>Ana penceredeki MultipleClient kutusu; uygulama kapanışında kaydedilir.</summary>
        public bool MultipleClientEnabled { get; set; }

        /// <summary>Ana penceredeki Dynamic kutusu; uygulama kapanışında kaydedilir.</summary>
        public bool DynamicAhFlowEnabled { get; set; }

        /// <summary>Tek istemci: Z/X posta ve guild bank kalibrasyonu (ekran çözünürlüğü ile).</summary>
        public SlotCalibrationPersist? SingleClientCalibration { get; set; }

        /// <summary>Çoklu istemci: sıralı WoW slotu (1, 2, …) → kayıtlı Z/X dikdörtgenleri. HWND oturumlar arası değişir; anahtar slot indeksidir.</summary>
        public Dictionary<string, SlotCalibrationPersist>? DualSlotCalibrations { get; set; }

        /// <summary>Makro oturumu üst süresi (dakika); ana uygulama yüklerken AppSettings.ExitTime olur. 0 = kapalı.</summary>
        public int ExitTimeMinutes { get; set; }

        /// <summary>Stall watchdog (~120 sn) ile OnCycleDowntime kurtarma sonrası ntfy metin.</summary>
        public bool NotifyOnStallRecovery { get; set; } = true;

        /// <summary>Posta / guild bank köşe referansı eşleşmezse (kritik tıklama iptali) ntfy metin.</summary>
        public bool NotifyOnCriticalClickFailure { get; set; } = true;

        /// <summary>ExitTime dolmadan kaç dakika önce tek seferlik uyarı. 0 = kapalı.</summary>
        public int ExitTimeNotifyMinutesBefore { get; set; } = 3;
    }

    public sealed class PersistedRectangleDto
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public sealed class SlotCalibrationPersist
    {
        public PersistedRectangleDto? MailBox { get; set; }
        public PersistedRectangleDto? GuildBank { get; set; }
        public int ScreenResolutionX { get; set; }
        public int ScreenResolutionY { get; set; }
    }
}
