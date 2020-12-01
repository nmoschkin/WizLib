using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace WizLib
{
    public class BulbItem : IBulb
    {
        [JsonProperty("mac")]
        public virtual PhysicalAddress MACAddress { get; protected set; }

        [JsonProperty("addr")]
        public virtual IPAddress IPAddress { get; protected set; }

        [JsonProperty("port")]
        public virtual int Port { get; protected set; }

        [JsonProperty("name")]
        public virtual string Name { get; set; }

        [JsonProperty("icon")]
        public virtual string Icon { get; set; }

        public static BulbItem CreateItemFromBulb(IBulb source)
        {
            return new BulbItem()
            {
                MACAddress = source.MACAddress,
                IPAddress = source.IPAddress,
                Port = source.Port,
                Name = source.Name
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
            return await GetBulb(ScanCondition.NotFound);
        }

        public async Task<Bulb> GetBulb(ScanCondition sc)
        {
            if (Bulb.BulbCache.ContainsKey(MACAddress.ToString()))
                return Bulb.BulbCache[MACAddress.ToString()];

            Bulb b;

            b = await Bulb.GetBulbByMacAddress(MACAddress, sc);

            if (b == null)
            {
                b = new Bulb(IPAddress, Port);
            }

            await b?.GetPilot();
            return b;
        }

    }

}
