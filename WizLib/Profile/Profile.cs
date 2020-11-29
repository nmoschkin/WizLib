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
        
        private IList<Bulb> bulbs;

        private IList<Room> rooms;

        private IList<Scene> scenes;

        private IList<Home> homes;

        private IList<LightMode> lightModes;

        private string name;

        public Profile()
        {
            bulbs = new List<Bulb>();
            rooms = new List<Room>();
            scenes = new List<Scene>();
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
        public IList<LightMode> LightModes
        {
            get => lightModes;
            set
            {
                SetProperty(ref lightModes, value);
            }
        }

        [JsonProperty("bulbs")]
        public IList<Bulb> Bulbs
        {
            get => bulbs;
            set
            {
                SetProperty(ref bulbs, value);
            }
        }

        [JsonProperty("rooms")]
        public IList<Room> Rooms
        {
            get => rooms;
            set
            {
                SetProperty(ref rooms, value);
            }
        }


        [JsonProperty("scenes")]
        public IList<Scene> Scenes
        {
            get => scenes;
            set
            {
                SetProperty(ref scenes, value);
            }
        }



    }
}
