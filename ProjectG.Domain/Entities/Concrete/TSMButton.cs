using ProjectG.DomainLayer.Entities.Abstract;
using ProjectG.DomainLayer.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectG.DomainLayer.Entities.Concrete
{
    public class TSMButton
    {

        public ButtonName Name { get; set; }
        public int AbsoluteX { get; set; }
        public int AbsoluteY { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public Color BackgroundColor { get; set; }
        public static Color BorderColorWhileActive { get; set; } = Color.FromArgb(18,18,18);
        public static Color BorderColorWhilePassive { get; set; } = Color.FromArgb(70, 0, 8);
        public bool Clickable { get; set; } = true;
        public Rectangle Border { get; set; }
        public ButtonContainer ButtonContainer { get; set; }
        public static ICollection<TSMButton> TSMButtons { get; set; } = new List<TSMButton>();

        


    }
}
