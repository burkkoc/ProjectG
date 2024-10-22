using ProjectG.ApplicationLayer.Enums;
using ProjectG.DomainLayer.Entities.Concrete;
using ProjectG.DomainLayer.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace ProjectG.ApplicationLayer.Services
{
    public class UtilityService
    {
        public static int GenerateRandom(int numberStart, int numberEnd)
        {
            if (numberStart > numberEnd)
                return 0;
            return new Random().Next(numberStart, numberEnd);
        }

        public static bool SetScreenResolution(int screenResX, int screenResY)
        {
            try
            {
                UserInput.ScreenResolutionX = screenResX;
                UserInput.ScreenResolutionY = screenResY;
                return true;
            }
            catch
            {
                return false;
            }

        }

        public static string GetDirectory(ActiveWindow? activeWindow)
        {
            string projectPath = AppDomain.CurrentDomain.BaseDirectory;
            string imagesFolder = Path.Combine(projectPath, "Images");
            if(!Directory.Exists(imagesFolder))
                Directory.CreateDirectory(imagesFolder);

            switch (activeWindow) 
            {
                case ActiveWindow.PostCancel:
                    return Path.Combine(imagesFolder, "PostCancel.png");
                case ActiveWindow.AHMenu:
                    return Path.Combine(imagesFolder, "AHMenu.png");
                case ActiveWindow.MailBox:
                    return Path.Combine(imagesFolder, "MailBox.png");
                case ActiveWindow.General:
                    return Path.Combine(imagesFolder, "General.png");
                case ActiveWindow.Chat:
                    return Path.Combine(imagesFolder, "Chat.png");
                default:
                    return Path.Combine(imagesFolder, "MailBoxText.png");

            }
        }

        public static string GetTesseractDirectory()
        {
            string projectPath = AppDomain.CurrentDomain.BaseDirectory;
            string tesseractFolder = Path.Combine(projectPath, "Tesseract-OCR", "tessdata");
            return tesseractFolder;
        }

        public static int SetCycleDowntime()
        {
            switch (AppSettings.CycleDowntime)
            {
                case CycleDowntime.Short:
                    return new Random().Next(15000, 25000);
                case CycleDowntime.ShortMedium:
                    return new Random().Next(35000, 65000);
                case CycleDowntime.Medium:
                    return new Random().Next(55000, 90000);
                case CycleDowntime.Long:
                    return new Random().Next(120000, 180000);
            }
            return 0;
        }

        public void ResetEverything()
        {
            AppSettings.State = State.OnCycleDowntime;
            TSMWindow.Reset();
            AppSettings.Reset();
        }


    }
}
