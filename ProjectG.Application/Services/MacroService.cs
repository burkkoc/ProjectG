
using ProjectG.DomainLayer.Entities.Concrete;
using ProjectG.DomainLayer.Entities.Enums;
using ProjectG.InfrastructureLayer.Services;
using System.Drawing;
using System.Reflection;
using ProjectG.DomainLayer.Entities.Concrete.Statics;

namespace ProjectG.ApplicationLayer.Services
{
    public class MacroService
    {
        
        SimulateService _simulateService = new();
        StateHandlerService _stateHandlerService;
        public MacroService()
        {
            _stateHandlerService = new(_simulateService);
        }

        
        public async Task Run()
        {
            AppSettings.State = State.OnCycleDowntime;
            await Task.Run(async () =>
            {
                while (AppSettings.Working)
                {
                    switch (AppSettings.State)
                    {
                        case State.OnCycleDowntime:
                            await _stateHandlerService.OnCycleDowntimeHandler();
                            break;
                        case State.WaitingForAHWindow:
                            await _stateHandlerService.WaitingForAHWindowHandler();
                            break;
                        case State.AHButtonsFound:
                            await _stateHandlerService.AHButtonsFoundHandler();
                            break;
                        case State.PostingLoaded:
                            await _stateHandlerService.PostingLoaded();
                            break;
                        case State.WaitingForLoadingCanceling:
                            await _stateHandlerService.WaitingForLoadingCancelingHandler();
                            break;
                        case State.PostingDone:
                            await _stateHandlerService.PostingDone();
                            break;
                        case State.WaitingForExitScan:
                            await _stateHandlerService.WaitingForExitScanHandler();
                            break;
                        case State.WaitingForMailBoxWindow:
                            break;
                        case State.WaitingForDetectMailBoxButtons:
                            await _stateHandlerService.WaitingForDetectMailBoxButtonsHandler();
                            break;
                        case State.MailBoxButtonsFound:
                            await _stateHandlerService.MailBoxButtonFoundHandler();
                            break;
                        case State.OpenAllMailButtonClicked:
                            break;
                        case State.AllCancelledButtonClicked:
                            break;
                        case State.WaitingForGettingItemsFromMailBox:
                            break;
                        case State.ExitButtonClicked:
                            await _stateHandlerService.ExitButtonClickedHandler();
                            break;
                        case State.RunPostButtonClicked:
                            await _stateHandlerService.RunPostButtonClickedHandler();
                            break;
                        case State.CancelingDone:
                            await _stateHandlerService.CancelingDoneHandler();
                            break;
                        case State.RunCancelButtonClicked:
                            await _stateHandlerService.RunCancelButtonClickedHandler();
                            break;
                        case State.CancelingLoaded:
                            await _stateHandlerService.CancelingLoadedHandler();
                            break;
                        case State.TargetKeyClicked:
                            break;
                        case State.InteractKeyClicked:
                            break;
                        case State.CloseWindowKeyClicked:
                            break;
                        case State.Stopped:
                            AppSettings.Working = false;
                            break;
                        case State.WindowNotFound:
                            await _stateHandlerService.WindowNotFoundHandler();
                            break;
                        default:
                            break;
                    }
                }
            });


        }

        //public async Task Test()
        //{
        //    await Task.Run(async () =>
        //    {
        //        int exitCounter = 0;
        //        bool result = false;
        //        bool status;
        //        Task<bool>? task1 = null;
        //        bool resTask = false;

        //        while (true)
        //        {
        //            switch (AppSettings.State)
        //            {
        //                case State.OnCycleDowntime:
        //                     await _stateHandlerService.OnCycleDowntimeHandler();
        //                    //if (exitCounter > 1)
        //                    //{
        //                    //    int rnd = new Random().Next(150000, 180000);
        //                    //    await Task.Delay(rnd);
        //                    //    exitCounter = 0;
        //                    //}
        //                    //else
        //                    //{

        //                    //    await Task.Delay(1000);
        //                    //    result = await _simulateService.SendMacroKey(WindowsInput.Native.VirtualKeyCode.VK_T);
        //                    //    AppSettings.State = State.TargetKeyClicked;
        //                    //    result = await _simulateService.SendMacroKey(WindowsInput.Native.VirtualKeyCode.VK_Y);
        //                    //    AppSettings.State = State.InteractKeyClicked;
        //                    //}
        //                    //AppSettings.State = State.WaitingForAHWindow;
        //                    break;
        //                case State.WaitingForAHWindow:
        //                    await _stateHandlerService.WaitingForAHWindowHandler();
        //                    //await ScreenService.CaptureScreen();
        //                    //if (_runCancelScan == null || _runPostScan == null)
        //                    //    result = await DetectAHMainMenu();
        //                    //else result = true;
        //                    //AppSettings.State = State.AHWindowFound;
        //                    //AppSettings.State = State.AHButtonsFound;
        //                    break;
        //                case State.AHWindowFound:
        //                    break;
        //                case State.WaitingForDetectAHButtons:
        //                    break;
        //                case State.AHButtonsFound:
        //                    await _stateHandlerService.AHButtonsFoundHandler();
        //                    //await Task.Delay(UtilityService.GenerateRandom(700, 10000));
        //                    //result = await _simulateService.MouseClick(_runCancelScan ?? throw new NullReferenceException());
        //                    //AppSettings.State = State.RunCancelButtonClicked;
        //                    break;
        //                //case State.WaitingForLoadingPosting:
        //                //    if(task1 == null)
        //                //    {
        //                //        task1 = Task.Run(() => TesseractService.IsTextExist(SearchTerms.PostCancelLoaded, Paths.PostCancelImagePath));
        //                //    }
        //                //    resTask = await task1;
        //                //    if (resTask)
        //                //        AppSettings.State = State.PostingLoaded;
        //                //    break;
        //                case State.PostingLoaded:
        //                    await _stateHandlerService.PostingLoaded();
        //                    //if (task1 == null)
        //                    //{
        //                    //    task1 = Task.Run(() => TesseractService.IsTextExist(SearchTerms.PostCancelFinished, Paths.PostCancelImagePath));
        //                    //}
        //                    //while (!task1.IsCompleted)
        //                    //{
        //                    //    await _simulateService.SendMacroKey(WindowsInput.Native.VirtualKeyCode.VK_E);
        //                    //}
        //                    //resTask = await task1;
        //                    //if (resTask)
        //                    //    AppSettings.State = State.PostingDone;
        //                    //task1 = null;


        //                    //await _simulateService.SendMacroKey(WindowsInput.Native.VirtualKeyCode.VK_E);
        //                    //status = await TesseractService.IsTextExist(SearchTerms.PostCancelFinished, Paths.PostCancelImagePath);
        //                    //if (status)
        //                    //    AppSettings.State = State.PostingDone;
        //                    break;
        //                case State.WaitingForLoadingCanceling:
        //                    await _stateHandlerService.WaitingForLoadingCanceling();
        //                    //if (resTask)
        //                    //    resTask = false;
        //                    //if (task1 == null)
        //                    //{
        //                    //    task1 = Task.Run(() => TesseractService.IsTextExist(SearchTerms.PostCancelLoaded, Paths.PostCancelImagePath));
        //                    //}
        //                    //resTask = await task1;
        //                    //if (resTask)
        //                    //    AppSettings.State = State.CancelingLoaded;
        //                    break;
        //                case State.PostingDone:
        //                    //AppSettings.State = State.WaitingForExitScan;
        //                    //AppSettings.State = State.WaitingForDetectMailBoxButtons;
        //                    await _stateHandlerService.PostingDone();
        //                    break;
        //                case State.WaitingForDetectCancel:
        //                    break;
        //                case State.WaitingforDetectSkip:
        //                    break;
        //                case State.WaitingForExitScan:
        //                    //if (_exitButton == null)
        //                    //    status = await DetectPostCancelButtons();
        //                    //else status = true;
        //                    //if (status)
        //                    //{
        //                    //    exitCounter++;
        //                    //    AppSettings.State = State.ExitButtonClicked;
        //                    //    await _simulateService.MouseClick(_exitButton ?? throw new NullReferenceException());
        //                    //}
        //                    await _stateHandlerService.WaitingForExitScan();

        //                    break;
        //                case State.WaitingForMailBoxWindow:
        //                    break;
        //                case State.WaitingForDetectMailBoxButtons:
        //                    //await Task.Delay(1000);
        //                    //await _simulateService.SendMacroKey(WindowsInput.Native.VirtualKeyCode.ESCAPE);
        //                    //await Task.Delay(1000);
        //                    //await _simulateService.MouseClick((Rectangle)AppSettings.MailBoxPosition);
        //                    await _stateHandlerService.WaitingForMailBoxButtons();
        //                    break;
        //                case State.MailBoxButtonsFound:
        //                    break;
        //                case State.OpenAllMailButtonClicked:
        //                    break;
        //                case State.AllCancelledButtonClicked:
        //                    break;
        //                case State.WaitingForGettingItemsFromMailBox:
        //                    break;
        //                case State.ExitButtonClicked:
        //                    await _stateHandlerService.ExitButtonClicked();
        //                    //if (exitCounter < 2)
        //                    //{
        //                    //    await Task.Delay(UtilityService.GenerateRandom(700, 7000));
        //                    //    await _simulateService.MouseClick(_runPostScan ?? throw new NullReferenceException());
        //                    //    AppSettings.State = State.RunPostButtonClicked;
        //                    //}
        //                    //else AppSettings.State = State.OnCycleDowntime;
        //                    break;
        //                case State.RunPostButtonClicked:
        //                    status = false;
        //                    status = await TesseractService.IsTextExist(SearchTerms.PostCancelLoaded, Paths.PostCancelImagePath);
        //                    if (status) AppSettings.State = State.PostingLoaded;
        //                    break;
        //                case State.CancelingDone:
        //                    status = await TesseractService.IsTextExist(SearchTerms.ExitScanWords, Paths.PostCancelImagePath);
        //                    if (status) AppSettings.State = State.WaitingForExitScan;
        //                    //await _simulateService.MouseClick(_exitButton ?? throw new NullReferenceException());
        //                    break;
        //                case State.RunCancelButtonClicked:
        //                    status = false;
        //                    status = await TesseractService.IsTextExist(SearchTerms.PostCancelLoaded, Paths.PostCancelImagePath);
        //                    if (status) AppSettings.State = State.CancelingLoaded;
        //                    break;
        //                case State.CancelingLoaded:
        //                    //status = false;
        //                    resTask = false;
        //                    if (task1 == null)
        //                    {
        //                        task1 = Task.Run(() => TesseractService.IsTextExist(SearchTerms.PostCancelFinished, Paths.PostCancelImagePath));
        //                    }
        //                    while (!task1.IsCompleted)
        //                    {
        //                        await _simulateService.SendMacroKey(WindowsInput.Native.VirtualKeyCode.VK_E);
        //                    }
        //                    resTask = await task1;
        //                    if (resTask)
        //                        AppSettings.State = State.CancelingDone;
        //                    task1 = null;


        //                    //status = TesseractService.IsTextExist(SearchTerms.PostCancelFinished, Paths.PostCancelImagePath);
        //                    //if(status)
        //                    //AppSettings.State = State.CancelingDone;
        //                    //result = sendKeyTSM
        //                    //res = isCancelingDone()
        //                    //if(res) AppSettings.State = State.CancelingDone;
        //                    break;
        //                case State.TargetKeyClicked:
        //                    break;
        //                case State.InteractKeyClicked:
        //                    break;
        //                case State.CloseWindowKeyClicked:
        //                    break;
        //                default:
        //                    break;
        //            }
        //            status = false;
        //            //if (!result)
        //            //    break;


        //            //if (result) await _simulateService.MouseClick(_runCancelScan ?? throw new NullReferenceException());
        //        }
        //    });


        //}



        //public async Task CreateMethodList(string methodName, ButtonName buttonName)
        //{

        //    //await Execute();
        //}
        //public async Task<bool> DetectMailBoxNodes()
        //{
        //    return await Task.Run(async () =>
        //    {
        //        if (TSMWindow.MailBoxBorder == null)
        //        {
        //            //Paths.GeneralImagePath = UtilityService.GetDirectory(ActiveWindow.General);
        //            //var ss = ScreenService.CaptureScreen(UserInput.ScreenResolutionX, UserInput.ScreenResolutionY, Paths.GeneralImagePath);
        //            TSMWindow.MailBoxBorder = await PixelProcessService.FindTSMWindow(Enums.ActiveWindow.MailBox);
        //        }

        //        //screenshotOfMailBox = ScreenService.CapturePartialScreen((Rectangle)TSMWindow.MailBoxBorder, Paths.MailBoxImagePath);



        //        List<string> responses = new List<string>();

        //        string[] searchingWords = ["Retrying", "failed"];
        //        if (screenshotOfMailBox == null) throw new NullReferenceException();
        //        //var res = TesseractService.IsTextExist(PixelProcessService.GetScreenshot(UserInput.ScreenResolutionX, UserInput.ScreenResolutionY), searchingWords);
        //        var result = TesseractService.FindTextLocation(SearchTerms.MailBoxButtonWords, Paths.MailBoxImagePath);
        //        foreach (var keywordForButtons in SearchTerms.MailBoxButtonWords)
        //        {
        //            if (!result.Any(r => r.Key == keywordForButtons))
        //            {
        //                responses.Add($"'{keywordForButtons}' {Response.ButtonCouldNotFound}");
        //            }
        //        }
        //        foreach (var keyword in result)
        //        {
        //            switch (keyword.Key)
        //            {

        //                case "Cancelled":
        //                    TSMButton cancelledButton = TSMButtonService.CreateTSMButton(ButtonName.AllCancelled, keyword.Value, (Rectangle)TSMWindow.MailBoxBorder);
        //                    _cancelledButton = cancelledButton;
        //                    TSMButton.TSMButtons.Add(_cancelledButton);
        //                    responses.Add(Response.MailBoxCancelledButtonFound);
        //                    break;
        //                case "Open":
        //                    TSMButton openAllMailButton = TSMButtonService.CreateTSMButton(ButtonName.OpenAllMails, keyword.Value, (Rectangle)TSMWindow.MailBoxBorder);
        //                    _openAllMailButton = openAllMailButton;
        //                    TSMButton.TSMButtons.Add(openAllMailButton);

        //                    responses.Add(Response.MailBoxGetAllButtonFound);
        //                    break;
        //                case "Retrying":
        //                    break;
        //                default:
        //                    break;
        //            }
        //        }


        //        return true;

        //    });

        //}

        //public async Task<bool> DetectPostCancelButtons()
        //{
        //    return await Task.Run(async () =>
        //    {
        //        var endTime = DateTime.UtcNow.Add(TimeSpan.FromSeconds(5)); // 5 saniye sonra zaman
        //        TSMWindow.AHBorder = await PixelProcessService.FindTSMWindow(Enums.ActiveWindow.PostCancel);

        //        List<string> responses = new List<string>();

        //        while (DateTime.UtcNow < endTime)
        //        {
        //            await ScreenService.CapturePartialScreen((Rectangle)TSMWindow.AHBorder, Enums.ActiveWindow.PostCancel);

        //            var result = TesseractService.FindTextLocation(SearchTerms.ExitScanWords, Paths.PostCancelImagePath);

        //            foreach (var keyword in result)
        //            {
        //                switch (keyword.Key)
        //                {
        //                    case "Exit" or "exit":
        //                        if (_exitButton == null)
        //                        {
        //                            TSMButton exitButton = TSMButtonService.CreateTSMButton(ButtonName.ExitScan, keyword.Value, (Rectangle)TSMWindow.AHBorder);
        //                            _exitButton = exitButton;
        //                            TSMButton.TSMButtons.Add(_exitButton);
        //                            responses.Add("Exit");
        //                        }
        //                        break;
        //                    default:
        //                        break;
        //                }
        //            }

        //            if (responses.Any(r => r.Contains("Exit"))) //exit bulundu
        //                return true;

        //            await Task.Delay(1000); // 1 saniye bekle
        //        }

        //        return false; // 5 saniye dolmadan doğru sonuç bulunamadı
        //    });
        //}
        //public async Task<bool> DetectAHMainMenu()
        //{
        //    return await Task.Run(async () =>
        //    {
        //        var timeout = TimeSpan.FromSeconds(20); // 20 saniyelik zaman aşımı süresi
        //        var endTime = DateTime.UtcNow.Add(timeout); // 20 saniye sonra zaman
        //        var endTime2 = DateTime.UtcNow.Add(TimeSpan.FromSeconds(5)); // 20 saniye sonra zaman

        //        while (DateTime.UtcNow < endTime)
        //        {
        //            while (DateTime.UtcNow < endTime2)
        //            {
        //                if (TSMWindow.AHBorder == null)
        //                {
        //                    await Task.Delay(500);
        //                    TSMWindow.AHBorder = await PixelProcessService.FindTSMWindow(Enums.ActiveWindow.AHMenu);
        //                }
        //                else break;
        //            }

        //            await ScreenService.CapturePartialScreen((Rectangle)TSMWindow.AHBorder, Enums.ActiveWindow.AHMenu);


        //            List<string> responses = new List<string>();


        //            var result = TesseractService.FindTextLocation(SearchTerms.AHMenuButtonsWords, Paths.AHMenuImagePath);

        //            foreach (var keyword in result)
        //            {
        //                switch (keyword.Key)
        //                {
        //                    case "Run":
        //                        if (_runPostScan == null)
        //                        {
        //                            TSMButton runPostScanButton = TSMButtonService.CreateTSMButton(ButtonName.RunPostScan, keyword.Value, (Rectangle)TSMWindow.AHBorder);
        //                            _runPostScan = runPostScanButton;
        //                            TSMButton.TSMButtons.Add(_runPostScan);
        //                            responses.Add(Response.AHMainMenuPostButtonFound);
        //                        }
        //                        break;
        //                    case "Cancel":
        //                        if (_runCancelScan == null)
        //                        {
        //                            TSMButton runCancelScan = TSMButtonService.CreateTSMButton(ButtonName.RunCancelScan, keyword.Value, (Rectangle)TSMWindow.AHBorder);
        //                            _runCancelScan = runCancelScan;
        //                            TSMButton.TSMButtons.Add(_runCancelScan);
        //                            responses.Add(Response.AHMainMenuCancelButtonFound);
        //                        }
        //                        break;
        //                    default:
        //                        break;
        //                }
        //            }
        //            if (responses.Any(r => r.Contains("Run Post Scan")) && responses.Any(r => r.Contains("Run Cancel Scan")) && responses.Count == 2)
        //                return true;

        //            responses.Clear();


        //            await Task.Delay(500); // 0.5 saniye bekle

        //        }

        //        return false; // 20 saniye dolmadan doğru sonuç bulunamadı
        //    });

        //}

        //public static List<MethodInfo> methods = new List<MethodInfo>();
        //public List<TSMButton> tsmButtonsForMethods = new List<TSMButton>();
        //public bool CreateLoopList(string methodName, string buttonName)
        //{

        //    Type type = typeof(SimulateService);

        //    var selectedAction = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
        //    if (selectedAction == null)
        //        throw new Exception();
        //    methods.Add(selectedAction);

        //    switch (buttonName)
        //    {
        //        case "Open All Mail":
        //            tsmButtonsForMethods.Add(_openAllMailButton);
        //            break;
        //        case "All Cancelled":
        //            tsmButtonsForMethods.Add(_cancelledButton);
        //            break;
        //        case "Run Post Scan":
        //            tsmButtonsForMethods.Add(_runPostScan);
        //            break;
        //        case "Run Cancel Scan":
        //            tsmButtonsForMethods.Add(_runCancelScan);
        //            break;
        //        //case "Close Window":
        //        //    tsmButtonsForMethods.Add(_closeWindow);
        //        //    break;
        //        default:
        //            return false;

        //    }

        //    return true;

        //}
        //CancellationTokenSource cts = new CancellationTokenSource();

        //public async Task Execute()
        //{
        //    var token = cts.Token;

        //    try
        //    {

        //        while (true)
        //        {

        //            token.ThrowIfCancellationRequested();

        //            int iteration = 0;
        //            foreach (MethodInfo method in methods)
        //            {
        //                if (token.IsCancellationRequested)
        //                    token.ThrowIfCancellationRequested();
        //                try
        //                {

        //                    var task = (Task)method.Invoke(_simulateService, new[] { tsmButtonsForMethods[iteration] });
        //                    //logic buraya gelecek
        //                    await task.ConfigureAwait(false);
        //                    iteration++;
        //                }
        //                catch (Exception ex)
        //                {
        //                    throw new Exception(ex.ToString());
        //                }
        //            }

        //        }
        //    }
        //    catch (OperationCanceledException)
        //    {

        //    }
        //    finally
        //    {
        //        cts.Dispose();
        //    }
        //}

        //public void CancelTask()
        //{
        //    cts.Cancel();
        //}


    }
}
