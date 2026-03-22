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
        private SettingsForm _settingsForm;
        public PG(MacroService macroService)
        {
            InitializeComponent();
            _settingsForm = new SettingsForm(this);
            _macroService = macroService;
            _uiHelper = new UIHelper(this);
            _uiHelper.Initiate();
            this.KeyPreview = true;

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
            if (e.KeyCode == Keys.Z)
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

        //private void btnSettings_Click(object sender, EventArgs e)
        //{
        //    this.Hide();
        //    _settingsForm.ShowDialog();
        //}

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
                if (cBoxDualClient.Checked)
                    AppSettings.DualClient = true;
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
