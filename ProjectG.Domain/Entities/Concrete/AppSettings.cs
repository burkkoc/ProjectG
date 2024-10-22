using ProjectG.DomainLayer.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectG.DomainLayer.Entities.Concrete
{
    public static class AppSettings
    {
        public static string ProfileName { get; set; } = "Default";
        public static CycleDowntime CycleDowntime { get; set; } //Her döngü arası saniye tipinde beklenilecek süre

        public static int[] AHShowsUpDowntime { get; set; } = [1, 10]; //AH açıldıktan sonra, sıradaki işlemle arasındaki süre

        public static int[] PostOrCancelDowntime { get; set; } = [1, 3]; //Post ya da cancel ekranları tamamen yüklendikten sonra
                                                                  //sonraki işlemle arasındaki süre

        public static int[] PostOrCancelDoneDowntime { get; set; } = [1, 4]; //post ya da cancel işlemi bittikten sonra, sonraki işlemle
                                                                      //arasındaki süre
        public static bool AHCloseRandomize { get; set; } //AH'deki iş tamamlandıktan sonra mailbox'u açma ile arasında geçen süre
                                                   //ihtimaller:
                                                   //exit butonuna tıklayıp ah menüsünde bekler
                                                   //hiçbir şey yapmadan post'u bitirdiği noktada bekler
                                                   //ah'yi kapatıp bekler, sıradaki işlem mailbox'u açmak olmak zorundadır
        public static bool SendGameToTheBackground { get; set; } //oyunu arka plana alınmış gibi gösterir

        //if sendgametothebackground is true
        public static int[] SendGameToTheBackgroundAxis { get; set; } = [0, 0]; //taskbar'da boş bir nokta alınmalıdır


        public static bool MailBoxOpenRandomize { get; set; }

        //if mailboxopenrandomize is true
        public static int MailBoxRandomizedPossibility { get; set; } //sıradaki döngüden sonra mailbox'u açma ihtimali artışı


        public static int[] MailBoxShowsUpDowntime { get; set; } = [1, 4]; //Mailbox ekranı tespit edilmesiyle sıradaki olayın işleme alınması arasındaki süre

        //if mailboxrandomizedpossibility is true
        public static bool MailBoxCloseRandomize { get; set; }

        public static State State { get; set; } = State.OnCycleDowntime;

        public static List<State> States = new List<State>();

        public static Color TargetButtonColor { get;set; }

        public static Color? ActiveButtonColor { get; set; } = null;

        public static Rectangle? MailBoxPosition { get; set; }
        public static bool Working { get; set; } = false;

        public static int Downtime { get; set; } = 0;
        public static void Reset()
        {
            MailBoxPosition = null;
            ActiveButtonColor = null;
            State = State.OnCycleDowntime;

        }


    }
}
