using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using WiZ.Observable;

namespace WiZ.Profiles
{
    public class Scene : ObservableBase
    {

        private ObservableDictionary<PhysicalAddress, BulbParams> bp
            = new ObservableDictionary<PhysicalAddress, BulbParams>(nameof(WiZ.BulbParams.MACAddress));

        private Guid sceneId;

        private string name;

        [JsonProperty("name")]
        public string Name
        {
            get => name;
            set
            {
                SetProperty(ref name, value);
            }
        }

        [JsonProperty("sceneId")]
        public Guid SceneId
        {
            get => sceneId;
            set
            {
                SetProperty(ref sceneId, value);
            }
        }

        [JsonProperty("bulbs")]
        public ObservableDictionary<PhysicalAddress, BulbParams> BulbParams
        {
            get => bp;
            set
            {
                SetProperty(ref bp, value);
            }
        }

        public Scene()
        {
            SceneId = Guid.NewGuid();
        }

        public Scene(string sceneId)
        {
            SceneId = Guid.Parse(sceneId);
        }

        public Scene(Guid sceneId)
        {
            SceneId = sceneId;
        }

    }
}
