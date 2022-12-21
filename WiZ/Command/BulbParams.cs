using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

using WiZ.Observable;

namespace WiZ
{
    /// <summary>
    /// Bulb configuration parameters.
    /// </summary>
    public class BulbParams : ObservableBase, ICloneable
    {
        #region Bulb Type Catalog

        public static readonly ReadOnlyDictionary<(int, int), string> BulbTypeCatalog
            = new ReadOnlyDictionary<(int, int), string>(new Dictionary<(int, int), string>()
            {
                { (37, 1), "Philips Color & Tunable-White BR30" },
                { (50, 1), "Philips Color & Tunable-White BR30" },
                { (33, 1), "Philips Color & Tunable-White A19" },
                { (60, 1), "Philips Color & Tunable-White A19" },
                { (20, 2), "Philips Color & Tunable-White A21" },
                { (40, 1), "Philips Color & Tunable-White E14 Candelabra" },
                { (80, 2), "Philips Color & Tunable-White Lighting Strip" },
                { (30, 1), "Philips Color & Tunable-White PAR38 Indoor/Outdoor Floodlight" }
            });

        #endregion Bulb Type Catalog

        #region Fields

        #region Pilot Fields

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

        #endregion Pilot Fields

        #region Registration Params

        private string phoneMac;

        private bool? register;

        private string phoneIp;

        private string id;

        #endregion Registration Params

        #region Results

        private int? rssi;

        private string src;

        private MACAddress? macaddr;

        private bool? success;

        private int? homeId;

        private int? roomId;

        private string fwVersion;

        private (int, int)? drvConf;

        private string ewfHex;

        private int[] ewf;

        private string moduleName;

        private int? schdPsetId;

        #endregion Results

        #region UserConfig

        private int? fadeIn;

        private int? fadeOut;

        private int? dftDim;

        private bool? opMode;

        private int? minDimming;

        #endregion UserConfig

        #region ModelConfig

        private int? ps;

        private int? pwmFreq;

        private int[] pwmRange;

        private int? wcr;

        private int? nowc;

        private int[] cctRange;

        private int[] renderFactor;

        private int? hasAdjMinDim;

        private int? hasTapSensor;

        private int? pm;

        private int? fanSpeed;

        #endregion ModelConfig

        #endregion Fields

        #region Settings Rules Enforcement, Copying, Clearing, Configuring, Cloning

        /// <summary>
        /// Set the configurate with the settings from the specified light mode and enforce rules.
        /// </summary>
        /// <param name="lm"></param>
        public void SetLightMode(LightMode lm)
        {
            if (lm.Settings != null)
            {
                lm.Settings.CopyTo(this);
            }

            EnforceRules(lm.Type);
        }

        /// <summary>
        /// Copy this set of parameters to another object.
        /// </summary>
        /// <param name="other"></param>
        public void CopyTo(BulbParams other)
        {
            var json = JsonConvert.SerializeObject(this, BulbCommand.DefaultJsonSettings);
            JsonConvert.PopulateObject(json, other, BulbCommand.DefaultJsonSettings);
        }

        /// <summary>
        /// Attempt to automatically enforce rules given the available current settings.
        /// </summary>
        /// <remarks>
        /// Accurate enforcement should not be considered guaranteed.
        /// </remarks>
        public void EnforceRules()
        {
            if (Scene == 0)
            {
                EnforceCustomColorRules();
            }
            else if (Scene <= 1000)
            {
                var lm = LightMode.LightModes[(int)Scene];
                EnforceRules(lm.Type);
            }
        }

        /// <summary>
        /// Enforce the rules of the specified light mode type.
        /// </summary>
        /// <param name="type"></param>
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

        /// <summary>
        /// Enforce rules for simple lighting modes (currently only 'Nightlight' applies.)
        /// </summary>
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

        /// <summary>
        /// Enforce rules for white light modes (Warm Light, Cold Light, Daylight, etc.)
        /// </summary>
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

        /// <summary>
        /// Enforce rules for custom color configuration.
        /// </summary>
        public void EnforceCustomColorRules()
        {
            //State = null;
            //Red = null;
            //Green = null;
            //Blue = null;
            WarmWhite = null;
            ColdWhite = null;
            Speed = null;
            Scene = null;
            Temperature = null;
            //Brightness = null;
            Delta = null;
            Duration = null;
        }

        /// <summary>
        /// Enforce rules for static light modes.
        /// </summary>
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

        /// <summary>
        /// Enforce rules for dynamic light modes.
        /// </summary>
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

        /// <summary>
        /// Enforce rules for the pulse/ping.
        /// </summary>
        public void EnforcePulseRules()
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
            //Delta = null;
            //Duration = null;
        }

        #endregion Settings Rules Enforcement, Copying, Clearing, Configuring, Cloning

        #region Pilot

        /// <summary>
        /// Gets or sets the brightness level.
        /// </summary>
        /// <remarks>
        /// Value must be between 10 and 100.
        /// </remarks>
        [JsonProperty("dimming")]
        public byte? Brightness
        {
            get => dimming;
            set
            {
                if (value > 100) value = 100;
                else if (value < 10) value = 10;

                SetProperty(ref dimming, value);
            }
        }

        /// <summary>
        /// Gets or sets the on/off state of the bulb.
        /// </summary>
        [JsonProperty("state")]
        public bool? State
        {
            get => state;
            set
            {
                SetProperty(ref state, value);
            }
        }

        /// <summary>
        /// Gets or sets the animation speed of a dynamic scene.
        /// </summary>
        /// <remarks>
        /// Value must be between 10 and 200.
        /// </remarks>
        [JsonProperty("speed")]
        public byte? Speed
        {
            get => speed;
            set
            {
                if (value != null)
                {
                    if (value < 10) value = 10;
                    else if (value > 200) value = 200;
                }

                SetProperty(ref speed, value);
            }
        }

        /// <summary>
        /// Gets or sets the white-light color temperature of the bulb.
        /// </summary>
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

        /// <summary>
        /// Gets or sets a built-in scene code.
        /// </summary>
        [JsonProperty("sceneId")]
        public int? Scene
        {
            get => sceneId;
            set
            {
                SetProperty(ref sceneId, value);
                OnPropertyChanged(nameof(LightModeInfo));
            }
        }

        /// <summary>
        /// Gets the <see cref="LightMode"/> instance of the built-in <see cref="Scene"/>.
        /// </summary>
        [JsonIgnore]
        public LightMode LightModeInfo
        {
            get
            {
                if (sceneId == null) return null;

                int sc = (int)sceneId;

                if (sc == 0)
                {
                    if (temp != null)
                    {
                        return LightMode.LightModes[-1];
                    }
                    else
                    {
                        return LightMode.LightModes[0];
                    }
                }
                else
                {
                    return LightMode.LightModes[sc];
                }
            }
        }

        /// <summary>
        /// Red color channel.
        /// </summary>
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

        /// <summary>
        /// Green color channel.
        /// </summary>
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

        /// <summary>
        /// Blue color channel.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the color for a custom color configuration.
        /// </summary>
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

                OnPropertyChanged(nameof(Red));
                OnPropertyChanged(nameof(Green));
                OnPropertyChanged(nameof(Blue));
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the warm-white value.
        /// </summary>
        [JsonProperty("w")]
        public byte? WarmWhite
        {
            get => w;
            set
            {
                SetProperty(ref w, value);
            }
        }

        /// <summary>
        /// Gets or sets the cold-white value.
        /// </summary>
        [JsonProperty("c")]
        public byte? ColdWhite
        {
            get => c;
            set
            {
                SetProperty(ref c, value);
            }
        }

        /// <summary>
        /// Gets or sets the pulse delta.
        /// </summary>
        [JsonProperty("delta")]
        public int? Delta
        {
            get => delta;
            set
            {
                SetProperty(ref delta, value);
            }
        }

        /// <summary>
        /// Gets or sets the pulse duration (in milliseconds.)
        /// </summary>
        [JsonProperty("duration")]
        public int? Duration
        {
            get => duration;
            set
            {
                SetProperty(ref duration, value);
            }
        }

        #endregion Pilot

        #region Returned Information

        /// <summary>
        /// The local Mac address.
        /// </summary>
        [JsonProperty("phoneMac")]
        public string PhoneMac
        {
            get => phoneMac;
            set
            {
                SetProperty(ref phoneMac, value);
            }
        }

        /// <summary>
        /// Pass true to register.
        /// </summary>
        /// <remarks>
        /// It is not recommended to register from the PC, right now.
        /// </remarks>
        [JsonProperty("register")]
        public bool? Register
        {
            get => register;
            set
            {
                SetProperty(ref register, value);
            }
        }

        /// <summary>
        /// The local IP address.
        /// </summary>
        [JsonProperty("phoneIp")]
        public string PhoneIp
        {
            get => phoneIp;
            set
            {
                SetProperty(ref phoneIp, value);
            }
        }

        /// <summary>
        /// Bulb Id.
        /// </summary>
        [JsonProperty("id")]
        public string Id
        {
            get => id;
            set
            {
                SetProperty(ref id, value);
            }
        }

        /// <summary>
        /// Received signal strength indicator.
        /// </summary>
        [JsonProperty("rssi")]
        public int? Rssi
        {
            get => rssi;
            set
            {
                if (SetProperty(ref rssi, value))
                {
                    OnPropertyChanged(nameof(Distance));
                }
            }
        }

        [JsonIgnore]
        public double Distance
        {
            get
            {
                if (rssi == null) return double.NaN;
                return CalculateDistance((double)rssi, (double)(2.4 * 1000));
            }
        }

        private double CalculateDistance(double signalLevelInDb, double freqInMHz)
        {
            double exp = (27.55 - (20 * Math.Log10(freqInMHz)) + Math.Abs(signalLevelInDb)) / 20.0;
            return Math.Pow(10.0, exp);
        }

        /// <summary>
        /// Source (can be bluetooth, lan, or wan)
        /// </summary>
        [JsonProperty("src")]
        public string Source
        {
            get => src;
            set
            {
                SetProperty(ref src, value);
            }
        }

        /// <summary>
        /// Bulb Mac address.
        /// </summary>
        [JsonProperty("mac")]
        public MACAddress? MACAddress
        {
            get => macaddr;
            set
            {
                SetProperty(ref macaddr, value);
            }
        }

        /// <summary>
        /// True if the last command was successful.
        /// </summary>
        [JsonProperty("success")]
        public bool? Success
        {
            get => success;
            set
            {
                SetProperty(ref success, value);
            }
        }

        /// <summary>
        /// Home Id.
        /// </summary>
        [JsonProperty("homeId")]
        public int? HomeId
        {
            get => homeId;
            set
            {
                SetProperty(ref homeId, value);
            }
        }

        /// <summary>
        /// Room Id.
        /// </summary>
        [JsonProperty("roomId")]
        public int? RoomId
        {
            get => roomId;
            set
            {
                SetProperty(ref roomId, value);
            }
        }

        /// <summary>
        /// Driver configuration.
        /// </summary>
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

        /// <summary>
        /// Driver configuration.
        /// </summary>
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

        /// <summary>
        /// Product module Ewf code.
        /// </summary>
        [JsonProperty("ewf")]
        public int[] Ewf
        {
            get => ewf;
            set
            {
                SetProperty(ref ewf, value);
            }
        }

        /// <summary>
        /// Product module Ewf hex code.
        /// </summary>
        [JsonProperty("ewfHex")]
        public string EwfHex
        {
            get => ewfHex;
            set
            {
                SetProperty(ref ewfHex, value);
            }
        }

        /// <summary>
        /// Firmware version.
        /// </summary>
        [JsonProperty("fwVersion")]
        public string FirmwareVersion
        {
            get => fwVersion;
            set
            {
                SetProperty(ref fwVersion, value);
            }
        }

        /// <summary>
        /// Product module name.
        /// </summary>
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

        /// <summary>
        /// Product bulb type description.
        /// </summary>
        [JsonIgnore]
        public string TypeDescription
        {
            get
            {
                if (drvConf != null && BulbTypeCatalog.ContainsKey(drvConf.Value))
                {
                    return BulbTypeCatalog[drvConf.Value];
                }
                else if (wcr != null && nowc != null && BulbTypeCatalog.ContainsKey(((int)wcr, (int)nowc)))
                {
                    return BulbTypeCatalog[((int)wcr, (int)nowc)];
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Rhythm Set Id
        /// </summary>
        [JsonProperty("schdPsetId")]
        public int? SchdPsetId
        {
            get => schdPsetId;
            set
            {
                SetProperty(ref schdPsetId, value);
            }
        }

        #region Returned from getModelConfig

        [JsonProperty("ps")]
        public int? Ps
        {
            get => ps;
            set
            {
                SetProperty(ref ps, value);
            }
        }

        [JsonProperty("pwmFreq")]
        public int? PwmFrequency
        {
            get => pwmFreq;
            set
            {
                SetProperty(ref pwmFreq, value);
            }
        }

        [JsonProperty("pwmRange")]
        public int[] PwmRange
        {
            get => pwmRange;
            set
            {
                SetProperty(ref pwmRange, value);
            }
        }

        [JsonProperty("wcr")]
        public int? WcR
        {
            get => wcr;
            set
            {
                SetProperty(ref wcr, value);
            }
        }

        [JsonProperty("nowc")]
        public int? NoWc
        {
            get => nowc;
            set
            {
                SetProperty(ref nowc, value);
            }
        }

        [JsonProperty("cctRange")]
        public int[] CCTRange
        {
            get => cctRange;
            set
            {
                SetProperty(ref cctRange, value);
            }
        }

        [JsonProperty("renderFactor")]
        public int[] RenderFactor
        {
            get => renderFactor;
            set
            {
                SetProperty(ref renderFactor, value);
            }
        }

        [JsonProperty("hasAdjMinDim")]
        public int? HasAdjustableMinDim
        {
            get => hasAdjMinDim;
            set
            {
                SetProperty(ref hasAdjMinDim, value);
            }
        }

        [JsonProperty("hasTapSensor")]
        public int? HasTapSensor
        {
            get => hasTapSensor;
            set
            {
                SetProperty(ref hasTapSensor, value);
            }
        }

        [JsonProperty("pm")]
        public int? Pm
        {
            get => pm;
            set
            {
                SetProperty(ref pm, value);
            }
        }

        [JsonProperty("fanSpeed")]
        public int? FanSpeed
        {
            get => fanSpeed;
            set
            {
                SetProperty(ref fanSpeed, value);
            }
        }

        #endregion Returned from getModelConfig

        #region Results from getUserConfig

        [JsonProperty("fadeIn")]
        public int? FadeIn
        {
            get => fadeIn;
            set
            {
                SetProperty(ref fadeIn, value);
            }
        }

        [JsonProperty("fadeOut")]
        public int? FadeOut
        {
            get => fadeOut;
            set
            {
                SetProperty(ref fadeOut, value);
            }
        }

        [JsonProperty("dftDim")]
        public int? DefaultBrightness
        {
            get => dftDim;
            set
            {
                SetProperty(ref dftDim, value);
            }
        }

        [JsonProperty("opMode")]
        public bool? OpMode
        {
            get => opMode;
            set
            {
                SetProperty(ref opMode, value);
            }
        }

        [JsonProperty("minDimming")]
        public int? MinBrightness
        {
            get => minDimming;
            set
            {
                SetProperty(ref minDimming, value);
            }
        }

        #endregion Results from getUserConfig

        #endregion Returned Information

        #region Object Overrides

        public override string ToString()
        {
            var s = TypeDescription;
            if (s != null) return s;
            return base.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            if (obj.GetType() != typeof(BulbParams)) return false;

            var flds = typeof(BulbParams).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

            bool bPass = true;

            foreach (var f in flds)
            {
                object a = f.GetValue(this);
                object b = f.GetValue(obj);

                if (a == null && b == null)
                {
                    continue;
                }
                else if (a == null && b != null)
                {
                    bPass = false;
                    break;
                }
                else if (a != null && b != null)
                {
                    bPass = false;
                    break;
                }

                bPass &= a.Equals(b);
                if (!bPass) break;
            }

            return bPass;
        }

        public override int GetHashCode()
        {
            var s = JsonConvert.SerializeObject(this);
            return s.GetHashCode();
        }

        #endregion Object Overrides

        #region Operators

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

        #endregion Operators
    }
}