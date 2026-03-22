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
        }

        void btnSave_Click(object? sender, EventArgs e)
        {
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
            var settings = new NtfyUserSettings { NtfyNotifyTopicUrl = normalized };
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
