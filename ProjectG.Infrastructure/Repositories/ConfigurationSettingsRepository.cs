using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProjectG.DomainLayer.Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace ProjectG.InfrastructureLayer.Repositories
{
    public class ConfigurationSettingsRepository
    {
        string currentPath;
        string settingsFilePath;

        public ConfigurationSettingsRepository()
        {
         currentPath = Directory.GetCurrentDirectory();
            settingsFilePath = Path.Combine(currentPath, "AppSettings.json");    
        }
        public bool AddNewSettings()
        {
             //settingsFilePath = Path.Combine(currentPath, "AppSettings.json");
            Random random = new Random();

            // Eğer dosya yoksa oluştur
            if (!File.Exists(settingsFilePath))
            {
                // Yeni bir AppSettings nesnesi oluştur
                var settings = new JObject
                {
                    ["ProfileName"] = AppSettings.ProfileName,
                    ["CycleDowntime"] = new JArray(AppSettings.CycleDowntime),
                    ["AHShowsUpDowntime"] = new JArray(AppSettings.AHShowsUpDowntime),
                    ["PostOrCancelDowntime"] = new JArray(AppSettings.PostOrCancelDowntime),
                    ["PostOrCancelDoneDowntime"] = new JArray(AppSettings.PostOrCancelDoneDowntime),
                    ["AHCloseRandomize"] = AppSettings.AHCloseRandomize,
                    ["SendGameToTheBackground"] = AppSettings.SendGameToTheBackground,
                    ["SendGameToTheBackgroundAxis"] = new JArray(AppSettings.SendGameToTheBackgroundAxis),
                    ["MailBoxOpenRandomize"] = AppSettings.MailBoxOpenRandomize,
                    ["MailBoxRandomizedPossibility"] = AppSettings.MailBoxRandomizedPossibility,
                    ["MailBoxShowsUpDowntime"] = new JArray(AppSettings.MailBoxShowsUpDowntime),
                    ["MailBoxCloseRandomize"] = AppSettings.MailBoxCloseRandomize
                };

                var jsonObject = new JObject
                {
                    [AppSettings.ProfileName] = settings
                };

                // JSON dosyasına yaz
                File.WriteAllText(settingsFilePath, jsonObject.ToString());

                return true;
            }
            return false;
        }

        public bool GetKeys()
        {
            
            var jsonString = File.ReadAllText(settingsFilePath);

            // JSON'u bir JObject'e dönüştür
            var jsonObject = JObject.Parse(jsonString);
            var topLevelKeys = jsonObject.Properties();
            List<string> keys = new List<string>();
            foreach (var key in topLevelKeys)
            {
                keys.Add(key.Name.ToString());
            }
            return true;
        }
    }
}
