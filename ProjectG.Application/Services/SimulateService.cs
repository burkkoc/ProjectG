using ProjectG.ApplicationLayer.Enums;
using ProjectG.DomainLayer.Entities.Concrete;
using ProjectG.DomainLayer.Entities.Enums;
using ProjectG.InfrastructureLayer.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsInput;
using WindowsInput.Native;

namespace ProjectG.ApplicationLayer.Services
{
    public class SimulateService
    {
        InputSimulator _inputSimulator = new InputSimulator();


        public async Task<bool> MouseMove(TSMButton tsmButton, int timeOutAsMilliseconds)
        {
            return await Task.Run(async () =>
            {
                _inputSimulator.Mouse.MoveMouseTo(tsmButton.AbsoluteX, tsmButton.AbsoluteY).Sleep(timeOutAsMilliseconds);

                return true;
            });
        }

        public async Task<bool> MouseMove(int timeOutAsMilliseconds, ButtonContainer buttonContainer)
        {
            return await Task.Run(() =>
            {
                int rnd1 = 0;
                int rnd2 = 0;
                Rectangle ahBorder = new Rectangle(0,0,0,0);
                Rectangle mailBorder = new Rectangle(0,0,0,0);
                switch (buttonContainer)
                {
                    case ButtonContainer.AH:
                        if (TSMWindow.AHBorder != null)
                        {
                            ahBorder = (Rectangle)TSMWindow.AHBorder;
                        }
                        do
                        {
                            rnd1 = UtilityService.GenerateRandom(100, UserInput.ScreenResolutionX);
                        } while (rnd1 > ahBorder.X && rnd1 < ahBorder.X + ahBorder.Width);
                        do
                        {
                            rnd2 = UtilityService.GenerateRandom(100, UserInput.ScreenResolutionY-100);
                        } while (rnd2 > ahBorder.Y && rnd2 < ahBorder.Y + ahBorder.Height);
                        break;
                    case ButtonContainer.MailBox:
                        if(TSMWindow.MailBoxBorder != null)
                            mailBorder = (Rectangle)TSMWindow.MailBoxBorder;
                        do
                        {
                            rnd1 = UtilityService.GenerateRandom(100, UserInput.ScreenResolutionX);
                        } while (rnd1 > mailBorder.X && rnd1 < mailBorder.X + mailBorder.Width);
                        do
                        {
                            rnd2 = UtilityService.GenerateRandom(100, UserInput.ScreenResolutionY-100);
                        } while (rnd2 > mailBorder.Y && rnd2 < mailBorder.Y + mailBorder.Height);
                        break;
                    default:
                        break;
                }


                int absoluteX = ScreenService.CalculateAbsolutePositionX(rnd1, UserInput.ScreenResolutionX);
                int absoluteY = ScreenService.CalculateAbsolutePositionY(rnd2, UserInput.ScreenResolutionY);

                _inputSimulator.Mouse.Sleep(timeOutAsMilliseconds).MoveMouseTo(absoluteX, absoluteY);

                return true;
            });
        }

        public async Task<bool> MouseClick(TSMButton tsmButton)
        {
            //switch (tsmButton.Name)
            //{
            //    case ButtonName.RunPostScan:
            //        break;
            //    case ButtonName.RunCancelScan:
            //        break;
            //    case ButtonName.OpenAllMails:
            //        break;
            //    case ButtonName.AllCancelled:
            //        break;
            //    case ButtonName.ExitScan:
            //        break;
            //    default:
            //        break;
            //}
            if (!await PixelProcessService.IsClickable(tsmButton))
                return false;
            await MouseMove(tsmButton, UtilityService.GenerateRandom(500, 1000));
            _inputSimulator.Mouse.Sleep(UtilityService.GenerateRandom(70, 100)).LeftButtonDown().Sleep(UtilityService.GenerateRandom(100,200)).LeftButtonUp().Sleep(500);
            await MouseMove(100, tsmButton.ButtonContainer);
            await Task.Delay(200);
            return true;

        }

        public async Task<bool> MouseClick(Rectangle rec)
        {
            bool result;
            int i = 0;
            do
            {
                int rnd1 = UtilityService.GenerateRandom(rec.X, rec.X + rec.Width);
                int rnd2 = UtilityService.GenerateRandom(rec.Y, rec.Y + rec.Height);
                int absX = ScreenService.CalculateAbsolutePositionX(rnd1, UserInput.ScreenResolutionX);
                int absY = ScreenService.CalculateAbsolutePositionY(rnd2, UserInput.ScreenResolutionY);

                _inputSimulator.Mouse.MoveMouseTo(absX, absY);
                await Task.Delay(2000);
                result = await ScreenService.CapturePartialScreen(new Rectangle(UserInput.ScreenResolutionX - 150, UserInput.ScreenResolutionY - 150, 150, 125));
                

                if (result)
                    result = await TesseractService.IsTextExist(SearchTerms.MailBoxWords, Paths.MailBoxTextImagePath);

                i++;
            }
            while (!result || i == 10);

            _inputSimulator.Mouse.Sleep(UtilityService.GenerateRandom(70, 100)).LeftButtonDown().Sleep(UtilityService.GenerateRandom(100,200)).LeftButtonUp().Sleep(500);
            if (result)
                return true;
            else return false;

        }

        public async Task<bool> SendMacroKey(VirtualKeyCode vkCode)
        {
            return await Task.Run(() =>
            {
                _inputSimulator.Keyboard.Sleep(UtilityService.GenerateRandom(50, 100)).KeyDown(vkCode).Sleep(UtilityService.GenerateRandom(50, 80)).KeyUp(vkCode);

                return true;
            });
        }




    }
}