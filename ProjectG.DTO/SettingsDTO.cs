using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectG.DTO
{
    public class SettingsDTO
    {
        public string ProfileName { get; set; } = "Default";
        public int[] CycleDowntime { get; set; } = [150,300]; //Her döngü arası saniye tipinde beklenilecek süre

        public int[] AHShowsUpDowntime { get; set; } = [1,10]; //AH açıldıktan sonra, sıradaki işlemle arasındaki süre

        public int[] PostOrCancelDowntime { get; set; } = [1,3]; //Post ya da cancel ekranları tamamen yüklendikten sonra
                                                                 //sonraki işlemle arasındaki süre

        public int[] PostOrCancelDoneDowntime { get; set; } = [1,4]; //post ya da cancel işlemi bittikten sonra, sonraki işlemle
                                                                     //arasındaki süre
        public bool AHCloseRandomize { get; set; } //AH'deki iş tamamlandıktan sonra mailbox'u açma ile arasında geçen süre
                                                   //ihtimaller:
                                                   //exit butonuna tıklayıp ah menüsünde bekler
                                                   //hiçbir şey yapmadan post'u bitirdiği noktada bekler
                                                   //ah'yi kapatıp bekler, sıradaki işlem mailbox'u açmak olmak zorundadır
        public bool SendGameToTheBackground { get; set; } //oyunu arka plana alınmış gibi gösterir

        //if sendgametothebackground is true
        public int[] SendGameToTheBackgroundAxis { get; set; } = [0, 0]; //taskbar'da boş bir nokta alınmalıdır
 

        public bool MailBoxOpenRandomize { get; set; }

        //if mailboxopenrandomize is true
        public int MailBoxRandomizedPossibility { get; set; } //sıradaki döngüden sonra mailbox'u açma ihtimali artışı


        public int[] MailBoxShowsUpDowntime { get; set; } = [1,4]; //Mailbox ekranı tespit edilmesiyle sıradaki olayın işleme alınması arasındaki süre

        //if mailboxrandomizedpossibility is true
        public bool MailBoxCloseRandomize { get; set; }






    }
}
