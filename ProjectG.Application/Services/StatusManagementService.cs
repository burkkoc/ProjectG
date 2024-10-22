using ProjectG.ApplicationLayer.Enums;
using ProjectG.DomainLayer.Entities.Concrete;
using ProjectG.InfrastructureLayer.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectG.ApplicationLayer.Services
{
    public class StatusManagementService
    {
        public static async Task<bool> IsCancelPostLoaded() //çalışıyor
        {
            if (Paths.PostCancelImagePath == string.Empty)
                Paths.PostCancelImagePath = UtilityService.GetDirectory(ActiveWindow.PostCancel);
            return await Task.Run(async () =>
               {
                   var timeout = TimeSpan.FromSeconds(20); // 20 saniyelik zaman aşımı süresi
                   var endTime = DateTime.UtcNow.Add(timeout); // 20 saniye sonra zaman

                   while (DateTime.UtcNow < endTime)
                   {
                       if (File.Exists(Paths.PostCancelImagePath))
                           File.Delete(Paths.PostCancelImagePath);

                       if(TSMWindow.AHBorder != null)
                            await ScreenService.CapturePartialScreen((Rectangle)TSMWindow.AHBorder, ActiveWindow.PostCancel);
                       var res = await TesseractService.IsTextExist(SearchTerms.PostCancelLoaded, Paths.PostCancelImagePath);
                       if (res)
                           return true;

                       await Task.Delay(500); // 0.5 saniye bekle
                   }

                   return false; // 20 saniye dolmadan doğru sonuç bulunamadı
               });
        }

        public static async Task<bool> IsAHWindowLoaded() //notsure
        {
            
            return await Task.Run(async () =>
            {
                int i = 0;
                var timeout = TimeSpan.FromSeconds(20); // 20 saniyelik zaman aşımı süresi
                var endTime = DateTime.UtcNow.Add(timeout); // 20 saniye sonra zaman

                while (DateTime.UtcNow < endTime)
                {
                    i++;
                    if (File.Exists(Paths.AHMenuImagePath))
                        File.Delete(Paths.AHMenuImagePath);

                    if(TSMWindow.AHBorder != null)
                        await ScreenService.CapturePartialScreen((Rectangle)TSMWindow.AHBorder, ActiveWindow.AHMenu);
                    var res = await TesseractService.IsTextExist(SearchTerms.AHMenuButtonsWords, Paths.AHMenuImagePath);
                    if (res)
                        return true;

                    await Task.Delay(500); // 0.5 saniye bekle
                }

                return false; // 20 saniye dolmadan doğru sonuç bulunamadı
            });

           
        }
    }
}
