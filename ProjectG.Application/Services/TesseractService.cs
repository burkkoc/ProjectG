using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tesseract;
using System.Windows.Forms;
using ProjectG.DomainLayer.Entities.Concrete;
using ProjectG.InfrastructureLayer.Services;

namespace ProjectG.ApplicationLayer.Services
{
    public class TesseractService
    {
        public static Dictionary<string, Rectangle> FindTextLocation(string[] searchTexts, string imagePath)
        {
            //int i = 0;
            Bitmap bmp = new Bitmap(imagePath);
            Dictionary<string, Rectangle> foundedButtons = new Dictionary<string, Rectangle>();
            using (var engine = new TesseractEngine(Paths.TesseractPath, "eng", EngineMode.Default))
            {
                using (var image = Pix.LoadFromFile(imagePath))
                {

                    using (var page = engine.Process(image))
                    {
                        int x1 = 0;
                        int y1 = 0;
                        int x2 = 0;
                        int y2 = 0;
                        using (var iterator = page.GetIterator())
                        {
                            iterator.Begin();
                            do
                            {
                                string text = iterator.GetText(PageIteratorLevel.Word);

                                foreach (string individualText in searchTexts)
                                {
                                    if (text != null && text.Contains(individualText, StringComparison.OrdinalIgnoreCase))
                                    {
                                        Rect boundingBox;
                                        if (iterator.TryGetBoundingBox(PageIteratorLevel.Word, out boundingBox))
                                        {
                                            x1 = boundingBox.X1;
                                            y1 = boundingBox.Y1;
                                            x2 = boundingBox.X2;
                                            y2 = boundingBox.Y2;
                                            AppSettings.TargetButtonColor = bmp.GetPixel(x1 - 2, y1 - 2);
                                            int centerPointOfX = (x1 + x2) / 2;
                                            int centerPointOfY = (y1 + y2) / 2;
                                            Rectangle rec;
                                            //if (individualText == "Exit")
                                            //    rec = PixelProcessService.GetBordersOfAButton(centerPointOfX, centerPointOfY, bmp, true);
                                            //else
                                            rec = PixelProcessService.GetBordersOfAButton(centerPointOfX, centerPointOfY, x1 - 2, y1 - 2, bmp);

                                            if (!foundedButtons.ContainsKey(individualText) /*&& bmp.GetPixel(rec.X, rec.Y) == Color.FromArgb(70, 0, 8) || bmp.GetPixel(rec.X, rec.Y) == Color.FromArgb(18,18,18)*/)
                                            {
                                                foundedButtons.Add(individualText, rec);
                                                using (Graphics g = Graphics.FromImage(bmp))
                                                {
                                                    using (Pen pen = new(Color.Green, 2))
                                                    {
                                                        g.DrawRectangle(pen, rec);
                                                    }
                                                }
                                                bmp.Save(Paths.AHMenuImagePath + "65.png");

                                            }


                                        }
                                    }
                                }

                            } while (iterator.Next(PageIteratorLevel.Word));
                            bmp.Dispose();
                            return foundedButtons;
                        }
                    }

                }
            }
        }

        public static async Task<bool> IsTextExist(string[] searchingWords, string imagePath)
        {
            return await Task.Run(async () =>
            {
                try
                {

                    string text = string.Empty;
                    if (TSMWindow.AHBorder != null && imagePath != Paths.ChatWindowImagePath)
                        await ScreenService.CapturePartialScreen((Rectangle)TSMWindow.AHBorder, Enums.ActiveWindow.PostCancel);
                    else if (TSMWindow.ChatWindow != null && imagePath == Paths.ChatWindowImagePath)
                        await ScreenService.CapturePartialScreen((Rectangle)TSMWindow.ChatWindow, Enums.ActiveWindow.Chat);

                    using (var engine = new TesseractEngine(Paths.TesseractPath, "eng", EngineMode.Default))
                    {
                        using (var pixImg = Pix.LoadFromFile(imagePath))
                        {
                            for (int i = 0; i < 5; i++)
                            {
                                using (var page = engine.Process(pixImg))
                                {
                                    using (var iterator = page.GetIterator())
                                    {
                                        iterator.Begin();
                                        do
                                        {
                                            string foundedText = iterator.GetText(PageIteratorLevel.Word);
                                            if (foundedText != null)
                                            {
                                                text += foundedText;
                                                foreach (var searchingWord in searchingWords)
                                                {
                                                    if (foundedText.Contains(searchingWord, StringComparison.OrdinalIgnoreCase))
                                                    {
                                                        return true;
                                                    }
                                                }
                                            }
                                        } while (iterator.Next(PageIteratorLevel.Word));

                                        if (imagePath == Paths.PostCancelImagePath || imagePath == Paths.ChatWindowImagePath)
                                        {
                                            return false;

                                        }

                                    }
                                }
                            }
                        }
                        return false;
                    }
                }
                catch
                {
                    return false;
                }
                //}
            });

        }


    }
}