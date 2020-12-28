using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Net;
using Newtonsoft.Json;

namespace WiZ.Profiles
{
    public interface IBulb
    {
        /// <summary>
        /// The MAC (PHysical) address of the bulb's network interface.
        /// </summary>
        MACAddress MACAddress { get; }

        /// <summary>
        /// The IP address of the bulb.
        /// </summary>
        IPAddress IPAddress { get; }
        
        /// <summary>
        /// The home ID of the bulb.
        /// </summary>
        int? HomeId { get; }


        /// <summary>
        /// The room Id of the bulb.
        /// </summary>
        int? RoomId { get; }
        
        /// <summary>
        /// The default network port 
        /// </summary>
        int Port { get; }

        /// <summary>
        /// The name of the bulb.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The name of the bulb icon.
        /// </summary>
        string Icon { get; set; }

        /// <summary>
        /// Get a <see cref="Bulb"/> object from this interface.
        /// </summary>
        /// <returns></returns>
        Task<Bulb> GetBulb();

    }

}
