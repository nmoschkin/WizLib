using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace WizLib
{
    public class Scene : ObservableBase
    {

        private KeyedObservableCollection<BulbParams> bp = new KeyedObservableCollection<BulbParams>(nameof(WizLib.BulbParams.MACAddress));

        private string sceneId;

        private string name;

        public string Name
        {
            get => name;
            set
            {
                SetProperty(ref name, value);
            }
        }

        public string SceneId
        {
            get => sceneId;
            set
            {
                SetProperty(ref sceneId, value);
            }
        }

        public KeyedObservableCollection<BulbParams> BulbParams
        {
            get => bp;
            set
            {
                SetProperty(ref bp, value);
            }
        }

        public Scene()
        {
            SceneId = Guid.NewGuid().ToString("d");
        }

        public Scene(string sceneId)
        {
            SceneId = sceneId;
        }



    }
}
