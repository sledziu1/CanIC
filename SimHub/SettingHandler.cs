using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Configuration;

namespace CanIC
{
    public class SettingHandler
    {
        private readonly Configuration configFile;
        private readonly KeyValueConfigurationCollection settings;

        private readonly string gearPrefixKey = "CanIC_gearPrefix";
        private readonly string gearPrefixDefVal = "false";

        private readonly string enableInGameKey = "CanIC_enableInGameOnly";
        private readonly string enableInGameDefVal = "false";

        public SettingHandler()
        {
            configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            settings = configFile.AppSettings.Settings;
        }

        public bool GetDefaultPrefix()
        {
            return Convert.ToBoolean(GetKey(gearPrefixKey, gearPrefixDefVal));
        }

        public bool GetDefaultEnableInGame()
        {
            return Convert.ToBoolean(GetKey(enableInGameKey, enableInGameDefVal));
        }

        public void SetDefaultPrefix(bool v)
        {
            SetKey(gearPrefixKey, v.ToString());
        }

        public void SetEnableInGame(bool v)
        {
            SetKey(enableInGameKey, v.ToString());
        }

        public string GetKey(string key, string defaultVal)
        {
            if (settings[key] == null)
            {
                settings.Add(key, defaultVal);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
                configFile.Save(ConfigurationSaveMode.Modified);
            }

            return settings[key].Value;
        }

        public void SetKey(string key, string val)
        {
            if (settings[key] == null)
            {
                settings.Add(key, val);
                
            }
            else
            {
                settings[key].Value = val;
            }

            ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            configFile.Save(ConfigurationSaveMode.Modified);
        }
    }
}
