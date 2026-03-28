using ProjectG.ApplicationLayer.Services;
using ProjectG.DomainLayer.Entities.Concrete;
using ProjectG.DomainLayer.Entities.Concrete.Statics;
using ProjectG.PresentationLayer;
using ProjectG.PresentationLayer.Services;

namespace ProjectG
{
    public partial class PG : Form
    {
        private readonly MacroService _macroService;
        private UIHelper _uiHelper;
        private Keys _mailboxLocateKey = Keys.Z;
        private bool _isLoadingCustomDowntimeSettings;
        private bool _isLoadingCycleDowntimeSelection;
        public PG(MacroService macroService)
        {
            InitializeComponent();
            _macroService = macroService;
            _uiHelper = new UIHelper(this);
            _uiHelper.Initiate();
            this.KeyPreview = true;
            LoadMailboxHotkeyFromSettings();
            LoadCustomDowntimeFromSettings();
            LoadSelectedCycleDowntimeFromSettings();
            numericCustomDowntimeMinSec.ValueChanged += CustomDowntimeInput_ValueChanged;
            numericCustomDowntimeMaxSec.ValueChanged += CustomDowntimeInput_ValueChanged;
            RadioShort.CheckedChanged += CycleDowntimeRadio_CheckedChanged;
            RadioMedium.CheckedChanged += CycleDowntimeRadio_CheckedChanged;
            RadioShortMedium.CheckedChanged += CycleDowntimeRadio_CheckedChanged;
            RadioCustom.CheckedChanged += CycleDowntimeRadio_CheckedChanged;

            //Paths.PostCancelImagePath = UtilityService.GetDirectory(ActiveWindow.PostCancel);
            //Paths.AHMenuImagePath = UtilityService.GetDirectory(ActiveWindow.AHMenu);
            //Paths.MailBoxImagePath = UtilityService.GetDirectory(ActiveWindow.MailBox);
            //Paths.GeneralImagePath = UtilityService.GetDirectory(ActiveWindow.General);
            //Paths.ChatWindowImagePath = UtilityService.GetDirectory(ActiveWindow.Chat);
            //Paths.MailBoxTextImagePath = UtilityService.GetDirectory(null);
            Paths.PostCancelImagePath = Path.Combine(Application.StartupPath, "Images", "PostCancel.png");
            Paths.AHMenuImagePath = Path.Combine(Application.StartupPath, "Images", "AHMenu.png");
            Paths.MailBoxImagePath = Path.Combine(Application.StartupPath, "Images", "MailBox.png");
            Paths.GeneralImagePath = Path.Combine(Application.StartupPath, "Images", "General.png");
            Paths.ChatWindowImagePath = Path.Combine(Application.StartupPath, "Images", "Chat.png");
            Paths.MailBoxTextImagePath = Path.Combine(Application.StartupPath, "Images", "MailBoxText.png");
            Paths.MailBoxCornerReferencePath = Path.Combine(Application.StartupPath, "MailBoxCornerReference.png");
            //Paths.TesseractPath = Path.Combine(Application.StartupPath, "Tesseract-OCR","tessdata");
            //TimerDowntime.Stop();

        }

        //public RadioButton radioBtnScreenManual => radioScreenManual;
        //public RadioButton radioBtnScreenAutoDetect => radioScreenAutoDetect;
        //public TextBox textBoxScreenHeight => txtScreenHeight;
        //public TextBox textBoxScreenWidth => txtScreenWidth;
        //public Button buttonViewImage => btnViewImage;
        //public Button buttonSettings => btnSettings;
        public RadioButton radioBtnShort => RadioShort;
        public RadioButton radioBtnMedium => RadioMedium;
        public RadioButton radioBtnCustom => RadioCustom;
        public RadioButton radioBtnShortMedium => RadioShortMedium;





        //private void RadioScreenManual_CheckedChanged(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        txtScreenWidth.Enabled = true;
        //        txtScreenHeight.Enabled = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"An error occurred: {ex}");
        //    }
        //}

        //private void RadioScreenAutoDetect_CheckedChanged(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        txtScreenWidth.Enabled = false;
        //        txtScreenHeight.Enabled = false;

        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"An error occurred: {ex}");
        //    }
        //}


        //private void btnViewImage_Click(object sender, EventArgs e)
        //{
        //    _uiHelper.ViewImage();
        //}


        private void PG_FormClosing(object sender, FormClosingEventArgs e)
        {
            //_uiHelper.DeleteImage();
            string projectPath = AppDomain.CurrentDomain.BaseDirectory;
            string imagesFolder = Path.Combine(projectPath, "Images");
            if (Directory.Exists(imagesFolder))
                //Directory.Delete(imagesFolder, true);
                Application.Exit();
        }

        private async void PG_KeyDown(object sender, KeyEventArgs e)
        {
            //if (e.Control && e.Alt && e.KeyCode == Keys.P)
            if (e.KeyCode == _mailboxLocateKey)
            {
                int x = Cursor.Position.X;
                int y = Cursor.Position.Y;
                btnStart.Enabled = true;
                AppSettings.MailBoxPosition = new Rectangle(x, y, 60, 60);
                btnStart.Text = "Start";

                await MailBoxCornerCalibration.SaveReferenceSnapshotAsync();

                //_macroService.CancelTask();
            }
        }

        void btnSettings_Click(object? sender, EventArgs e)
        {
            using var f = new SettingsForm();
            f.StartPosition = FormStartPosition.Manual;
            var b = Bounds;
            f.Location = new Point(b.Left, b.Bottom + 4);
            f.TopMost = true;
            if (f.ShowDialog(this) == DialogResult.OK)
                LoadMailboxHotkeyFromSettings();
        }

        private void LoadMailboxHotkeyFromSettings()
        {
            var settings = NtfySettingsStore.Load();
            if (!string.IsNullOrWhiteSpace(settings.MailboxLocateHotkey)
                && Enum.TryParse<Keys>(settings.MailboxLocateHotkey, true, out var parsedKey))
            {
                _mailboxLocateKey = parsedKey;
            }
            else
            {
                _mailboxLocateKey = Keys.Z;
            }
        }

        private void ApplyCustomDowntimeFromUi()
        {
            int minSec = (int)numericCustomDowntimeMinSec.Value;
            int maxSec = (int)numericCustomDowntimeMaxSec.Value;
            int maxAllowed = (int)numericCustomDowntimeMaxSec.Maximum;
            int minAllowed = (int)numericCustomDowntimeMinSec.Minimum;
            if (minSec >= maxSec)
            {
                maxSec = minSec + 1;
                if (maxSec > maxAllowed)
                {
                    maxSec = maxAllowed;
                    minSec = maxAllowed - 1;
                    if (minSec < minAllowed)
                        minSec = minAllowed;
                    numericCustomDowntimeMinSec.Value = minSec;
                }
                numericCustomDowntimeMaxSec.Value = maxSec;
            }
            AppSettings.CustomCycleDowntimeMs = [minSec * 1000, maxSec * 1000];
        }

        private void LoadCustomDowntimeFromSettings()
        {
            _isLoadingCustomDowntimeSettings = true;
            try
            {
                var settings = NtfySettingsStore.Load();
                int minAllowed = (int)numericCustomDowntimeMinSec.Minimum;
                int minMaxAllowed = (int)numericCustomDowntimeMinSec.Maximum;
                int maxMinAllowed = (int)numericCustomDowntimeMaxSec.Minimum;
                int maxAllowed = (int)numericCustomDowntimeMaxSec.Maximum;
                int minSec = Math.Clamp(settings.CustomDowntimeMinSeconds, minAllowed, minMaxAllowed);
                int maxSec = Math.Clamp(settings.CustomDowntimeMaxSeconds, maxMinAllowed, maxAllowed);

                if (minSec >= maxSec)
                {
                    maxSec = Math.Min(maxAllowed, minSec + 1);
                    if (minSec >= maxSec)
                        minSec = Math.Max(minAllowed, maxSec - 1);
                }

                numericCustomDowntimeMinSec.Value = minSec;
                numericCustomDowntimeMaxSec.Value = maxSec;
                ApplyCustomDowntimeFromUi();
            }
            finally
            {
                _isLoadingCustomDowntimeSettings = false;
            }
        }

        private void SaveCustomDowntimeToSettings()
        {
            var settings = NtfySettingsStore.Load();
            settings.CustomDowntimeMinSeconds = (int)numericCustomDowntimeMinSec.Value;
            settings.CustomDowntimeMaxSeconds = (int)numericCustomDowntimeMaxSec.Value;
            NtfySettingsStore.Save(settings);
        }

        private void CustomDowntimeInput_ValueChanged(object? sender, EventArgs e)
        {
            if (_isLoadingCustomDowntimeSettings)
                return;

            ApplyCustomDowntimeFromUi();
            RadioCustom.Checked = true;
            SaveCustomDowntimeToSettings();
        }

        private void LoadSelectedCycleDowntimeFromSettings()
        {
            _isLoadingCycleDowntimeSelection = true;
            try
            {
                var selected = NtfySettingsStore.Load().SelectedCycleDowntime;
                switch (selected?.Trim().ToLowerInvariant())
                {
                    case "medium":
                        RadioMedium.Checked = true;
                        break;
                    case "shortmedium":
                        RadioShortMedium.Checked = true;
                        break;
                    case "custom":
                        RadioCustom.Checked = true;
                        break;
                    default:
                        RadioShort.Checked = true;
                        break;
                }
            }
            finally
            {
                _isLoadingCycleDowntimeSelection = false;
            }
        }

        private void SaveSelectedCycleDowntimeToSettings()
        {
            var settings = NtfySettingsStore.Load();
            settings.SelectedCycleDowntime = RadioShort.Checked ? "Short"
                : RadioMedium.Checked ? "Medium"
                : RadioShortMedium.Checked ? "ShortMedium"
                : "Custom";
            NtfySettingsStore.Save(settings);
        }

        private void CycleDowntimeRadio_CheckedChanged(object? sender, EventArgs e)
        {
            if (_isLoadingCycleDowntimeSelection || sender is not RadioButton radio || !radio.Checked)
                return;

            SaveSelectedCycleDowntimeToSettings();
        }

        private void SetCustomDowntimeInputsEnabled(bool enabled)
        {
            numericCustomDowntimeMinSec.Enabled = enabled;
            numericCustomDowntimeMaxSec.Enabled = enabled;
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {

            if (!AppSettings.Working)
            {
                btnStart.Text = "Stop";
                AppSettings.Working = true;
                AppSettings.DualClient = cBoxDualClient.Checked;
                AppSettings.DynamicAhFlow = cBoxDynamic.Checked;
                ApplyCustomDowntimeFromUi();
                _uiHelper.SetCycleDowntime();

                //btnSettings.Enabled = false;
                //btnViewImage.Enabled = false;
                btnStart.Enabled = false;
                radioBtnCustom.Enabled = false;
                radioBtnMedium.Enabled = false;
                radioBtnShort.Enabled = false;
                radioBtnShortMedium.Enabled = false;
                SetCustomDowntimeInputsEnabled(false);
                //TimerDowntime.Start();
                TimerDowntime.Enabled = true;
                string projectPath = AppDomain.CurrentDomain.BaseDirectory;
                string imagesFolder = Path.Combine(projectPath, "Images");
                if (Directory.Exists(imagesFolder))
                    Directory.Delete(imagesFolder, true);
            }
            else
            {
                btnStart.Text = "Start";
                AppSettings.Working = false;
                gBoxCycleDowntime.Enabled = true;
                //btnSettings.Enabled = true;
                //btnViewImage.Enabled = true;
                btnStart.Enabled = false;
                radioBtnCustom.Enabled = true;
                radioBtnMedium.Enabled = true;
                radioBtnShort.Enabled = true;
                radioBtnShortMedium.Enabled = true;
                SetCustomDowntimeInputsEnabled(true);
                TSMWindow.Reset();
                AppSettings.Reset();
                StaticTSMButtons.Reset();
            }

            await _macroService.Run();

            if (!AppSettings.Working)
            {
                btnStart.Enabled = true;
                btnStart.Text = "Start";
                radioBtnCustom.Enabled = true;
                radioBtnMedium.Enabled = true;
                radioBtnShort.Enabled = true;
                radioBtnShortMedium.Enabled = true;
                SetCustomDowntimeInputsEnabled(true);
            }
        }

        int timeLeft = -3;
        int onetwothree = 0;
        private void TimerDowntime_Tick(object sender, EventArgs e)
        {
            if (LblState.Text != AppSettings.State.ToString())
            {
                LblState.Text = AppSettings.State.ToString();
            }
            if (AppSettings.Downtime > 0 && timeLeft == -3)
            {
                timeLeft = (AppSettings.Downtime / 1000) - 1;
            }
            else if (timeLeft != -3)
            {
                timeLeft--;
                if (timeLeft > 0)
                    LblDowntime.Text = "Downtime: " + timeLeft.ToString();
                else
                    LblDowntime.Text = "Working...";

            }
            else
            {
                string dots = onetwothree == 0 ? "." : onetwothree == 1 ? ".." : onetwothree == 2 ? "..." : ".";
                LblDowntime.Text = $"Working{dots}";

                if (onetwothree == 3)
                    onetwothree = 1;
                else
                    onetwothree++;


            }

        }

        private void TimerInternetUi_Tick(object sender, EventArgs e)
        {
            if (AppSettings.IsInternetReachable)
            {
                LblInternetStatus.Text = "Online";
                LblInternetStatus.ForeColor = Color.FromArgb(0, 118, 56);
            }
            else
            {
                LblInternetStatus.Text = "Offline";
                LblInternetStatus.ForeColor = Color.FromArgb(176, 58, 46);
            }
        }
    }
}
