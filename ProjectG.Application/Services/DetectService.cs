using ProjectG.DomainLayer.Entities.Concrete;
using ProjectG.DomainLayer.Entities.Concrete.Statics;
using ProjectG.DomainLayer.Entities.Enums;
using ProjectG.InfrastructureLayer.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectG.ApplicationLayer.Services
{
    public class DetectService
    {



        public static async Task<bool> DetectAHMainMenu()
        {
            return await Task.Run(async () =>
            {
                var timeout = TimeSpan.FromSeconds(20); // 20 saniyelik zaman aşımı süresi
                var endTime = DateTime.UtcNow.Add(timeout); // 20 saniye sonra zaman
                var endTime2 = DateTime.UtcNow.Add(TimeSpan.FromSeconds(5)); // 20 saniye sonra zaman

                while (DateTime.UtcNow < endTime)
                {
                    while (DateTime.UtcNow < endTime2)
                    {
                        if (TSMWindow.AHBorder == null)
                        {
                            await Task.Delay(500);
                            TSMWindow.AHBorder = await PixelProcessService.FindTSMWindow(Enums.ActiveWindow.AHMenu);
                        }
                        else break;
                    }
                    if(TSMWindow.AHBorder != null) 
                        await ScreenService.CapturePartialScreen((Rectangle)TSMWindow.AHBorder, Enums.ActiveWindow.AHMenu);


                    List<string> responses = new List<string>();

                    
                    var result = TesseractService.FindTextLocation(SearchTerms.AHMenuButtonsWords, Paths.AHMenuImagePath);

                    foreach (var keyword in result)
                    {
                        switch (keyword.Key)
                        {
                            case "Run":
                                if (StaticTSMButtons.RunPostScan == null)
                                {
                                    if (TSMWindow.AHBorder != null)
                                        StaticTSMButtons.RunPostScan = TSMButtonService.CreateTSMButton(ButtonName.RunPostScan, keyword.Value, (Rectangle)TSMWindow.AHBorder, ButtonContainer.AH);

                                    TSMButton.TSMButtons.Add(StaticTSMButtons.RunPostScan ?? throw new ArgumentNullException());
                                    responses.Add(Response.AHMainMenuPostButtonFound);
                                }
                                break;
                            case "Cancel":
                                if (StaticTSMButtons.RunCancelScan == null)
                                {
                                    if (TSMWindow.AHBorder != null)
                                        StaticTSMButtons.RunCancelScan = TSMButtonService.CreateTSMButton(ButtonName.RunCancelScan, keyword.Value, (Rectangle)TSMWindow.AHBorder, ButtonContainer.AH);
                                    TSMButton.TSMButtons.Add(StaticTSMButtons.RunCancelScan ?? throw new ArgumentNullException());
                                    responses.Add(Response.AHMainMenuCancelButtonFound);
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    if (responses.Any(r => r.Contains("Run Post Scan")) && responses.Any(r => r.Contains("Run Cancel Scan")) && responses.Count == 2)
                        return true;

                    responses.Clear();


                    await Task.Delay(500); // 0.5 saniye bekle

                }

                return false; // 20 saniye dolmadan doğru sonuç bulunamadı
            });

        }

        public static async Task<bool> DetectPostCancelButtons()
        {
            return await Task.Run(async () =>
            {
                var endTime = DateTime.UtcNow.Add(TimeSpan.FromSeconds(5)); // 5 saniye sonra zaman
                TSMWindow.AHBorder = await PixelProcessService.FindTSMWindow(Enums.ActiveWindow.PostCancel);

                List<string> responses = new List<string>();

                while (DateTime.UtcNow < endTime)
                {
                    await ScreenService.CapturePartialScreen((Rectangle)TSMWindow.AHBorder, Enums.ActiveWindow.PostCancel);

                    var result = TesseractService.FindTextLocation(SearchTerms.ExitScanWords, Paths.PostCancelImagePath);

                    foreach (var keyword in result)
                    {
                        switch (keyword.Key)
                        {
                            case "Exit" or "exit":
                                if (StaticTSMButtons.ExitButton == null)
                                {
                                    TSMButton exitButton = TSMButtonService.CreateTSMButton(ButtonName.ExitScan, keyword.Value, (Rectangle)TSMWindow.AHBorder, ButtonContainer.AH);
                                    StaticTSMButtons.ExitButton = exitButton;
                                    TSMButton.TSMButtons.Add(StaticTSMButtons.ExitButton);
                                    responses.Add("Exit");
                                }
                                break;
                            default:
                                break;
                        }
                    }

                    if (responses.Any(r => r.Contains("Exit"))) //exit bulundu
                        return true;

                    await Task.Delay(1000); // 1 saniye bekle
                }

                return false; // 5 saniye dolmadan doğru sonuç bulunamadı
            });
        }

        public static async Task<bool> DetectMailBoxMenu()
        {
            return await Task.Run(async () =>
            {

                var timeout = TimeSpan.FromSeconds(20); // 20 saniyelik zaman aşımı süresi
                var endTime = DateTime.UtcNow.Add(timeout); // 20 saniye sonra zaman
                var endTime2 = DateTime.UtcNow.Add(TimeSpan.FromSeconds(5)); // 20 saniye sonra zaman

                while (DateTime.UtcNow < endTime)
                {
                    while (DateTime.UtcNow < endTime2)
                    {
                        if (TSMWindow.MailBoxBorder == null)
                        {
                            TSMWindow.MailBoxBorder = await PixelProcessService.FindTSMWindow(Enums.ActiveWindow.MailBox);
                            await Task.Delay(500);
                        }
                        else break;
                    }


                    var result = TesseractService.FindTextLocation(SearchTerms.MailBoxButtonWords, Paths.MailBoxImagePath);

                    foreach (var keyword in result)
                    {
                        switch (keyword.Key)
                        {
                            case "Open":
                                if (TSMWindow.MailBoxBorder != null)
                                {

                                    StaticTSMButtons.OpenAllMailButton = TSMButtonService.CreateTSMButton(ButtonName.OpenAllMails, keyword.Value, (Rectangle)TSMWindow.MailBoxBorder, ButtonContainer.MailBox);
                                    if(StaticTSMButtons.OpenAllMailButton != null)
                                        TSMButton.TSMButtons.Add(StaticTSMButtons.OpenAllMailButton);
                                }
                                return true;

                            default:
                                break;
                        }
                    }

                }
                    return false;
            });
        }
    }
}
