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

namespace WizLib
{

    public class Home : ViewModelBase
    {

        private string homeId;
        private string name;

        private KeyedObservableCollection<Room> rooms = new KeyedObservableCollection<Room>(nameof(Room.RoomId));

        [JsonProperty("rooms")]
        public KeyedObservableCollection<Room> Rooms
        {
            get => rooms;
            set
            {
                SetProperty(ref rooms, value);
            }
        }

        [JsonProperty("homeId")]
        public string HomeId
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

        public static KeyedObservableCollection<Home> GenerateHomes(IEnumerable<Bulb> bulbs)
        {
            Room nr;
            Home nh;

            var kh = new KeyedObservableCollection<Home>(nameof(HomeId));

            foreach (var b in bulbs)
            {
                if (b.Settings?.HomeId != null && b.Settings?.RoomId != null)
                {
                    var hid = b.Settings.HomeId.Value.ToString();
                    var rid = b.Settings.RoomId.Value.ToString();

                    if (kh.ContainsKey(hid, out nh))
                    {
                        if (nh.Rooms.ContainsKey(rid, out nr))
                        {
                            if (nr.Bulbs.ContainsKey(b.MACAddress))
                            {
                                nr.Bulbs[b.MACAddress] = b;
                            }
                            else
                            {
                                nr.Bulbs.Add(b);
                            }
                        }
                        else
                        {
                            nr = new Room() { RoomId = b.Settings.RoomId.ToString() };

                            nr.Bulbs.Add(b);
                            nh.Rooms.Add(nr);
                        }
                    }
                    else
                    {
                        nh = new Home() { HomeId = b.Settings.HomeId.ToString() };
                        nr = new Room() { RoomId = b.Settings.RoomId.ToString() };

                        nr.Bulbs.Add(b);
                        nh.Rooms.Add(nr);

                        kh.Add(nh);
                    }
                }
            }

            return kh;
        }

    }
}
