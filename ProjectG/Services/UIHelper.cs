using ProjectG.ApplicationLayer.Services;
using ProjectG.DomainLayer.Entities.Concrete;
using ProjectG.DomainLayer.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

namespace ProjectG.PresentationLayer.Services
{
    public class UIHelper
    {
        private readonly PG _pg;
        int[]? screenResArray;
        public UIHelper(PG pg)
        {
            _pg = pg;
        }


        public void Initiate()
        {
            if (screenResArray == null)
                screenResArray = GetScreenResolution();
            UtilityService.SetScreenResolution(screenResArray[0], screenResArray[1]);
        }

        public void PrintResponses(IEnumerable<string> response)
        {

            StringBuilder res = new StringBuilder();
            foreach (var responseItem in response)
            {
                res.Append($"{responseItem} \n\r");
            }
            MessageBox.Show(res.ToString());


        }

        public void ViewImage()
        {
            string screenshotPath = Directory.GetCurrentDirectory();
            Process.Start(new ProcessStartInfo(screenshotPath) { UseShellExecute = true });

        }

        //public void DeleteImage()
        //{
        //    string screenshotPath = @"C:\Users\bkoc\Downloads\output.png";
        //    //if(File.Exists(screenshotPath))
        //    //    File.Delete(screenshotPath);
        //}


        public int[] DetectScreenResolution()
        {

            if (Screen.PrimaryScreen == null) throw new Exception("Primary screen is not detected.");
            return [Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height];
        }

        public int[] GetScreenResolution()
        {
            //if (_pg.radioBtnScreenAutoDetect.Checked)
            return DetectScreenResolution();
            //else
            //return [int.Parse(_pg.textBoxScreenWidth.Text), int.Parse(_pg.textBoxScreenHeight.Text)];

        }

        public void SetCycleDowntime()
        {
            if (_pg.radioBtnShort.Checked)
                AppSettings.CycleDowntime = CycleDowntime.Short;
            else if (_pg.radioBtnMedium.Checked)
                AppSettings.CycleDowntime = CycleDowntime.Medium;
            else if (_pg.radioBtnShortMedium.Checked)
                AppSettings.CycleDowntime = CycleDowntime.ShortMedium;
            else
                AppSettings.CycleDowntime = CycleDowntime.Long;


        }

    }
}
