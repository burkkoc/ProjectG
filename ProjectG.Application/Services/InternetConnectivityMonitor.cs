using ProjectG.DomainLayer.Entities.Concrete;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;

namespace ProjectG.ApplicationLayer.Services
{
    /// <summary>
    /// Uygulama açıkken yaklaşık saniyede bir internet erişilebilirliğini kontrol eder; sonuç <see cref="AppSettings.IsInternetReachable"/> alanına yazılır.
    /// Bir kez kesinti yaşanıp bağlantı geri geldiğinde tam ekran görüntüsü ntfy ile paylaşılır.
    /// ntfy uygulamasında yalnızca <see cref="NtfyNotifyTopicUrl"/> konusuna abone olun; <see cref="NtfyFileUploadTopicUrl"/> ayrı bildirim üretmesin diye dosya yükleme içindir (abone olmayın).
    /// </summary>
    [SupportedOSPlatform("windows")]
    public static class InternetConnectivityMonitor
    {
        const string DefaultNtfyNotifyTopicUrl = "https://ntfy.sh/bk-pc";

        static string _ntfyNotifyTopicUrl = DefaultNtfyNotifyTopicUrl;
        static string _ntfyFileUploadTopicUrl = DeriveFileUploadTopicUrl(DefaultNtfyNotifyTopicUrl);

        const string NtfyUploadFilename = "photo.png";

        /// <summary>Bildirimin gideceği konu URL’si (ör. https://ntfy.sh/mytopic).</summary>
        public static string NtfyNotifyTopicUrl => _ntfyNotifyTopicUrl;

        /// <summary>Yalnızca dosya yükleme; bu konuya abone olmayın (…/mytopic-upload).</summary>
        public static string NtfyFileUploadTopicUrl => _ntfyFileUploadTopicUrl;

        /// <summary>Kayıtlı veya varsayılan ntfy adreslerini uygular.</summary>
        public static void ApplyNtfyUrls(string? notifyTopicUrl)
        {
            var t = string.IsNullOrWhiteSpace(notifyTopicUrl) ? DefaultNtfyNotifyTopicUrl : notifyTopicUrl.Trim();
            try
            {
                var u = new Uri(t);
                if (u.Scheme != Uri.UriSchemeHttp && u.Scheme != Uri.UriSchemeHttps)
                    throw new InvalidOperationException();
                _ntfyNotifyTopicUrl = u.AbsoluteUri.TrimEnd('/');
                _ntfyFileUploadTopicUrl = DeriveFileUploadTopicUrl(_ntfyNotifyTopicUrl);
            }
            catch
            {
                _ntfyNotifyTopicUrl = DefaultNtfyNotifyTopicUrl;
                _ntfyFileUploadTopicUrl = DeriveFileUploadTopicUrl(_ntfyNotifyTopicUrl);
            }
        }

        static string DeriveFileUploadTopicUrl(string notifyUrl)
        {
            var u = new Uri(notifyUrl);
            var path = u.AbsolutePath.TrimEnd('/');
            if (path.Length == 0)
                throw new ArgumentException("Invalid ntfy topic path.");
            if (path.EndsWith("-upload", StringComparison.OrdinalIgnoreCase))
                return new UriBuilder(u) { Path = path }.Uri.AbsoluteUri.TrimEnd('/');
            var uploadPath = path + "-upload";
            return new UriBuilder(u) { Path = uploadPath }.Uri.AbsoluteUri.TrimEnd('/');
        }

        static readonly object Gate = new();
        static CancellationTokenSource? _cts;
        static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(4) };
        static readonly HttpClient NtfyUploadHttp = new() { Timeout = TimeSpan.FromMinutes(2) };

        /// <summary>En az bir kez offline görüldükten sonra tekrar online olunduğunda ekran görüntüsü gönderilir.</summary>
        static volatile bool _hadOfflineSinceLastNotify;

        [DllImport("user32.dll")]
        static extern int GetSystemMetrics(int nIndex);

        const int SM_XVIRTUALSCREEN = 76;
        const int SM_YVIRTUALSCREEN = 77;
        const int SM_CXVIRTUALSCREEN = 78;
        const int SM_CYVIRTUALSCREEN = 79;

        public static void Start()
        {
            lock (Gate)
            {
                if (_cts != null)
                    return;
                _cts = new CancellationTokenSource();
                _ = Task.Run(() => LoopAsync(_cts.Token));
            }
        }

        public static void Stop()
        {
            CancellationTokenSource? cts;
            lock (Gate)
            {
                cts = _cts;
                _cts = null;
            }
            if (cts == null)
                return;
            try { cts.Cancel(); } catch { /* ignore */ }
            cts.Dispose();
        }

        static async Task LoopAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                bool ok = false;
                try
                {
                    if (NetworkInterface.GetIsNetworkAvailable())
                    {
                        using var req = new HttpRequestMessage(HttpMethod.Head, "https://connectivitycheck.gstatic.com/generate_204");
                        using var resp = await Http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
                        ok = resp.IsSuccessStatusCode;
                    }
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    break;
                }
                catch
                {
                    ok = false;
                }

                if (!ok)
                    _hadOfflineSinceLastNotify = true;
                else if (_hadOfflineSinceLastNotify)
                {
                    _hadOfflineSinceLastNotify = false;
                    _ = Task.Run(() => CaptureScreenAndUploadToNtfyAsync(ct), CancellationToken.None);
                }

                AppSettings.IsInternetReachable = ok;

                try
                {
                    await Task.Delay(1000, ct).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    break;
                }
            }
        }

        static async Task CaptureScreenAndUploadToNtfyAsync(CancellationToken ct)
        {
            try
            {
                byte[] png = await Task.Run(
                    () =>
                    {
                        using var bmp = CaptureVirtualScreenBitmap();
                        using var ms = new MemoryStream();
                        bmp.Save(ms, ImageFormat.Png);
                        return ms.ToArray();
                    },
                    ct).ConfigureAwait(false);

                using var content = new ByteArrayContent(png);
                content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                using var putReq = new HttpRequestMessage(HttpMethod.Put, _ntfyFileUploadTopicUrl) { Content = content };
                putReq.Headers.TryAddWithoutValidation("Filename", NtfyUploadFilename);
                using var putResp = await NtfyUploadHttp.SendAsync(putReq, ct).ConfigureAwait(false);
                putResp.EnsureSuccessStatusCode();
                var json = await putResp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                if (!TryParseNtfyFileUploadResponse(json, out var fileUrl))
                    return;

                using var postReq = new HttpRequestMessage(HttpMethod.Post, _ntfyNotifyTopicUrl);
                postReq.Headers.TryAddWithoutValidation("Click", fileUrl);
                postReq.Headers.TryAddWithoutValidation("Title", BuildReconnectNotificationTitle());
                postReq.Content = new StringContent(
                    $"link: {fileUrl}",
                    Encoding.UTF8,
                    "text/plain");

                using var postResp = await NtfyUploadHttp.SendAsync(postReq, ct).ConfigureAwait(false);
                postResp.EnsureSuccessStatusCode();
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
            }
            catch
            {
                // Bildirim başarısız olsa bile monitör döngüsü çalışmaya devam eder.
            }
        }

        /// <summary>
        /// HttpClient istek başlıkları yalnızca ASCII kabul eder; Türkçe veya Unicode başlık bu hatayı verir.
        /// </summary>
        static string BuildReconnectNotificationTitle()
        {
            var machine = SanitizeAsciiHeaderValue(Environment.MachineName);
            var when = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            return $"Internet reconnected | {machine} | {when} local";
        }

        static string SanitizeAsciiHeaderValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "?";
            var sb = new StringBuilder(value.Length);
            foreach (char c in value)
                sb.Append(c <= 0x7f ? c : '?');
            return sb.ToString();
        }

        static bool TryParseNtfyFileUploadResponse(string json, [NotNullWhen(true)] out string? fileUrl)
        {
            fileUrl = null;
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (root.TryGetProperty("attachment", out var att) && att.ValueKind == JsonValueKind.Object
                    && att.TryGetProperty("url", out var urlEl) && urlEl.ValueKind == JsonValueKind.String)
                {
                    fileUrl = urlEl.GetString();
                    if (!string.IsNullOrEmpty(fileUrl))
                        return true;
                }

                if (!root.TryGetProperty("id", out var idEl) || idEl.ValueKind != JsonValueKind.String)
                    return false;
                var messageId = idEl.GetString();
                if (string.IsNullOrEmpty(messageId))
                    return false;

                var ext = Path.GetExtension(NtfyUploadFilename);
                if (string.IsNullOrEmpty(ext))
                    ext = ".png";
                ext = ext.TrimStart('.');
                var authority = new Uri(_ntfyFileUploadTopicUrl).GetLeftPart(UriPartial.Authority);
                fileUrl = $"{authority}/file/{messageId}.{ext}";
                return true;
            }
            catch
            {
                return false;
            }
        }

        static Bitmap CaptureVirtualScreenBitmap()
        {
            int x = GetSystemMetrics(SM_XVIRTUALSCREEN);
            int y = GetSystemMetrics(SM_YVIRTUALSCREEN);
            int w = GetSystemMetrics(SM_CXVIRTUALSCREEN);
            int h = GetSystemMetrics(SM_CYVIRTUALSCREEN);
            if (w <= 0 || h <= 0)
                w = h = 1;
            var bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(x, y, 0, 0, new Size(w, h), CopyPixelOperation.SourceCopy);
            }
            return bmp;
        }
    }
}
