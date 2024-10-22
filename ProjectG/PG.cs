
using ProjectG.ApplicationLayer.Enums;
using ProjectG.ApplicationLayer.Services;
using ProjectG.DomainLayer.Entities.Concrete;
using ProjectG.DomainLayer.Entities.Concrete.Statics;
using ProjectG.DomainLayer.Entities.Enums;
using ProjectG.PresentationLayer;
using ProjectG.PresentationLayer.Services;
using System.Collections.Generic;

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
        public RadioButton radioBtnLong => RadioLong;
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

                //_macroService.CancelTask();
            }
        }

        //private void btnSettings_Click(object sender, EventArgs e)
        //{
        //    this.Hide();
        //    _settingsForm.ShowDialog();
        //}

        private async void btnStart_Click(object sender, EventArgs e)
        {
            
            if (!AppSettings.Working)
            {
                btnStart.Text = "Stop";
                AppSettings.Working = true;
                _uiHelper.SetCycleDowntime();

                //btnSettings.Enabled = false;
                //btnViewImage.Enabled = false;
                btnStart.Enabled = false;
                radioBtnLong.Enabled = false;
                radioBtnMedium.Enabled = false;
                radioBtnShort.Enabled = false;
                radioBtnShortMedium.Enabled = false;
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
                TSMWindow.Reset();
                AppSettings.Reset();
                StaticTSMButtons.Reset();
            }

            await _macroService.Run();
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
    }
}
