using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using WiZ.Observable;

namespace WiZ.Profiles
{
    public class BulbItem : IBulb
    {
        [KeyProperty]
        [JsonProperty("mac")]
        public virtual MACAddress MACAddress { get; set; }

        [JsonProperty("addr")]
        public virtual IPAddress IPAddress { get; set; }

        [JsonProperty("port")]
        public virtual int Port { get; set; }

        [JsonProperty("name")]
        public virtual string Name { get; set; }

        [JsonProperty("icon")]
        public virtual string Icon { get; set; }

        [JsonProperty("roomId")]
        public int? RoomId { get; set; }

        [JsonProperty("homeId")]
        public int? HomeId { get; set; }

        public static BulbItem CreateItemFromBulb(IBulb source)
        {
            return new BulbItem()
            {
                MACAddress = source.MACAddress,
                IPAddress = source.IPAddress,
                Port = source.Port,
                Name = source.Name,
                HomeId = source.HomeId,
                RoomId = source.RoomId
            };
        }

        public static async Task<IList<Bulb>> CreateBulbsFromInterfaceList(IEnumerable<IBulb> source)
        {
            var l = new List<Bulb>();

            foreach (var b in source)
            {
                l.Add(await b.GetBulb());
            }

            return l;
        }

        public async Task<Bulb> GetBulb()
        {
            return await GetBulb(ScanCondition.Never);
        }

        public async Task<Bulb> GetBulb(ScanCondition sc)
        {
            Bulb b;

            if (Bulb.BulbCache.ContainsKey(MACAddress))
            {

                b = Bulb.BulbCache[MACAddress];

                b.Name = Name;
                b.Icon = Icon;
                b.HomeId = HomeId;
                b.RoomId = RoomId;

                return b;
            }

            b = await Bulb.GetBulbByMacAddress(MACAddress, sc);

            if (b == null)
            {
                b = new Bulb(IPAddress, Port);
            }

            b.Name = Name;
            b.Icon = Icon;
            b.HomeId = HomeId;
            b.RoomId = RoomId;

            return b;
        }


        public override string ToString()
        {
            return Name ?? MACAddress.ToString();
        }
    }

}
