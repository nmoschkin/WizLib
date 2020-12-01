using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace WizLib.Profiles
{
    public class Profile : ObservableBase, IProfile
    {
        private Guid projectId;

        private List<BulbItem> bulbs;

        private List<Home> homes;

        private List<LightMode> lightModes;

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

            bulbs = new List<BulbItem>();
            homes = new List<Home>();
            lightModes = new List<LightMode>();
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
        public List<Home> Homes
        {
            get => homes;
            set
            {
                SetProperty(ref homes, value);
            }
        }

        [JsonProperty("lightModes")]
        public List<LightMode> CustomLightModes
        {
            get => lightModes;
            set
            {
                SetProperty(ref lightModes, value);
            }
        }

        [JsonProperty("bulbs")]
        public List<BulbItem> Bulbs
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
                if (b.MACAddress == null) continue;

                foreach (var b2 in this.bulbs)
                {
                    if (b2.MACAddress == null) continue;

                    if (b.MACAddress.ToString() == b2.MACAddress.ToString())
                    {
                        b2.Name = b.Name;
                        b2.IPAddress = b.IPAddress.Clone();
                        b2.Icon = b.Icon;
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

                for (i = c; i >= 0; i--)
                {
                    var b = this.bulbs[i];
                    bchk = false;
                    foreach (var b2 in bulbs)
                    {
                        if (b.MACAddress.ToString() == b2.MACAddress.ToString())
                        {
                            bchk = true;
                            break;
                        }
                    }

                    if (!bchk)
                    {
                        this.bulbs.RemoveAt(i);
                    }
                }
            }

        }
    }
}
