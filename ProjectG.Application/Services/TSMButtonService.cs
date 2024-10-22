using ProjectG.DomainLayer.Entities.Abstract;
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
    public static class TSMButtonService
    {
        static Rectangle finalRectangle;
        public static TSMButton CreateTSMButton(ButtonName name, Rectangle border, Rectangle tsmWindowBorder, ButtonContainer buttonContainer)
        {
            finalRectangle = new Rectangle(border.X + tsmWindowBorder.X, border.Y + tsmWindowBorder.Y, border.Width, border.Height);
            TSMButton tsmButton = new TSMButton
            {
                Name = name,
                Border = finalRectangle,
                X = finalRectangle.X,
                Y = finalRectangle.Y,
                AbsoluteX = ScreenService.CalculateAbsolutePositionX(UtilityService.GenerateRandom(finalRectangle.X, finalRectangle.X + finalRectangle.Width), UserInput.ScreenResolutionX),
                AbsoluteY = ScreenService.CalculateAbsolutePositionY(UtilityService.GenerateRandom(finalRectangle.Y, finalRectangle.Y + finalRectangle.Height), UserInput.ScreenResolutionY),
                Clickable = true,
                ButtonContainer = buttonContainer,
                BackgroundColor = PixelProcessService.GetBackgroundColor(finalRectangle.X, finalRectangle.Y)
            };

            if (tsmButton.Name == ButtonName.OpenAllMails && StaticTSMButtons.RunCancelScan != null)
                tsmButton.BackgroundColor = StaticTSMButtons.RunCancelScan.BackgroundColor;
            return tsmButton;
        }

        //public static TSMButton UpdateTSMButton(TSMButton tsmButton)
        //{
        //    tsmButton.AbsoluteX = ScreenService.CalculateAbsolutePositionX(UtilityService.GenerateRandom(tsmButton.Border.X, tsmButton.Border.X + tsmButton.Border.Width), UserInput.ScreenResolutionX);
        //    tsmButton.AbsoluteY = ScreenService.CalculateAbsolutePositionY(UtilityService.GenerateRandom(tsmButton.Border.Y, tsmButton.Border.Y + tsmButton.Border.Height), UserInput.ScreenResolutionY);

        //    return tsmButton;
        //}


    }
}
