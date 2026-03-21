using System.Drawing;
using ProjectG.DomainLayer.Entities.Concrete;
using ProjectG.InfrastructureLayer.Services;

namespace ProjectG.ApplicationLayer.Services
{
    public static class MailBoxCornerCalibration
    {
        public const int RegionSize = 200;

        /// <summary>
        /// Sağ alt köşe pikselinden başlayarak aynı sütunda yukarı çıkar; RGB #000 ile karşılaşılan noktayı
        /// <see cref="RegionSize"/>x<see cref="RegionSize"/> dikdörtgenin sağ-alt köşesi yapar (ekran sınırlarıyla kırpılır).
        /// Sütunda hiç #000 yoksa köşe (w-1, h-1) kullanılır.
        /// </summary>
        public static Rectangle GetBlackAnchoredCornerRegion()
        {
            int w = UserInput.ScreenResolutionX;
            int h = UserInput.ScreenResolutionY;
            if (w <= 0 || h <= 0)
                return Rectangle.Empty;

            int columnX = w - 1;
            int anchorY = ScreenService.FindFirstPureBlackPixelYFromBottom(columnX, w, h);
            if (anchorY < 0)
                anchorY = h - 1;

            return BuildBlackAnchoredRegion(columnX, anchorY, RegionSize, w, h);
        }

        static Rectangle BuildBlackAnchoredRegion(int anchorX, int anchorY, int size, int screenW, int screenH)
        {
            int left = anchorX - size + 1;
            int top = anchorY - size + 1;
            var desired = new Rectangle(left, top, size, size);
            return Rectangle.Intersect(desired, new Rectangle(0, 0, screenW, screenH));
        }

        public static Task<bool> SaveReferenceSnapshotAsync()
        {
            var region = GetBlackAnchoredCornerRegion();
            if (region.Width <= 0 || region.Height <= 0)
                return Task.FromResult(false);

            return ScreenService.CaptureRegionToFile(region, Paths.MailBoxCornerReferencePath);
        }
    }
}
