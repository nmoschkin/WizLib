using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using WiZ.Contracts;
using WiZ.Helpers;
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

        public static Task<TList> CreateBulbsFromInterfaceList<TList, TBulb>(IEnumerable<IBulb> source)
            where TList : IList<TBulb>, new()
            where TBulb : IBulbController
        {
            return Task.Run(() =>
            {
                var l = new TList();

                Parallel.ForEach(source, async (b) =>
                {
                    var res = await b.GetBulbController<TBulb>();

                    lock (l)
                    {
                        l.Add(res);
                    }
                });

                return l;
            });
        }

        public async Task<IBulbController> GetBulbController()
        {
            var b = new Bulb(MACAddress.None);
            await GetBulb(b, ScanCondition.Never);
            return b;
        }

        public async Task<T> GetBulbController<T>() where T : IBulbController
        {
            var b = BulbFactory.Instance.CreateBulb<T>(MACAddress);
            await GetBulb(b, ScanCondition.Never);
            return b;
        }

        public bool CanGetBulbController => true;

        protected async Task GetBulb(IBulbController b, ScanCondition sc)
        {
            if (Bulb.BulbCache.ContainsKey(MACAddress))
            {
                b = Bulb.BulbCache[MACAddress];

                b.Name = Name;
                b.Icon = Icon;
                b.HomeId = HomeId;
                b.RoomId = RoomId;

                return;
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
        }

        public override string ToString()
        {
            return Name ?? MACAddress.ToString();
        }
    }
}