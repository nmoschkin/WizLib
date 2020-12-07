using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using WizLib.Observable;

namespace WizLib.Profiles
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

        public void AddUpdateBulbs(IEnumerable<IBulb> bulbs, bool removeMissing)
        {

            bool bchk;
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
            }

        }
    }
}
