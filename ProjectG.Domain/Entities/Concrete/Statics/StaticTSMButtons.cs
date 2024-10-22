using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectG.DomainLayer.Entities.Concrete.Statics
{
    public static class StaticTSMButtons
    {

        public static TSMButton? OpenAllMailButton { get; set; } = null;
        public static TSMButton? CancelledButton { get; set; } = null;
        public static TSMButton? RunPostScan { get; set; } = null;
        public static TSMButton? RunCancelScan { get; set; } = null;
        public static TSMButton? ExitButton { get; set; } = null;
        public static void Reset()
        {
            OpenAllMailButton = null;
            CancelledButton = null;
            RunPostScan = null;
            RunCancelScan = null;
            ExitButton = null;
        }
    }
}
