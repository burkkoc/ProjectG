using System.Diagnostics;
using System.Runtime.InteropServices;
using ProjectG.DomainLayer.Entities.Concrete;

namespace ProjectG.ApplicationLayer.Services
{
    /// <summary>
    /// İki (veya daha fazla) WoW istemcisi arasında önceki pencereden diğerine geçer; gerekirse görev çubuğundan Alt+Tab kullanılmaz.
    /// </summary>
    public static class WowDualClientWindowSwitcher
    {
        /// <summary>İşlem adı uzantısız (GetProcessesByName).</summary>
        static readonly string[] WowProcessBaseNames = ["Wow", "WowClassic", "WoW"];

        const int SwRestore = 9;

        /// <summary>Sıralı benzersiz ana pencere tutamaçları (dual / UI eşlemesi için kararlı sıra).</summary>
        public static List<IntPtr> GetSortedWowMainWindowHandles() => CollectWowMainWindowHandlesSorted();

        public static IntPtr GetForegroundWindowHandle() => GetForegroundWindow();

        /// <summary>
        /// Odak, WoW ana penceresi veya bu ana pencerenin alt penceresi olduğunda sıralı listedeki indeksi döner
        /// (<see cref="GetForegroundWindow"/> genelde tam <see cref="Process.MainWindowHandle"/> değildir).
        /// </summary>
        public static bool TryGetSortedWowIndexForForeground(IReadOnlyList<IntPtr> sortedHandles, IntPtr foregroundWindow, out int sortedIndex)
        {
            sortedIndex = -1;
            if (foregroundWindow == IntPtr.Zero || sortedHandles.Count == 0)
                return false;

            for (int i = 0; i < sortedHandles.Count; i++)
            {
                if (sortedHandles[i] == foregroundWindow)
                {
                    sortedIndex = i;
                    return true;
                }
            }

            IntPtr root = GetAncestor(foregroundWindow, GA_ROOT);
            if (root == IntPtr.Zero)
                root = foregroundWindow;

            for (int i = 0; i < sortedHandles.Count; i++)
            {
                if (sortedHandles[i] == root)
                {
                    sortedIndex = i;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Kalibrasyon (Z/X): önce ön plan WoW ise onu; değilse <see cref="Cursor"/> altındaki pencerenin kök HWND’ini kullanır.
        /// Böylece odak Project G iken fare WoW üzerindeyken doğru slota yazılır.
        /// </summary>
        public static bool TryResolveWowClientForCalibration(IReadOnlyList<IntPtr> sortedHandles, out IntPtr mainWindow, out int sortedIndex)
        {
            mainWindow = IntPtr.Zero;
            sortedIndex = -1;
            if (sortedHandles.Count == 0)
                return false;

            IntPtr fg = GetForegroundWindow();
            for (int i = 0; i < sortedHandles.Count; i++)
            {
                if (sortedHandles[i] == fg)
                {
                    mainWindow = sortedHandles[i];
                    sortedIndex = i;
                    return true;
                }
            }

            if (!GetCursorPos(out var pt))
                return false;

            IntPtr under = WindowFromPoint(pt);
            if (under == IntPtr.Zero)
                return false;

            IntPtr root = GetAncestor(under, GA_ROOT);
            if (root == IntPtr.Zero)
                root = under;

            for (int i = 0; i < sortedHandles.Count; i++)
            {
                if (sortedHandles[i] == root)
                {
                    mainWindow = root;
                    sortedIndex = i;
                    return true;
                }
            }

            return false;
        }

        /// <summary><see cref="AppSettings.DualClient"/> açıkken UI; <paramref name="handles"/> null ise yeniden tarar.</summary>
        public static void PublishDualClientUiState(IReadOnlyList<IntPtr>? handles = null)
        {
            if (!AppSettings.DualClient || !AppSettings.Working)
            {
                AppSettings.DualClientActiveSlot = 0;
                AppSettings.DualClientTotalWow = 0;
                AppSettings.DualClientWaitRemainingSeconds = 0;
                return;
            }

            List<IntPtr> list = handles == null
                ? CollectWowMainWindowHandlesSorted()
                : new List<IntPtr>(handles);
            AppSettings.DualClientTotalWow = list.Count;
            IntPtr fg = GetForegroundWindow();
            AppSettings.DualClientActiveSlot = TryGetSortedWowIndexForForeground(list, fg, out int idx) ? idx + 1 : 0;
        }

        public static bool TrySwitchBetweenWowWindows()
        {
            var handles = CollectWowMainWindowHandlesSorted();
            if (handles.Count < 2)
                return false;

            IntPtr fg = GetForegroundWindow();
            TryGetSortedWowIndexForForeground(handles, fg, out int idx);
            IntPtr target = idx < 0
                ? handles[0]
                : handles[(idx + 1) % handles.Count];

            return TryActivateWindow(target);
        }

        static List<IntPtr> CollectWowMainWindowHandlesSorted()
        {
            var set = new HashSet<IntPtr>();
            foreach (var baseName in WowProcessBaseNames)
            {
                Process[] processes;
                try
                {
                    processes = Process.GetProcessesByName(baseName);
                }
                catch
                {
                    continue;
                }

                foreach (var p in processes)
                {
                    try
                    {
                        IntPtr h = p.MainWindowHandle;
                        if (h != IntPtr.Zero)
                            set.Add(h);
                    }
                    catch
                    {
                        // ignored
                    }
                    finally
                    {
                        try
                        {
                            p.Dispose();
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }
            }

            return set.OrderBy(x => x.ToInt64()).ToList();
        }

        static bool TryActivateWindow(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero)
                return false;

            if (IsIconic(hWnd))
                ShowWindowAsync(hWnd, SwRestore);

            IntPtr fg = GetForegroundWindow();
            uint fgThread = fg != IntPtr.Zero ? GetWindowThreadProcessId(fg, out _) : 0;
            uint curThread = GetCurrentThreadId();
            bool attached = false;
            if (fgThread != 0 && fgThread != curThread)
                attached = AttachThreadInput(fgThread, curThread, true);

            BringWindowToTop(hWnd);
            bool ok = SetForegroundWindow(hWnd);

            if (attached)
                AttachThreadInput(fgThread, curThread, false);

            return ok;
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        static extern IntPtr WindowFromPoint(POINT pt);

        [DllImport("user32.dll", ExactSpelling = true)]
        static extern IntPtr GetAncestor(IntPtr hwnd, uint gaFlags);

        const uint GA_ROOT = 2;

        [StructLayout(LayoutKind.Sequential)]
        struct POINT
        {
            public int X;
            public int Y;
        }

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("kernel32.dll")]
        static extern uint GetCurrentThreadId();

        [DllImport("user32.dll")]
        static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);
    }
}
