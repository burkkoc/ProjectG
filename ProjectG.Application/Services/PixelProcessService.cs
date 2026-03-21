using AForge.Imaging.Filters;
using ProjectG.ApplicationLayer.Enums;
using ProjectG.DomainLayer.Entities.Concrete;
using ProjectG.DomainLayer.Entities.Concrete.Statics;
using ProjectG.DomainLayer.Entities.Enums;
using ProjectG.InfrastructureLayer.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Tesseract;
using static System.Net.Mime.MediaTypeNames;
//using static System.Net.Mime.MediaTypeNames;

#pragma warning disable CA1416

namespace ProjectG.ApplicationLayer.Services
{

    public class PixelProcessService
    {
        public static Rectangle GetBordersOfAButton(int centerX, int centerY, int x, int y, Bitmap bmp, bool isExitScan = false, bool repeating = false)
        {
            int matchingPixelRight = 0;
            int matchingPixelLeft = 0;
            int matchingPixelUp = 0;
            int matchingPixelDown = 0;
            //Color targetColor = TSMButton.BorderColorWhileActive;

            Bitmap bmp1 = new Bitmap(bmp);
            Bitmap bmp2 = new Bitmap(bmp);
            Bitmap bmp3 = new Bitmap(bmp);
            Bitmap bmp4 = new Bitmap(bmp);
            Color targetColor = FindColor(bmp1, x, y);


            //if (repeating)
            //    targetColor = TSMButton.BorderColorWhilePassive;

            //if (isExitScan)
            //    targetColor = Color.FromArgb(0, 0, 0);

            Parallel.Invoke(
                    () => matchingPixelRight = GetButtonBoundings(bmp1, centerX, centerY, targetColor, 1, 0) - 10,
                    () => matchingPixelLeft = GetButtonBoundings(bmp2, centerX, centerY, targetColor, -1, 0) + 10,
                    () => matchingPixelUp = GetButtonBoundings(bmp3, centerX, centerY, targetColor, 0, -1) + 7,
                    () => matchingPixelDown = GetButtonBoundings(bmp4, centerX, centerY, targetColor, 0, 1) - 7
            );
            Rectangle rec = new Rectangle(matchingPixelLeft, matchingPixelUp, matchingPixelRight - matchingPixelLeft, matchingPixelDown - matchingPixelUp);

            //if (rec.Width < 5 && targetColor != Color.FromArgb(57, 0, 0))
            //{
            //    rec = GetBordersOfAButton(startX, startY, bmp, true);
            //}
            bmp1.Dispose();
            bmp2.Dispose();
            bmp3.Dispose();
            bmp4.Dispose();
            return rec;
        }
        static int GetButtonBoundings(Bitmap bmp, int startX, int startY, Color targetColor, int xDirection, int yDirection)
        {
            int x = startX;
            int y = startY;

            for (int i = 0; i < bmp.Width; i++)
            {
                if (x >= 0 && x < bmp.Width && y >= 0 && y < bmp.Height)
                {
                    Color pixelColor = bmp.GetPixel(x, y);
                    if (IsColorMatch(pixelColor, targetColor))
                    {
                        return (xDirection != 0) ? x : y;
                    }
                }

                x += xDirection;
                y += yDirection;
            }

            return 0;
        }

        public static bool IsColorMatch(Color pixelColor, Color targetColor)
        {
            return pixelColor.R == targetColor.R &&
                   pixelColor.G == targetColor.G &&
                   pixelColor.B == targetColor.B;
        }

        public static async Task<bool> IsColorExist(Color targetColor, string imagePath)
        {
            //await ScreenService.CapturePartialScreen((Rectangle)TSMWindow.AHBorder, Enums.ActiveWindow.PostCancel);

            using (var bmp = new Bitmap(imagePath))
            {
                int x = 0;
                int y = 0;


                for (int i = 0; i < bmp.Width; i++)
                {
                    for (int k = 0; k < bmp.Height; k++)
                    {
                        if (x >= 0 && x < bmp.Width && y >= 0 && y < bmp.Height)
                        {
                            Color pixelColor = bmp.GetPixel(x, y);
                            if (IsColorMatch(pixelColor, targetColor))
                            {
                                return true;
                            }
                        }
                        y++;
                        if (y == bmp.Height) y = 0;
                    }
                    x++;
                }
                return false;
            }
        }
        public static async Task<bool> IsWindowOpen(ActiveWindow activeWindow)
        {
            return await Task.Run(async () =>
            {

                int positionX = 0;
                int positionY = 0;

                if (activeWindow == ActiveWindow.AHMenu)
                {
                    if (TSMWindow.AHBorder != null)
                    {
                        positionX = TSMWindow.AHBorder.Value.X + 3;
                        positionY = TSMWindow.AHBorder.Value.Y + 3;
                    }
                }
                else
                {
                    if (TSMWindow.MailBoxBorder != null)
                    {
                        positionX = TSMWindow.MailBoxBorder.Value.X + 3;
                        positionY = TSMWindow.MailBoxBorder.Value.Y + 3;
                    }
                }
                for (int i = 0; i < 3; i++)
                {
                    using (Bitmap screenPixel = new Bitmap(1, 1))
                    {
                        using (Graphics g = Graphics.FromImage(screenPixel))
                        {
                            g.CopyFromScreen(positionX, positionY, 0, 0, new Size(1, 1));
                        }

                        Color color = screenPixel.GetPixel(0, 0);

                        if (StaticTSMButtons.RunCancelScan != null)
                        {
                            if (IsColorMatch(color, StaticTSMButtons.RunCancelScan.BackgroundColor))
                                return true;
                            else
                                await Task.Delay(150);
                        }
                    }
                }
                return false;
            });
        }


        public static Color FindColor(Bitmap bmp, int startPointX, int startPointY)
        {
            int startX = startPointX;
            int startY = startPointY;

            if (startX < 0 || startX >= bmp.Width || startY < 0 || startY >= bmp.Height)
            {
                throw new ArgumentOutOfRangeException("Starting point is out of bounds.");
            }

            for (int x = startX; x >= 0; x--)
            {
                // O anki pikselin rengini alıyoruz
                Color currentPixelColor = bmp.GetPixel(x, startY);

                // Piksel rengi referans renkten farklıysa kaydediyoruz
                if (currentPixelColor != AppSettings.TargetButtonColor)
                {
                    return currentPixelColor; // İlk farklı piksel bulunduğunda rengi döndür
                }
            }

            return Color.FromArgb(0, 0, 0);
        }

        public static async Task<Rectangle?> FindTSMWindow(ActiveWindow activeWindow)
        {
            //Bitmap generalScreenshot;
            Paths.GeneralImagePath = UtilityService.GetDirectory(ActiveWindow.General);
            if (await ScreenService.CaptureScreen())
            {
                using (Bitmap generalScreenshot = new Bitmap(Paths.GeneralImagePath))
                {
                    //Color targetBorderColor = Color.FromArgb(35, 35, 35);
                    Color targetBorderColor = Color.FromArgb(0, 0, 0);
                    int startX = -1, startY = -1, width = 0, height = 0;

                    // Ekranı tarayıp hedef renkli pikseli bul
                    for (int y = 0; y < generalScreenshot.Height; y++)
                    {
                        for (int x = 0; x < generalScreenshot.Width; x++)
                        {
                            Color pixelColor = generalScreenshot.GetPixel(x, y);
                            if (IsColorMatch(pixelColor, targetBorderColor))
                            {
                                // İlk bulduğumuz noktayı başlangıç (sol üst köşe) olarak alıyoruz
                                startX = x;
                                startY = y;

                                // Genişliği belirle (X ekseni boyunca tarayarak 14,14,14 olan kısmı bul)
                                while (x < generalScreenshot.Width && IsColorMatch(generalScreenshot.GetPixel(x, y), targetBorderColor))
                                {
                                    x++;
                                }
                                width = x - startX;

                                // Yüksekliği belirle (Y ekseni boyunca tarayarak 14,14,14 olan kısmı bul)
                                int tempY = y;
                                while (tempY < generalScreenshot.Height && IsColorMatch(generalScreenshot.GetPixel(startX, tempY), targetBorderColor))
                                {
                                    tempY++;
                                }
                                height = tempY - startY;
                                if (width > 150 && height > 150)
                                {

                                    Rectangle rec = new Rectangle(startX, startY, width, height);
                                    using (Graphics g = Graphics.FromImage(generalScreenshot))
                                    {
                                        using (Pen pen = new(Color.Red, 2))
                                        {
                                            g.DrawRectangle(pen, rec);
                                        }
                                    }

                                    await ScreenService.CapturePartialScreen(rec, activeWindow);
                                    //generalScreenshot.Save(filePath);
                                    //screenshot.Save(filePath);

                                    // Dikdörtgeni bulduktan sonra işlemi durdur
                                    return rec;
                                }
                            }
                        }
                    }

                }
            }
            return null; // Eğer hiç bulunamazsa boş bir dikdörtgen dön
        }

        public static async Task<Rectangle?> FindChatWindow(ActiveWindow activeWindow)
        {
            //Bitmap generalScreenshot;
            Paths.GeneralImagePath = UtilityService.GetDirectory(ActiveWindow.General);
            string path = string.Empty;
            if (activeWindow == ActiveWindow.Chat)
                path = Paths.GeneralImagePath;
            else if (activeWindow == ActiveWindow.MailboxText)
                path = Paths.MailBoxTextImagePath;

            if (await ScreenService.CaptureScreen())
            {
                using (Bitmap generalScreenshot = new Bitmap(path))
                {
                    Color targetBorderColor = Color.FromArgb(0, 0, 0);
                    int startX = -1, startY = -1, width = 0, height = 0;

                    // Ekranı tarayıp hedef renkli pikseli bul
                    for (int x = 0; x < generalScreenshot.Width; x++)
                    {
                        for (int y = 100; y < generalScreenshot.Height; y++)
                        {
                            Color pixelColor = generalScreenshot.GetPixel(x, y);
                            if (IsColorMatch(pixelColor, targetBorderColor))
                            {
                                startX = x;
                                startY = y;

                                while (y < generalScreenshot.Height && IsColorMatch(generalScreenshot.GetPixel(x, y), targetBorderColor))
                                {
                                    y++;
                                }
                                height = y - startY;

                                while (x < generalScreenshot.Width && IsColorMatch(generalScreenshot.GetPixel(x, y - 3), targetBorderColor))
                                {

                                    x++;
                                }

                                width = x - startX;

                                if (width > 200 && height > 80)
                                {
                                    Rectangle rec = new Rectangle(startX, startY, width, height);
                                    using (Graphics g = Graphics.FromImage(generalScreenshot))
                                    {
                                        using (Pen pen = new(Color.Red, 2))
                                        {
                                            g.DrawRectangle(pen, rec);
                                        }
                                    }
                                    await ScreenService.CapturePartialScreen(rec, activeWindow);
                                    return rec;
                                }
                                else
                                {
                                    x = 0;
                                }
                            }


                        }
                    }
                }
            }
            return null; // Eğer hiç bulunamazsa boş bir dikdörtgen dön
        }

        public static async Task<bool> IsClickable(TSMButton tsmButton, bool isActive = true)
        {
            Color? passiveColor = null;
            int positionX = tsmButton.X;
            int positionY = tsmButton.Y;
            for (int i = 0; i < 90; i++)
            {
                await Task.Delay(333);
                using (Bitmap screenPixel = new Bitmap(1, 1))
                {
                    using (Graphics g = Graphics.FromImage(screenPixel))
                    {
                        if (isActive)
                            g.CopyFromScreen(positionX, positionY, 0, 0, new Size(1, 1));
                        else
                        {
                            g.CopyFromScreen(positionX - 10, positionY, 0, 0, new Size(1, 1));
                            passiveColor = screenPixel.GetPixel(0, 0);
                            g.CopyFromScreen(positionX, positionY, 0, 0, new Size(1, 1));
                        }
                    }

                    Color color = screenPixel.GetPixel(0, 0);
                    if (tsmButton.BackgroundColor == color)
                        return true;
                    if (passiveColor != null && PixelProcessService.IsColorMatch((Color)passiveColor, color) && !isActive)
                        return true;
                }
            }

            return false;
        }

        public static Color GetBackgroundColor(int x, int y)
        {
            using (Bitmap screenPixel = new Bitmap(1, 1))
            {
                using (Graphics g = Graphics.FromImage(screenPixel))
                    g.CopyFromScreen(x, y, 0, 0, new Size(1, 1));
                return screenPixel.GetPixel(0, 0);
            }
        }

        /// <summary>
        /// Ekran bölgesini Z ile kaydedilen referans PNG ile karşılaştırır; aynı piksel oranı <paramref name="minimumMatchPercent"/> üzerindeyse true.
        /// </summary>
        public static async Task<bool> ScreenRegionMatchesSavedReferenceAsync(Rectangle screenRegion, string referenceImagePath, double minimumMatchPercent = 90.0)
        {
            return await Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(referenceImagePath) || !File.Exists(referenceImagePath))
                    return false;

                using Bitmap? current = ScreenService.CaptureRegionToBitmap(screenRegion);
                if (current is null)
                    return false;

                using Bitmap loadedRef = new Bitmap(referenceImagePath);
                if (loadedRef.Width != current.Width || loadedRef.Height != current.Height)
                    return false;

                using Bitmap refArgb = loadedRef.Clone(new Rectangle(0, 0, loadedRef.Width, loadedRef.Height), PixelFormat.Format32bppArgb);
                using Bitmap curArgb = current.Clone(new Rectangle(0, 0, current.Width, current.Height), PixelFormat.Format32bppArgb);

                double percent = ComputeIdenticalPixelPercentArgb(refArgb, curArgb);
                return percent >= minimumMatchPercent;
            });
        }

        /// <summary>
        /// Referans ile ekran bölgesi piksel piksel birebir aynı mı (yüzde 100).
        /// </summary>
        public static async Task<bool> ScreenRegionFullyMatchesSavedReferenceAsync(Rectangle screenRegion, string referenceImagePath)
        {
            return await Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(referenceImagePath) || !File.Exists(referenceImagePath))
                    return false;

                using Bitmap? current = ScreenService.CaptureRegionToBitmap(screenRegion);
                if (current is null)
                    return false;

                using Bitmap loadedRef = new Bitmap(referenceImagePath);
                if (loadedRef.Width != current.Width || loadedRef.Height != current.Height)
                    return false;

                using Bitmap refArgb = loadedRef.Clone(new Rectangle(0, 0, loadedRef.Width, loadedRef.Height), PixelFormat.Format32bppArgb);
                using Bitmap curArgb = current.Clone(new Rectangle(0, 0, current.Width, current.Height), PixelFormat.Format32bppArgb);

                return AreBitmapsFullyIdenticalArgb(refArgb, curArgb);
            });
        }

        static bool AreBitmapsFullyIdenticalArgb(Bitmap a, Bitmap b)
        {
            if (a.Width != b.Width || a.Height != b.Height)
                return false;

            var rect = new Rectangle(0, 0, a.Width, a.Height);
            BitmapData da = a.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData db = b.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                int stride = da.Stride;
                int w = a.Width;
                int h = a.Height;

                for (int y = 0; y < h; y++)
                {
                    int row = y * stride;
                    for (int x = 0; x < w; x++)
                    {
                        int offset = row + x * 4;
                        if (Marshal.ReadInt32(da.Scan0, offset) != Marshal.ReadInt32(db.Scan0, offset))
                            return false;
                    }
                }

                return true;
            }
            finally
            {
                a.UnlockBits(da);
                b.UnlockBits(db);
            }
        }

        static double ComputeIdenticalPixelPercentArgb(Bitmap a, Bitmap b)
        {
            var rect = new Rectangle(0, 0, a.Width, a.Height);
            BitmapData da = a.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData db = b.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                int stride = da.Stride;
                int w = a.Width;
                int h = a.Height;
                int match = 0;
                int total = w * h;

                for (int y = 0; y < h; y++)
                {
                    int row = y * stride;
                    for (int x = 0; x < w; x++)
                    {
                        int offset = row + x * 4;
                        int pa = Marshal.ReadInt32(da.Scan0, offset);
                        int pb = Marshal.ReadInt32(db.Scan0, offset);
                        if (pa == pb)
                            match++;
                    }
                }

                return total == 0 ? 0 : 100.0 * match / total;
            }
            finally
            {
                a.UnlockBits(da);
                b.UnlockBits(db);
            }
        }

    }
}

