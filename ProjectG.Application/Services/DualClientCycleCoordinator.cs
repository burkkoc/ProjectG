using ProjectG.DomainLayer.Entities.Concrete;
using System.Collections.Concurrent;

namespace ProjectG.ApplicationLayer.Services
{
    public enum DualDowntimeGateResult
    {
        Ok,
        NotEnoughWowWindows,
        ForegroundNotWow,
    }

    /// <summary>
    /// Her WoW ana penceresi için ayrı <see cref="State.OnCycleDowntime"/> bitiş zamanı;
    /// pencereye dönünce kalan süre kadar bekler, yeni döngü süresini bu pencereye yazar, sonra diğer pencereye geçer.
    /// </summary>
    public static class DualClientCycleCoordinator
    {
        static readonly ConcurrentDictionary<IntPtr, DateTime> NextWorkAllowedUtcByMainWindow = new();

        public static void Reset() => NextWorkAllowedUtcByMainWindow.Clear();

        /// <summary>
        /// Ön plandaki WoW için (varsa) kalan beklemeyi uygular, yeni cycle downtime kaydeder, <see cref="SimulateService.SwitchWindow"/> çağırır.
        /// </summary>
        public static async Task<DualDowntimeGateResult> RunOnCycleDowntimeGateAsync(SimulateService simulate)
        {
            var handles = WowDualClientWindowSwitcher.GetSortedWowMainWindowHandles();
            if (handles.Count < 2)
                return DualDowntimeGateResult.NotEnoughWowWindows;

            WowDualClientWindowSwitcher.PublishDualClientUiState(handles);

            if (!TryResolveForegroundIndex(handles, out int idx))
            {
                await simulate.SwitchWindow();
                await Task.Delay(150);
                if (!TryResolveForegroundIndex(handles, out idx))
                    return DualDowntimeGateResult.ForegroundNotWow;
            }

            IntPtr thisHwnd = handles[idx];

            if (NextWorkAllowedUtcByMainWindow.TryGetValue(thisHwnd, out var deadline)
                && deadline > DateTime.UtcNow)
            {
                await WaitClientCooldownWithUiAsync(deadline, idx + 1, handles.Count);
            }

            int downtimeMs = UtilityService.SetCycleDowntime();
            AppSettings.Downtime = downtimeMs;
            NextWorkAllowedUtcByMainWindow[thisHwnd] = DateTime.UtcNow.AddMilliseconds(downtimeMs);
            AppSettings.DualClientWaitRemainingSeconds = 0;

            await simulate.SwitchWindow();
            AppSettings.Downtime = 0;
            WowDualClientWindowSwitcher.PublishDualClientUiState();

            return DualDowntimeGateResult.Ok;
        }

        static bool TryResolveForegroundIndex(IReadOnlyList<IntPtr> handles, out int index)
        {
            IntPtr fg = WowDualClientWindowSwitcher.GetForegroundWindowHandle();
            return WowDualClientWindowSwitcher.TryGetSortedWowIndexForForeground(handles, fg, out index);
        }

        static async Task WaitClientCooldownWithUiAsync(DateTime deadlineUtc, int displaySlot1Based, int totalWow)
        {
            while (true)
            {
                var now = DateTime.UtcNow;
                if (now >= deadlineUtc)
                    break;

                double sec = Math.Ceiling((deadlineUtc - now).TotalSeconds);
                int s = (int)Math.Clamp(sec, 1, int.MaxValue);
                AppSettings.DualClientWaitRemainingSeconds = s;
                AppSettings.DualClientActiveSlot = displaySlot1Based;
                AppSettings.DualClientTotalWow = totalWow;
                int step = (int)Math.Min(250, Math.Max(1, (deadlineUtc - now).TotalMilliseconds));
                await Task.Delay(step);
            }

            AppSettings.DualClientWaitRemainingSeconds = 0;
        }
    }
}
