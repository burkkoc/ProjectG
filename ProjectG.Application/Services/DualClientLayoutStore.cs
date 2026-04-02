using System.Drawing;
using System.Globalization;
using System.Linq;
using ProjectG.DomainLayer.Entities.Concrete;
using ProjectG.DomainLayer.Entities.Concrete.Statics;

namespace ProjectG.ApplicationLayer.Services
{
    /// <summary>
    /// Çoklu WoW: her ana pencere (<see cref="IntPtr"/> HWND) için ayrı tarama durumu; klasör adı sıralı listedeki 1-based indextir.
    /// </summary>
    public static class DualClientLayoutStore
    {
        /// <summary>Dual mod için en az bu kadar WoW penceresi gerekir.</summary>
        public const int MinimumWowWindowsForDual = 2;

        static readonly Dictionary<IntPtr, DualClientLayoutSnapshot> Snapshots = new();
        static readonly Dictionary<int, SlotCalibrationPersist> PersistedCalibBySortedIndex = new();
        static IntPtr _lastAppliedHwnd = IntPtr.Zero;

        /// <summary>Son <see cref="EnsureGlobalsMatchForegroundWow"/> ile eşlenen WoW ana penceresi; ön plan çözülemezken restock anahtarı yedeği.</summary>
        public static IntPtr LastAppliedWowMainWindowHandle => _lastAppliedHwnd;

        /// <summary>Oturum izini sıfırlar; HWND snapshot sözlüğü kalır.</summary>
        public static void ClearActiveSlotTracking() => _lastAppliedHwnd = IntPtr.Zero;

        /// <summary>Tüm HWND anlık görüntülerini ve oturum izini siler.</summary>
        public static void ResetAllSnapshots()
        {
            Snapshots.Clear();
            _lastAppliedHwnd = IntPtr.Zero;
        }

        public static void LoadPersistedSlotsFromSettings(NtfyUserSettings settings)
        {
            PersistedCalibBySortedIndex.Clear();
            if (settings.DualSlotCalibrations == null)
                return;
            foreach (var kv in settings.DualSlotCalibrations)
            {
                if (!int.TryParse(kv.Key, NumberStyles.Integer, CultureInfo.InvariantCulture, out int oneBased))
                    continue;
                int idx = oneBased - 1;
                if (idx < 0 || kv.Value == null)
                    continue;
                PersistedCalibBySortedIndex[idx] = kv.Value;
            }
        }

        public static void FlushPersistedDualSlotsToSettings(NtfyUserSettings settings)
        {
            settings.DualSlotCalibrations = PersistedCalibBySortedIndex.Count == 0
                ? null
                : PersistedCalibBySortedIndex
                    .OrderBy(kv => kv.Key)
                    .ToDictionary(kv => (kv.Key + 1).ToString(CultureInfo.InvariantCulture), kv => kv.Value);
        }

        /// <summary>Çift istemci + en az iki WoW açıksa, sıra ve kalıcı kalibrasyonu <see cref="AppSettings"/> ile hizalar (Start’tan önce).</summary>
        public static bool TryApplyCalibrationFromPersistedToAppIfDualReady()
        {
            var handles = WowDualClientWindowSwitcher.GetSortedWowMainWindowHandles();
            if (handles.Count < MinimumWowWindowsForDual)
                return false;
            if (!WowDualClientWindowSwitcher.TryResolveWowClientForCalibration(handles, out var mainHwnd, out int idx))
                return false;

            var s = GetOrCreateSnapshot(mainHwnd);
            ApplyPersistedSlotCalibration(idx, s);
            if (s.MailBoxPosition is null && s.GuildBankPosition is null)
                return false;

            AppSettings.MailBoxPosition = s.MailBoxPosition;
            AppSettings.GuildBankPosition = s.GuildBankPosition;
            if (s.ScreenResolutionX > 0 && s.ScreenResolutionY > 0)
            {
                UserInput.ScreenResolutionX = s.ScreenResolutionX;
                UserInput.ScreenResolutionY = s.ScreenResolutionY;
            }

            return true;
        }

        public static void ApplySingleClientCalibrationFromPersist(SlotCalibrationPersist? p)
        {
            if (p == null)
                return;
            ApplySlotCalibrationPersistToApp(p);
        }

        /// <summary>
        /// WoW henüz yokken dual kalibrasyonu diskten yüklendiyse, en küçük slot indeksindeki Z/X + çözünürlüğü <see cref="AppSettings"/> ile eşler (Start düğmesi için).
        /// </summary>
        public static void ApplyFirstPersistedDualSlotCalibrationToAppIfAny()
        {
            if (PersistedCalibBySortedIndex.Count == 0)
                return;
            int minSlot = PersistedCalibBySortedIndex.Keys.Min();
            ApplySlotCalibrationPersistToApp(PersistedCalibBySortedIndex[minSlot]);
        }

        static void ApplySlotCalibrationPersistToApp(SlotCalibrationPersist p)
        {
            if (p.MailBox is { Width: > 0, Height: > 0 })
                AppSettings.MailBoxPosition = FromDto(p.MailBox);
            if (p.GuildBank is { Width: > 0, Height: > 0 })
                AppSettings.GuildBankPosition = FromDto(p.GuildBank);
            if (p.ScreenResolutionX > 0 && p.ScreenResolutionY > 0)
            {
                UserInput.ScreenResolutionX = p.ScreenResolutionX;
                UserInput.ScreenResolutionY = p.ScreenResolutionY;
            }
        }

        public static SlotCalibrationPersist? BuildSingleCalibrationPersistFromGlobals()
        {
            if (AppSettings.MailBoxPosition is null && AppSettings.GuildBankPosition is null)
                return null;
            return new SlotCalibrationPersist
            {
                MailBox = ToDto(AppSettings.MailBoxPosition),
                GuildBank = ToDto(AppSettings.GuildBankPosition),
                ScreenResolutionX = UserInput.ScreenResolutionX,
                ScreenResolutionY = UserInput.ScreenResolutionY
            };
        }

        static PersistedRectangleDto? ToDto(Rectangle? r) =>
            r is { Width: > 0, Height: > 0 } rect
                ? new PersistedRectangleDto { X = rect.X, Y = rect.Y, Width = rect.Width, Height = rect.Height }
                : null;

        static Rectangle? FromDto(PersistedRectangleDto? d) =>
            d is { Width: > 0, Height: > 0 }
                ? new Rectangle(d.X, d.Y, d.Width, d.Height)
                : null;

        static SlotCalibrationPersist SnapshotRectsToPersist(DualClientLayoutSnapshot s) =>
            new()
            {
                MailBox = ToDto(s.MailBoxPosition),
                GuildBank = ToDto(s.GuildBankPosition),
                ScreenResolutionX = s.ScreenResolutionX,
                ScreenResolutionY = s.ScreenResolutionY
            };

        static void ApplyPersistedSlotCalibration(int sortedIndex, DualClientLayoutSnapshot s)
        {
            if (!PersistedCalibBySortedIndex.TryGetValue(sortedIndex, out var p))
                return;
            if (s.MailBoxPosition is null && p.MailBox is { Width: > 0, Height: > 0 })
                s.MailBoxPosition = FromDto(p.MailBox);
            if (s.GuildBankPosition is null && p.GuildBank is { Width: > 0, Height: > 0 })
                s.GuildBankPosition = FromDto(p.GuildBank);
            if ((s.ScreenResolutionX <= 0 || s.ScreenResolutionY <= 0) && p.ScreenResolutionX > 0 && p.ScreenResolutionY > 0)
            {
                s.ScreenResolutionX = p.ScreenResolutionX;
                s.ScreenResolutionY = p.ScreenResolutionY;
            }
        }

        static void SyncPersistedSlotFromSnapshot(int sortedIndex, DualClientLayoutSnapshot s)
        {
            bool hasMail = s.MailBoxPosition is { Width: > 0, Height: > 0 };
            bool hasGuild = s.GuildBankPosition is { Width: > 0, Height: > 0 };
            bool hasScreen = s.ScreenResolutionX > 0 && s.ScreenResolutionY > 0;
            if (!hasMail && !hasGuild && !hasScreen)
                return;
            PersistedCalibBySortedIndex[sortedIndex] = SnapshotRectsToPersist(s);
        }

        static int? TryGetSortedIndexForHandle(IntPtr mainWindowHandle)
        {
            var handles = WowDualClientWindowSwitcher.GetSortedWowMainWindowHandles();
            for (int i = 0; i < handles.Count; i++)
            {
                if (handles[i] == mainWindowHandle)
                    return i;
            }

            return null;
        }

        public static string GetDualImageRootForSortedIndex(int sortedIndex) =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "Dual", (sortedIndex + 1).ToString());

        /// <summary>Görüntü dosya yollarını <c>Images/Dual/{sortedIndex+1}/</c> altına ayarlar.</summary>
        public static void ApplyPathsForSortedIndex(int sortedIndex)
        {
            if (sortedIndex < 0)
                return;

            string root = GetDualImageRootForSortedIndex(sortedIndex);
            Directory.CreateDirectory(root);
            Paths.AHMenuImagePath = Path.Combine(root, "AHMenu.png");
            Paths.GeneralImagePath = Path.Combine(root, "General.png");
            Paths.PostCancelImagePath = Path.Combine(root, "PostCancel.png");
            Paths.MailBoxImagePath = Path.Combine(root, "MailBox.png");
            Paths.GuildBankImagePath = Path.Combine(root, "GuildBank.png");
            Paths.ChatWindowImagePath = Path.Combine(root, "Chat.png");
            Paths.MailBoxTextImagePath = Path.Combine(root, "MailBoxText.png");
            Paths.MailBoxCornerReferencePath = Path.Combine(root, "MailBoxCornerReference.png");
            Paths.GuildBankCornerReferencePath = Path.Combine(root, "GuildBankCornerReference.png");
        }

        static DualClientLayoutSnapshot GetOrCreateSnapshot(IntPtr mainWindowHandle)
        {
            if (!Snapshots.TryGetValue(mainWindowHandle, out var s))
            {
                s = new DualClientLayoutSnapshot();
                Snapshots[mainWindowHandle] = s;
            }

            return s;
        }

        public static void SaveGlobalsToHandle(IntPtr mainWindowHandle)
        {
            if (mainWindowHandle == IntPtr.Zero)
                return;

            var s = GetOrCreateSnapshot(mainWindowHandle);
            s.AHBorder = TSMWindow.AHBorder;
            s.BorderColor = TSMWindow.BorderColor;
            s.MailBoxBorder = TSMWindow.MailBoxBorder;
            s.BankBorder = TSMWindow.BankBorder;
            s.ChatWindow = TSMWindow.ChatWindow;
            s.MailboxTextWindow = TSMWindow.MailboxTextWindow;

            s.OpenAllMailButton = StaticTSMButtons.OpenAllMailButton;
            s.CancelledButton = StaticTSMButtons.CancelledButton;
            s.RunPostScan = StaticTSMButtons.RunPostScan;
            s.RunCancelScan = StaticTSMButtons.RunCancelScan;
            s.ExitButton = StaticTSMButtons.ExitButton;
            s.RestockButton = StaticTSMButtons.RestockButton;

            s.TsmButtonsList = TSMButton.TSMButtons.ToList();
            s.MailBoxPosition = AppSettings.MailBoxPosition;
            s.GuildBankPosition = AppSettings.GuildBankPosition;
            s.ScreenResolutionX = UserInput.ScreenResolutionX;
            s.ScreenResolutionY = UserInput.ScreenResolutionY;
        }

        public static void LoadGlobalsFromHandle(IntPtr mainWindowHandle, int sortedIndex)
        {
            if (mainWindowHandle == IntPtr.Zero || sortedIndex < 0)
                return;

            ApplyPathsForSortedIndex(sortedIndex);
            var s = GetOrCreateSnapshot(mainWindowHandle);
            ApplyPersistedSlotCalibration(sortedIndex, s);

            TSMWindow.AHBorder = s.AHBorder;
            TSMWindow.BorderColor = s.BorderColor;
            TSMWindow.MailBoxBorder = s.MailBoxBorder;
            TSMWindow.BankBorder = s.BankBorder;
            TSMWindow.ChatWindow = s.ChatWindow;
            TSMWindow.MailboxTextWindow = s.MailboxTextWindow;

            StaticTSMButtons.OpenAllMailButton = s.OpenAllMailButton;
            StaticTSMButtons.CancelledButton = s.CancelledButton;
            StaticTSMButtons.RunPostScan = s.RunPostScan;
            StaticTSMButtons.RunCancelScan = s.RunCancelScan;
            StaticTSMButtons.ExitButton = s.ExitButton;
            StaticTSMButtons.RestockButton = s.RestockButton;

            TSMButton.TSMButtons = s.TsmButtonsList.Count > 0
                ? new List<TSMButton>(s.TsmButtonsList)
                : new List<TSMButton>();

            AppSettings.MailBoxPosition = s.MailBoxPosition;
            AppSettings.GuildBankPosition = s.GuildBankPosition;
            if (s.ScreenResolutionX > 0 && s.ScreenResolutionY > 0)
            {
                UserInput.ScreenResolutionX = s.ScreenResolutionX;
                UserInput.ScreenResolutionY = s.ScreenResolutionY;
            }
        }

        /// <summary>Ön plandaki WoW değiştiyse önceki HWND durumunu kaydeder ve yenisini yükleer.</summary>
        public static void EnsureGlobalsMatchForegroundWow(bool requireMacroRunning = true)
        {
            if (!AppSettings.DualClient)
                return;
            if (requireMacroRunning && !AppSettings.Working)
                return;

            var handles = WowDualClientWindowSwitcher.GetSortedWowMainWindowHandles();
            if (handles.Count < MinimumWowWindowsForDual)
                return;

            IntPtr fg = WowDualClientWindowSwitcher.GetForegroundWindowHandle();
            if (!WowDualClientWindowSwitcher.TryGetSortedWowIndexForForeground(handles, fg, out int idx))
                return;

            if (fg == _lastAppliedHwnd)
                return;

            if (_lastAppliedHwnd != IntPtr.Zero)
                SaveGlobalsToHandle(_lastAppliedHwnd);

            LoadGlobalsFromHandle(fg, idx);
            _lastAppliedHwnd = fg;
        }

        public static void FlushActiveSlotToSnapshot()
        {
            if (!AppSettings.DualClient)
                return;
            if (_lastAppliedHwnd != IntPtr.Zero)
                SaveGlobalsToHandle(_lastAppliedHwnd);
        }

        /// <summary>Locate: ön plandaki WoW için Paths + snapshot’taki kalibrasyonu <see cref="AppSettings"/> ile eşler.</summary>
        public static void ApplyPathsForForegroundCalibrationSlot(bool dualChecked)
        {
            if (!dualChecked)
                return;

            var handles = WowDualClientWindowSwitcher.GetSortedWowMainWindowHandles();
            if (handles.Count < MinimumWowWindowsForDual)
                return;

            if (!WowDualClientWindowSwitcher.TryResolveWowClientForCalibration(handles, out var mainHwnd, out int idx))
                return;

            ApplyPathsForSortedIndex(idx);
            var s = GetOrCreateSnapshot(mainHwnd);
            ApplyPersistedSlotCalibration(idx, s);

            AppSettings.MailBoxPosition = s.MailBoxPosition;
            AppSettings.GuildBankPosition = s.GuildBankPosition;
            if (s.ScreenResolutionX > 0 && s.ScreenResolutionY > 0)
            {
                UserInput.ScreenResolutionX = s.ScreenResolutionX;
                UserInput.ScreenResolutionY = s.ScreenResolutionY;
            }
        }

        /// <summary>Posta / guild locate sonrası ilgili HWND snapshot’ına yazar.</summary>
        public static void PersistCalibrationRectsForForeground(bool dualChecked, Rectangle? mailBox, Rectangle? guildBank, int screenX, int screenY)
        {
            if (!dualChecked)
                return;

            var handles = WowDualClientWindowSwitcher.GetSortedWowMainWindowHandles();
            if (handles.Count < MinimumWowWindowsForDual)
                return;

            if (!WowDualClientWindowSwitcher.TryResolveWowClientForCalibration(handles, out var mainHwnd, out int idx))
                return;

            var s = GetOrCreateSnapshot(mainHwnd);
            s.MailBoxPosition = mailBox;
            s.GuildBankPosition = guildBank;
            s.ScreenResolutionX = screenX;
            s.ScreenResolutionY = screenY;
            SyncPersistedSlotFromSnapshot(idx, s);
        }
    }
}
