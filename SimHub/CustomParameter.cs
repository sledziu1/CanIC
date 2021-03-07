using SimHub.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CanIC
{
    public class CustomParameter
    {
        public string name { get; }
        public string property { get; set; }
        public string compareValue { get; set; }

        private readonly string prefix = "CanIC_custom_";

        private readonly PluginData.Numerical plugDat;
        private readonly SettingHandler setH;

        public CustomParameter(string x, string par, SettingHandler sh)
        {
            name = x;
            plugDat = new PluginData.Numerical(par, 0);
            setH = sh;

            property = setH.GetKey(prefix + "prop_" + name, "");
            compareValue = setH.GetKey(prefix + "comp_" + name, "");
        }

        public CustomParameter(string x, PluginData.Numerical pd, SettingHandler sh)
        {
            name = x;
            plugDat = pd;
            setH = sh;

            property = setH.GetKey(prefix + "prop_" + name, "");
            compareValue = setH.GetKey(prefix + "comp_" + name, "");
        }

        public PluginData.Numerical GetPlugData()
        {
            return plugDat;
        }

        public void TryUpdate(PluginManager plMan)
        {
            if (property != null && compareValue != null && property != "" && compareValue != "")
            {
                    object propVal = plMan.GetPropertyValue(property);

                    if (propVal != null)
                    {

                        if (plMan.GetPropertyValue(property).ToString() == compareValue)
                        {
                            plugDat.Set(1);
                        }
                        else
                        {
                            plugDat.Set(0);
                        }
                    }
            }
            else
            {
                plugDat.Set(0);
            }
        }

        public void SaveSettings()
        {
            setH.SetKey(prefix + "prop_" + name, property);
            setH.SetKey(prefix + "comp_" + name, compareValue);
        }
    }
}
