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

namespace WizLib.Profiles
{

    public class Home : ObservableBase
    {

        private int homeId;
        private string name;

        private KeyedObservableCollection<int, Room> rooms = new KeyedObservableCollection<int, Room>(nameof(Room.RoomId));

        [JsonProperty("rooms")]
        public KeyedObservableCollection<int, Room> Rooms
        {
            get => rooms;
            set
            {
                SetProperty(ref rooms, value);
            }
        }

        [JsonProperty("homeId")]
        public int HomeId
        {
            get => homeId;
            set
            {
                SetProperty(ref homeId, value);
            }
        }

        [JsonProperty("name")]
        public string Name
        {
            get => name;
            set
            {
                SetProperty(ref name, value);
            }
        }

        public static KeyedObservableCollection<int, Home> GenerateHomes(IEnumerable<Bulb> bulbs)
        {
            Room nr;
            Home nh;

            var kh = new KeyedObservableCollection<int, Home>(nameof(HomeId));

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

                            if (smac != null && nr.Bulbs.ContainsKey(smac))
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

    }
}
