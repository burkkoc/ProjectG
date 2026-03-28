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
            txtRestockMaxNotificationCount.Text = s.RestockMaxNotificationCount.ToString();
            txtRestockThresholdPercent.Text = s.RestockThresholdPercent.ToString();
            txtCancelingLoadedExtraThresholdSeconds.Text = s.CancelingLoadedExtraThresholdSeconds.ToString();
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

            settings.NtfyNotifyTopicUrl = normalized;
            settings.MailboxLocateHotkey = mailboxKeyText.ToUpperInvariant();
            settings.RestockMaxNotificationCount = maxRestockNotificationCount;
            settings.RestockThresholdPercent = restockThresholdPercent;
            settings.CancelingLoadedExtraThresholdSeconds = cancelingLoadedExtraThresholdSeconds;
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
