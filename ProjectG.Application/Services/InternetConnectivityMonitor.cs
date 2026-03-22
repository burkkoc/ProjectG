using ProjectG.DomainLayer.Entities.Concrete;
using System.Net.Http;
using System.Net.NetworkInformation;

namespace ProjectG.ApplicationLayer.Services
{
    /// <summary>
    /// Uygulama açıkken yaklaşık saniyede bir internet erişilebilirliğini kontrol eder; sonuç <see cref="AppSettings.IsInternetReachable"/> alanına yazılır.
    /// </summary>
    public static class InternetConnectivityMonitor
    {
        static readonly object Gate = new();
        static CancellationTokenSource? _cts;
        static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(4) };

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
    }
}
