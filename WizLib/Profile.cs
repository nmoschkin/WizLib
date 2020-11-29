using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WizLib
{
    public class Profile : ViewModelBase
    {
        private Guid projectId;
        
        private List<Bulb> bulbs;

        private List<Room> rooms;

        private List<Scene> scenes;

        private List<Home> homes;

        private List<LightMode> lightModes;

        private string name;

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
        public List<LightMode> LightModes
        {
            get => lightModes;
            set
            {
                SetProperty(ref lightModes, value);
            }
        }

        [JsonProperty("bulbs")]
        public List<Bulb> Bulbs
        {
            get => bulbs;
            set
            {
                SetProperty(ref bulbs, value);
            }
        }

        [JsonProperty("rooms")]
        public List<Room> Rooms
        {
            get => rooms;
            set
            {
                SetProperty(ref rooms, value);
            }
        }


        [JsonProperty("scenes")]
        public List<Scene> Scenes
        {
            get => scenes;
            set
            {
                SetProperty(ref scenes, value);
            }
        }



    }
}
