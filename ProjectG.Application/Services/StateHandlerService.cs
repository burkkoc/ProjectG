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
        public StateHandlerService(SimulateService simulateService)
        {
            _simulateService = simulateService;
        }
        public async Task OnCycleDowntimeHandler()
        {
            try
            {

                await Task.Delay(1000);
                await _simulateService.MouseMove(50, ButtonContainer.AH);
                if (TSMWindow.ChatWindow == null)
                {
                    TSMWindow.ChatWindow = await PixelProcessService.FindChatWindow(Enums.ActiveWindow.Chat);
                }
                if (!startingNow)
                {
                    AppSettings.Downtime = UtilityService.SetCycleDowntime();
                    await Task.Delay(AppSettings.Downtime);
                    AppSettings.Downtime = 0;
                }
                if (startingNow || isResetted)
                {
                    result = await _simulateService.SendMacroKey(WindowsInput.Native.VirtualKeyCode.VK_T);
                    await Task.Delay(new Random().Next(50, 100));
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
                    }

                    if (StaticTSMButtons.RunCancelScan != null)
                    {
                        result = await _simulateService.SendMacroKey(WindowsInput.Native.VirtualKeyCode.VK_Y);
                        //await Task.Delay(100);
                        if (result && await PixelProcessService.IsWindowOpen(Enums.ActiveWindow.AHMenu))
                            result = await PixelProcessService.IsClickable(StaticTSMButtons.RunCancelScan ?? throw new NullReferenceException());
                    }

                    if (result)
                    {
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


                if (StaticTSMButtons.RunCancelScan == null || StaticTSMButtons.RunPostScan == null)
                {
                    await Task.Delay(500);
                    await ScreenService.CaptureScreen(); //
                    result = await DetectService.DetectAHMainMenu();

                }
                else result = true;

                if (!await PixelProcessService.IsWindowOpen(Enums.ActiveWindow.AHMenu))
                {
                    AppSettings.State = State.WindowNotFound;
                    return;

                }

                if (result)
                {
                    AppSettings.State = State.AHButtonsFound;
                    result = false;
                }
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
                result = await PixelProcessService.IsClickable(StaticTSMButtons.RunCancelScan ?? throw new NullReferenceException());
                if (result)
                {
                    result = false;
                    result = await _simulateService.MouseClick(StaticTSMButtons.RunCancelScan ?? throw new NullReferenceException());
                    AppSettings.State = State.RunCancelButtonClicked;
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
                else await _simulateService.SendMacroKey(WindowsInput.Native.VirtualKeyCode.VK_E);
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
                        AppSettings.State = State.OnCycleDowntime;
                        result = false;
                    }
                }
                else AppSettings.State = State.OnCycleDowntime;


                if (await PixelProcessService.IsClickable(StaticTSMButtons.OpenAllMailButton, false))
                    await _simulateService.SendMacroKey(WindowsInput.Native.VirtualKeyCode.ESCAPE);
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
                await Task.Delay(1000);

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
                if (exitCounter < 2)
                {
                    //await Task.Delay(UtilityService.GenerateRandom(500, 1000));
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
                    return;
                }

                if (isWindowOpen.IsCompleted && !isWindowOpen.Result)
                {
                    AppSettings.State = State.WindowNotFound;
                    return;
                }
                if (resultTask.IsCompleted && resultTask.Result)
                {
                    result = true;
                    resultTask = null;
                    isWindowOpen = null;
                    isFailed = null;
                    AppSettings.State = State.CancelingDone;
                }
                else await _simulateService.SendMacroKey(WindowsInput.Native.VirtualKeyCode.VK_E);

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
        private async Task ResetCycle()
        {
            AppSettings.State = State.OnCycleDowntime;
            isResetted = true;
            result = false;
            resultTask = null;
            isWindowOpen = null;
            isFailed = null;
            isClickable = null;
            if (TSMWindow.AHBorder != null && await PixelProcessService.IsWindowOpen(Enums.ActiveWindow.AHMenu) || (TSMWindow.MailBoxBorder != null && await PixelProcessService.IsWindowOpen(Enums.ActiveWindow.MailBox)))
            {
                await _simulateService.SendMacroKey(WindowsInput.Native.VirtualKeyCode.ESCAPE);
            }
        }



    }
}
