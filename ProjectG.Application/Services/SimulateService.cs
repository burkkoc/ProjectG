using ProjectG.ApplicationLayer.Enums;
using ProjectG.DomainLayer.Entities.Concrete;
using ProjectG.DomainLayer.Entities.Enums;
using ProjectG.InfrastructureLayer.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
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
                Rectangle ahBorder = new Rectangle(0, 0, 0, 0);
                Rectangle mailBorder = new Rectangle(0, 0, 0, 0);
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
                            rnd2 = UtilityService.GenerateRandom(100, UserInput.ScreenResolutionY - 100);
                        } while (rnd2 > ahBorder.Y && rnd2 < ahBorder.Y + ahBorder.Height);
                        break;
                    case ButtonContainer.MailBox:
                        if (TSMWindow.MailBoxBorder != null)
                            mailBorder = (Rectangle)TSMWindow.MailBoxBorder;
                        do
                        {
                            rnd1 = UtilityService.GenerateRandom(100, UserInput.ScreenResolutionX);
                        } while (rnd1 > mailBorder.X && rnd1 < mailBorder.X + mailBorder.Width);
                        do
                        {
                            rnd2 = UtilityService.GenerateRandom(100, UserInput.ScreenResolutionY - 100);
                        } while (rnd2 > mailBorder.Y && rnd2 < mailBorder.Y + mailBorder.Height);
                        break;
                    case ButtonContainer.Bank:
                        if (TSMWindow.BankBorder != null)
                            mailBorder = (Rectangle)TSMWindow.BankBorder;
                        do
                        {
                            rnd1 = UtilityService.GenerateRandom(100, UserInput.ScreenResolutionX);
                        } while (rnd1 > mailBorder.X && rnd1 < mailBorder.X + mailBorder.Width);
                        do
                        {
                            rnd2 = UtilityService.GenerateRandom(100, UserInput.ScreenResolutionY - 100);
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

        /// <summary>
        /// Dikdörtgen içinde rastgele bir noktaya gidilir; hareket öncesi <paramref name="minSleepMs"/>–<paramref name="maxSleepMs"/> arası gecikme uygulanır.
        /// </summary>
        public async Task<bool> MouseMove(Rectangle bounds, int minSleepMs, int maxSleepMs)
        {
            return await Task.Run(() =>
            {
                if (bounds.Width <= 0 || bounds.Height <= 0)
                    return false;

                int rnd1 = UtilityService.RandomClickCoordinateInRange(bounds.X, bounds.X + bounds.Width);
                int rnd2 = UtilityService.RandomClickCoordinateInRange(bounds.Y, bounds.Y + bounds.Height);
                int absoluteX = ScreenService.CalculateAbsolutePositionX(rnd1, UserInput.ScreenResolutionX);
                int absoluteY = ScreenService.CalculateAbsolutePositionY(rnd2, UserInput.ScreenResolutionY);
                int sleepMs = UtilityService.GenerateRandom(minSleepMs, maxSleepMs + 1);

                _inputSimulator.Mouse.Sleep(sleepMs).MoveMouseTo(absoluteX, absoluteY);
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
            _inputSimulator.Mouse.Sleep(UtilityService.GenerateRandom(70, 100)).LeftButtonDown().Sleep(UtilityService.GenerateRandom(100, 200)).LeftButtonUp().Sleep(UtilityService.GenerateRandom(400, 651));
            await MouseMove(100, tsmButton.ButtonContainer);
            await Task.Delay(UtilityService.GenerateRandom(120, 321));
            return true;

        }

        public Task<bool> MouseClick(Rectangle rec) =>
            MouseClickWithCornerReferenceAsync(rec, Paths.MailBoxCornerReferencePath);

        /// <summary>Mailbox sonrası guild bank konumu; sağ-alt köşe referansı guild bank ile kaydedilmiş olmalı.</summary>
        public Task<bool> MouseClickGuildBank(Rectangle rec) =>
            MouseClickWithCornerReferenceAsync(rec, Paths.GuildBankCornerReferencePath);

        async Task<bool> MouseClickWithCornerReferenceAsync(Rectangle rec, string cornerReferencePath)
        {
            async Task<bool> CompareCornerFullMatchAsync()
            {
                var cornerRegion = MailBoxCornerCalibration.GetBlackAnchoredCornerRegion();
                if (cornerRegion.Width <= 0 || cornerRegion.Height <= 0)
                    return false;
                return await PixelProcessService.ScreenRegionFullyMatchesSavedReferenceAsync(cornerRegion, cornerReferencePath);
            }

            int rnd1 = UtilityService.RandomClickCoordinateInRange(rec.X, rec.X + rec.Width);
            int rnd2 = UtilityService.RandomClickCoordinateInRange(rec.Y, rec.Y + rec.Height);
            int absX = ScreenService.CalculateAbsolutePositionX(rnd1, UserInput.ScreenResolutionX);
            int absY = ScreenService.CalculateAbsolutePositionY(rnd2, UserInput.ScreenResolutionY);
            _inputSimulator.Mouse.MoveMouseTo(absX, absY);
            await Task.Delay(UtilityService.GenerateRandom(1800, 2201));

            bool result = await CompareCornerFullMatchAsync();

            if (!result)
            {
                for (int w = 0; w < 10 && !result; w++)
                {
                    await MouseMove(rec, 90, 240);
                    await Task.Delay(UtilityService.GenerateRandom(380, 720));
                    result = await CompareCornerFullMatchAsync();
                }
            }

            if (!result)
            {
                bool guildBank = cornerReferencePath.IndexOf("GuildBank", StringComparison.OrdinalIgnoreCase) >= 0;
                await OptionalMacroNtfyNotifier.NotifyCornerReferenceFailureIfEnabledAsync(guildBank);
                AppSettings.State = State.OnCycleDowntime;
                return false;
            }

            _inputSimulator.Mouse.Sleep(UtilityService.GenerateRandom(70, 100)).LeftButtonDown().Sleep(UtilityService.GenerateRandom(100, 200)).LeftButtonUp().Sleep(UtilityService.GenerateRandom(400, 651));
            await Task.Delay(UtilityService.GenerateRandom(120, 321));
            return true;
        }

        public async Task<bool> SendMacroKey(VirtualKeyCode vkCode)
        {
            return await Task.Run(() =>
            {
                _inputSimulator.Keyboard.Sleep(UtilityService.GenerateRandom(50, 100)).KeyDown(vkCode).Sleep(UtilityService.GenerateRandom(50, 80)).KeyUp(vkCode);

                return true;
            });
        }

        /// <summary>
        /// Makro döngüsünde sık basılan tuş için değişken ön/son bekleme ve basılı tutma; nadiren kısa ikinci dokunuş.
        /// </summary>
        /// <param name="allowPossibleSecondTap">false: tek basış (ör. ESC ile pencere kapatma).</param>
        public async Task<bool> SendHumanizedMacroKey(VirtualKeyCode vkCode, bool allowPossibleSecondTap = true)
        {
            await Task.Delay(UtilityService.GenerateRandom(40, 221));

            int beforeDown = UtilityService.GenerateRandom(12, 91);
            int holdMs = UtilityService.GenerateRandom(28, 99);
            int afterUp = UtilityService.GenerateRandom(15, 66);
            await Task.Run(() =>
            {
                _inputSimulator.Keyboard.Sleep(beforeDown).KeyDown(vkCode).Sleep(holdMs).KeyUp(vkCode).Sleep(afterUp);
            });

            if (allowPossibleSecondTap && Random.Shared.Next(100) < 7)
            {
                await Task.Delay(UtilityService.GenerateRandom(50, 181));
                holdMs = UtilityService.GenerateRandom(18, 56);
                await Task.Run(() =>
                {
                    _inputSimulator.Keyboard.Sleep(UtilityService.GenerateRandom(8, 36)).KeyDown(vkCode).Sleep(holdMs).KeyUp(vkCode);
                });
            }

            await Task.Delay(UtilityService.GenerateRandom(70, 361));
            return true;
        }

        /// <summary>
        /// Post/Cancel yüklemede E ile sık tarama: tur aralığı kısa kalır; basılı tutma tek parmak “hızlı vuruş” gibi değişken ve insani (çok kısa makine hissi yok).
        /// </summary>
        public async Task<bool> SendRapidMacroKey(VirtualKeyCode vkCode)
        {
            int beforeDown = UtilityService.GenerateRandom(5, 26);
            int holdMs = Random.Shared.Next(100) < 82
                ? UtilityService.GenerateRandom(24, 58)
                : UtilityService.GenerateRandom(58, 95);
            int afterUp = UtilityService.GenerateRandom(6, 24);
            await Task.Run(() =>
            {
                _inputSimulator.Keyboard.Sleep(beforeDown).KeyDown(vkCode).Sleep(holdMs).KeyUp(vkCode).Sleep(afterUp);
            });
            await Task.Delay(UtilityService.GenerateRandom(8, 42));
            return true;
        }

        /// <summary>
        /// DualClient: önce iki WoW ana penceresi arasında doğrudan geçiş dener; yalnızca tek istemci veya Win32 başarısızsa Alt+Tab.
        /// </summary>
        public async Task<bool> SwitchWindow()
        {
            return await Task.Run(() =>
            {
                if (WowDualClientWindowSwitcher.TrySwitchBetweenWowWindows())
                {
                    Thread.Sleep(UtilityService.GenerateRandom(90, 201));
                    return true;
                }

                _inputSimulator.Keyboard.Sleep(UtilityService.GenerateRandom(50, 100)).KeyDown(VirtualKeyCode.LMENU).Sleep(UtilityService.GenerateRandom(50, 80)).KeyDown(VirtualKeyCode.TAB).Sleep(UtilityService.GenerateRandom(50, 80)).KeyUp(VirtualKeyCode.TAB).Sleep(UtilityService.GenerateRandom(50, 80)).KeyUp(VirtualKeyCode.LMENU).Sleep(UtilityService.GenerateRandom(50, 80));

                return true;
            });
        }


    }
}