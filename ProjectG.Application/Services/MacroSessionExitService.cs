using System.Diagnostics;
using ProjectG.DomainLayer.Entities.Concrete;

namespace ProjectG.ApplicationLayer.Services
{
    /// <summary>
    /// <see cref="AppSettings.ExitTime"/> dakikası dolunca WoW süreçlerini sonlandırır ve uygulamayı kapatır.
    /// </summary>
    public static class MacroSessionExitService
    {
        static readonly string[] WowProcessNames = ["WoW", "WowClassic"];

        /// <summary>
        /// Makro oturumu <see cref="AppSettings.ExitTime"/> dakikayı aştıysa WoW'u kapatır ve <see cref="Environment.Exit(int)"/> çağırır.
        /// </summary>
        public static void ExitIfScheduledDurationElapsed()
        {
            OptionalMacroNtfyNotifier.TryNotifyExitTimeApproachingIfEnabled();
            if (AppSettings.ExitTime <= 0)
                return;
            if (AppSettings.MacroSessionStartedUtc == default)
                return;
            if (DateTime.UtcNow - AppSettings.MacroSessionStartedUtc < TimeSpan.FromMinutes(AppSettings.ExitTime))
                return;

            AppSettings.Working = false;
            KillWowGameProcesses();
            Environment.Exit(0);
        }

        static void KillWowGameProcesses()
        {
            foreach (string name in WowProcessNames)
            {
                try
                {
                    foreach (var p in Process.GetProcessesByName(name))
                    {
                        try
                        {
                            p.Kill(entireProcessTree: false);
                        }
                        catch
                        {
                            // ignored
                        }
                        finally
                        {
                            p.Dispose();
                        }
                    }
                }
                catch
                {
                    // ignored
                }
            }
        }
    }
}
