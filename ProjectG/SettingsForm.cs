using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProjectG.PresentationLayer
{
    public partial class SettingsForm : Form
    {
        PG _pg;
        public SettingsForm(PG pg)
        {
            _pg = pg;
            InitializeComponent();
        }

        private void btnDone_Click(object sender, EventArgs e)
        {
            Random random = new Random();

            // Yeni bir AppSettings nesnesi oluştur
            //var settings = new AppSettings
            //{
            //    ProfileName = "Profile" + random.Next(1, 1000), // Rastgele bir profil adı
            //    CycleDowntime = new int[] { random.Next(0, 60), random.Next(0, 60) }, // 0-60 saniye arası
            //    AHShowsUpDowntime = new int[] { random.Next(0, 60), random.Next(0, 60) },
            //    PostOrCancelDowntime = new int[] { random.Next(0, 60), random.Next(0, 60) },
            //    PostOrCancelDoneDowntime = new int[] { random.Next(0, 60), random.Next(0, 60) },
            //    AHCloseRandomize = random.Next(0, 2) == 1, // Rastgele true/false
            //    SendGameToTheBackground = random.Next(0, 2) == 1,
            //    SendGameToTheBackgroundAxis = new int[] { random.Next(0, 1920), random.Next(0, 1080) }, // 1920x1080 ekran çözünürlüğü
            //    MailBoxOpenRandomize = random.Next(0, 2) == 1,
            //    MailBoxRandomizedPossibility = random.Next(0, 100), // 0-100 arasında bir olasılık
            //    MailBoxShowsUpDowntime = new int[] { random.Next(0, 60), random.Next(0, 60) },
            //    MailBoxCloseRandomize = random.Next(0, 2) == 1
            //};

            //ConfigurationService configurationService = new ConfigurationService();
            //configurationService.SaveSettings(settings);
            //configurationService.GetKeys();

            this.Hide();
            _pg.ShowDialog();
        }
    }
}
