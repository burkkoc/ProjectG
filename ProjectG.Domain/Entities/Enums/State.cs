using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectG.DomainLayer.Entities.Enums
{
    public enum State
    {
        WaitingForAHWindow,
        AHWindowFound,
        WaitingForDetectAHButtons,
        AHButtonsFound,
        WaitingForLoadingPosting,
        PostingLoaded,
        CancelingLoaded,
        PostingDone,
        CancelingDone,
        WaitingForLoadingCanceling,
        WaitingForDetectCancel,
        WaitingforDetectSkip,
        WaitingForExitScan,
        WaitingForMailBoxWindow,
        WaitingForDetectMailBoxButtons,
        MailBoxButtonsFound,
        OpenAllMailButtonClicked,
        AllCancelledButtonClicked,
        WaitingForGettingItemsFromMailBox,
        ExitButtonClicked,
        RunPostButtonClicked,
        RunCancelButtonClicked,
        TargetKeyClicked,
        InteractKeyClicked,
        CloseWindowKeyClicked,
        OnCycleDowntime,
        TSMKeyClicked,
        Stopped,
        WindowNotFound


    }
}
