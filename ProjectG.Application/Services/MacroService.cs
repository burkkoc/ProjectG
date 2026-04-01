
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
        /// <summary>OnCycleDowntime dışında bu süreden (120 sn) uzun kalınırsa kurtarma: AH açıksa ESC, sonra downtime.</summary>
        static readonly TimeSpan StallTimeoutOperational = TimeSpan.FromSeconds(120);

        /// <summary><see cref="State.OnCycleDowntime"/> içinde uzun beklemeler (cycle downtime vb.) için üst sınır.</summary>
        static readonly TimeSpan StallTimeoutOnCycleDowntime = TimeSpan.FromMinutes(18);
        static readonly TimeSpan RunPostMissingNotifyThreshold = TimeSpan.FromMinutes(10);
        static readonly TimeSpan RunPostMissingNotifyRetryBackoff = TimeSpan.FromMinutes(1);

        SimulateService _simulateService = new();
        StateHandlerService _stateHandlerService;
        State? _watchdogLastState;
        DateTime _watchdogStateEnteredUtc;
        readonly Dictionary<State, TimeSpan> _stateTotalDurations = new();
        readonly object _stateDurationGate = new();
        int? _dynamicRestockThresholdSec;
        int? _baselineRunPostSec;
        readonly object _restockLogGate = new();
        double _restockThresholdRatio = 0.90;
        int _restockMaxNotificationCount = 3;
        int _restockNotificationSentCount;
        DateTime _lastRunPostButtonClickedEnteredUtc = DateTime.UtcNow;
        bool _runPostMissingNotificationSent;
        DateTime _nextRunPostMissingNotifyAttemptUtc = DateTime.MinValue;
        DateTime? _cancelFlowStartedUtc;
        /// <summary>RunPost girişi; çıkış ölçümü <see cref="State.PostingLoaded"/> sona erdiğinde yapılır.</summary>
        DateTime? _postingPhaseStartUtc;
        /// <summary><see cref="State.OnCycleDowntime"/> girişinde sıfırlanır; sadece ilk posting fazı guild döngü karşılaştırmasına yazılır.</summary>
        int _postingPhasesCompletedInCycle;

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
            _dynamicRestockThresholdSec = null;
            _baselineRunPostSec = null;
            _restockNotificationSentCount = 0;
            GuildBankRestockTrigger.ResetSession(NtfySettingsStore.Load());
            DualClientCycleCoordinator.Reset();
            DualClientLayoutStore.ClearActiveSlotTracking();
            AppSettings.State = State.OnCycleDowntime;
            _lastRunPostButtonClickedEnteredUtc = DateTime.UtcNow;
            _runPostMissingNotificationSent = false;
            _nextRunPostMissingNotifyAttemptUtc = DateTime.MinValue;
            _cancelFlowStartedUtc = null;
            _postingPhaseStartUtc = null;
            _postingPhasesCompletedInCycle = 0;
            await Task.Run(async () =>
            {
                while (AppSettings.Working)
                {
                    if (AppSettings.DualClient)
                        DualClientLayoutStore.EnsureGlobalsMatchForegroundWow();

                    var currentState = AppSettings.State;
                    var nowUtc = DateTime.UtcNow;
                    if (_watchdogLastState != currentState)
                    {
                        if (_watchdogLastState.HasValue)
                        {
                            var exitedState = _watchdogLastState.Value;
                            var elapsedInState = nowUtc - _watchdogStateEnteredUtc;

                            if (exitedState == State.PostingLoaded && _postingPhaseStartUtc.HasValue)
                            {
                                var phaseElapsed = nowUtc - _postingPhaseStartUtc.Value;
                                await HandleRunPostingPhaseCompletedAsync(phaseElapsed);
                                _postingPhaseStartUtc = null;
                            }
                            else if (exitedState == State.RunPostButtonClicked && currentState != State.PostingLoaded)
                                _postingPhaseStartUtc = null;

                            await HandleStateExitAsync(exitedState, elapsedInState, nowUtc);
                        }
                        _watchdogLastState = currentState;
                        _watchdogStateEnteredUtc = nowUtc;
                        if (currentState == State.OnCycleDowntime)
                            _postingPhasesCompletedInCycle = 0;
                        if (currentState == State.RunPostButtonClicked)
                        {
                            _postingPhaseStartUtc = nowUtc;
                            _lastRunPostButtonClickedEnteredUtc = nowUtc;
                            _runPostMissingNotificationSent = false;
                            _nextRunPostMissingNotifyAttemptUtc = DateTime.MinValue;
                        }
                        if (currentState == State.RunCancelButtonClicked)
                            _cancelFlowStartedUtc = nowUtc;
                        if (currentState == State.WaitingForLoadingCanceling)
                            _cancelFlowStartedUtc ??= nowUtc;
                    }
                    else
                    {
                        var limit = StallTimeoutForState(currentState);
                        if (limit > TimeSpan.Zero && nowUtc - _watchdogStateEnteredUtc > limit)
                        {
                            await _stateHandlerService.RecoverFromStallAsync(currentState);
                            _watchdogLastState = AppSettings.State;
                            _watchdogStateEnteredUtc = nowUtc;
                            continue;
                        }
                    }

                    await TryNotifyRunPostMissingAsync(currentState, nowUtc);

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
                        case State.WaitingForDetectGuildBankWindow:
                            await _stateHandlerService.WaitingForDetectGuildBankWindowHandler();
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

        async Task TryNotifyRunPostMissingAsync(State currentState, DateTime nowUtc)
        {
            if (_runPostMissingNotificationSent || nowUtc < _nextRunPostMissingNotifyAttemptUtc)
                return;

            var elapsed = nowUtc - _lastRunPostButtonClickedEnteredUtc;
            if (elapsed < RunPostMissingNotifyThreshold)
                return;

            var title = $"probably dc | {Environment.MachineName}";
            var message =
                $"RunPostButtonClicked state'ine {Math.Floor(elapsed.TotalMinutes)} dakikadir girilemedi. " +
                $"Current state: {currentState}.";
            LogRestockFlow($"runpost-missing notify trigger: elapsed={elapsed.TotalMinutes:F1}m, state={currentState}");

            bool sent = await InternetConnectivityMonitor
                .SendScreenshotNotificationAsync(title, message)
                .ConfigureAwait(false);

            if (sent)
            {
                _runPostMissingNotificationSent = true;
                LogRestockFlow("runpost-missing notify result: success");
            }
            else
            {
                _nextRunPostMissingNotifyAttemptUtc = nowUtc + RunPostMissingNotifyRetryBackoff;
                LogRestockFlow($"runpost-missing notify result: failed; retry at {_nextRunPostMissingNotifyAttemptUtc:HH:mm:ss}");
            }
        }

        Task HandleStateExitAsync(State exitedState, TimeSpan elapsed, DateTime transitionUtc)
        {
            lock (_stateDurationGate)
            {
                if (_stateTotalDurations.TryGetValue(exitedState, out var total))
                    _stateTotalDurations[exitedState] = total + elapsed;
                else
                    _stateTotalDurations[exitedState] = elapsed;
            }

            if (exitedState == State.CancelingDone && _cancelFlowStartedUtc.HasValue)
            {
                var cancelPhase = transitionUtc - _cancelFlowStartedUtc.Value;
                _cancelFlowStartedUtc = null;
                if (cancelPhase >= TimeSpan.Zero)
                    ApplyCancelPhaseToDynamicShortDowntime(cancelPhase);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Posting süresi: <see cref="State.RunPostButtonClicked"/> girişinden <see cref="State.PostingLoaded"/> çıkışına kadar.
        /// </summary>
        async Task HandleRunPostingPhaseCompletedAsync(TimeSpan phaseElapsed)
        {
            int runPostSec = (int)Math.Floor(phaseElapsed.TotalSeconds);
            bool firstPostingPhaseInCycle = _postingPhasesCompletedInCycle == 0;
            LogRestockFlow(
                $"Posting phase (RunPost..PostingLoaded) done: elapsed={phaseElapsed.TotalSeconds:F2}s ({runPostSec}s), cycleFirst={firstPostingPhaseInCycle}");
            GuildBankRestockTrigger.RecordPostingPhaseCompleted(runPostSec, firstPostingPhaseInCycle);
            _postingPhasesCompletedInCycle++;

            if (!_dynamicRestockThresholdSec.HasValue)
            {
                _baselineRunPostSec = runPostSec;
                _dynamicRestockThresholdSec = (int)Math.Floor(runPostSec * _restockThresholdRatio);
                LogRestockFlow($"threshold initialized from first posting phase: baseline={_baselineRunPostSec.Value}s, ratio={_restockThresholdRatio:F2}, shortThreshold={_dynamicRestockThresholdSec.Value}s");
                return;
            }

            if (GuildBankRestockTrigger.TryConsumeAwaitingPostAfterGuildBank())
            {
                int thresholdSec = _dynamicRestockThresholdSec.Value;
                int prevBaselineSec = _baselineRunPostSec ?? runPostSec;

                if (runPostSec <= thresholdSec)
                {
                    if (_restockNotificationSentCount >= _restockMaxNotificationCount)
                    {
                        LogRestockFlow($"notify skipped: max notification reached ({_restockMaxNotificationCount})");
                    }
                    else
                    {
                        string title = $"Restock detected | {Environment.MachineName}";
                        int shorterPercent = 0;
                        if (prevBaselineSec > 0)
                        {
                            double diffRatio = (prevBaselineSec - runPostSec) / (double)prevBaselineSec;
                            shorterPercent = (int)Math.Round(Math.Max(0, diffRatio) * 100, MidpointRounding.AwayFromZero);
                        }

                        int ratioPct = (int)Math.Round(_restockThresholdRatio * 100.0, MidpointRounding.AwayFromZero);
                        string message =
                            $"Guild bank sonrasi ilk post kisa: {runPostSec}s (esik: {thresholdSec}s = onceki ana {prevBaselineSec}s x %{ratioPct}). " +
                            $"Onceki ana normale gore yaklasik %{shorterPercent} daha kisa.";
                        LogRestockFlow($"guild-bank sonrasi post: runpost={runPostSec}s <= shortThreshold={thresholdSec}s (ratio={_restockThresholdRatio:F2})");
                        bool sent = await InternetConnectivityMonitor.SendTextNotificationAsync(title, message).ConfigureAwait(false);
                        if (sent)
                        {
                            _restockNotificationSentCount++;
                            GuildBankRestockTrigger.RecordRestockNotificationSent();
                        }
                        LogRestockFlow($"notify result: {(sent ? "success" : "failed")}");
                    }
                }
                else
                {
                    LogRestockFlow(
                        $"guild-bank sonrasi post normal sayilir: runpost={runPostSec}s > shortThreshold={thresholdSec}s; bildirim yok");
                }

                _baselineRunPostSec = runPostSec;
                _dynamicRestockThresholdSec = (int)Math.Floor(runPostSec * _restockThresholdRatio);
                LogRestockFlow(
                    $"guild sonrasi ana post guncellendi: yeni baseline={runPostSec}s, shortThreshold={_dynamicRestockThresholdSec.Value}s");
                return;
            }

            if (firstPostingPhaseInCycle)
            {
                _baselineRunPostSec = runPostSec;
                RefreshShortThresholdFromBaseline();
            }

            if (runPostSec <= _dynamicRestockThresholdSec.Value)
                LogRestockFlow($"kisa posting fazı ({runPostSec}s) ama guild bank sonrasi degil; bildirim yok");
            else
                LogRestockFlow($"no notify: posting phase={runPostSec}s > shortThreshold={_dynamicRestockThresholdSec.Value}s");
        }

        /// <summary>
        /// RunCancelButtonClicked girişinden CancelingDone çıkışına kadar süre T (sn) ise sonraki Short cycle downtime
        /// yaklaşık [minMult×T, maxMult×T+ekstra] sn; çarpanlar <see cref="NtfyUserSettings"/> üzerinden. Alt uç en az ~15 / ~25 sn.
        /// </summary>
        void ApplyCancelPhaseToDynamicShortDowntime(TimeSpan cancelPhaseDuration)
        {
            var cfg = NtfySettingsStore.Load();
            double minMult = cfg.DynamicShortAfterCancelMinTMultiplier is > 0 and < 1_000_000
                ? cfg.DynamicShortAfterCancelMinTMultiplier
                : 2;
            double maxMult = cfg.DynamicShortAfterCancelMaxTMultiplier is > 0 and < 1_000_000
                ? cfg.DynamicShortAfterCancelMaxTMultiplier
                : 2;
            double maxExtraSec = cfg.DynamicShortAfterCancelMaxExtraSeconds >= 0 && cfg.DynamicShortAfterCancelMaxExtraSeconds < 1_000_000
                ? cfg.DynamicShortAfterCancelMaxExtraSeconds
                : 10;

            double sec = Math.Max(0, cancelPhaseDuration.TotalSeconds);
            int rawMinMs = (int)Math.Round(minMult * sec * 1000.0);
            int rawMaxMs = (int)Math.Round((maxMult * sec + maxExtraSec) * 1000.0);
            const int minFloorMs = 15_000;
            const int maxFloorMs = 25_000;
            int minMs = Math.Max(minFloorMs, rawMinMs);
            int maxMs = Math.Max(maxFloorMs, rawMaxMs);
            if (maxMs <= minMs)
                maxMs = minMs + 10_000;
            AppSettings.DynamicShortCycleDowntimeMs = [minMs, maxMs];
            LogRestockFlow(
                $"dynamic short downtime: T={cancelPhaseDuration.TotalSeconds:F1}s, mult [{minMult:0.###}×T, {maxMult:0.###}×T+{maxExtraSec:0.###}s] -> [{minMs},{maxMs}]ms (raw [{rawMinMs},{rawMaxMs}]ms)");
        }

        /// <summary>İlk posting süresi ile threshold oranından «kısa posting» üst sınırı (saniye).</summary>
        void RefreshShortThresholdFromBaseline()
        {
            if (_baselineRunPostSec is int b && b > 0)
                _dynamicRestockThresholdSec = (int)Math.Floor(b * _restockThresholdRatio);
        }

        void LoadRestockSettings()
        {
            var s = NtfySettingsStore.Load();
            _restockMaxNotificationCount = Math.Max(0, s.RestockMaxNotificationCount);
            _restockThresholdRatio = Math.Clamp(s.RestockThresholdPercent / 100.0, 0.0, 1.0);
            RefreshShortThresholdFromBaseline();
            LogRestockFlow($"settings loaded: maxNotification={_restockMaxNotificationCount}, thresholdPercent={s.RestockThresholdPercent}, thresholdRatio={_restockThresholdRatio:F2}, shortThresholdSec={_dynamicRestockThresholdSec?.ToString() ?? "(yok)"}");
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
