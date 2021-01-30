using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;

using WiZ.Helpers;
using WiZ.Localization;
using WiZ.Observable;
using WiZ.Profiles;

namespace WiZ
{
    /// <summary>
    /// ScanForBulbs callback delegate.
    /// </summary>
    /// <param name="b"></param>
    public delegate void BulbScanCallback(Bulb b);

    /// <summary>
    /// The condition on which to perform a scan to discover WiZ bulbs.
    /// </summary>
    public enum ScanCondition
    {
        /// <summary>
        /// Never perform a scan.
        /// </summary>
        Never,

        /// <summary>
        /// Only scan if there is no item in the cache.
        /// </summary>
        NotFound,

        /// <summary>
        /// Always perform a scan.
        /// </summary>
        Always
    }

    /// <summary>
    /// Bulb scanning modes indicating which command to broadcast to discover bulbs on the local network.
    /// </summary>
    public enum ScanMode
    {
        /// <summary>
        /// Scan using the 'registration' command
        /// </summary>
        Registration,

        /// <summary>
        /// Scan using the 'getPilot' command
        /// </summary>
        GetPilot,

        /// <summary>
        /// Scan using the 'getSystemConfig' command (default)
        /// </summary>
        GetSystemConfig
    }

    /// <summary>
    /// Encapsulates the characteristics and behavior of a WiZ light bulb.
    /// </summary>
    public class Bulb : ObservableBase, IBulb
    {
        #region Public Fields

        /// <summary>
        /// The default port for WiZ bulbs.
        /// </summary>
        public const int DefaultPort = 38899;

        #endregion Public Fields

        #region Protected Fields

        protected static Dictionary<MACAddress, Bulb> bulbCache = new Dictionary<MACAddress, Bulb>();
        protected static bool allUdpActive;
        protected bool udpActive;
        protected IPAddress addr;
        protected string bulbType;
        protected string icon;
        protected string name;
        protected int port = DefaultPort;
        protected BulbParams settings;
        protected int timeout = 2000;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Instantiate a new bulb object.
        /// </summary>
        /// <param name="addr">IP address of the bulb.</param>
        public Bulb(IPAddress addr) : this(addr, DefaultPort, 2000)
        {
        }

        /// <summary>
        /// Instantiate a new bulb object.
        /// </summary>
        /// <param name="addr">IP address of the bulb.</param>
        public Bulb(string addr) : this(IPAddress.Parse(addr), DefaultPort, 2000)
        {
        }

        /// <summary>
        /// Instantiate a new bulb object.
        /// </summary>
        /// <param name="addr">IP address of the bulb.</param>
        /// <param name="port">Port number for the bulb.</param>
        public Bulb(IPAddress addr, int port) : this(addr, port, 2000)
        {
        }

        /// <summary>
        /// Instantiate a new bulb object.
        /// </summary>
        /// <param name="addr">IP address of the bulb.</param>
        /// <param name="port">Port number for the bulb.</param>
        public Bulb(string addr, int port) : this(IPAddress.Parse(addr), port, 2000)
        {
        }

        /// <summary>
        /// Instantiate a new bulb object.
        /// </summary>
        /// <param name="addr">IP address of the bulb.</param>
        /// <param name="port">Port number for the bulb.</param>
        /// <param name="timeout">Timeout for bulb commands.</param>
        public Bulb(string addr, int port, int timeout) : this(IPAddress.Parse(addr), port, timeout)
        {
        }

        /// <summary>
        /// Instantiate a new bulb object.
        /// </summary>
        /// <param name="addr">IP address of the bulb.</param>
        /// <param name="port">Port number for the bulb.</param>
        /// <param name="timeout">Timeout for bulb commands.</param>
        public Bulb(IPAddress addr, int port, int timeout)
        {
            this.addr = addr;
            this.port = port;
            this.timeout = timeout;

            Settings = new BulbParams();

            GetPilot().ContinueWith((a) =>
            {
                if (MACAddress == MACAddress.None) return;

                Monitor.Enter(bulbCache);

                var smac = MACAddress;
                if (BulbCache.ContainsKey(smac))
                {
                    if (BulbCache[smac] != this)
                    {
                        BulbCache[smac] = this;
                    }
                }
                else
                {
                    BulbCache.Add(smac, this);
                }

                Monitor.Exit(bulbCache);
            });
        }

        public Bulb(MACAddress macAddr)
        {
            port = DefaultPort;

            Settings = new BulbParams();
            Settings.MACAddress = macAddr.Clone();

            Monitor.Enter(bulbCache);

            var smac = MACAddress;
            if (BulbCache.ContainsKey(smac))
            {
                if (BulbCache[smac] != this)
                {
                    BulbCache[smac] = this;
                }
            }
            else
            {
                BulbCache.Add(smac, this);
            }

            Monitor.Exit(bulbCache);

        }

        #endregion Public Constructors

        #region Public Properties

        public static Dictionary<MACAddress, Bulb> BulbCache
        {
            get => bulbCache;
            protected set
            {
                bulbCache = value;
            }
        }

        /// <summary>
        /// Gets or sets the brightness of the bulb
        /// using whole values between 10 and 100.
        /// </summary>
        /// <remarks>
        /// This property is live.
        /// </remarks>
        public virtual byte? Brightness
        {
            get => Settings?.Brightness;
            set
            {
                if (Settings == null)
                {
                    Settings = new BulbParams();
                }

                if (Settings.Brightness == value) return;

                Settings.Brightness = value;
                if (value == null) return;

                var stg = new BulbCommand(BulbMethod.SetPilot);
                stg.Params.Brightness = value;

                _ = SendCommand(stg);
            }
        }

        /// <summary>
        /// Gets the bulb type.
        /// </summary>
        public virtual string BulbType
        {
            get => bulbType;
            internal set
            {
                SetProperty(ref bulbType, value);
            }
        }

        /// <summary>
        /// Gets or sets the brightness of the bulb
        /// using whole values between 10 and 100.
        /// </summary>
        /// <remarks>
        /// This property is live.
        /// </remarks>
        public virtual Color? Color
        {
            get => Settings?.Color;
            set
            {
                if (Settings == null)
                {
                    Settings = new BulbParams();
                }

                if (Settings.Color == value) return;

                Settings.Color = value;
                if (value == null) return;

                var stg = new BulbCommand(BulbMethod.SetPilot);
                stg.Params.Color = value;

                _ = SendCommand(stg);
            }
        }

        public virtual int? Temperature
        {
            get => Settings?.Temperature;
            set
            {
                if (Settings == null)
                {
                    Settings = new BulbParams();
                }

                if (Settings.Temperature == value) return;

                Settings.Temperature = value;
                if (value == null) return;

                var stg = new BulbCommand(BulbMethod.SetPilot);

                stg.Params.Temperature = value;
                stg.Params.Scene = 0;

                _ = SendCommand(stg);
            }
        }

        public virtual int? HomeId
        {
            get => settings?.HomeId;
            set
            {
                if (Settings == null)
                {
                    Settings = new BulbParams();
                }

                if (Settings.HomeId == value) return;

                Settings.HomeId = value;
                OnPropertyChanged(nameof(HomeId));

                //if (value == null) return;

                //var stg = new BulbCommand(BulbMethod.SetSystemConfig);
                //stg.Params.HomeId = value;

                //_ = SendCommand(stg);
            }
        }

        public virtual string Icon
        {
            get => icon;
            set
            {
                SetProperty(ref icon, value);
            }
        }

        /// <summary>
        /// Gets the bulb's IP address.
        /// </summary>
        public virtual IPAddress IPAddress
        {
            get => addr;
            internal set
            {
                SetProperty(ref addr, value);
            }
        }

        /// <summary>
        /// Gets or sets whether or not the bulb is on.
        /// </summary>
        /// <remarks>
        /// This property is live.
        /// </remarks>
        public virtual bool? IsPoweredOn
        {
            get => Settings?.State;
            set
            {
                if (value == null) return;

                if (Settings == null)
                {
                    Settings = new BulbParams();
                }

                if (Settings.State == value) return;

                Settings.State = value;

                var stg = new BulbCommand(BulbMethod.SetPilot);
                stg.Params.State = value;

                _ = SendCommand(stg).ContinueWith((a) => _ = GetPilot());
            }
        }


        /// <summary>
        /// Gets or sets the light mode (scene Id)
        /// </summary>
        /// <remarks>
        /// This property is live.
        /// </remarks>
        public virtual LightMode LightMode
        {
            get => Settings?.LightModeInfo;
            set
            {
                if (Settings == null)
                {
                    Settings = new BulbParams();
                }

                if (Settings.Scene == value) return;

                Settings.Scene = value;
                if (value == null) return;

                var stg = new BulbCommand(BulbMethod.SetPilot);
                stg.Params.Scene = value;

                _ = SendCommand(stg);
            }
        }

        /// <summary>
        /// Gets or sets the MAC address for the bulb.
        /// </summary>
        [KeyProperty()]
        public virtual MACAddress MACAddress
        {
            get => Settings?.MACAddress ?? MACAddress.None;
            internal set
            {
                if (Settings == null)
                {
                    Settings = new BulbParams();
                }

                Settings.MACAddress = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of this bulb.
        /// </summary>
        public virtual string Name
        {
            get
            {
                if (name == null)
                {
                    name = ToString();
                }

                return name;
            }
            set
            {
                SetProperty(ref name, value);
            }
        }

        /// <summary>
        /// Gets or sets the communications port (default is 38899).
        /// </summary>
        public virtual int Port
        {
            get => port;
            set
            {
                SetProperty(ref port, value);
            }
        }

        public virtual int? RoomId
        {
            get => Settings?.RoomId;
            set
            {
                if (Settings == null)
                {
                    Settings = new BulbParams();
                }

                if (Settings.RoomId == value) return;

                Settings.RoomId = value;
                //if (value == null) return;

                //var stg = new BulbCommand(BulbMethod.SetSystemConfig);
                //stg.Params.RoomId = value;

                //_ = SendCommand(stg);
            }
        }

        /// <summary>
        /// Gets a description of the current lighting mode.
        /// </summary>
        public virtual string Scene
        {
            get
            {
                if (settings == null)
                {
                    return LightMode.LightModes[0].ToString();
                }
                else if (settings.Scene == null && settings.State == null)
                {
                    return AppResources.UnknownState;
                }
                else if (settings.State == false)
                {
                    return AppResources.Off;
                }
                else if ((settings.Scene ?? 0) == 0)
                {
                    if (settings.Color != null)
                    {
                        var c = (Color)settings.Color;
                        var s = LightMode.FindNameForColor(c);

                        if (s != null)
                        {
                            return $"{AppResources.CustomColor}: {s}";
                        }
                        else
                        {
                            return $"{AppResources.CustomColor}: RGB ({c.R}, {c.G}, {c.B})";
                        }
                    }
                    else if (settings.Temperature != null)
                    {
                        var t = (int)settings.Temperature;
                        
                        return $"{AppResources.WhiteLight}: {t} K";
                    }
                    else
                    {
                        return LightMode.LightModes[0].ToString();
                    }
                }
                else if (LightMode.LightModes.ContainsKey(settings.Scene ?? 0))
                {
                    return LightMode.LightModes[settings.Scene ?? 0].ToString();
                }
                else
                {
                    return LightMode.LightModes[0].ToString();
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="BulbParams"/> object used to configure this bulb.
        /// </summary>
        public virtual BulbParams Settings
        {
            get => settings;
            set
            {
                if (settings == value) return;

                if (settings != null)
                {
                    settings.PropertyChanged -= SettingsChanged;
                }

                settings = value;

                if (settings != null)
                {
                    settings.PropertyChanged += SettingsChanged;
                }

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the speed of the bulb
        /// using whole values between 10 and 100.
        /// </summary>
        /// <remarks>
        /// This property is live.
        /// </remarks>
        public virtual byte? Speed
        {
            get => Settings?.Speed;
            set
            {
                if (Settings == null)
                {
                    Settings = new BulbParams();
                }

                if (Settings.Speed == value) return;

                Settings.Speed = value;
                if (value == null) return;

                var stg = new BulbCommand(BulbMethod.SetPilot);
                stg.Params.Speed = value;

                _ = SendCommand(stg);
            }
        }


        /// <summary>
        /// Gets or sets the default timeout for commands (in milliseconds).
        /// </summary>
        public virtual int Timeout
        {
            get => timeout;
            set
            {
                SetProperty(ref timeout, value);
            }
        }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Create a <see cref="Bulb"/> object using only a MAC address.
        /// </summary>
        /// <param name="macAddr">MAC Address</param>
        /// <param name="scan">Condition on which to perform a network scan for bulbs.</param>
        /// <param name="localAddr">Local address for the scan.  If none is provided, one will be automatically selected.</param>
        /// <param name="localMac">Local hardware address for the scan.  If none is provided, one will be automatically selected.</param>
        /// <returns></returns>
        public static async Task<Bulb> GetBulbByMacAddress(string macAddr, ScanCondition scan, IPAddress localAddr = null, MACAddress? localMac = null)
        {
            return await GetBulbByMacAddress(MACAddress.Parse(macAddr), scan, localAddr, localMac);
        }

        /// <summary>
        /// Create a <see cref="Bulb"/> object using only a MAC address.
        /// </summary>
        /// <param name="macAddr">MAC Address</param>
        /// <param name="scan">Condition on which to perform a network scan for bulbs.</param>
        /// <param name="localAddr">Local address for the scan.  If none is provided, one will be automatically selected.</param>
        /// <param name="localMac">Local hardware address for the scan.  If none is provided, one will be automatically selected.</param>
        /// <returns></returns>
        public static async Task<Bulb> GetBulbByMacAddress(MACAddress macAddr, ScanCondition scan, IPAddress localAddr = null, MACAddress? localMac = null)
        {
            var smac = macAddr;

            if (scan != ScanCondition.Always)
            {
                if (BulbCache.ContainsKey(smac))
                {
                    return BulbCache[smac];
                }
            }

            if (scan == ScanCondition.Never) return new Bulb(macAddr);

            await ScanForBulbs(localAddr, localMac, ScanMode.GetSystemConfig, 1000);

            if (BulbCache.ContainsKey(smac))
            {
                return BulbCache[smac];
            }

            return new Bulb(macAddr);
        }

        /// <summary>
        /// Scan for bulbs on the default network.
        /// </summary>
        /// <param name="mode">The broadcast <see cref="ScanMode"/> to use when scanning.</param>
        /// <param name="timeout">Timeout for scan, in milliseconds.</param>
        /// <param name="callback">Callback function that is called for each discovered bulb.</param>
        /// <returns></returns>
        public static async Task<List<Bulb>> ScanForBulbs(
            ScanMode mode = ScanMode.GetSystemConfig,
            int timeout = 5000,
            BulbScanCallback callback = null)
        {
            return await ScanForBulbs(NetworkHelper.DefaultLocalIP, NetworkHelper.DefaultLocalMAC, mode, timeout, callback);
        }

        /// <summary>
        /// Scan for bulbs on the specified network.
        /// </summary>
        /// <param name="localAddr">The local IP address to bind to.</param>
        /// <param name="macAddr">The MAC address of the local interface being bound.</param>
        /// <param name="mode">The broadcast <see cref="ScanMode"/> to use when scanning.</param>
        /// <param name="timeout">Timeout for scan, in milliseconds.</param>
        /// <param name="callback">Callback function that is called for each discovered bulb.</param>
        /// <returns></returns>
        public static async Task<List<Bulb>> ScanForBulbs(
            IPAddress localAddr,
            MACAddress? macAddr,
            ScanMode mode = ScanMode.GetSystemConfig,
            int timeout = 5000,
            BulbScanCallback callback = null)
        {
            allUdpActive = true;

            List<Bulb> bulbs = new List<Bulb>();

            byte[] buffer = null;
            int port = DefaultPort;

            if (localAddr == null)
            {
                localAddr = NetworkHelper.DefaultLocalIP;
            }

            localAddr = localAddr.Clone();

            if (macAddr == null)
            {
                macAddr = (MACAddress)NetworkHelper.DefaultLocalMAC;
            }

            var udpClient = new UdpClient();

            udpClient.ExclusiveAddressUse = false;
            udpClient.Client.Bind(new IPEndPoint(localAddr, DefaultPort));
            udpClient.DontFragment = true;

            var from = new IPEndPoint(0, 0);
            var timeupVal = DateTime.Now.AddMilliseconds(timeout);

            var t = Task.Run(async () =>
            {
                int tdelc = 0;

                while (timeupVal > DateTime.Now)
                {
                    if (udpClient.Available > 0)
                    {
                        string json = null;
                        Bulb bulb = null;
                        BulbCommand p = null;

                        var recvBuffer = udpClient.Receive(ref from);
                        if (from.Address == localAddr) continue;

                        try
                        {
                            json = Encoding.UTF8.GetString(recvBuffer);

                            ConsoleHelper.LogInput(json, localAddr, from.Address);


                            if (json != null)
                            {
                                p = new BulbCommand(json);
                                if (p.Result?.MACAddress == null) continue;

                                var smac = p.Result?.MACAddress ?? MACAddress.None;

                                if (smac != MACAddress.None && BulbCache.ContainsKey(smac))
                                {
                                    bulb = BulbCache[smac];

                                    p.Result.CopyTo(bulb.Settings);

                                    bulb.IPAddress = from.Address;
                                    bulb.Port = from.Port;
                                }
                                else
                                {
                                    bulb = new Bulb(from.Address, from.Port);
                                    bulb.Settings = p.Result;
                                }

                                bool already = false;

                                foreach (var bchk in bulbs)
                                {
                                    if (bulb.Settings == null)
                                    {
                                        throw new ArgumentNullException();
                                    }

                                    if (bulb.Settings?.MACAddress.Equals(bchk.Settings?.MACAddress) ?? false)
                                    {
                                        already = true;
                                        break;
                                    }
                                }

                                if (already) continue;

                                bulbs.Add(bulb);

                                json = null;
                                p = null;

                                if (callback != null)
                                {
                                    callback(bulb);
                                }

                                bulb = null;
                            }
                        }
                        catch
                        {
                            continue;
                        }
                    }

                    await Task.Delay(10);

                    tdelc++;
                    if (tdelc >= 50)
                    {
                        udpClient.Send(buffer, buffer.Length, "255.255.255.255", port);
                        tdelc = 0;
                    }
                }
            });

            var pilot = new BulbCommand();

            if (mode == ScanMode.Registration)
            {
                pilot.Method = BulbMethod.Registration;
                pilot.Params.PhoneMac = macAddr.ToString().Replace(":", "").ToLower();
                pilot.Params.Register = false;
                pilot.Params.PhoneIp = localAddr.ToString();
                pilot.Params.Id = "12";
            }
            else if (mode == ScanMode.GetPilot)
            {
                pilot.Method = BulbMethod.GetPilot;
            }
            else
            {
                pilot.Method = BulbMethod.GetSystemConfig;
            }

            var data = pilot.AssembleCommand();

            ConsoleHelper.LogOutput(data, localAddr.ToString(), "255.255.255.255");

            buffer = Encoding.UTF8.GetBytes(data);

            udpClient.Send(buffer, buffer.Length, "255.255.255.255", port);

            await t;

            udpClient?.Close();
            udpClient?.Dispose();

            allUdpActive = false;

            return bulbs;
        }

        /// <summary>
        /// Sets the color and brightness of all the specified bulbs.
        /// </summary>
        /// <param name="bulbs">Bulbs to set.</param>
        /// <param name="c">New custom color</param>
        /// <param name="brightness">Brightness (a whole-number value between 10 and 100)</param>
        public static async Task SetLights(IEnumerable<Bulb> bulbs, Color c, byte? brightness = null)
        {
            BulbParams bp = new BulbParams();

            bp.Color = c;
            bp.Brightness = brightness;

            await SetLights(bulbs, bp);
        }

        /// <summary>
        /// Sets the light mode of all the specified bulbs.
        /// </summary>
        /// <param name="bulbs">Bulbs to set.</param>
        /// <param name="lm">LightMode to enable</param>
        /// <param name="brightness">Brightness (a whole-number value between 10 and 100)</param>
        ///
        public static async Task SetLights(IEnumerable<Bulb> bulbs, LightMode lm, byte? brightness = null, byte? speed = null, int? colorTemp = null)
        {
            BulbParams bp = new BulbParams();

            bp.Scene = lm.Code;
            bp.Brightness = brightness;
            bp.Speed = speed;
            bp.Temperature = colorTemp;

            bp.EnforceRules(lm.Type);

            await SetLights(bulbs, bp);
        }

        /// <summary>
        /// Sets the brightness of all the specified bulbs.
        /// </summary>
        /// <param name="bulbs">Bulbs to set.</param>
        /// <param name="brightness">Brightness (a whole-number value between 10 and 100)</param>
        public static async Task SetLights(IEnumerable<Bulb> bulbs, byte? brightness = null, byte? speed = null)
        {
            BulbParams bp = new BulbParams();

            bp.Brightness = brightness;
            bp.Speed = speed;

            await SetLights(bulbs, bp);
        }

        /// <summary>
        /// Set many bulbs with the same configuration.
        /// No wait for return, and no getPilot is called.
        /// </summary>
        /// <param name="bulbs">Bulbs to set.</param>
        /// <param name="bp">Configuration options</param>
        public static async Task SetLights(IEnumerable<Bulb> bulbs, BulbParams bp)
        {
            var cmd = new BulbCommand(BulbMethod.SetPilot);
            cmd.Params = bp;
            if (bp.Speed != null) bp.Id = "1";

            foreach (var b in bulbs)
            {
                await b.SendCommand(cmd);
            }
        }

        public async Task<Bulb> GetBulb()
        {
            return await Task.Run(() =>
            {
                return this;
            });
        }
        /// <summary>
        /// Refresh the bulb settings with 'getPilot'.
        /// </summary>
        /// <returns>True if successful.</returns>
        public virtual async Task<bool> GetPilot()
        {
            return await GetMethod(BulbMethod.GetPilot);
        }

        /// <summary>
        /// Refresh the bulb settings with 'getSystemConfig'.
        /// </summary>
        /// <returns>True if successful.</returns>
        public virtual async Task<bool> GetSystemConfig()
        {
            return await GetMethod(BulbMethod.GetSystemConfig);
        }

        /// <summary>
        /// Pulse the bulb
        /// </summary>
        /// <param name="delta">The dimming amount.</param>
        /// <param name="pulseTime">The length of the pulse (in milliseconds.)</param>
        public void Pulse(int delta = -50, int pulseTime = 250)
        {
            var cmd = new BulbCommand(BulbMethod.Pulse);
            cmd.Params = new BulbParams()
            {
                Delta = delta,
                Duration = pulseTime
            };

            _ = SendCommand(cmd);
        }

        /// <summary>
        /// Sets the light mode to the specified scene.
        /// </summary>
        /// <param name="scene">Light mode to enable.</param>
        /// <returns>Result from the bulb.</returns>
        public virtual async Task<BulbCommand> SetLightMode(LightMode scene)
        {
            var cmd = new BulbCommand(BulbMethod.SetPilot);

            if (scene.Settings != null)
            {
                cmd.Params = scene.Settings;
            }
            else
            {
                // set scene
                cmd.Params.State = true;
                cmd.Params.Scene = scene.Code;
            }

            var ret = await SendCommand(cmd);
            await GetPilot();
            return ret;
        }

        /// <summary>
        /// Sets the light mode to the specified scene and brightness.
        /// </summary>
        /// <param name="scene">Light mode to enable.</param>
        /// <param name="brightness">Brightness (a whole-number value between 10 and 100)</param>
        /// <returns>Result from the bulb.</returns>
        public virtual async Task<BulbCommand> SetLightMode(LightMode scene, byte brightness)
        {
            var cmd = new BulbCommand(BulbMethod.SetPilot);

            // set scene
            cmd.Params.State = true;
            cmd.Params.Brightness = brightness;
            cmd.Params.Scene = scene.Code;

            var ret = await SendCommand(cmd);
            await GetPilot();
            return ret;
        }

        /// <summary>
        /// Sets the light mode to the specified custom color.
        /// </summary>
        /// <param name="c">Color to enable.</param>
        /// <returns>Result from the bulb.</returns>
        public virtual async Task<BulbCommand> SetLightMode(Color c)
        {
            var cmd = new BulbCommand(BulbMethod.SetPilot);

            // set scene
            cmd.Params.State = true;
            cmd.Params.Scene = 0;
            cmd.Params.Red = c.R;
            cmd.Params.Green = c.G;
            cmd.Params.Blue = c.B;

            var ret = await SendCommand(cmd);
            await GetPilot();
            return ret;
        }

        /// <summary>
        /// Sets the light mode to the specified custom color and brightness.
        /// </summary>
        /// <param name="c">New custom color</param>
        /// <param name="brightness">Brightness (a whole-number value between 10 and 100)</param>
        /// <returns>Result from the bulb.</returns>
        public virtual async Task<BulbCommand> SetLightMode(Color c, byte brightness)
        {
            var cmd = new BulbCommand(BulbMethod.SetPilot);

            cmd.Params.State = true;
            cmd.Params.Brightness = brightness;
            cmd.Params.Scene = 0;
            cmd.Params.Red = c.R;
            cmd.Params.Green = c.G;
            cmd.Params.Blue = c.B;

            var ret = await SendCommand(cmd);
            await GetPilot();
            return ret;
        }

        /// <summary>
        /// Toggles the on/off state of the bulb.
        /// </summary>
        /// <returns>Result from the bulb.</returns>
        public virtual async Task<BulbCommand> Switch()
        {
            if (Settings == null)
            {
                Settings = new BulbParams();
            }

            if (Settings.State ?? false)
            {
                return await TurnOff();
            }
            else
            {
                return await TurnOn();
            }
        }

        /// <summary>
        /// Returns a string representation of the current object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(name))
            {
                return name;
            }
            else if (Settings?.MACAddress != null)
            {
                return Settings?.MACAddress?.ToString();
            }
            else
            {
                return addr?.ToString();
            }
        }

        /// <summary>
        /// Turn the bulb off.
        /// </summary>
        /// <returns>Result from the bulb.</returns>
        public virtual async Task<BulbCommand> TurnOff()
        {
            var cmd = new BulbCommand(BulbMethod.SetPilot);

            cmd.Params.State = false;
            var ret = await SendCommand(cmd);
            await GetPilot();
            return ret;
        }

        /// <summary>
        /// Turn the bulb on.
        /// </summary>
        /// <returns>Result from the bulb.</returns>
        public virtual async Task<BulbCommand> TurnOn()
        {
            var cmd = new BulbCommand(BulbMethod.SetPilot);

            cmd.Params.State = true;
            var ret = await SendCommand(cmd);
            await GetPilot();
            return ret;
        }

        #endregion Public Methods

        #region Internal Methods

        /// <summary>
        /// Run a 'get' method on the bulb.
        /// </summary>
        /// <param name="m">The method to run (must be a 'get' method)</param>
        /// <returns>True if successful.</returns>
        internal async Task<bool> GetMethod(BulbMethod m)
        {
            if (m.IsSetMethod || m.IsInboundOnly) return false;

            var cmd = new BulbCommand(m);
            string json;

            try
            {
                cmd.Params?.EnforceRules();

                json = cmd.AssembleCommand();
                json = await SendCommand(json);
            }
            catch
            {
                return false;
            }

            if (string.IsNullOrEmpty(json)) return false;

            if (Settings != null)
            {
                cmd.Result = Settings;
                cmd.Result.ClearPilot();

                JsonConvert.PopulateObject(json, cmd, BulbCommand.DefaultJsonSettings);
            }
            else
            {
                cmd = JsonConvert.DeserializeObject<BulbCommand>(json, BulbCommand.DefaultJsonSettings);
                Settings = cmd.Result;
            }

            OnPropertyChanged(nameof(Scene));
            OnPropertyChanged(nameof(LightMode));
            return true;
        }

        #endregion Internal Methods

        #region Protected Methods

        protected async Task<BulbCommand> SendCommand(BulbCommand cmd)
        {
            if (cmd.Method == BulbMethod.SetPilot)
            {
                cmd.Params?.EnforceRules();
            }

            var x = await SendCommand(cmd.AssembleCommand());
            return new BulbCommand(x);
        }

        protected async Task<string> SendCommand(string cmd)
        {
            ConsoleHelper.LogOutput(cmd, null, IPAddress);

            byte[] bOut = Encoding.UTF8.GetBytes(cmd);
            var buffer = await SendUDP(bOut);

            string json;

            if (buffer?.Length > 0)
            {
                json = Encoding.UTF8.GetString(buffer).Trim('\x0');
                ConsoleHelper.LogInput(json, null, IPAddress);

                return json;
            }
            else
            {
                return null;
            }
        }

        protected async Task<byte[]> SendUDP(byte[] cmd, IPAddress localAddr = null)
        {
            if (Port <= 0 || IPAddress == null) return null;

            while (allUdpActive || udpActive)
            {
                await Task.Delay(10);
            }

            udpActive = true;

            List<Bulb> bulbs = new List<Bulb>();

            byte[] buffer = cmd;
            int port = DefaultPort;

            localAddr = localAddr?.Clone();
            var addr = this.addr.Clone();

            var udpClient = new UdpClient();

            udpClient.ExclusiveAddressUse = false;

            if (localAddr != null)
            {
                udpClient.Client.Bind(new IPEndPoint(localAddr, DefaultPort));
            }

            udpClient.DontFragment = true;

            var from = new IPEndPoint(0, 0);
            var timeupVal = DateTime.Now.AddMilliseconds(timeout);

            var t = Task.Run(async () =>
            {
                int tdelc = 0;

                while (timeupVal > DateTime.Now)
                {
                    if (udpClient.Available > 0)
                    {
                        try
                        {
                            var recvBuffer = udpClient.Receive(ref from);
                            if (addr.ToString() == from.Address.ToString())
                            {
                                buffer = recvBuffer;
                                return;
                            }
                        }
                        catch
                        {
                            continue;
                        }
                    }

                    await Task.Delay(10);
                    tdelc++;

                    if (tdelc >= 50)
                    {
                        udpClient.Send(buffer, buffer.Length, addr.ToString(), port);
                        tdelc = 0;
                    }
                }
            });

            udpClient.Send(buffer, buffer.Length, addr.ToString(), port);
            await t;

            udpClient?.Close();
            udpClient?.Dispose();

            udpActive = false;

            return buffer;
        }

        protected void SettingsChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(BulbParams.Brightness))
            {
                OnPropertyChanged(nameof(Brightness));
            }
            else if (e.PropertyName == nameof(BulbParams.Speed))
            {
                OnPropertyChanged(nameof(Speed));
            }
            else if (e.PropertyName == nameof(BulbParams.State))
            {
                OnPropertyChanged(nameof(IsPoweredOn));
            }
            else if (e.PropertyName == nameof(BulbParams.MACAddress))
            {
                OnPropertyChanged(nameof(MACAddress));
            }
            else if (e.PropertyName == nameof(BulbParams.Color))
            {
                OnPropertyChanged(nameof(Color));
            }
            else if (e.PropertyName == nameof(BulbParams.LightModeInfo))
            {
                OnPropertyChanged(nameof(LightMode));
            }
        }

        #endregion Protected Methods
    }
}