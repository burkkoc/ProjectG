using System.Globalization;
using ProjectG.ApplicationLayer.Services;

namespace ProjectG.PresentationLayer
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
        }

        void SettingsForm_Load(object? sender, EventArgs e)
        {
            var s = NtfySettingsStore.Load();
            txtNtfyNotifyUrl.Text = s.NtfyNotifyTopicUrl;
            txtMailboxLocateHotkey.Text = s.MailboxLocateHotkey;
            txtGuildBankLocateHotkey.Text = s.GuildBankLocateHotkey;
            txtRestockMaxNotificationCount.Text = s.RestockMaxNotificationCount.ToString();
            txtRestockThresholdPercent.Text = s.RestockThresholdPercent.ToString();
            txtGuildBankMinIntervalMinutes.Text = s.GuildBankMinIntervalMinutes.ToString(CultureInfo.InvariantCulture);
            txtGuildBankMaxIntervalMinutes.Text = s.GuildBankMaxIntervalMinutes.ToString(CultureInfo.InvariantCulture);
            txtGuildBankAfterNotifyDeltaSec.Text = s.GuildBankAfterNotifyMinSecondsShorterThanFirst.ToString(CultureInfo.InvariantCulture);
            txtCancelingLoadedExtraThresholdSeconds.Text = s.CancelingLoadedExtraThresholdSeconds.ToString();
            txtDynamicShortMinTMult.Text = s.DynamicShortAfterCancelMinTMultiplier.ToString(CultureInfo.InvariantCulture);
            txtDynamicShortMaxTMult.Text = s.DynamicShortAfterCancelMaxTMultiplier.ToString(CultureInfo.InvariantCulture);
            txtDynamicShortMaxExtraSec.Text = s.DynamicShortAfterCancelMaxExtraSeconds.ToString(CultureInfo.InvariantCulture);
        }

        void btnSave_Click(object? sender, EventArgs e)
        {
            var settings = NtfySettingsStore.Load();
            var url = txtNtfyNotifyUrl.Text.Trim();
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)
                || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                MessageBox.Show(
                    "Geçerli bir http(s) adresi girin (ör. https://ntfy.sh/konu-adiniz).",
                    "Project G",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            var path = uri.AbsolutePath.Trim('/');
            if (string.IsNullOrEmpty(path))
            {
                MessageBox.Show(
                    "Konu yolu gerekli (ör. https://ntfy.sh/benim-konum).",
                    "Project G",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            var normalized = uri.AbsoluteUri.TrimEnd('/');
            var mailboxKeyText = txtMailboxLocateHotkey.Text.Trim();
            if (string.IsNullOrWhiteSpace(mailboxKeyText) || !Enum.TryParse<Keys>(mailboxKeyText, true, out _))
            {
                MessageBox.Show(
                    "Geçerli bir tuş girin (ör. Z, F6, NumPad1).",
                    "Project G",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            var guildBankKeyText = txtGuildBankLocateHotkey.Text.Trim();
            if (string.IsNullOrWhiteSpace(guildBankKeyText) || !Enum.TryParse<Keys>(guildBankKeyText, true, out _))
            {
                MessageBox.Show(
                    "Guild Bank icin geçerli bir tuş girin (ör. X, F6, NumPad1).",
                    "Project G",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtRestockMaxNotificationCount.Text.Trim(), out var maxRestockNotificationCount) || maxRestockNotificationCount < 0)
            {
                MessageBox.Show(
                    "Restock Max Notification Count icin 0 veya daha buyuk bir tam sayi girin.",
                    "Project G",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtRestockThresholdPercent.Text.Trim(), out var restockThresholdPercent)
                || restockThresholdPercent < 0
                || restockThresholdPercent > 100)
            {
                MessageBox.Show(
                    "Restock Threshold (%) icin 0-100 arasinda bir tam sayi girin.",
                    "Project G",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (!double.TryParse(txtGuildBankMinIntervalMinutes.Text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var guildBankMinIntervalMin)
                || guildBankMinIntervalMin < 0.5
                || guildBankMinIntervalMin > 10080)
            {
                MessageBox.Show(
                    "Guild bank araligi (min dk) icin 0.5-10080 arasi sayi girin (orn. 2).",
                    "Project G",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (!double.TryParse(txtGuildBankMaxIntervalMinutes.Text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var guildBankMaxIntervalMin)
                || guildBankMaxIntervalMin < 0.5
                || guildBankMaxIntervalMin > 10080)
            {
                MessageBox.Show(
                    "Guild bank araligi (max dk) icin 0.5-10080 arasi sayi girin.",
                    "Project G",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (guildBankMaxIntervalMin < guildBankMinIntervalMin)
            {
                MessageBox.Show(
                    "Guild bank max dakika, min dakikadan kucuk olamaz.",
                    "Project G",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            var guildBankAfterNotifyText = txtGuildBankAfterNotifyDeltaSec.Text.Trim();
            int guildBankAfterNotifyDeltaSec;
            if (string.IsNullOrEmpty(guildBankAfterNotifyText))
                guildBankAfterNotifyDeltaSec = 0;
            else if (!int.TryParse(guildBankAfterNotifyText, out guildBankAfterNotifyDeltaSec)
                || guildBankAfterNotifyDeltaSec < 0
                || guildBankAfterNotifyDeltaSec > 86400)
            {
                MessageBox.Show(
                    "Bildirim sonrasi min. fark (sn) icin bos (0) veya 0-86400 arasi tam sayi girin.",
                    "Project G",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtCancelingLoadedExtraThresholdSeconds.Text.Trim(), out var cancelingLoadedExtraThresholdSeconds)
                || cancelingLoadedExtraThresholdSeconds < 0)
            {
                MessageBox.Show(
                    "CancelingLoaded Extra Threshold (sec) icin 0 veya daha buyuk bir tam sayi girin.",
                    "Project G",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (!double.TryParse(txtDynamicShortMinTMult.Text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var dsMinT)
                || dsMinT <= 0
                || dsMinT > 1000)
            {
                MessageBox.Show(
                    "Short dinamik bekleme: Min (T carpani) icin 0'dan buyuk, en fazla 1000 arasi sayi girin (orn. 2).",
                    "Project G",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (!double.TryParse(txtDynamicShortMaxTMult.Text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var dsMaxT)
                || dsMaxT <= 0
                || dsMaxT > 1000)
            {
                MessageBox.Show(
                    "Short dinamik bekleme: Max (T carpani) icin 0'dan buyuk, en fazla 1000 arasi sayi girin (orn. 2).",
                    "Project G",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (!double.TryParse(txtDynamicShortMaxExtraSec.Text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var dsExtra)
                || dsExtra < 0
                || dsExtra > 86_400)
            {
                MessageBox.Show(
                    "Short dinamik bekleme: Max ek saniye icin 0-86400 arasi sayi girin (orn. 10).",
                    "Project G",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            settings.NtfyNotifyTopicUrl = normalized;
            settings.MailboxLocateHotkey = mailboxKeyText.ToUpperInvariant();
            settings.GuildBankLocateHotkey = guildBankKeyText.ToUpperInvariant();
            settings.RestockMaxNotificationCount = maxRestockNotificationCount;
            settings.RestockThresholdPercent = restockThresholdPercent;
            settings.GuildBankMinIntervalMinutes = guildBankMinIntervalMin;
            settings.GuildBankMaxIntervalMinutes = guildBankMaxIntervalMin;
            settings.GuildBankAfterNotifyMinSecondsShorterThanFirst = guildBankAfterNotifyDeltaSec;
            settings.CancelingLoadedExtraThresholdSeconds = cancelingLoadedExtraThresholdSeconds;
            settings.DynamicShortAfterCancelMinTMultiplier = dsMinT;
            settings.DynamicShortAfterCancelMaxTMultiplier = dsMaxT;
            settings.DynamicShortAfterCancelMaxExtraSeconds = dsExtra;
            NtfySettingsStore.Save(settings);
            InternetConnectivityMonitor.ApplyNtfyUrls(normalized);
            DialogResult = DialogResult.OK;
            Close();
        }

        void btnCancel_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
