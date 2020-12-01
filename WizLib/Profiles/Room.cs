using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace WizLib.Profiles
{
    public class Room : ObservableBase
    {
        private string roomId;
        private string name;

        private KeyedObservableCollection<BulbItem> bulbs = new KeyedObservableCollection<BulbItem>(nameof(Bulb.MACAddress));

        private KeyedObservableCollection<Scene> scenes = new KeyedObservableCollection<Scene>(nameof(Scene.SceneId));

        public KeyedObservableCollection<Room> RoomCache { get; private set; } = new KeyedObservableCollection<Room>(nameof(RoomId));


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

        [JsonProperty("roomId")]
        public string RoomId
        {
            get => roomId;
            set
            {
                if (SetProperty(ref roomId, value) && value != null)
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
            get => name;
            set
            {
                SetProperty(ref name, value);
            }
        }

        [JsonProperty("bulbs")]
        public KeyedObservableCollection<BulbItem> Bulbs
        {
            get => bulbs;
            set
            {
                SetProperty(ref bulbs, value);
            }
        }

        [JsonProperty("scenes")]
        public KeyedObservableCollection<Scene> Scenes
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

    }
}
