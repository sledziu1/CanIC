using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

using SimHub.Plugins;
using GameReaderCommon;

using System.Windows.Media;

namespace CanIC
{
    public class SerialHandler
    {
        private readonly PluginMain parent;
        private bool previousSerialOpenState = false;

        private Timer timer = new Timer();
        private SerialPort serial = new SerialPort();
        private Queue<string> commandBuffer = new Queue<string>();

        private PluginData.Numerical speed = new PluginData.Numerical("s",5);
        private PluginData.Numerical rpm = new PluginData.Numerical("r",80);
        private PluginData.Gear gear = new PluginData.Gear();
        public PluginData.Numerical handbrake = new PluginData.Numerical("h",0);
        private PluginData.Numerical enable = new PluginData.Numerical("e",0);

        public List<CustomParameter> customPar;

        private bool inTest = false;
        public bool enableInGameOnly = false;

        public PluginData.Button button = new PluginData.Button("b");

        private readonly List<PluginData.IPluginData> parList;

        public SerialHandler(PluginMain p)
        {
            parent = p;

            //serial.PortName = SerialHandler.GetPort("STM32 Virtual ComPort");
            serial.BaudRate = 921600;
            serial.DataBits = 8;
            serial.Parity = Parity.None;
            serial.StopBits = StopBits.One;
            // Set the read/write timeouts
            serial.ReadTimeout = 500;
            serial.WriteTimeout = 500;

            parList = new List<PluginData.IPluginData>
            {
                speed,
                rpm,
                handbrake,
                gear,
                button,
                enable
            };

            customPar = new List<CustomParameter>()
            {
                new CustomParameter("Normal Light", "l0", parent.settingHandler),
                new CustomParameter("Long Beam", "l1", parent.settingHandler),
                new CustomParameter("Blinker Left", "l2", parent.settingHandler),
                new CustomParameter("Blinker Right", "l3", parent.settingHandler),
                new CustomParameter("Fog Front", "l4", parent.settingHandler),
                new CustomParameter("Fog Rear", "l5", parent.settingHandler),
                new CustomParameter("Handbrake", handbrake, parent.settingHandler),
                new CustomParameter("ESP off", "w2", parent.settingHandler),
                new CustomParameter("ESP active", "w1", parent.settingHandler),
                new CustomParameter("Check engine", "w6", parent.settingHandler),
                new CustomParameter("Cruise control", "w9", parent.settingHandler)
            };

            foreach(var x in customPar)
            {
                parList.Add(x.GetPlugData());
            }

            timer.AutoReset = true;
            timer.Elapsed += Timer_Elapsed;
            timer.Interval = 1.0;
            timer.Start();
        }

        ~SerialHandler()
        {
            End();
        }

        public void End()
        {
            if(serial!=null)
            {
                timer.Stop();
                serial.Close();
                serial.Dispose();
                serial = null;
            }
        }

        /* cyclic task */
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if(serial.IsOpen)
            {
                foreach(var x in parList)
                {
                    Enque(x);
                }

                //System.Diagnostics.Debug.WriteLine(commandBuffer.Count.ToString());

                if(commandBuffer.Count>0)
                {
                    try
                    {
                        serial.Write(commandBuffer.Peek());
                        commandBuffer.Dequeue();
                    }
                    catch (Exception)
                    {
                        /* fail */
                    }
                }
            }
            else
            {
                if(previousSerialOpenState)
                {
                    ConnectionLost();
                }


                TryToConnect();
            }

            previousSerialOpenState = serial.IsOpen;
        }

        private void ConnectionLost()
        {
            if (parent.settingsControl != null)
            {
                parent.settingsControl.Dispatcher.BeginInvoke((Action)(() =>
                {
                    parent.settingsControl.label1.Content = "ERR";
                    parent.settingsControl.label1.Foreground = new SolidColorBrush(Colors.Red);
                }));
            }
        }

        private void ConnectionEstablished()
        {
            ResetData();
            if (parent.settingsControl != null)
            {
                parent.settingsControl.Dispatcher.BeginInvoke((Action)(() =>
                {
                    parent.settingsControl.label1.Content = "OK";
                    parent.settingsControl.label1.Foreground = new SolidColorBrush(Color.FromRgb(0x16, 0xAE, 0x2B));
                }));
            }
        }

        public void UpdateConnectionGUI()
        {
            if (serial.IsOpen)
            {
                ConnectionEstablished();
            }
            else
            {
                ConnectionLost();
            }
        }

        private void TryToConnect()
        {
            String portName =  GetPort("CanIC");
            if(portName != null)
            {
                serial.PortName = portName;

                try
                {
                    serial.Open();
                    serial.DiscardOutBuffer();
                    serial.DiscardInBuffer();
                    commandBuffer.Clear();
                    ConnectionEstablished();
                }
                catch (Exception)
                {
                    ;
                }
            }
            else
            {
                /* device not connected */
            }
        }

        public static String GetPort(string busDescr)
        {
            String retval = null;

            List<Win32DeviceMgmt.DeviceInfo> devList = Win32DeviceMgmt.GetAllCOMPorts();

            foreach (Win32DeviceMgmt.DeviceInfo dev in devList)
            {
                if(dev.bus_description == busDescr)
                {
                    retval = dev.name;
                    break;
                }
            }

            return retval;
        }

        private void Enque(PluginData.IPluginData obj)
        {
            if(obj.IsUpdateNeeded())
            {
                commandBuffer.Enqueue("*"+obj.GetCmd() + obj.GetStr() + "#");
                obj.ClearUpdateNeeded();
            }
        }


        public void ProcessNewDate(PluginManager pluginManager, ref GameData data)
        {
            if(inTest)
            {
                /* do nothing */
                enable.Set(1);
            }
            else if (data.GameRunning)
            {
                if (data.OldData != null && data.NewData != null)
                {
                    speed.Set(data.NewData.SpeedKmh);
                    rpm.Set(data.NewData.Rpms);
                    gear.Set(data.NewData.Gear);
                    //handbrake.Set(data.NewData.Brake);

                    
                    foreach (var x in customPar)
                    {
                        x.TryUpdate(pluginManager);
                    }

                    enable.Set(1);
                }
            }
            else if(data.GameInMenu || data.GamePaused)
            {
                /* do nothing */
            }
            else
            {
                /* not in game */
                if (enableInGameOnly) enable.Set(0); else enable.Set(1);

                ResetData();
            }
        }

        private void ResetData()
        {
            speed.Set(0.0);
            rpm.Set(0.0);
            gear.Set("");
            handbrake.Set(0.0);

            foreach(CustomParameter cp in customPar)
            {
                cp.GetPlugData().Set(0);
            }

            if (enableInGameOnly) enable.Set(0); else enable.Set(1);
        }

        public void Test()
        {
            if(!inTest)
            {
                inTest = true;
                speed.Set(260);
                //gear.set
                rpm.Set(8100);

                foreach (CustomParameter cp in customPar)
                {
                    cp.GetPlugData().Set(1);
                }

                //Task.Delay(TimeSpan.FromSeconds(5)).ContinueWith(_ => TestEnd());
                Task.Delay(TimeSpan.FromSeconds(5)).ContinueWith(_ => inTest = false);
            }
        }

        public void SetGearPrefixD(Boolean en)
        {
            gear.SetPrefixD(en);
        }
    }
}
