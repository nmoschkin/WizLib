using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace WizLib
{
    public class Room : ViewModelBase    
    {
        private string roomId;
        private string name;

        private KeyedObservableCollection<Bulb> bulbs = new KeyedObservableCollection<Bulb>(nameof(Bulb.MACAddress));

        private KeyedObservableCollection<Scene> scenes = new KeyedObservableCollection<Scene>(nameof(Scene.SceneId));

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
                SetProperty(ref roomId, value);
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
        public KeyedObservableCollection<Bulb> Bulbs
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
