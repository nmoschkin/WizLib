using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using WiZ.Observable;

namespace WiZ.Profiles
{
    public class Room : ObservableBase
    {
        private int roomId;
        private string name;

        private ObservableDictionary<MACAddress, BulbItem> bulbs = new ObservableDictionary<MACAddress, BulbItem>();

        private ObservableDictionary<Guid, Scene> scenes = new ObservableDictionary<Guid, Scene>(nameof(Scene.SceneId));

        public static ObservableDictionary<int, Room> RoomCache { get; private set; } = new ObservableDictionary<int, Room>();


        private Scene currentScene;

        [JsonProperty("currentScene")]
        public Scene CurrentScene
        {
            get => currentScene;
            set
            {
                SetProperty(ref currentScene, value);
            }
        }

        [KeyProperty]
        [JsonProperty("roomId")]
        public int RoomId
        {
            get => roomId;
            set
            {
                if (SetProperty(ref roomId, value))
                {
                    if (RoomCache.ContainsKey(roomId))
                    {
                        RoomCache[roomId] = this;
                    }
                    else
                    {
                        RoomCache.Add(this);
                    }
                }
            }
        }

        [JsonProperty("name")]
        public string Name
        {
            get => name ?? roomId.ToString();
            set
            {
                SetProperty(ref name, value);
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

        [JsonProperty("scenes")]
        public ObservableDictionary<Guid, Scene> Scenes
        {
            get => scenes;
            set
            {
                SetProperty(ref scenes, value);
            }
        }

        public Room(string json)
        {
            JsonConvert.PopulateObject(json, this);
        }

        public Room()
        {
        }

        public override string ToString()
        {
            return name ?? roomId.ToString();
        }

    }
}
