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
            return Random.Shared.Next(numberStart, numberEnd);
        }

        /// <summary>
        /// Dikdörtgen içi tıklama koordinatı. <paramref name="edgeLean"/> ile uçlara eğilim ayarlanır:
        /// düşük değer iki uniformun ortalaması (merkeze yakın), yüksek değer min/max örnekleri (kenarlara yakın).
        /// </summary>
        public static int RandomClickCoordinateInRange(int minInclusive, int exclusiveMax, double edgeLean = 0.58)
        {
            if (minInclusive >= exclusiveMax)
                return minInclusive;
            int a = Random.Shared.Next(minInclusive, exclusiveMax);
            int b = Random.Shared.Next(minInclusive, exclusiveMax);
            if (Random.Shared.NextDouble() < edgeLean)
                return Random.Shared.Next(0, 2) == 0 ? Math.Min(a, b) : Math.Max(a, b);
            return (a + b) / 2;
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
                case ActiveWindow.Bank:
                    return Path.Combine(imagesFolder, "GuildBank.png");
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
                {
                    int[] r = AppSettings.DynamicShortCycleDowntimeMs;
                    if (r is null || r.Length < 2)
                        return Random.Shared.Next(15000, 25001);
                    int lo = Math.Max(1000, r[0]);
                    int hi = Math.Max(lo + 1, r[1]);
                    return Random.Shared.Next(lo, hi + 1);
                }
                case CycleDowntime.ShortMedium:
                    return Random.Shared.Next(35000, 65000);
                case CycleDowntime.Medium:
                    return Random.Shared.Next(55000, 90000);
                case CycleDowntime.Custom:
                {
                    int[] r = AppSettings.CustomCycleDowntimeMs;
                    if (r is null || r.Length < 2)
                        return Random.Shared.Next(120000, 180001);
                    int lo = Math.Max(1000, r[0]);
                    int hi = Math.Max(lo + 1, r[1]);
                    return Random.Shared.Next(lo, hi + 1);
                }
            }
            return 0;
        }

        public void ResetEverything()
        {
            DualClientCycleCoordinator.Reset();
            AppSettings.State = State.OnCycleDowntime;
            TSMWindow.Reset();
            AppSettings.Reset();
        }


    }
}
