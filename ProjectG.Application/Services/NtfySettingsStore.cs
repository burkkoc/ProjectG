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
                        return o;
                }
            }
            catch
            {
                // ignore, use defaults
            }

            return new NtfyUserSettings();
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
