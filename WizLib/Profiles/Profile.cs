using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WizLib
{
    public class Profile : ObservableBase, IProfile
    {
        private Guid projectId;
        
        private IList<BulbItem> bulbs;

        private IList<Home> homes;

        private IList<LightMode> lightModes;

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
        public IList<Home> Homes
        {
            get => homes;
            set
            {
                SetProperty(ref homes, value);
            }
        }

        [JsonProperty("lightModes")]
        public IList<LightMode> CustomLightModes
        {
            get => lightModes;
            set
            {
                SetProperty(ref lightModes, value);
            }
        }

        [JsonProperty("bulbs")]
        public IList<BulbItem> Bulbs
        {
            get => bulbs;
            set
            {
                SetProperty(ref bulbs, value);
            }
        }

    }
}
