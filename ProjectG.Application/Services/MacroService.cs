
using ProjectG.DomainLayer.Entities.Concrete;
using ProjectG.DomainLayer.Entities.Enums;
using ProjectG.InfrastructureLayer.Services;
using System.Drawing;
using System.Reflection;
using ProjectG.DomainLayer.Entities.Concrete.Statics;
using System.Runtime.Versioning;
using System.Diagnostics;
using System.Text;

namespace ProjectG.ApplicationLayer.Services
{
    [SupportedOSPlatform("windows")]
    public class MacroService
    {
        /// <summary>Çoğu state için bu süreden uzun kalınırsa <see cref="State.OnCycleDowntime"/>'a dönülür.</summary>
        static readonly TimeSpan StallTimeoutOperational = TimeSpan.FromMinutes(2);

        /// <summary><see cref="State.OnCycleDowntime"/> içinde uzun beklemeler (cycle downtime vb.) için üst sınır.</summary>
        static readonly TimeSpan StallTimeoutOnCycleDowntime = TimeSpan.FromMinutes(18);

        SimulateService _simulateService = new();
        StateHandlerService _stateHandlerService;
        State? _watchdogLastState;
        DateTime _watchdogStateEnteredUtc;
        readonly Dictionary<State, TimeSpan> _stateTotalDurations = new();
        readonly object _stateDurationGate = new();
        int? _dynamicRestockThresholdSec;
        int? _baselineCombinedSec;
        int? _lastNotifiedCombinedSec;
        TimeSpan _pendingPostFlowDuration = TimeSpan.Zero;
        readonly object _restockLogGate = new();
        double _restockThresholdRatio = 0.90;
        int _restockMaxNotificationCount = 3;
        int _restockNotificationSentCount;

        public MacroService()
        {
            _stateHandlerService = new(_simulateService);
            LoadRestockSettings();
        }

        static TimeSpan StallTimeoutForState(State s)
        {
            if (s == State.Stopped)
                return TimeSpan.Zero;
            return s == State.OnCycleDowntime ? StallTimeoutOnCycleDowntime : StallTimeoutOperational;
        }


        public async Task Run()
        {
            LoadRestockSettings();
            AppSettings.State = State.OnCycleDowntime;
            await Task.Run(async () =>
            {
                while (AppSettings.Working)
                {
                    var currentState = AppSettings.State;
                    if (_watchdogLastState != currentState)
                    {
                        if (_watchdogLastState.HasValue)
                            await HandleStateExitAsync(_watchdogLastState.Value, DateTime.UtcNow - _watchdogStateEnteredUtc);
                        _watchdogLastState = currentState;
                        _watchdogStateEnteredUtc = DateTime.UtcNow;
                    }
                    else
                    {
                        var limit = StallTimeoutForState(currentState);
                        if (limit > TimeSpan.Zero && DateTime.UtcNow - _watchdogStateEnteredUtc > limit)
                        {
                            await _stateHandlerService.RecoverFromStallAsync();
                            _watchdogLastState = AppSettings.State;
                            _watchdogStateEnteredUtc = DateTime.UtcNow;
                            continue;
                        }
                    }

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

        async Task HandleStateExitAsync(State exitedState, TimeSpan elapsed)
        {
            lock (_stateDurationGate)
            {
                if (_stateTotalDurations.TryGetValue(exitedState, out var total))
                    _stateTotalDurations[exitedState] = total + elapsed;
                else
                    _stateTotalDurations[exitedState] = elapsed;
            }

            if (exitedState == State.RunPostButtonClicked || exitedState == State.WaitingForLoadingPosting)
            {
                _pendingPostFlowDuration += elapsed;
                LogRestockFlow($"accumulate {exitedState}: +{elapsed.TotalSeconds:F2}s, pending={_pendingPostFlowDuration.TotalSeconds:F2}s");
            }

            if (exitedState == State.PostingLoaded)
            {
                var combined = _pendingPostFlowDuration + elapsed;
                _pendingPostFlowDuration = TimeSpan.Zero;
                int combinedSec = (int)Math.Floor(combined.TotalSeconds);
                LogRestockFlow($"PostingLoaded exit: elapsed={elapsed.TotalSeconds:F2}s, combined={combined.TotalSeconds:F2}s ({combinedSec}s)");

                if (!_dynamicRestockThresholdSec.HasValue)
                {
                    _baselineCombinedSec = combinedSec;
                    _dynamicRestockThresholdSec = (int)Math.Floor(combinedSec * _restockThresholdRatio);
                    LogRestockFlow($"threshold initialized from first combined: baseline={_baselineCombinedSec.Value}s, threshold={_dynamicRestockThresholdSec.Value}s");
                }
                else if (combinedSec <= _dynamicRestockThresholdSec.Value)
                {
                    if (_restockNotificationSentCount >= _restockMaxNotificationCount)
                    {
                        LogRestockFlow($"notify skipped: max notification reached ({_restockMaxNotificationCount})");
                        return;
                    }

                    string title = "Restock detected";
                    int shorterPercent = 0;
                    if (_baselineCombinedSec.HasValue && _baselineCombinedSec.Value > 0)
                    {
                        double diffRatio = (_baselineCombinedSec.Value - combinedSec) / (double)_baselineCombinedSec.Value;
                        shorterPercent = (int)Math.Round(Math.Max(0, diffRatio) * 100, MidpointRounding.AwayFromZero);
                    }

                    string message =
                        $"RunPostButtonClicked+WaitingForLoadingPosting+PostingLoaded: {combinedSec}s, threshold: {_dynamicRestockThresholdSec.Value}s. " +
                        $"Normal suresinden yaklasik %{shorterPercent} daha kisaydi.";
                    LogRestockFlow($"triggered notify: combined={combinedSec}s <= threshold={_dynamicRestockThresholdSec.Value}s");
                    bool sent = await InternetConnectivityMonitor.SendTextNotificationAsync(title, message).ConfigureAwait(false);
                    if (sent)
                    {
                        _restockNotificationSentCount++;
                        _lastNotifiedCombinedSec = combinedSec;
                        _dynamicRestockThresholdSec = (int)Math.Floor(_lastNotifiedCombinedSec.Value * 0.90);
                        LogRestockFlow($"next threshold from last notify: last={_lastNotifiedCombinedSec.Value}s, nextThreshold={_dynamicRestockThresholdSec.Value}s");
                    }
                    LogRestockFlow($"notify result: {(sent ? "success" : "failed")}");
                }
                else
                {
                    LogRestockFlow($"no notify: combined={combinedSec}s > threshold={_dynamicRestockThresholdSec.Value}s");
                }
            }
            else if (exitedState != State.RunPostButtonClicked && exitedState != State.WaitingForLoadingPosting)
            {
                // Akış beklenmedik state ile kırıldıysa eski süreyi taşımayalım.
                if (_pendingPostFlowDuration > TimeSpan.Zero)
                    LogRestockFlow($"flow reset at state={exitedState}, pending was {_pendingPostFlowDuration.TotalSeconds:F2}s");
                _pendingPostFlowDuration = TimeSpan.Zero;
            }
        }

        void LoadRestockSettings()
        {
            var s = NtfySettingsStore.Load();
            _restockMaxNotificationCount = Math.Max(0, s.RestockMaxNotificationCount);
            _restockThresholdRatio = Math.Clamp(s.RestockThresholdPercent / 100.0, 0.0, 1.0);
            LogRestockFlow($"settings loaded: maxNotification={_restockMaxNotificationCount}, thresholdPercent={s.RestockThresholdPercent}, thresholdRatio={_restockThresholdRatio:F2}");
        }

        void LogRestockFlow(string message)
        {
            var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] restock-flow: {message}";
            Debug.WriteLine(line);
            try
            {
                lock (_restockLogGate)
                {
                    var localDir = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "ProjectG");
                    Directory.CreateDirectory(localDir);
                    var localPath = Path.Combine(localDir, "restock-debug.log");
                    File.AppendAllText(localPath, line + Environment.NewLine, Encoding.UTF8);

                    var appPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "restock-debug.log");
                    File.AppendAllText(appPath, line + Environment.NewLine, Encoding.UTF8);
                }
            }
            catch
            {
                // logging should not break macro flow
            }
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
