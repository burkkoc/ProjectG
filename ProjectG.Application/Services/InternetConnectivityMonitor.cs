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
using System.Diagnostics;

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

        const string NtfyUploadFilename = "photo.jpg";
        const int NtfyMaxUploadBytes = 900_000;
        static readonly TimeSpan SendRetryBackoff = TimeSpan.FromSeconds(20);

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
        static readonly TimeSpan OfflineScreenshotDelay = TimeSpan.FromSeconds(10);
        static readonly TimeSpan MinOfflineDurationForNtfy = TimeSpan.FromSeconds(2);

        /// <summary>En az bir kez offline görüldükten sonra tekrar online olunduğunda ekran görüntüsü gönderilir.</summary>
        static volatile bool _hadOfflineSinceLastNotify;
        static DateTime _offlineDetectedUtc;
        static byte[]? _pendingOfflineScreenshotPng;
        static bool _isCapturingOfflineScreenshot;
        static bool _isSendingOfflineNotification;
        static DateTime _nextSendAttemptUtc;
        static bool _offlineQualifiedForNtfy;

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
                LogNtfyDebug("monitor", "start requested");
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
            bool? lastOk = null;
            LogNtfyDebug("monitor", "loop started");
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

                if (lastOk != ok)
                {
                    LogNtfyDebug("monitor", ok ? "state online" : "state offline");
                    lastOk = ok;
                }

                if (!ok)
                {
                    if (!_hadOfflineSinceLastNotify)
                    {
                        _offlineDetectedUtc = DateTime.UtcNow;
                        _offlineQualifiedForNtfy = false;
                        _nextSendAttemptUtc = DateTime.MinValue;
                        LogNtfyDebug("monitor", "offline detected; 10s timer started");
                    }
                    _hadOfflineSinceLastNotify = true;

                    if (!_offlineQualifiedForNtfy && DateTime.UtcNow - _offlineDetectedUtc > MinOfflineDurationForNtfy)
                    {
                        _offlineQualifiedForNtfy = true;
                        LogNtfyDebug("monitor", "offline duration qualified for ntfy (>2s)");
                    }
                }
                else if (_hadOfflineSinceLastNotify && !_offlineQualifiedForNtfy)
                {
                    var outage = DateTime.UtcNow - _offlineDetectedUtc;
                    LogNtfyDebug("monitor", $"offline ignored (<=2s), duration={outage.TotalSeconds:F2}s");
                    _hadOfflineSinceLastNotify = false;
                    _pendingOfflineScreenshotPng = null;
                    _isCapturingOfflineScreenshot = false;
                    _isSendingOfflineNotification = false;
                    _nextSendAttemptUtc = DateTime.MinValue;
                }
                else if (_hadOfflineSinceLastNotify
                    && _pendingOfflineScreenshotPng is not null
                    && !_isSendingOfflineNotification
                    && DateTime.UtcNow >= _nextSendAttemptUtc)
                {
                    _isSendingOfflineNotification = true;
                    var screenshot = _pendingOfflineScreenshotPng;
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            if (screenshot is not null)
                            {
                                bool sent = await SendScreenshotToNtfyAsync(screenshot, ct).ConfigureAwait(false);
                                if (sent)
                                {
                                    _hadOfflineSinceLastNotify = false;
                                    _pendingOfflineScreenshotPng = null;
                                    _nextSendAttemptUtc = DateTime.MinValue;
                                }
                                else
                                {
                                    _nextSendAttemptUtc = DateTime.UtcNow + SendRetryBackoff;
                                    LogNtfyDebug("monitor", $"send failed; next retry at {_nextSendAttemptUtc:HH:mm:ss}");
                                }
                            }
                        }
                        finally
                        {
                            _isSendingOfflineNotification = false;
                        }
                    }, CancellationToken.None);
                }

                if (_hadOfflineSinceLastNotify
                    && _offlineQualifiedForNtfy
                    && _pendingOfflineScreenshotPng is null
                    && !_isCapturingOfflineScreenshot
                    && DateTime.UtcNow - _offlineDetectedUtc >= OfflineScreenshotDelay)
                {
                    _isCapturingOfflineScreenshot = true;
                    LogNtfyDebug("monitor", "offline screenshot capture started");
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            _pendingOfflineScreenshotPng = await CaptureScreenPayloadAsync(ct).ConfigureAwait(false);
                            LogNtfyDebug("ntfy-send-flow", $"offline-screenshot-captured bytes={_pendingOfflineScreenshotPng.Length}");
                        }
                        catch (OperationCanceledException) when (ct.IsCancellationRequested)
                        {
                        }
                        catch (Exception ex)
                        {
                            LogNtfyDebug("ntfy-send-flow", $"offline-screenshot-exception {ex.GetType().Name}: {ex.Message}");
                        }
                        finally
                        {
                            _isCapturingOfflineScreenshot = false;
                        }
                    }, CancellationToken.None);
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

        static async Task<byte[]> CaptureScreenPayloadAsync(CancellationToken ct)
        {
            return await Task.Run(
                () =>
                {
                    using var bmp = CaptureVirtualScreenBitmap();
                    return EncodeScreenshotForNtfy(bmp);
                },
                ct).ConfigureAwait(false);
        }

        static async Task<bool> SendScreenshotToNtfyAsync(byte[] png, CancellationToken ct)
        {
            const string flow = "ntfy-send-flow";
            try
            {
                LogNtfyDebug(flow, $"start notify='{_ntfyNotifyTopicUrl}' upload='{_ntfyFileUploadTopicUrl}'");
                LogNtfyDebug(flow, $"capture-ok bytes={png.Length}");

                using var content = new ByteArrayContent(png);
                content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                using var putReq = new HttpRequestMessage(HttpMethod.Put, _ntfyFileUploadTopicUrl) { Content = content };
                putReq.Headers.TryAddWithoutValidation("Filename", NtfyUploadFilename);
                using var putResp = await NtfyUploadHttp.SendAsync(putReq, ct).ConfigureAwait(false);
                var json = await putResp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                LogNtfyDebug(flow, $"upload-response status={(int)putResp.StatusCode} body='{TruncateForLog(json)}'");
                if (!putResp.IsSuccessStatusCode)
                    return false;

                if (!TryParseNtfyFileUploadResponse(json, out var fileUrl))
                {
                    LogNtfyDebug(flow, "upload-parse-failed");
                    return false;
                }
                LogNtfyDebug(flow, $"upload-parse-ok fileUrl='{fileUrl}'");

                using var postReq = new HttpRequestMessage(HttpMethod.Post, _ntfyNotifyTopicUrl);
                postReq.Headers.TryAddWithoutValidation("Click", fileUrl);
                postReq.Headers.TryAddWithoutValidation("Title", BuildReconnectNotificationTitle());
                postReq.Content = new StringContent(
                    $"link: {fileUrl}",
                    Encoding.UTF8,
                    "text/plain");

                using var postResp = await NtfyUploadHttp.SendAsync(postReq, ct).ConfigureAwait(false);
                var notifyBody = await postResp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                LogNtfyDebug(flow, $"notify-response status={(int)postResp.StatusCode} body='{TruncateForLog(notifyBody)}'");
                if (!postResp.IsSuccessStatusCode)
                    return false;

                LogNtfyDebug(flow, "done");
                return true;
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                LogNtfyDebug(flow, "cancelled");
                return false;
            }
            catch (Exception ex)
            {
                // Bildirim başarısız olsa bile monitör döngüsü çalışmaya devam eder.
                LogNtfyDebug(flow, $"exception {ex.GetType().Name}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Fotoğraf olmadan metin bildirimi gönderir.
        /// </summary>
        public static async Task<bool> SendTextNotificationAsync(string title, string message, CancellationToken ct = default)
        {
            const string flow = "ntfy-text-flow";
            try
            {
                using var postReq = new HttpRequestMessage(HttpMethod.Post, _ntfyNotifyTopicUrl);
                postReq.Headers.TryAddWithoutValidation("Title", SanitizeAsciiHeaderValue(title));
                postReq.Content = new StringContent(message ?? string.Empty, Encoding.UTF8, "text/plain");

                using var postResp = await NtfyUploadHttp.SendAsync(postReq, ct).ConfigureAwait(false);
                var body = await postResp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                LogNtfyDebug(flow, $"notify-response status={(int)postResp.StatusCode} body='{TruncateForLog(body)}'");
                return postResp.IsSuccessStatusCode;
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                LogNtfyDebug(flow, "cancelled");
                return false;
            }
            catch (Exception ex)
            {
                LogNtfyDebug(flow, $"exception {ex.GetType().Name}: {ex.Message}");
                return false;
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

        static byte[] EncodeScreenshotForNtfy(Bitmap source)
        {
            byte[]? bestEffort = null;
            double scale = 1.0;
            int[] qualities = [75, 60, 45, 35];

            for (int attempt = 0; attempt < 8; attempt++)
            {
                int w = Math.Max(1, (int)Math.Round(source.Width * scale));
                int h = Math.Max(1, (int)Math.Round(source.Height * scale));
                using var resized = ResizeBitmap(source, w, h);

                foreach (var q in qualities)
                {
                    var jpeg = SaveJpegToBytes(resized, q);
                    if (bestEffort is null || jpeg.Length < bestEffort.Length)
                        bestEffort = jpeg;
                    if (jpeg.Length <= NtfyMaxUploadBytes)
                        return jpeg;
                }

                scale *= 0.8;
            }

            return bestEffort ?? SaveJpegToBytes(source, 35);
        }

        static Bitmap ResizeBitmap(Bitmap source, int width, int height)
        {
            var target = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            using var g = Graphics.FromImage(target);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            g.DrawImage(source, new Rectangle(0, 0, width, height));
            return target;
        }

        static byte[] SaveJpegToBytes(Bitmap bmp, long quality)
        {
            using var ms = new MemoryStream();
            var encoder = GetJpegEncoder();
            if (encoder is null)
            {
                bmp.Save(ms, ImageFormat.Jpeg);
                return ms.ToArray();
            }

            using var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
            bmp.Save(ms, encoder, encoderParams);
            return ms.ToArray();
        }

        static ImageCodecInfo? GetJpegEncoder()
        {
            foreach (var codec in ImageCodecInfo.GetImageEncoders())
            {
                if (codec.FormatID == ImageFormat.Jpeg.Guid)
                    return codec;
            }
            return null;
        }

        static void LogNtfyDebug(string scope, string message)
        {
            var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {scope}: {message}";
            Debug.WriteLine(line);
            try
            {
                var localDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "ProjectG");
                Directory.CreateDirectory(localDir);
                var localPath = Path.Combine(localDir, "ntfy-debug.log");
                File.AppendAllText(localPath, line + Environment.NewLine, Encoding.UTF8);

                // Ek görünürlük: uygulama klasörüne de aynı logdan yaz.
                var appPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ntfy-debug.log");
                File.AppendAllText(appPath, line + Environment.NewLine, Encoding.UTF8);
            }
            catch
            {
                // logging should never break monitor flow
            }
        }

        static string TruncateForLog(string value, int maxLen = 300)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;
            return value.Length <= maxLen ? value : value[..maxLen] + "...";
        }
    }
}
