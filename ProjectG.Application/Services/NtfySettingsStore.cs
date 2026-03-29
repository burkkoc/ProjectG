using System.Text.Json;

namespace ProjectG.ApplicationLayer.Services
{
    public static class NtfySettingsStore
    {
        static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        static string FilePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ProjectG",
            "user-settings.json");

        public static NtfyUserSettings Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    var json = File.ReadAllText(FilePath);
                    var o = JsonSerializer.Deserialize<NtfyUserSettings>(json);
                    if (o != null && !string.IsNullOrWhiteSpace(o.NtfyNotifyTopicUrl))
                    {
                        NormalizeUserSettings(o);
                        return o;
                    }
                }
            }
            catch
            {
                // ignore, use defaults
            }

            var fresh = new NtfyUserSettings();
            NormalizeUserSettings(fresh);
            return fresh;
        }

        static void NormalizeUserSettings(NtfyUserSettings o)
        {
            if (o.DynamicShortAfterCancelMinTMultiplier <= 0 || o.DynamicShortAfterCancelMinTMultiplier >= 1_000_000)
                o.DynamicShortAfterCancelMinTMultiplier = 2;
            if (o.DynamicShortAfterCancelMaxTMultiplier <= 0 || o.DynamicShortAfterCancelMaxTMultiplier >= 1_000_000)
                o.DynamicShortAfterCancelMaxTMultiplier = 2;
            if (o.DynamicShortAfterCancelMaxExtraSeconds < 0 || o.DynamicShortAfterCancelMaxExtraSeconds >= 1_000_000)
                o.DynamicShortAfterCancelMaxExtraSeconds = 10;
            if (o.GuildBankMinIntervalMinutes < 0.5 || o.GuildBankMinIntervalMinutes > 10_080)
                o.GuildBankMinIntervalMinutes = 2;
            if (o.GuildBankMaxIntervalMinutes <= 0)
                o.GuildBankMaxIntervalMinutes = o.GuildBankMinIntervalMinutes;
            else
            {
                if (o.GuildBankMaxIntervalMinutes < 0.5)
                    o.GuildBankMaxIntervalMinutes = 0.5;
                else if (o.GuildBankMaxIntervalMinutes > 10_080)
                    o.GuildBankMaxIntervalMinutes = 10_080;
            }
            if (o.GuildBankMaxIntervalMinutes < o.GuildBankMinIntervalMinutes)
                o.GuildBankMaxIntervalMinutes = o.GuildBankMinIntervalMinutes;
            if (o.GuildBankAfterNotifyMinSecondsShorterThanFirst < 0 || o.GuildBankAfterNotifyMinSecondsShorterThanFirst > 86_400)
                o.GuildBankAfterNotifyMinSecondsShorterThanFirst = 60;
        }

        public static void Save(NtfyUserSettings settings)
        {
            var dir = Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(settings, JsonOptions));
        }
    }
}
