using ProjectG.DomainLayer.Entities.Concrete;
using ProjectG.DomainLayer.Entities.Enums;
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

        /// <summary>
        /// İptal fazı sonrası Short cycle [min,max] ms; çift istemicide her WoW ana penceresi ayrı (global <see cref="AppSettings.DynamicShortCycleDowntimeMs"/> yedeği).
        /// </summary>
        static readonly ConcurrentDictionary<IntPtr, int[]> PerClientDynamicShortCycleDowntimeMs = new();

        /// <summary>
        /// <see cref="State.OnCycleDowntime"/> içinde kalındığı sürece handler birden fazla çağrılabilir; istemci değişimi yalnızca bu kalışa bir kez yapılmalı.
        /// </summary>
        static bool _dualClientHandoffPendingThisDowntimeStay = true;

        /// <summary>Yeni WoW'a geçildi; downtime oyuncusu bir kez T/Y ile AH hedefini ve sohbet penceresini yenilemeli.</summary>
        static bool _pendingAhPrepAfterWoWHandoff;

        public static void Reset()
        {
            NextWorkAllowedUtcByMainWindow.Clear();
            PerClientDynamicShortCycleDowntimeMs.Clear();
            _dualClientHandoffPendingThisDowntimeStay = true;
            _pendingAhPrepAfterWoWHandoff = false;
        }

        /// <summary>Cancel→Short dinamik aralığı bu WoW için saklar.</summary>
        public static void SetPerClientDynamicShortCycleDowntimeMs(IntPtr mainWindowHandle, int minMs, int maxMs)
        {
            if (mainWindowHandle == IntPtr.Zero || !AppSettings.DualClient)
                return;
            PerClientDynamicShortCycleDowntimeMs[mainWindowHandle] = [minMs, maxMs];
        }

        /// <summary>
        /// <see cref="CycleDowntime.Short"/> ve çift istemici için bu pencereye özgü aralık varsa döner.
        /// </summary>
        public static bool TryGetPerClientDynamicShortCycleDowntimeMs(IntPtr mainWindowHandle, out int[] rangeMs)
        {
            rangeMs = null!;
            if (mainWindowHandle == IntPtr.Zero || !AppSettings.DualClient)
                return false;
            if (!PerClientDynamicShortCycleDowntimeMs.TryGetValue(mainWindowHandle, out var r) || r is not { Length: >= 2 })
                return false;
            rangeMs = r;
            return true;
        }

        /// <summary>Makro <see cref="State.OnCycleDowntime"/> durumuna her geçişte çağrılır.</summary>
        public static void MarkOnCycleDowntimeEntered() => _dualClientHandoffPendingThisDowntimeStay = true;

        /// <summary>Tam el değişimi yapıldıysa true döner ve bayrağı temizler (aynı çağrıda tüketilir).</summary>
        public static bool TryConsumePendingAhPrepAfterWoWHandoff()
        {
            if (!_pendingAhPrepAfterWoWHandoff)
                return false;
            _pendingAhPrepAfterWoWHandoff = false;
            return true;
        }

        /// <summary>
        /// Ön plandaki WoW için (varsa) kalan beklemeyi uygular, yeni cycle downtime kaydeder, <see cref="SimulateService.SwitchWindow"/> çağırır.
        /// </summary>
        public static async Task<DualDowntimeGateResult> RunOnCycleDowntimeGateAsync(SimulateService simulate)
        {
            var handles = WowDualClientWindowSwitcher.GetSortedWowMainWindowHandles();
            if (handles.Count < 2)
                return DualDowntimeGateResult.NotEnoughWowWindows;

            if (!_dualClientHandoffPendingThisDowntimeStay)
            {
                WowDualClientWindowSwitcher.PublishDualClientUiState(handles);
                return DualDowntimeGateResult.Ok;
            }

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

            int[]? shortOverride = null;
            if (AppSettings.CycleDowntime == CycleDowntime.Short
                && TryGetPerClientDynamicShortCycleDowntimeMs(thisHwnd, out var pr))
                shortOverride = pr;

            int downtimeMs = UtilityService.SetCycleDowntime(shortOverride);
            AppSettings.Downtime = downtimeMs;
            NextWorkAllowedUtcByMainWindow[thisHwnd] = DateTime.UtcNow.AddMilliseconds(downtimeMs);
            AppSettings.DualClientWaitRemainingSeconds = 0;

            await simulate.SwitchWindow();
            AppSettings.Downtime = 0;
            WowDualClientWindowSwitcher.PublishDualClientUiState();
            _dualClientHandoffPendingThisDowntimeStay = false;
            _pendingAhPrepAfterWoWHandoff = true;

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
