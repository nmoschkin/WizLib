using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WizLib
{
    public static class NetworkHelper
    {
        private static IPAddress defLocal;

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
        public static IPAddress LocalAddress
        {
            get => defLocal;
            set => defLocal = value;
        }

        static NetworkHelper()
        {
            LocalAddress = FindDefaultIP();
        }

        /// <summary>
        /// Find the first connection with internet access from a collection of local IP addresses.
        /// If a collection is not provided, it will retrieved from the system.
        /// </summary>
        /// <param name="locals">Collection of local IP addresses.</param>
        /// <returns></returns>
        public static IPAddress FindDefaultIP(IEnumerable<IPAddress> locals = null)
        {
            if (locals == null)
            {
                locals = GetLocalAddresses();
            }

            IPAddress ret = null;
            Socket sock;

            foreach (var locaddr in locals)
            {
                sock = null;

                try
                {
                    sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    sock.Bind(new IPEndPoint(locaddr.Clone(), 53));
                    sock.Connect(new IPEndPoint(DefaultPingIP.Clone(), 53));
                }
                catch
                {
                    continue;
                }

                ret = locaddr;

                sock?.Close();
                sock = null;

                break;
            }

            return ret;
        }

        /// <summary>
        /// Gets an array of all available local IPv4 addresses
        /// </summary>
        /// <param name="withGatewaysOnly">Only include addresses with default gateways.</param>
        /// <returns></returns>
        public static IPAddress[] GetLocalAddresses(bool withGatewaysOnly = false)
        {
            var net = NetworkInterface.GetAllNetworkInterfaces();

            IPAddress addr;
            List<IPAddress> addrs = new List<IPAddress>();

            foreach (var iface in net)
            {
                var ipprops = iface.GetIPProperties();
                var gpass = false;

                addr = null;
                
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
                    foreach (var la in ipprops.UnicastAddresses)
                    {
                        if (la.Address.AddressFamily == AddressFamily.InterNetwork && la.Address != IPAddress.Any)
                        {
                            addr = la.Address;
                            break;
                        }
                    }

                    if (addr != null)
                    {
                        addrs.Add(addr);
                    }
                }

            }

            return addrs.ToArray();
        }
    }

}
