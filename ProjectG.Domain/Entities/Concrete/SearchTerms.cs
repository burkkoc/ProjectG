using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectG.DomainLayer.Entities.Concrete
{
    public static class SearchTerms
    {
        public static string[] PostCancelLoaded { get; set; } = ["Posting", "Canceling"];
        public static string[] PostCancelFinished { get; set; } = ["Done", "Rescan"];

        public static string[] AHMenuButtonsWords { get; set; } = ["Run", "Cancel"];

        public static string[] MailBoxButtonWords = [/*"Cancelled",*/ "Open"];

        public static string[] ExitScanWords { get; set; } = ["Exit"];
        public static string[] ChatWords { get; set; } = ["Retrying", "failed"];
        public static string[] MailBoxWords { get; set; } = ["Mailbox"];







    }
}
