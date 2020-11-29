using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace WizLib
{

    /// <summary>
    /// Bulb configuration parameters.
    /// </summary>
    public sealed class BulbParams : ViewModelBase, ICloneable
    {

        public static readonly ReadOnlyDictionary<(int, int), string> BulbTypeCatalog
            = new ReadOnlyDictionary<(int, int), string>(new Dictionary<(int, int), string>()
            {
                { (37, 1), "Phillips Color & Tunable-White BR30" },
                { (50, 1), "Phillips Color & Tunable-White BR30" },
                { (33, 1), "Philips Color & Tunable-White A19" },
                { (60, 1), "Philips Color & Tunable-White A19" },
                { (20, 2), "Philips Color & Tunable-White A21" }
            });

        private bool? state;

        private byte? r;

        private byte? g;

        private byte? b;

        private byte? w;

        private byte? c;

        private byte? speed;

        private int? sceneId;

        private int? temp;

        private byte? dimming;

        private int? delta;

        private int? duration;

        // registration params

        private string phoneMac;

        private bool? register;

        private string phoneIp;

        private string id;

        // results

        private int? rssi;

        private string src;

        private string macaddr;

        private bool? success;

        private int? homeId;

        private int? roomId;

        private string fwVersion;

        private (int, int)? drvConf;

        private string ewfHex;

        private int[] ewf;

        private string moduleName;

        public void SetLightMode(LightMode lm)
        {
            if (lm.Settings != null)
            {
                lm.Settings.CopyTo(this);
            }

            EnforceRules(lm.Type);
        }

        public void CopyTo(BulbParams other)
        {
            var json = JsonConvert.SerializeObject(this);
            JsonConvert.PopulateObject(json, other);
        }

        public void EnforceRules()
        {
            if (Scene == null || Scene == 0)
            {
                EnforceCustomColorRules();
            }
            else if (Scene <= 1000)
            {
                var lm = LightMode.LightModes[(int)Scene];
                EnforceRules(lm.Type);
            }
        }

        public void EnforceRules(LightModeType type)
        {
            switch (type)
            {
                case LightModeType.CustomColor:
                    EnforceCustomColorRules();
                    break;

                case LightModeType.Celebrations:
                case LightModeType.Dynamic:

                    EnforceDynamicSceneRules();
                    break;

                case LightModeType.Simple:
                    EnforceSimpleLightRules();
                    break;

                case LightModeType.WhiteLight:
                    EnforceWhiteLightRules();
                    break;

                case LightModeType.Static:
                    EnforceStaticSceneRules();
                    break;
            }
        }

        /// <summary>
        /// Clear only 'setPilot' configuration settings.
        /// System configuration is preserved.
        /// </summary>
        public void ClearPilot()
        {
            State = null;
            Red = null;
            Green = null;
            Blue = null;
            WarmWhite = null;
            ColdWhite = null;
            Speed = null;
            Scene = null;
            Temperature = null;
            Brightness = null;
            Delta = null;
            Duration = null;
        }

        /// <summary>
        /// Clone this paramater set to a new instance.
        /// </summary>
        /// <returns></returns>
        public object Clone() => MemberwiseClone();

        /// <summary>
        /// Clone this parameter set to a new instance, and optionally clear 'getSystemConfig' settings.
        /// </summary>
        /// <param name="clearSysConfig">True to clear system configuration settings.</param>
        /// <returns></returns>
        public BulbParams Clone(bool clearSysConfig)
        {
            var p = (BulbParams)Clone();
            if (!clearSysConfig) return p;

            // registration params

            p.PhoneMac = null;
            p.Register = null;
            p.PhoneIp = null;
            p.Id = null;

            // results

            p.Rssi = null;
            p.Source = null;
            p.MACAddress = null;
            p.Success = null;
            p.HomeId = null;
            p.RoomId = null;
            p.FirmwareVersion = null;
            p.ModuleName = null;

            return p;
        }

        public void EnforceSimpleLightRules()
        {

            //State = null;
            Red = null;
            Green = null;
            Blue = null;
            WarmWhite = null;
            ColdWhite = null;
            Speed = null;
            //Scene = null;
            Temperature = null;
            Brightness = null;
            Delta = null;
            Duration = null;
        }

        public void EnforceWhiteLightRules()
        {
            //State = null;
            Red = null;
            Green = null;
            Blue = null;
            //WarmWhite = null;
            //ColdWhite = null;
            Speed = null;
            //Scene = null;
            //Temperature = null;
            //Brightness = null;
            Delta = null;
            Duration = null;
        }

        public void EnforceCustomColorRules()
        {
            //State = null;
            //Red = null;
            //Green = null;
            //Blue = null;
            WarmWhite = 0;
            ColdWhite = 0;
            Speed = null;
            Scene = null;
            Temperature = null;
            //Brightness = null;
            Delta = null;
            Duration = null;
        }

        public void EnforceStaticSceneRules()
        {

            //State = null;
            Red = null;
            Green = null;
            Blue = null;
            WarmWhite = null;
            ColdWhite = null;
            Speed = null;
            //Scene = null;
            Temperature = null;
            //Brightness = null;
            Delta = null;
            Duration = null;
        }

        public void EnforceDynamicSceneRules()
        {

            //State = null;
            Red = null;
            Green = null;
            Blue = null;
            WarmWhite = null;
            ColdWhite = null;
            //Speed = null;
            //Scene = null;
            Temperature = null;
            //Brightness = null;
            Delta = null;
            Duration = null;
        }

        public void EnforcePulseRules()
        {
            //State = null;
            //Red = null;
            //Green = null;
            //Blue = null;
            //WarmWhite = null;
            //ColdWhite = null;
            //Speed = null;
            //Scene = null;
            //Temperature = null;
            //Brightness = null;
            Delta = null;
            Duration = null;
        }

        [JsonProperty("dimming")]
        public byte? Brightness
        {
            get => dimming;
            set
            {
                if (value > 100) value = 100;
                SetProperty(ref dimming, value);
            }
        }

        [JsonProperty("state")]
        public bool? State
        {
            get => state;
            set
            {
                SetProperty(ref state, value);
            }
        }

        [JsonProperty("speed")]
        public byte? Speed
        {
            get => speed;
            set
            {
                if (value != null)
                {
                    if (value < 1) value = 1;
                    else if (value > 100) value = 100;
                }

                SetProperty(ref speed, value);
            }
        }

        [JsonProperty("temp")]
        public int? Temperature
        {
            get => temp;
            set
            {
                if (value != null)
                {
                    if (value > 10000) value = 10000;
                    else if (value < 1000) value = 1000;
                }

                SetProperty(ref temp, value);
            }
        }

        [JsonProperty("sceneId")]
        public int? Scene
        {
            get => sceneId;
            set
            {
                SetProperty(ref sceneId, value);
                OnPropertyChanged("LightModeInfo");
            }
        }

        [JsonIgnore]
        public LightMode LightModeInfo
        {
            get
            {
                if (LightMode.LightModes.ContainsKey(sceneId ?? 0))
                {
                    return LightMode.LightModes[sceneId ?? 0];
                }
                else
                {
                    return null;
                }
            }
        }

        [JsonProperty("r")]
        public byte? Red
        {
            get => r;
            set
            {
                if (r == value) return;
                SetProperty(ref r, value);
                OnPropertyChanged("Color");
            }
        }

        [JsonProperty("g")]
        public byte? Green
        {
            get => g;
            set
            {
                if (g == value) return;
                SetProperty(ref g, value);
                OnPropertyChanged("Color");
            }
        }

        [JsonProperty("b")]
        public byte? Blue
        {
            get => b;
            set
            {
                if (b == value) return;
                SetProperty(ref b, value);
                OnPropertyChanged("Color");
            }
        }

        [JsonIgnore]
        public System.Drawing.Color? Color
        {
            get
            {
                if (r != null && g != null & b != null)
                {
                    return System.Drawing.Color.FromArgb((byte)r, (byte)g, (byte)b);
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value == null && r == null && g == null && b == null)
                {
                    return;
                }
                else if (value == null)
                {
                    r = g = b = null;
                }
                else
                {
                    if (r != null && g != null && b != null)
                    {
                        System.Drawing.Color cur = System.Drawing.Color.FromArgb((byte)r, (byte)g, (byte)b);
                        if (value == cur) return;
                    }

                    var c = (System.Drawing.Color)value;

                    r = c.R;
                    g = c.G;
                    b = c.B;
                }

                OnPropertyChanged("Red");
                OnPropertyChanged("Green");
                OnPropertyChanged("Blue");
                OnPropertyChanged();
            }
        }

        [JsonProperty("w")]
        public byte? WarmWhite
        {
            get => w;
            set
            {
                SetProperty(ref w, value);
            }
        }

        [JsonProperty("c")]
        public byte? ColdWhite
        {
            get => c;
            set
            {
                SetProperty(ref c, value);
            }
        }

        [JsonProperty("delta")]
        public int? Delta
        {
            get => delta;
            set
            {
                SetProperty(ref delta, value);
            }
        }

        [JsonProperty("duration")]
        public int? Duration
        {
            get => duration;
            set
            {
                SetProperty(ref duration, value);
            }
        }
                
        #region Returned Information


        [JsonProperty("phoneMac")]
        public string PhoneMac
        {
            get => phoneMac;
            set
            {
                SetProperty(ref phoneMac, value);
            }
        }

        [JsonProperty("register")]
        public bool? Register
        {
            get => register;
            set
            {
                SetProperty(ref register, value);
            }
        }

        [JsonProperty("phoneIp")]
        public string PhoneIp
        {
            get => phoneIp;
            set
            {
                SetProperty(ref phoneIp, value);
            }
        }

        [JsonProperty("id")]
        public string Id
        {
            get => id;
            set
            {
                SetProperty(ref id, value);
            }
        }

        [JsonProperty("rssi")]
        public int? Rssi
        {
            get => rssi;
            set
            {
                SetProperty(ref rssi, value);
            }
        }

        [JsonProperty("src")]
        public string Source
        {
            get => src;
            set
            {
                SetProperty(ref src, value);
            }
        }

        [JsonProperty("mac")]
        public string MACAddress
        {
            get => macaddr;
            set
            {
                SetProperty(ref macaddr, value);
            }
        }

        [JsonProperty("success")]
        public bool? Success
        {
            get => success;
            set
            {
                SetProperty(ref success, value);
            }
        }

        [JsonProperty("homeId")]
        public int? HomeId
        {
            get => homeId;
            set
            {
                SetProperty(ref homeId, value);
            }
        }

        [JsonProperty("roomId")]
        public int? RoomId
        {
            get => roomId;
            set
            {
                SetProperty(ref roomId, value);
            }
        }

        [JsonProperty("drvConf")]
        public (int, int)? DrvConf
        {
            get => drvConf;
            set
            {
                if (SetProperty(ref drvConf, value))
                {
                    OnPropertyChanged(nameof(DriverConfig));
                }
            }
        }

        [JsonIgnore]
        public string DriverConfig
        {
            get
            {
                if (drvConf != null)
                {
                    return $"{drvConf.Value.Item1}, {drvConf.Value.Item2}";
                }
                else
                {
                    return null;
                }
            }
        }

        [JsonProperty("ewf")] 
        public int[] Ewf
        {
            get => ewf;
            set
            {
                SetProperty(ref ewf, value);
            }
        }

        [JsonProperty("ewfHex")]
        public string EwfHex
        {
            get => ewfHex;
            set
            {
                SetProperty(ref ewfHex, value);
            }
        }

        [JsonProperty("fwVersion")]
        public string FirmwareVersion
        {
            get => fwVersion;
            set
            {
                SetProperty(ref fwVersion, value);
            }
        }

        [JsonProperty("moduleName")] 
        public string ModuleName
        {
            get => moduleName;
            set
            {
                SetProperty(ref moduleName, value);
                OnPropertyChanged("TypeDescription");
            }
        }

        [JsonIgnore]
        public string TypeDescription
        {
            get
            {
                if (drvConf != null && BulbTypeCatalog.ContainsKey(drvConf.Value))
                {
                    return BulbTypeCatalog[drvConf.Value];
                }
                else
                {
                    return null;
                }
            }
        }

        #endregion

        public override string ToString()
        {
            var s = TypeDescription;
            if (s != null) return s;
            return base.ToString();
        }

        public override bool Equals(object obj)
        {
            var s1 = JsonConvert.SerializeObject(this);
            var s2 = JsonConvert.SerializeObject(obj);

            return (s1 == s2);
        }

        public override int GetHashCode()
        {
            var s = JsonConvert.SerializeObject(this);
            return s.GetHashCode();
        }

        public static bool operator ==(BulbParams v1, BulbParams v2)
        {
            if (!(v1 is object) && !(v2 is object))
            {
                return true;
            }
            else if (!(v1 is object) && (v2 is object))
            {
                return false;
            }
            else if ((v1 is object) && !(v2 is object))
            {
                return false;
            }


            return v1.Equals(v2);
        }

        public static bool operator !=(BulbParams v1, BulbParams v2)
        {

            if (!(v1 is object) && !(v2 is object))
            {
                return false;
            }
            else if (!(v1 is object) && (v2 is object))
            {
                return true;
            }
            else if ((v1 is object) && !(v2 is object))
            {
                return true;
            }

            return !v1.Equals(v2);
        }

    }


    internal class TupleConverter : JsonConverter<(int, int)?>
    {
        public override (int, int)? ReadJson(JsonReader reader, Type objectType, [AllowNull] (int, int)? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader is JsonTextReader jr)
            {

                int x1 = 0;
                int x2 = 0;

                if (jr.Read() && jr.Value is long i)
                {
                    x1 = (int)i;
                    
                    if (jr.Read() && jr.Value is long j)
                    {
                        x2 = (int)j;
                    }

                }

                jr.Read();

                return (x1, x2);
            }
            else
            {
                return null;
            }
        }

        public override void WriteJson(JsonWriter writer, [AllowNull] (int, int)? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                writer.WriteValue(new int[] { value.Value.Item1, value.Value.Item2 });
            }
        }
    }
}
