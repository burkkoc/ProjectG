using System.Drawing;

namespace ProjectG.DomainLayer.Entities.Concrete
{
    /// <summary>
    /// Tek bir WoW oturumu için TSM sınırları, butonlar ve kalibrasyon; dual client sırasında slot olarak saklanır.
    /// </summary>
    public sealed class DualClientLayoutSnapshot
    {
        public Rectangle? AHBorder { get; set; }
        public Color BorderColor { get; set; } = Color.FromArgb(0, 0, 0);
        public Rectangle? MailBoxBorder { get; set; }
        public Rectangle? BankBorder { get; set; }
        public Rectangle? ChatWindow { get; set; }
        public Rectangle? MailboxTextWindow { get; set; }

        public TSMButton? OpenAllMailButton { get; set; }
        public TSMButton? CancelledButton { get; set; }
        public TSMButton? RunPostScan { get; set; }
        public TSMButton? RunCancelScan { get; set; }
        public TSMButton? ExitButton { get; set; }
        public TSMButton? RestockButton { get; set; }

        /// <summary><see cref="TSMButton.TSMButtons"/> için o oturumdaki liste kopyası.</summary>
        public List<TSMButton> TsmButtonsList { get; set; } = new();

        public Rectangle? MailBoxPosition { get; set; }
        public Rectangle? GuildBankPosition { get; set; }

        public int ScreenResolutionX { get; set; }
        public int ScreenResolutionY { get; set; }
    }
}
