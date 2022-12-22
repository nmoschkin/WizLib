using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Net;
using Newtonsoft.Json;
using WiZ.Observable;

namespace WiZ.Contracts
{
    public interface IBulb
    {
        /// <summary>
        /// The MAC (Physical) address of the bulb's network interface.
        /// </summary>
        [KeyProperty]
        MACAddress MACAddress { get; }

        /// <summary>
        /// The IP address of the bulb.
        /// </summary>
        [JsonConverter(typeof(IPAddressConverter))]
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
        /// Get a <see cref="IBulbController"/> object from this interface.
        /// </summary>
        /// <returns></returns>
        Task<IBulbController> GetBulbController();

        /// <summary>
        /// Get a <see cref="Bulb"/> object from this interface.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IBulbController"/> to create.</typeparam>
        /// <returns></returns>
        Task<T> GetBulbController<T>() where T : IBulbController;

        /// <summary>
        /// Returns true if the bulb controller can be retrieved.
        /// </summary>
        bool CanGetBulbController { get; }
    }
}