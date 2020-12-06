using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Net;
using Newtonsoft.Json;

namespace WizLib.Profiles
{
    public interface IBulb
    {
        MACADDRESS MACAddress { get; }

        IPAddress IPAddress { get; }
        
        int? HomeId { get; }

        int? RoomId { get; }

        int Port { get; }

        string Name { get; set; }

        string Icon { get; set; }

        Task<Bulb> GetBulb();

    }

}
