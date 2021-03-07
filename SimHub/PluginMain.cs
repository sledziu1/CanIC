using GameReaderCommon;
using SimHub.Plugins;
using System;
using System.Windows.Forms;
using System.Windows.Controls;
using System.IO.Ports;
using System.Diagnostics;


namespace CanIC
{
    [PluginName("CanIC")]
    [PluginAuthor("sledziu1")]
    [PluginDescrition("Control external cluster")]
    public class PluginMain : IPlugin, IDataPlugin, IWPFSettings
    {
        public bool gearupIndicatorEnabled = false;

        public SettingsControlDemo settingsControl;

        public SerialHandler serialHandler=null;

        public readonly SettingHandler settingHandler = new SettingHandler();

        /// <summary>
        /// Instance of the current plugin manager
        /// </summary>
        public PluginManager PluginManager { get; set; }

  
        /// <summary>
        /// called one time per game data update
        /// </summary>
        /// <param name="pluginManager"></param>
        /// <param name="data"></param>
        public void DataUpdate(PluginManager pluginManager, ref GameData data)
        {
            serialHandler.ProcessNewDate(pluginManager, ref data);
        }

        /// <summary>
        /// Called at plugin manager stop, close/displose anything needed here !
        /// </summary>
        /// <param name="pluginManager"></param>
        public void End(PluginManager pluginManager)
        {
            settingsControl = null;
            serialHandler.End();
            serialHandler = null;
            Debug.WriteLine("END called");
        }

        /// <summary>
        /// Return you winform settings control here, return null if no settings control
        /// 
        /// </summary>
        /// <param name="pluginManager"></param>
        /// <returns></returns>
        public System.Windows.Forms.Control GetSettingsControl(PluginManager pluginManager)
        {
            return null;
        }

        public System.Windows.Controls.Control GetWPFSettingsControl(PluginManager pluginManager)
        {
            if (settingsControl == null)
            {
                settingsControl = new SettingsControlDemo(this);
            }

            serialHandler.UpdateConnectionGUI();

            return settingsControl;
        }

        /// <summary>
        /// Called after plugins startup
        /// </summary>
        /// <param name="pluginManager"></param>
        public void Init(PluginManager pluginManager)
        {
            if (serialHandler == null)
            {
                serialHandler = new SerialHandler(this);
            }
            Debug.WriteLine("INIT called");
        }
    }
}