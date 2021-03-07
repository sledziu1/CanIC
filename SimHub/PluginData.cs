using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CanIC
{
    public class PluginData
    {
        public interface IPluginData
        {
            String GetStr();
            Boolean IsUpdateNeeded();
            void ClearUpdateNeeded();
            String GetCmd();
        }

        public abstract class Updateable
        {
            protected Boolean updateNeeded = false;

            public Boolean IsUpdateNeeded()
            {
                return updateNeeded;
            }

            public void ClearUpdateNeeded()
            {
                updateNeeded = false;
            }
        }

        public class Numerical : Updateable, IPluginData
        {
            private Decimal value = new Decimal(0.0);
            private readonly string command;
            private bool overrideEnabled = false;
            private readonly Decimal acceptableDiff;

            public Numerical(String cmd, double diff)
            {
                command = cmd;
                acceptableDiff = new Decimal(diff);
            }

            public void Set(double v)
            {
                if (!overrideEnabled)
                {
                    Decimal decv = new Decimal(v);
                    if (Math.Abs(value - decv) > acceptableDiff)
                    {
                        value = decv;

                        updateNeeded = true;
                    }
                }
            }

            public String GetStr()
            {
                return Decimal.ToInt16(value).ToString();
            }

            public String GetCmd()
            {
                return command;
            }

            public void OverrideEnable()
            {
                overrideEnabled = true;
                value = new Decimal(1);
                updateNeeded = true;
            }

            public void OverrideDisable()
            {
                overrideEnabled = false;
                value = new Decimal(0);
                updateNeeded = true;
            }
        }

        public class Button : Updateable, IPluginData
        {
            private String value = "";
            private readonly string command;

            public Button(String cmd)
            {
                command = cmd;
            }

            public void LongPress()
            {
                value = "1";
                updateNeeded = true;
            }

            public void ShortPress()
            {
                value = "0";
                updateNeeded = true;
            }

            public String GetStr()
            {
                return value;
            }

            public String GetCmd()
            {
                return command;
            }
        }


        public class Gear : Updateable, IPluginData
        {
            public String val;

            public Boolean prefixD=false;

            public void Set(String str)
            {
                if(val != str)
                {
                    val = str;
                    updateNeeded = true;
                }
            }

            public void SetPrefixD(Boolean enabled)
            {
                prefixD = enabled;
                updateNeeded = true;
            }

            private Gear_Enum StrToEnum(String str)
            {
                Gear_Enum retv = Gear_Enum.unknown;

                Int16 numVal;
                bool isNumb = Int16.TryParse(str, out numVal);

                if(isNumb)
                {
                    if(1 <= numVal && numVal <= 6)
                    {
                        if (prefixD == true)
                        {
                            numVal += 10;
                        }

                        retv = (Gear_Enum)numVal;
                    }
                    else
                    {
                        retv = Gear_Enum.D;
                    }
                }
                else
                {
                    switch (str)
                    {
                        case "P":
                            retv = Gear_Enum.P;
                            break;
                        case "R":
                            retv = Gear_Enum.R;
                            break;
                        case "N":
                            retv = Gear_Enum.R;
                            break;
                        default:
                            retv = Gear_Enum.unknown;
                            break;
                    }
                }

                return retv;
            }

            public String GetCmd()
            {
                return "g";
            }

            public String GetStr()
            {
                return ((Int16)StrToEnum(val)).ToString();
            }

            public enum Gear_Enum
            {
                g0=0,
                g1=1,
                g2=2,
                g3=3,
                g4=4,
                g5=5,
                g6=6,
                D1=11,
                D2=12,
                D3=13,
                D4=14,
                D5=15,
                D6=16,
                P=21,
                R=22,
                N=23,
                D=24,
                Ds=25,
                A=26,
                Dx=27,
                gUP=28,
                gDN=29,
                unknown=30
            }
        }
    }
}
