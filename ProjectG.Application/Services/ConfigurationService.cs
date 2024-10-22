using ProjectG.DomainLayer.Entities.Concrete;
using ProjectG.InfrastructureLayer.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectG.ApplicationLayer.Services
{
    public class ConfigurationService
    {
        private ConfigurationSettingsRepository _settingsRepository;
        public ConfigurationService()
        {
            _settingsRepository = new ConfigurationSettingsRepository();
        }
        public void LoadSettings(string settingsName) //void, sınıf oluşturulduğunda değişecek 
        {

        }

        public bool SaveSettings()
        {

            var res = _settingsRepository.AddNewSettings();
            if(res)
                return true;

            return false;
        }

        public bool GetKeys()
        {
            var res = _settingsRepository.GetKeys();

            if (res)
                return true;

            return false;
        }
    }
}
