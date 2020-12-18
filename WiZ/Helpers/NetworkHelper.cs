using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WiZ
{
    /// <summary>
    /// Networking Helpers
    /// </summary>
    public static class NetworkHelper
    {
        private static IPAddress defAddr;
        private static MACAddress defMac;

        /// <summary>
        /// The default remote IP address to use to test internet connectivity.
        /// Default is OpenDNS primary name server.
        /// </summary>
        /// <remarks>
        /// This IP address must point to a name server.
        /// </remarks>
        public static IPAddress DefaultPingIP { get; set; } = IPAddress.Parse("208.67.222.222");

        /// <summary>
        /// The default local address to bind to for UDP calls to bulbs.
        /// </summary>
        public static IPAddress DefaultLocalIP
        {
            get
            {
                if (defAddr == null)
                {
                    RefreshDefaultIP();
                }

                return defAddr;
            }
            set
            {

            }
        }

        /// <summary>
        /// The default local interface to bind to for UDP calls to bulbs.
        /// </summary>
        public static MACAddress DefaultLocalMAC
        {
            get
            {
                if (defMac == MACAddress.None)
                {
                    RefreshDefaultIP();
                }

                return defMac;
            }
        }

        static NetworkHelper()
        {
            RefreshDefaultIP();
        }

        /// <summary>
        /// Find the first connection with internet access.
        /// </summary>
        /// <returns></returns>
        public static void RefreshDefaultIP()
        {
            var locals = GetLocalAddresses(true);

            foreach (var local in locals)
            {
                foreach (var locaddr in local.Value)
                {
                    defAddr = locaddr;
                    defMac = local.Key;

                    return;
                }
            }
        }

        /// <summary>
        /// Gets the status of the internet connection for the local machine.
        /// </summary>
        /// <returns>True if there is internet.</returns>
        public static async Task<bool> GetHasInternet() 
        {
            var p = new Ping();
            var reply = await p.SendPingAsync(DefaultPingIP);

            return (reply.Status == IPStatus.Success);
        }

        /// <summary>
        /// Gets an dictionary of all active local network interfaces keyed by <see cref="MACAddress"/>.
        /// </summary>
        /// <param name="withGatewaysOnly">Only include addresses with default gateways.</param>
        /// <returns></returns>
        public static Dictionary<MACAddress, List<IPAddress>> GetLocalAddresses(bool withGatewaysOnly = false)
        {
            var net = NetworkInterface.GetAllNetworkInterfaces();

            Dictionary<MACAddress, List<IPAddress>> addrs = new Dictionary<MACAddress, List<IPAddress>>();
            KeyValuePair<MACAddress, List<IPAddress>> kvp;

            foreach (var iface in net)
            {
                var ipprops = iface.GetIPProperties();
                var gpass = false;

                if (withGatewaysOnly)
                {
                    foreach (var g in ipprops.GatewayAddresses)
                    {
                        if (g.Address.AddressFamily == AddressFamily.InterNetwork && g.Address != IPAddress.Any)
                        {
                            gpass = true;
                            break;
                        }
                    }
                }

                if (!withGatewaysOnly || gpass)
                {
                    kvp = new KeyValuePair<MACAddress, List<IPAddress>>((MACAddress)iface.GetPhysicalAddress(), new List<IPAddress>());
                    
                    foreach (var la in ipprops.UnicastAddresses)
                    {
                        if (la.Address.AddressFamily == AddressFamily.InterNetwork && la.Address != IPAddress.Any)
                        {
                            kvp.Value.Add(la.Address);
                            break;
                        }
                    }

                    addrs.Add(kvp.Key, kvp.Value);
                }

            }

            return addrs;
        }
    }

}
