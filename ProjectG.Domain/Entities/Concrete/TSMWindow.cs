using ProjectG.DomainLayer.Entities.Abstract;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectG.DomainLayer.Entities.Concrete
{
    public static class TSMWindow
    {
        public static Rectangle? AHBorder { get; set; } = null;
        public static Color BorderColor { get; set; } = Color.FromArgb(0, 0, 0);
        public static Rectangle? MailBoxBorder { get; set; } = null;

        public static Rectangle? ChatWindow {  get; set; } = null;

        public static Rectangle? MailboxTextWindow { get; set; } = null;

        public static void Reset()
        {
            AHBorder = null;
            BorderColor = Color.FromArgb(0, 0, 0);
            MailBoxBorder = null;
            ChatWindow = null;
            MailboxTextWindow = null;
        }
    }
}
