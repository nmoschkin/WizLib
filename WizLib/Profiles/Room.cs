using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace WizLib.Profiles
{
    public class Room : ObservableBase
    {
        private int roomId;
        private string name;

        private KeyedObservableCollection<BulbAddress, BulbItem> bulbs = new KeyedObservableCollection<BulbAddress, BulbItem>(nameof(Bulb.MACAddress));

        private KeyedObservableCollection<Guid, Scene> scenes = new KeyedObservableCollection<Guid, Scene>(nameof(Scene.SceneId));

        public static KeyedObservableCollection<int, Room> RoomCache { get; private set; } = new KeyedObservableCollection<int, Room>(nameof(RoomId));


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
            get => name;
            set
            {
                SetProperty(ref name, value);
            }
        }

        [JsonProperty("bulbs")]
        public KeyedObservableCollection<BulbAddress, BulbItem> Bulbs
        {
            get => bulbs;
            set
            {
                SetProperty(ref bulbs, value);
            }
        }

        [JsonProperty("scenes")]
        public KeyedObservableCollection<Guid, Scene> Scenes
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
