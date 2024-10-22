using AForge.Imaging.Filters;
using ProjectG.ApplicationLayer.Enums;
using ProjectG.DomainLayer.Entities.Concrete;
using ProjectG.DomainLayer.Entities.Enums;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ProjectG.InfrastructureLayer.Services
{
    public class ScreenService
    {
        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(nint hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, nint hdcSrc, int nXSrc, int nYSrc, int dwRop);

        [DllImport("user32.dll")]
        private static extern nint GetDesktopWindow();

        [DllImport("user32.dll")]
        private static extern nint GetWindowDC(nint hWnd);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(nint hObject);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(nint hWnd, nint hDc);

        private const int SRCCOPY = 0x00CC0020;

        //        public static async Task<bool> CaptureScreen()
        //        {
        //            try
        //            {
        //                return await Task.Run(() =>
        //                {
        //                    int screenWidth = UserInput.ScreenResolutionX;
        //                    int screenHeight = UserInput.ScreenResolutionY;


        //#pragma warning disable CA1416
        //                    Bitmap bmp = new(screenWidth, screenHeight, PixelFormat.Format32bppArgb);

        //                    // Hedef grafik nesnesi
        //                    using (Graphics g = Graphics.FromImage(bmp))
        //                    {
        //                        // Kaynak ekran DC'si
        //                        nint hdcSrc = GetWindowDC(GetDesktopWindow());
        //                        // Hedef grafik DC'si
        //                        nint hdcDest = g.GetHdc();

        //                        // BitBlt ile ekran görüntüsü alma
        //                        BitBlt(hdcDest, 0, 0, screenWidth, screenHeight, hdcSrc, 0, 0, SRCCOPY);

        //                        // DC'leri serbest bırakma
        //                        g.ReleaseHdc(hdcDest);
        //                        ReleaseDC(GetDesktopWindow(), hdcSrc);
        //                    }
        //                    if (File.Exists(Paths.GeneralImagePath))
        //                        File.Delete(Paths.GeneralImagePath);

        //                    bmp.Save(Paths.GeneralImagePath, ImageFormat.Png);
        //                    bmp.Dispose();
        //                    return true;
        //                });
        //            }
        //            catch
        //            {
        //                return false;
        //            }
        //        }
        public static async Task<bool> CaptureScreen()
        {
            try
            {
                return await Task.Run(() =>
                {
                    int screenWidth = UserInput.ScreenResolutionX;
                    int screenHeight = UserInput.ScreenResolutionY;

#pragma warning disable CA1416
                    using (Bitmap bmp = new(screenWidth, screenHeight, PixelFormat.Format32bppArgb))
                    {
                        // Hedef grafik nesnesi
                        using (Graphics g = Graphics.FromImage(bmp))
                        {
                            // Kaynak ekran DC'si
                            nint hdcSrc = GetWindowDC(GetDesktopWindow());
                            // Hedef grafik DC'si
                            nint hdcDest = g.GetHdc();

                            try
                            {
                                // BitBlt ile ekran görüntüsü alma
                                BitBlt(hdcDest, 0, 0, screenWidth, screenHeight, hdcSrc, 0, 0, SRCCOPY);
                            }
                            finally
                            {
                                // DC'leri serbest bırakma
                                g.ReleaseHdc(hdcDest);
                                ReleaseDC(GetDesktopWindow(), hdcSrc);
                            }
                        }

                        if (File.Exists(Paths.GeneralImagePath))
                        {
                            File.Delete(Paths.GeneralImagePath);
                        }
                        bmp.Save(Paths.GeneralImagePath, ImageFormat.Png);
                    }

                    return true;
                });
            }
            catch
            {
                return false;
            }
        }


        public static async Task<bool> CapturePartialScreen(Rectangle captureRectangle, ActiveWindow activeWindow)
        {

            return await Task.Run(() =>
            {
                try
                {

                    // Ekran görüntüsünü alacağımız bitmap nesnesi
                    if (captureRectangle == Rectangle.Empty)
                    {
                        throw new Exception("AH could not be detected.");
                    }

                    // Bitmap'i using bloğuna almayın, döndürülecek
                    Bitmap bitmap = new Bitmap(captureRectangle.Width, captureRectangle.Height);

                    // Grafik nesnesi oluşturuyoruz
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        nint hdcSrc = GetWindowDC(IntPtr.Zero); // Tüm ekranın DC'sini al
                        nint hdcDest = g.GetHdc(); // Hedef grafik DC'si

                        // Ekran görüntüsünü BitBlt ile al
                        BitBlt(hdcDest, 0, 0, captureRectangle.Width, captureRectangle.Height, hdcSrc, captureRectangle.X, captureRectangle.Y, SRCCOPY);

                        // DC'leri serbest bırak
                        g.ReleaseHdc(hdcDest);
                        ReleaseDC(IntPtr.Zero, hdcSrc);
                    }

                    switch (activeWindow)
                    {
                        case ActiveWindow.PostCancel:
                            bitmap.Save(Paths.PostCancelImagePath, ImageFormat.Png);
                            break;
                        case ActiveWindow.AHMenu:
                            bitmap.Save(Paths.AHMenuImagePath, ImageFormat.Png);
                            break;
                        case ActiveWindow.MailBox:
                            bitmap.Save(Paths.MailBoxImagePath, ImageFormat.Png);
                            break;
                        case ActiveWindow.General:
                            bitmap.Save(Paths.GeneralImagePath, ImageFormat.Png);
                            break;
                        case ActiveWindow.Chat:
                            bitmap.Save(Paths.ChatWindowImagePath, ImageFormat.Png);
                            break;
                        case ActiveWindow.MailboxText:
                            bitmap.Save(Paths.MailBoxTextImagePath, ImageFormat.Png);
                            break;
                        default:
                            break;
                    }
                    bitmap.Dispose();

                    return true;
                }
                catch
                {
                    return false;
                }
            });
        }

        public static async Task<bool> CapturePartialScreen(Rectangle captureRectangle)
        {

            return await Task.Run(() =>
            {
                try
                {

                    // Ekran görüntüsünü alacağımız bitmap nesnesi
                    //if (captureRectangle == Rectangle.Empty)
                    //{
                    //    throw new Exception("AH could not be detected.");
                    //}

                    // Bitmap'i using bloğuna almayın, döndürülecek
                    Bitmap bitmap = new Bitmap(captureRectangle.Width, captureRectangle.Height);

                    // Grafik nesnesi oluşturuyoruz
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        nint hdcSrc = GetWindowDC(IntPtr.Zero); // Tüm ekranın DC'sini al
                        nint hdcDest = g.GetHdc(); // Hedef grafik DC'si

                        // Ekran görüntüsünü BitBlt ile al
                        BitBlt(hdcDest, 0, 0, captureRectangle.Width, captureRectangle.Height, hdcSrc, captureRectangle.X, captureRectangle.Y, SRCCOPY);

                        // DC'leri serbest bırak
                        g.ReleaseHdc(hdcDest);
                        ReleaseDC(IntPtr.Zero, hdcSrc);
                    }

                    GrayscaleRMY grayScale = new GrayscaleRMY();
                    Bitmap blackAndWhiteImage = grayScale.Apply(bitmap);
                    BrightnessCorrection brightness = new BrightnessCorrection(50); // 50, parlaklık seviyesini artırır
                    brightness.ApplyInPlace(blackAndWhiteImage);
                    ContrastCorrection contrast = new ContrastCorrection(40); // 40, kontrast seviyesini artırır
                    contrast.ApplyInPlace(blackAndWhiteImage);
                    GammaCorrection gamma = new GammaCorrection(1.5); // 1.5 gamma değeri ile parlaklık artar
                    gamma.ApplyInPlace(blackAndWhiteImage);

                    blackAndWhiteImage.Save(Paths.MailBoxTextImagePath, ImageFormat.Png);

                    //bitmap.Save(Paths.MailBoxTextImagePath, ImageFormat.Png);
                    bitmap.Dispose();
                    blackAndWhiteImage.Dispose();

                    //ConvertImageToBlackAndWhite(Paths.MailBoxTextImagePath, "MailBoxText2");

                    return true;
                }
                catch
                {
                    return false;
                }
            });
        }

        public static int CalculateAbsolutePositionX(int positionX, int screenResX)
        {

            return (positionX * 65535) / screenResX;
        }


        public static int CalculateAbsolutePositionY(int positionY, int screenResY)
        {
            return (positionY * 65535) / screenResY;
        }

        static void ConvertImageToBlackAndWhite(string inputImagePath, string outputImagePath)
        {
            Bitmap image = new Bitmap(inputImagePath);
            GrayscaleRMY grayScale = new GrayscaleRMY();
            Bitmap blackAndWhiteImage = grayScale.Apply(image);
            Threshold threshold = new Threshold(60);

            threshold.ApplyInPlace(blackAndWhiteImage);


            blackAndWhiteImage.Save(outputImagePath, ImageFormat.Png);

            blackAndWhiteImage.Dispose();
            image.Dispose();

        }
    }
}
