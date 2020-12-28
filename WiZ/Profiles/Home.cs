using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using Newtonsoft.Json;
using System.Collections;
using WiZ.Observable;

namespace WiZ.Profiles
{

    public class Home : ObservableBase
    {

        private int homeId;
        private string name;

        private ObservableDictionary<int, Room> rooms = new ObservableDictionary<int, Room>(nameof(Room.RoomId));

        public static ObservableDictionary<int, Home> HomeCache { get; private set; } = new ObservableDictionary<int, Home>();


        [JsonProperty("rooms")]
        public ObservableDictionary<int, Room> Rooms
        {
            get => rooms;
            set
            {
                SetProperty(ref rooms, value);
            }
        }

        [KeyProperty]
        [JsonProperty("homeId")]
        public int HomeId
        {
            get => homeId;
            set
            {

                if (SetProperty(ref homeId, value))
                {
                    if (HomeCache.ContainsKey(homeId))
                    {
                        HomeCache[homeId] = this;
                    }
                    else
                    {
                        HomeCache.Add(this);
                    }
                }
            }
        }

        [JsonProperty("name")]
        public string Name
        {
            get => name ?? homeId.ToString();
            set
            {
                SetProperty(ref name, value);
            }
        }

        public IList<IBulb> GetAllBulbsInHome()
        {
            var l = new List<IBulb>();

            foreach (var r in rooms)
            {
                l.AddRange(r.Bulbs.Values);
            }

            return l;

        }
        public static ObservableDictionary<int, Home> GenerateHomes(IEnumerable<Bulb> bulbs)
        {
            Room nr;
            Home nh;

            var kh = new ObservableDictionary<int, Home>(nameof(HomeId));

            foreach (var b in bulbs)
            {
                if (b.Settings?.HomeId != null && b.Settings?.RoomId != null)
                {
                    var hid = (int)b.Settings.HomeId;
                    var rid = (int)b.Settings.RoomId;

                    var nb = BulbItem.CreateItemFromBulb(b);

                    if (kh.ContainsKey(hid, out nh))
                    {

                        if (nh.Rooms.ContainsKey(rid, out nr))
                        {
                            var smac = b.MACAddress;

                            if (smac != MACAddress.None && nr.Bulbs.ContainsKey(smac))
                            {
                                nr.Bulbs[smac] = nb;
                            }
                            else
                            {
                                nr.Bulbs.Add(nb);
                            }
                        }
                        else
                        {
                            nr = new Room() { RoomId = b.Settings.RoomId ?? -1 };

                            nr.Bulbs.Add(nb);
                            nh.Rooms.Add(nr);
                        }
                    }
                    else
                    {
                        nh = new Home() { HomeId = b.Settings.HomeId ?? -1 };
                        nr = new Room() { RoomId = b.Settings.RoomId ?? -1 };

                        nr.Bulbs.Add(nb);
                        nh.Rooms.Add(nr);

                        kh.Add(nh);
                    }
                }
            }

            return kh;
        }

        public override string ToString()
        {
            return name ?? homeId.ToString();
        }


    }
}
