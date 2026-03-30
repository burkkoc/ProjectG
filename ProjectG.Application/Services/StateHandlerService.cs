using ProjectG.DomainLayer.Entities.Concrete;
using ProjectG.DomainLayer.Entities.Concrete.Statics;
using ProjectG.DomainLayer.Entities.Enums;
using ProjectG.InfrastructureLayer.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsInput.Native;

namespace ProjectG.ApplicationLayer.Services
{
    public class StateHandlerService
    {
        private readonly SimulateService _simulateService;
        private int exitCounter;
        private bool result;
        private Task<bool>? resultTask = null;
        private Task<bool>? isFailed = null;
        private Task<bool>? isClickable = null;
        private bool isResetted;
        private bool amIGoingToCloseMailbox;
        private Task<bool>? isWindowOpen = null;
        private bool startingNow = true;
        private DateTime? cancelingLoadedEnteredUtc;
        private TimeSpan? firstCancelingLoadedDuration;
        private TimeSpan cancelingLoadedExtraThreshold = TimeSpan.FromSeconds(5);

        enum DynamicAhFlowVariant
        {
            None,
            ClassicCancelFirst,
            PostDelayCancelPost,
            PostThenCancel,
            PostDelayThenCancel,
        }

        DynamicAhFlowVariant _dynamicAhFlowVariant;
        bool _dynamicAhFlowLocked;
        bool _dynamicAhPostFirstExitExpectCancel;
        bool _dynamicAhV2MailboxAfterSecondPost;
        bool _dynamicAhForceMailboxPostingDone;
        bool _onCycleDowntimeEntryEscHandled;

        /// <summary>Bir önceki AH beklemede pencere/algı başarısız olduysa bir sonraki girişte hedefle + etkileşim tuşu basılır.</summary>
        bool _prepNextWaitingForAhWindowTargetAndInteract;

        static readonly VirtualKeyCode AhWindowRetryTargetKey = VirtualKeyCode.VK_T;
        /// <summary>WoW varsayılan etkileşim tuşu; bağlantı farklıysa sabiti güncelleyin.</summary>
        static readonly VirtualKeyCode AhWindowRetryInteractKey = VirtualKeyCode.VK_Y;

        public StateHandlerService(SimulateService simulateService)
        {
            _simulateService = simulateService;
            LoadCancelingLoadedThresholdSettings();
        }
        public async Task OnCycleDowntimeHandler()
        {
            try
            {
                await Task.Delay(UtilityService.GenerateRandom(2400, 3601));
                await _simulateService.MouseMove(50, ButtonContainer.AH);
                if (TSMWindow.ChatWindow == null)
                {
                    TSMWindow.ChatWindow = await PixelProcessService.FindChatWindow(Enums.ActiveWindow.Chat);
                }
                if (!startingNow)
                {
                    //AppSettings.Downtime = UtilityService.SetCycleDowntime();
                    //await Task.Delay(AppSettings.Downtime);
                    //AppSettings.Downtime = 0;
                    if (AppSettings.DualClient)
                    {
                        if (!_onCycleDowntimeEntryEscHandled)
                        {
                            await TrySendSingleEntryEscIfAnyWindowOpenAsync();
                            _onCycleDowntimeEntryEscHandled = true;
                        }
                        await ManageDualClient();
                    }
                    else
                    {
                        AppSettings.Downtime = UtilityService.SetCycleDowntime();
                        if (!_onCycleDowntimeEntryEscHandled)
                        {
                            await DelayAndTryEntryEscWeightedAsync(AppSettings.Downtime);
                            _onCycleDowntimeEntryEscHandled = true;
                        }
                        else
                            await Task.Delay(AppSettings.Downtime);
                        AppSettings.Downtime = 0;
                    }

                }
                else if (!_onCycleDowntimeEntryEscHandled)
                {
                    await TrySendSingleEntryEscIfAnyWindowOpenAsync();
                    _onCycleDowntimeEntryEscHandled = true;
                }
                if (startingNow || isResetted)
                {
                    result = await _simulateService.SendMacroKey(WindowsInput.Native.VirtualKeyCode.VK_T);
                    await Task.Delay(UtilityService.GenerateRandom(42, 108));
                    bool ahMenuOpen = await PixelProcessService.IsWindowOpen(Enums.ActiveWindow.AHMenu);
                    if (!ahMenuOpen)
                        result = await _simulateService.SendMacroKey(WindowsInput.Native.VirtualKeyCode.VK_Y);
                    startingNow = false;

                }
                if (TSMWindow.ChatWindow != null)
                {

                    if (exitCounter > 0 || isResetted)
                    {
                        exitCounter = 0;
                        if (isResetted)
                            isResetted = false;
                        ResetDynamicAhFlowSession();
                    }

                    if (StaticTSMButtons.RunCancelScan != null)
                    {
                        bool ahMenuOpenForY = await PixelProcessService.IsWindowOpen(Enums.ActiveWindow.AHMenu);
                        if (!ahMenuOpenForY)
                            result = await _simulateService.SendMacroKey(WindowsInput.Native.VirtualKeyCode.VK_Y);
                        else
                            result = true;
                        if (result && await PixelProcessService.IsWindowOpen(Enums.ActiveWindow.AHMenu))
                            result = await PixelProcessService.IsClickable(StaticTSMButtons.RunCancelScan ?? throw new NullReferenceException());
                    }

                    if (result)
                    {
                        _onCycleDowntimeEntryEscHandled = false;
                        AppSettings.State = State.WaitingForAHWindow;
                        result = false;
                    }

                }
            }
            catch
            {
                AppSettings.State = State.WindowNotFound;

            }
        }


        public async Task WaitingForAHWindowHandler()
        {
            try
            {
                if (_prepNextWaitingForAhWindowTargetAndInteract)
                {
                    _prepNextWaitingForAhWindowTargetAndInteract = false;
                    await _simulateService.SendMacroKey(AhWindowRetryTargetKey);
                    await Task.Delay(UtilityService.GenerateRandom(42, 108));
                    await _simulateService.SendMacroKey(AhWindowRetryInteractKey);
                    await Task.Delay(UtilityService.GenerateRandom(120, 280));
                }

                if (StaticTSMButtons.RunCancelScan == null || StaticTSMButtons.RunPostScan == null)
                {
                    await Task.Delay(UtilityService.GenerateRandom(380, 651));
                    await ScreenService.CaptureScreen(); //
                    result = await DetectService.DetectAHMainMenu();

                }
                else result = true;

                if (!await PixelProcessService.IsWindowOpen(Enums.ActiveWindow.AHMenu))
                {
                    _prepNextWaitingForAhWindowTargetAndInteract = true;
                    AppSettings.State = State.WindowNotFound;
                    return;

                }

                if (result)
                {
                    _prepNextWaitingForAhWindowTargetAndInteract = false;
                    AppSettings.State = State.AHButtonsFound;
                    result = false;
                }
                else
                    _prepNextWaitingForAhWindowTargetAndInteract = true;
            }
            catch
            {
                await ResetCycle();
            }
        }

        public async Task AHButtonsFoundHandler()
        {
            try
            {
                if (!await PixelProcessService.IsWindowOpen(Enums.ActiveWindow.AHMenu))
                {
                    AppSettings.State = State.WindowNotFound;
                    return;

                }

                if (!AppSettings.DynamicAhFlow)
                {
                    result = await PixelProcessService.IsClickable(StaticTSMButtons.RunCancelScan ?? throw new NullReferenceException());
                    if (result)
                    {
                        result = false;
                        result = await _simulateService.MouseClick(StaticTSMButtons.RunCancelScan ?? throw new NullReferenceException());
                        AppSettings.State = State.RunCancelButtonClicked;
                    }
                    return;
                }

                if (!_dynamicAhFlowLocked)
                {
                    _dynamicAhFlowLocked = true;
                    _dynamicAhFlowVariant = PickDynamicAhFlowVariantWeighted();
                }

                if (_dynamicAhFlowVariant == DynamicAhFlowVariant.ClassicCancelFirst)
                {
                    result = await PixelProcessService.IsClickable(StaticTSMButtons.RunCancelScan ?? throw new NullReferenceException());
                    if (result)
                    {
                        result = false;
                        result = await _simulateService.MouseClick(StaticTSMButtons.RunCancelScan ?? throw new NullReferenceException());
                        AppSettings.State = State.RunCancelButtonClicked;
                    }
                }
                else
                {
                    result = await PixelProcessService.IsClickable(StaticTSMButtons.RunPostScan ?? throw new NullReferenceException());
                    if (result)
                    {
                        result = false;
                        result = await _simulateService.MouseClick(StaticTSMButtons.RunPostScan ?? throw new NullReferenceException());
                        AppSettings.State = State.RunPostButtonClicked;
                    }
                }
            }
            catch
            {
                await ResetCycle();
            }


        }


        public async Task RunCancelButtonClickedHandler()
        {
            try
            {
                //await Task.Delay(300);
                if (!await PixelProcessService.IsWindowOpen(Enums.ActiveWindow.AHMenu))
                {
                    AppSettings.State = State.WindowNotFound;
                    return;

                }
                bool isCanceling;
                if (StaticTSMButtons.ExitButton != null)
                {
                    isCanceling = await PixelProcessService.IsClickable(StaticTSMButtons.ExitButton ?? throw new NullReferenceException());
                    if (!isCanceling)
                    {
                        AppSettings.State = State.WindowNotFound;
                        return;
                    }
                }

                result = await TesseractService.IsTextExist(SearchTerms.PostCancelLoaded, Paths.PostCancelImagePath);
                if (result)
                {
                    AppSettings.State = State.CancelingLoaded;
                    result = false;
                }
            }
            catch
            {
                await ResetCycle();
            }
        }

        public async Task PostingLoaded()
        {
            try
            {
                if (resultTask == null || !resultTask.Result)
                {
                    resultTask = Task.Run(() => TesseractService.IsTextExist(SearchTerms.PostCancelFinished, Paths.PostCancelImagePath));
                }

                if (isWindowOpen == null || isWindowOpen.Result)
                {
                    isWindowOpen = Task.Run(() => PixelProcessService.IsWindowOpen(Enums.ActiveWindow.AHMenu));
                }



                if (isWindowOpen.IsCompleted && !isWindowOpen.Result)
                {
                    AppSettings.State = State.WindowNotFound;
                    return;
                }

                if (resultTask.IsCompleted && resultTask.Result)
                {
                    resultTask = null;
                    isWindowOpen = null;
                    isFailed = null;
                    AppSettings.State = State.PostingDone;
                }
                else
                    await _simulateService.SendHumanizedMacroKey(WindowsInput.Native.VirtualKeyCode.VK_E);
            }
            catch
            {
                await ResetCycle();
            }
        }

        public async Task WaitingForLoadingCancelingHandler()
        {
            try
            {
                //await Task.Delay(300);
                if (!await PixelProcessService.IsWindowOpen(Enums.ActiveWindow.AHMenu))
                {
                    AppSettings.State = State.WindowNotFound;
                    return;

                }
                if (result)
                    result = false;

                if (resultTask == null)
                {
                    resultTask = Task.Run(() => TesseractService.IsTextExist(SearchTerms.PostCancelLoaded, Paths.PostCancelImagePath));
                }

                result = await resultTask;
                if (result)
                {
                    AppSettings.State = State.CancelingLoaded;
                    result = false;
                }


                resultTask = null;
            }
            catch
            {
                await ResetCycle();
            }
        }

        public async Task PostingDone()
        {
            try
            {
                //await Task.Delay(300);
                if (!await PixelProcessService.IsWindowOpen(Enums.ActiveWindow.AHMenu))
                {
                    AppSettings.State = State.WindowNotFound;
                    return;

                }

                if (_dynamicAhForceMailboxPostingDone)
                {
                    _dynamicAhForceMailboxPostingDone = false;
                }
                else if (ShouldInterceptPostingDoneForDynamicAhFlow())
                {
                    if (_dynamicAhFlowVariant == DynamicAhFlowVariant.PostDelayCancelPost
                        || _dynamicAhFlowVariant == DynamicAhFlowVariant.PostDelayThenCancel)
                    {
                        await Task.Delay(Random.Shared.Next(5000, 10001));
                    }

                    _dynamicAhPostFirstExitExpectCancel = true;
                    AppSettings.State = State.WaitingForExitScan;
                    result = false;
                    return;
                }

                if (AppSettings.MailBoxPosition == null)
                    throw new NullReferenceException();

                //await Task.Delay(new Random().Next(50, 600));

                result = await _simulateService.MouseClick((Rectangle)AppSettings.MailBoxPosition);
                if (result)
                {
                    AppSettings.State = State.WaitingForDetectMailBoxButtons;
                    result = false;
                }
            }
            catch
            {
                await ResetCycle();
            }
        }

        public async Task MailBoxButtonFoundHandler()
        {
            try
            {
                //await Task.Delay(500);
                if (!await PixelProcessService.IsWindowOpen(Enums.ActiveWindow.MailBox))
                {
                    AppSettings.State = State.WindowNotFound;
                    return;

                }
                result = false;


                //await PixelProcessService.FindTSMWindow(Enums.ActiveWindow.MailBox);
                result = await PixelProcessService.IsClickable(StaticTSMButtons.OpenAllMailButton ?? throw new NullReferenceException());
                if (result)
                {
                    await _simulateService.MouseClick(StaticTSMButtons.OpenAllMailButton ?? throw new NullReferenceException());
                    //await Task.Delay(1000);
                    result = false;
                    result = await PixelProcessService.IsClickable(StaticTSMButtons.OpenAllMailButton, false);
                    if (result)
                    {
                        _onCycleDowntimeEntryEscHandled = false;
                        AppSettings.State = State.OnCycleDowntime;
                        result = false;
                    }
                }
                else
                {
                    _onCycleDowntimeEntryEscHandled = false;
                    AppSettings.State = State.OnCycleDowntime;
                }


                if (await PixelProcessService.IsClickable(StaticTSMButtons.OpenAllMailButton, false))
                {
                    bool onlyMailBoxOpen = await PixelProcessService.IsWindowOpen(Enums.ActiveWindow.MailBox)
                        && !await PixelProcessService.IsWindowOpen(Enums.ActiveWindow.AHMenu);
                    // Posta sonrası her seferinde kapatma değil: eşik jitter’lı; önce kısa veya daha uzun bekleme.
                    int closeChanceThreshold = UtilityService.GenerateRandom(30, 48);
                    int longPauseThreshold = UtilityService.GenerateRandom(26, 40);
                    if (onlyMailBoxOpen && Random.Shared.Next(100) < closeChanceThreshold)
                    {
                        if (Random.Shared.Next(100) < longPauseThreshold)
                            await Task.Delay(UtilityService.GenerateRandom(1200, 2901));
                        else
                            await Task.Delay(UtilityService.GenerateRandom(280, 1050));

                        await _simulateService.SendHumanizedMacroKey(WindowsInput.Native.VirtualKeyCode.ESCAPE, allowPossibleSecondTap: false);
                    }

                    if (AppSettings.DualClient)
                        await _simulateService.SwitchWindow();
                }

                if (AppSettings.State == State.OnCycleDowntime && AppSettings.GuildBankPosition != null
                    && GuildBankRestockTrigger.ShouldRunGuildBank(NtfySettingsStore.Load()))
                {
                    await Task.Delay(UtilityService.GenerateRandom(320, 880));
                    bool guildClicked = await _simulateService.MouseClickGuildBank((Rectangle)AppSettings.GuildBankPosition);
                    if (guildClicked)
                    {
                        TSMWindow.BankBorder = null;
                        AppSettings.State = State.WaitingForDetectGuildBankWindow;
                    }
                }

                GuildBankRestockTrigger.RollCycleBaselinesAfterDowntimeGuildCheck();
                //amIGoingToCloseMailbox = new Random().Next(0, 2) == 1;
                //{
                //    if (amIGoingToCloseMailbox)

                //}
            }
            catch
            {
                await ResetCycle();
            }
        }

        public async Task WaitingForDetectMailBoxButtonsHandler()
        {
            try
            {
                await Task.Delay(UtilityService.GenerateRandom(820, 1181));

                if (StaticTSMButtons.OpenAllMailButton == null)
                    result = await DetectService.DetectMailBoxMenu();
                else result = true;

                if (!await PixelProcessService.IsWindowOpen(Enums.ActiveWindow.MailBox))
                {
                    AppSettings.State = State.WindowNotFound;
                    return;

                }

                if (result)
                {
                    AppSettings.State = State.MailBoxButtonsFound;
                    result = false;
                }
                else AppSettings.State = State.PostingDone;
            }
            catch
            {
                await ResetCycle();
            }


        }

        public async Task WaitingForDetectGuildBankWindowHandler()
        {
            try
            {
                await Task.Delay(UtilityService.GenerateRandom(820, 1181));

                TSMWindow.BankBorder = null;
                result = await DetectService.DetectGuildBankWindow();

                if (!await PixelProcessService.IsWindowOpen(Enums.ActiveWindow.Bank))
                {
                    AppSettings.State = State.WindowNotFound;
                    return;
                }

                if (StaticTSMButtons.RestockButton != null)
                {
                    if (await PixelProcessService.IsClickable(StaticTSMButtons.RestockButton))
                    {
                        await _simulateService.MouseClick(StaticTSMButtons.RestockButton);
                        await PixelProcessService.IsClickable(StaticTSMButtons.RestockButton, false);
                    }
                }

                await _simulateService.SendHumanizedMacroKey(WindowsInput.Native.VirtualKeyCode.VK_2, allowPossibleSecondTap: false);

                TSMWindow.BankBorder = null;
                _onCycleDowntimeEntryEscHandled = false;
                AppSettings.State = State.OnCycleDowntime;
                result = false;
                GuildBankRestockTrigger.RecordGuildBankFlowCompleted(NtfySettingsStore.Load());
            }
            catch
            {
                await ResetCycle();
            }
        }

        public async Task WaitingForExitScanHandler()
        {
            try
            {
                //await Task.Delay(300);
                if (!await PixelProcessService.IsWindowOpen(Enums.ActiveWindow.AHMenu))
                {
                    AppSettings.State = State.WindowNotFound;
                    return;

                }
                if (StaticTSMButtons.ExitButton == null)
                    result = await DetectService.DetectPostCancelButtons();
                else result = true;
                if (result)
                {
                    exitCounter++;
                    await _simulateService.MouseClick(StaticTSMButtons.ExitButton ?? throw new NullReferenceException());
                    AppSettings.State = State.ExitButtonClicked;
                    result = false;
                }

            }
            catch
            {
                await ResetCycle();
            }
        }

        public async Task ExitButtonClickedHandler()
        {
            try
            {
                //await Task.Delay(100);
                if (!await PixelProcessService.IsWindowOpen(Enums.ActiveWindow.AHMenu))
                {
                    AppSettings.State = State.WindowNotFound;
                    return;

                }

                if (!AppSettings.DynamicAhFlow
                    || _dynamicAhFlowVariant == DynamicAhFlowVariant.None
                    || _dynamicAhFlowVariant == DynamicAhFlowVariant.ClassicCancelFirst)
                {
                    if (exitCounter < 2)
                    {
                        await _simulateService.MouseClick(StaticTSMButtons.RunPostScan ?? throw new NullReferenceException());
                        AppSettings.State = State.RunPostButtonClicked;
                    }
                    return;
                }

                if (exitCounter == 1 && _dynamicAhPostFirstExitExpectCancel)
                {
                    await _simulateService.MouseClick(StaticTSMButtons.RunCancelScan ?? throw new NullReferenceException());
                    AppSettings.State = State.RunCancelButtonClicked;
                    _dynamicAhPostFirstExitExpectCancel = false;
                    return;
                }

                if (exitCounter == 2)
                {
                    if (_dynamicAhFlowVariant == DynamicAhFlowVariant.PostDelayCancelPost)
                    {
                        await _simulateService.MouseClick(StaticTSMButtons.RunPostScan ?? throw new NullReferenceException());
                        AppSettings.State = State.RunPostButtonClicked;
                        _dynamicAhV2MailboxAfterSecondPost = true;
                        return;
                    }

                    if (_dynamicAhFlowVariant == DynamicAhFlowVariant.PostThenCancel
                        || _dynamicAhFlowVariant == DynamicAhFlowVariant.PostDelayThenCancel)
                    {
                        _dynamicAhForceMailboxPostingDone = true;
                        AppSettings.State = State.PostingDone;
                        return;
                    }
                }

                if (exitCounter < 2)
                {
                    await _simulateService.MouseClick(StaticTSMButtons.RunPostScan ?? throw new NullReferenceException());
                    AppSettings.State = State.RunPostButtonClicked;
                }
            }
            catch
            {
                await ResetCycle();
            }
        }

        public async Task RunPostButtonClickedHandler()
        {
            try
            {
                if (!await PixelProcessService.IsWindowOpen(Enums.ActiveWindow.AHMenu))
                {
                    AppSettings.State = State.WindowNotFound;
                }
                result = await TesseractService.IsTextExist(SearchTerms.PostCancelLoaded, Paths.PostCancelImagePath);
                if (result)
                {
                    AppSettings.State = State.PostingLoaded;
                    result = false;
                }
            }
            catch
            {
                await ResetCycle();
            }
        }

        public async Task CancelingDoneHandler()
        {
            try
            {
                if (!await PixelProcessService.IsWindowOpen(Enums.ActiveWindow.AHMenu))
                {
                    AppSettings.State = State.WindowNotFound;
                    return;
                }
                AppSettings.State = State.WaitingForExitScan;
                result = false;
            }
            catch
            {
                await ResetCycle();
            }
        }

        public async Task CancelingLoadedHandler()
        {
            try
            {
                cancelingLoadedEnteredUtc ??= DateTime.UtcNow;
                if (firstCancelingLoadedDuration.HasValue
                    && DateTime.UtcNow - cancelingLoadedEnteredUtc.Value > firstCancelingLoadedDuration.Value + cancelingLoadedExtraThreshold)
                {
                    AppSettings.State = State.CancelingDone;
                    cancelingLoadedEnteredUtc = null;
                    return;
                }

                if (resultTask == null || !resultTask.Result)
                {
                    resultTask = Task.Run(() => TesseractService.IsTextExist(SearchTerms.PostCancelFinished, Paths.PostCancelImagePath));
                }

                if (isFailed == null || !isFailed.Result)
                {
                    isFailed = Task.Run(() => TesseractService.IsTextExist(SearchTerms.ChatWords, Paths.ChatWindowImagePath));
                }

                if (isWindowOpen == null || isWindowOpen.Result)
                {
                    isWindowOpen = Task.Run(() => PixelProcessService.IsWindowOpen(Enums.ActiveWindow.AHMenu));
                }



                if (isFailed.Result && isFailed.IsCompleted)
                {
                    AppSettings.State = State.CancelingDone;
                    TrySetFirstCancelingLoadedDuration();
                    cancelingLoadedEnteredUtc = null;
                    return;
                }

                if (isWindowOpen.IsCompleted && !isWindowOpen.Result)
                {
                    AppSettings.State = State.WindowNotFound;
                    cancelingLoadedEnteredUtc = null;
                    return;
                }
                if (resultTask.IsCompleted && resultTask.Result)
                {
                    result = true;
                    resultTask = null;
                    isWindowOpen = null;
                    isFailed = null;
                    AppSettings.State = State.CancelingDone;
                    TrySetFirstCancelingLoadedDuration();
                    cancelingLoadedEnteredUtc = null;
                }
                else
                    await _simulateService.SendHumanizedMacroKey(WindowsInput.Native.VirtualKeyCode.VK_E);

            }
            catch
            {
                await ResetCycle();
            }
        }

        public async Task WindowNotFoundHandler()
        {
            await ResetCycle();
        }

        /// <summary>
        /// <see cref="State.OnCycleDowntime"/> dışında ~120 sn aşıldığında: AH menü açıksa ESC, ardından <see cref="ResetCycle"/> → <see cref="State.OnCycleDowntime"/>.
        /// </summary>
        public async Task RecoverFromStallAsync(State stalledInState)
        {
            if (stalledInState != State.OnCycleDowntime
                && await PixelProcessService.IsWindowOpen(Enums.ActiveWindow.AHMenu))
            {
                await _simulateService.SendHumanizedMacroKey(VirtualKeyCode.ESCAPE, allowPossibleSecondTap: false);
                await Task.Delay(UtilityService.GenerateRandom(120, 280));
            }

            await ResetCycle();
        }

        private async Task ResetCycle()
        {
            GuildBankRestockTrigger.ClearCurrentCycleFirstPost();
            _onCycleDowntimeEntryEscHandled = false;
            AppSettings.State = State.OnCycleDowntime;
            isResetted = true;
            result = false;
            resultTask = null;
            isWindowOpen = null;
            isFailed = null;
            isClickable = null;
            cancelingLoadedEnteredUtc = null;
            ResetDynamicAhFlowSession();
            //if (TSMWindow.AHBorder != null && await PixelProcessService.IsWindowOpen(Enums.ActiveWindow.AHMenu) || (TSMWindow.MailBoxBorder != null && await PixelProcessService.IsWindowOpen(Enums.ActiveWindow.MailBox)))
            //{
            //    await _simulateService.SendMacroKey(WindowsInput.Native.VirtualKeyCode.ESCAPE);
            //}
        }

        private void TrySetFirstCancelingLoadedDuration()
        {
            if (firstCancelingLoadedDuration.HasValue || !cancelingLoadedEnteredUtc.HasValue)
                return;
            firstCancelingLoadedDuration = DateTime.UtcNow - cancelingLoadedEnteredUtc.Value;
        }

        /// <summary>
        /// Downtime boyunca tek seferlik ESC zamanı: çoğunlukla ilk 1-2 sn, düşük olasılıkla daha geç bir nokta.
        /// </summary>
        async Task DelayAndTryEntryEscWeightedAsync(int totalDowntimeMs)
        {
            int total = Math.Max(1000, totalDowntimeMs);
            int roll = Random.Shared.Next(100);
            int escAtMs;
            if (roll < 72)
            {
                // Ağırlık: girişe yakın
                escAtMs = Math.Min(total, UtilityService.GenerateRandom(1000, 2201));
            }
            else if (roll < 92)
            {
                // Orta bölge
                int lo = Math.Min(total, Math.Max(2200, (int)(total * 0.35)));
                int hi = Math.Min(total, Math.Max(lo + 1, (int)(total * 0.60)));
                escAtMs = UtilityService.GenerateRandom(lo, hi + 1);
            }
            else
            {
                // Düşük olasılık: geç zaman (örn. 20s için ~14s civarı)
                int lo = Math.Min(total, Math.Max(2200, (int)(total * 0.65)));
                int hi = Math.Min(total, Math.Max(lo + 1, (int)(total * 0.90)));
                escAtMs = UtilityService.GenerateRandom(lo, hi + 1);
            }

            int beforeEscMs = Math.Clamp(escAtMs, 0, total);
            int afterEscMs = Math.Max(0, total - beforeEscMs);
            if (beforeEscMs > 0)
                await Task.Delay(beforeEscMs);
            await TrySendSingleEntryEscIfAnyWindowOpenAsync();
            if (afterEscMs > 0)
                await Task.Delay(afterEscMs);
        }

        async Task TrySendSingleEntryEscIfAnyWindowOpenAsync()
        {
            bool anyWindowOpen =
                await PixelProcessService.IsWindowOpen(Enums.ActiveWindow.AHMenu)
                || await PixelProcessService.IsWindowOpen(Enums.ActiveWindow.MailBox)
                || await PixelProcessService.IsWindowOpen(Enums.ActiveWindow.Bank);
            if (!anyWindowOpen)
                return;
            await _simulateService.SendHumanizedMacroKey(VirtualKeyCode.ESCAPE, allowPossibleSecondTap: false);
            await Task.Delay(UtilityService.GenerateRandom(120, 280));
        }

        private void LoadCancelingLoadedThresholdSettings()
        {
            var settings = NtfySettingsStore.Load();
            cancelingLoadedExtraThreshold = TimeSpan.FromSeconds(Math.Max(0, settings.CancelingLoadedExtraThresholdSeconds));
        }

        void ResetDynamicAhFlowSession()
        {
            _dynamicAhFlowVariant = DynamicAhFlowVariant.None;
            _dynamicAhFlowLocked = false;
            _dynamicAhPostFirstExitExpectCancel = false;
            _dynamicAhV2MailboxAfterSecondPost = false;
            _dynamicAhForceMailboxPostingDone = false;
        }

        /// <summary>Taban V1=22, V2=38, V3=28, V4=12 yüzde + küçük jitter; normalize edilip tek seçim.</summary>
        static DynamicAhFlowVariant PickDynamicAhFlowVariantWeighted()
        {
            double w1 = 22 + Random.Shared.Next(-3, 4);
            double w2 = 38 + Random.Shared.Next(-3, 4);
            double w3 = 28 + Random.Shared.Next(-3, 4);
            double w4 = 12 + Random.Shared.Next(-3, 4);
            w1 = Math.Max(1, w1);
            w2 = Math.Max(1, w2);
            w3 = Math.Max(1, w3);
            w4 = Math.Max(1, w4);
            double sum = w1 + w2 + w3 + w4;
            double r = Random.Shared.NextDouble() * sum;
            if (r < w1)
                return DynamicAhFlowVariant.ClassicCancelFirst;
            r -= w1;
            if (r < w2)
                return DynamicAhFlowVariant.PostDelayCancelPost;
            r -= w2;
            if (r < w3)
                return DynamicAhFlowVariant.PostThenCancel;
            return DynamicAhFlowVariant.PostDelayThenCancel;
        }

        bool ShouldInterceptPostingDoneForDynamicAhFlow()
        {
            if (!AppSettings.DynamicAhFlow)
                return false;
            if (_dynamicAhV2MailboxAfterSecondPost)
                return false;
            return _dynamicAhFlowVariant == DynamicAhFlowVariant.PostDelayCancelPost
                || _dynamicAhFlowVariant == DynamicAhFlowVariant.PostThenCancel
                || _dynamicAhFlowVariant == DynamicAhFlowVariant.PostDelayThenCancel;
        }

        private async Task ManageDualClient()
        {
            await _simulateService.SwitchWindow();
        }



    }
}
