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
    public class Profile : ObservableBase, IProfile
    {
        private Guid projectId;

        private ObservableDictionary<MACAddress, BulbItem> bulbs;

        private ObservableDictionary<int, Home> homes;

        private ObservableDictionary<int, LightMode> lightModes;

        private string name;


        public Profile(string name) : this(name, Guid.NewGuid())
        {
        }

        public Profile(Guid projectId) : this(null, projectId)
        {
        }

        public Profile() : this(null, Guid.NewGuid())
        {
        }


        public Profile(string name, Guid projectId)
        {
            this.projectId = projectId;
            this.name = name;

            bulbs = new ObservableDictionary<MACAddress, BulbItem>();
            homes = new ObservableDictionary<int, Home>();
            lightModes = new ObservableDictionary<int, LightMode>();
        }

        [JsonProperty("projectId")]
        public Guid ProjectId
        {
            get => projectId;
            set
            {
                SetProperty(ref projectId, value);
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

        [JsonProperty("homes")]
        public ObservableDictionary<int, Home> Homes
        {
            get => homes;
            set
            {
                SetProperty(ref homes, value);
            }
        }

        public Room FindRoomById(int roomId)
        {
            foreach (Home h in Homes)
            {
                foreach (Room r in h.Rooms)
                {
                    if (r.RoomId == roomId)
                    {
                        return r;
                    }
                }
            }

            return null;
        }

        public Home FindHomeById(int homeId)
        {
            foreach (Home h in Homes)
            {
                if (h.HomeId == homeId)
                {
                    return h;
                }
            }

            return null;
        }

        public Room MatchBulbsToRoom(IEnumerable<IBulb> bulbs)
        {
            List<IBulb> blook1 = new List<IBulb>(bulbs);
            List<IBulb> blook2;

            blook1.Sort((a, b) =>
            {
                return a.MACAddress.CompareTo(b.MACAddress);
            });

            foreach (Home h in Homes)
            {
                foreach (Room r in h.Rooms)
                {
                    
                    blook2 = new List<IBulb>(r.Bulbs.Values);
                    if (blook2.Count != blook1.Count) continue;

                    blook2.Sort((a, b) =>
                    {
                        return a.MACAddress.CompareTo(b.MACAddress);
                    });

                    bool bmatch = true;
                    int i, c = blook1.Count;

                    for (i = 0; i < c; i++)
                    {

                        if (!blook1[i].MACAddress.Equals(blook2[i].MACAddress))
                        {
                            bmatch = false;
                            break;
                        }
                    }

                    if (bmatch)
                    {
                        return r;
                    }

                }
            }

            return null;
        }


        public Home MatchBulbsToHome(IEnumerable<IBulb> bulbs)
        {
            List<IBulb> blook1 = new List<IBulb>(bulbs);
            List<IBulb> blook2;

            blook1.Sort((a, b) =>
            {
                return a.MACAddress.CompareTo(b.MACAddress);
            });

            foreach (Home h in Homes)
            {

                blook2 = new List<IBulb>(h.GetAllBulbsInHome());
                if (blook2.Count != blook1.Count) continue;

                blook2.Sort((a, b) =>
                {
                    return a.MACAddress.CompareTo(b.MACAddress);
                });

                bool bmatch = true;
                int i, c = blook1.Count;

                for (i = 0; i < c; i++)
                {

                    if (!blook1[i].MACAddress.Equals(blook2[i].MACAddress))
                    {
                        bmatch = false;
                        break;
                    }
                }

                if (bmatch)
                {
                    return h;
                }

            }

            return null;
        }


        [JsonProperty("lightModes")]
        public ObservableDictionary<int, LightMode> CustomLightModes
        {
            get => lightModes;
            set
            {
                SetProperty(ref lightModes, value);
            }
        }

        [JsonProperty("bulbs")]
        public ObservableDictionary<MACAddress, BulbItem> Bulbs
        {
            get => bulbs;
            set
            {
                SetProperty(ref bulbs, value);
            }
        }

        public void BuildUpdateProfile(IEnumerable<IBulb> bulbs, bool removeMissing)
        {

            bool bchk;

            List<int> fhomes = new List<int>();
            List<int> frooms = new List<int>();

            foreach (var b in bulbs)
            {
                bchk = false;
                if (b.MACAddress == MACAddress.None) continue;

                foreach (var b2 in this.bulbs)
                {
                    if (b2.MACAddress == MACAddress.None) continue;

                    if (b.MACAddress == b2.MACAddress)
                    {
                        b2.Name = b.Name;
                        b2.IPAddress = b.IPAddress.Clone();
                        b2.Icon = b.Icon;
                        b2.HomeId = b.HomeId;
                        b2.RoomId = b.RoomId;
                        bchk = true;

                        break;
                    }
                }

                if (!bchk)
                {
                    this.bulbs.Add(BulbItem.CreateItemFromBulb(b));
                }

                if (b.HomeId is int hh)
                {
                    if (!fhomes.Contains(hh)) fhomes.Add(hh);

                    var h = FindHomeById(hh);

                    if (h == null) 
                    {
                        h = new Home() { HomeId = hh };
                        Homes.Add(h);
                    }

                    if (b.RoomId is int rr)
                    {
                        if (!frooms.Contains(rr)) frooms.Add(rr);

                        var r = FindRoomById(rr);

                        if (r == null)
                        {
                            r = new Room() { RoomId = rr };
                            h.Rooms.Add(r);
                        }
                    }
                }
            }

            if (removeMissing)
            {
                int c = this.bulbs.Count - 1;
                int i;
                var bwalk = this.bulbs as IList<BulbItem>;

                for (i = c; i >= 0; i--)
                {
                    var b = bwalk[i];

                    bchk = false;
                    foreach (var b2 in bulbs)
                    {
                        if (b.MACAddress == b2.MACAddress)
                        {
                            bchk = true;
                            break;
                        }
                    }

                    if (!bchk)
                    {
                        bwalk.RemoveAt(i);
                    }
                }

                c = Homes.Count - 1;
                var hv = Homes as IList<Home>;

                for (i = c; i >= 0; i--)
                {
                    if (!fhomes.Contains(hv[i].HomeId))
                    {
                        hv.RemoveAt(i);
                        continue;
                    }
                    else
                    {
                        var hr = hv[i].Rooms as IList<Room>;
                        int j, d = hr.Count - 1;

                        for (j = d; j >= 0; j--)
                        {
                            if (!frooms.Contains(hr[j].RoomId))
                            {
                                hr.RemoveAt(j);
                                continue;
                            }
                        }
                    }
                }
            }
        }
    }
}
