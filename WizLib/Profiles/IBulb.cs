using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Net;
using Newtonsoft.Json;

namespace WizLib
{
    public interface IBulb
    {
        PhysicalAddress MACAddress { get; }

        IPAddress IPAddress { get; }

        int Port { get; }

        string Name { get; set; }
        
        string Icon { get; set; }
        
        Task<Bulb> GetBulb();

    }

}
