using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WiZ;
using WiZ.Observable;
using WiZ.Profiles;
using WiZ.Helpers;

namespace WizBulb.ViewModels
{
    public class LightModeList : ObservableBase
    {

        private ObservableCollection<LightMode> modeList;
        private IEnumerable<Bulb> selection;

        public ObservableCollection<LightMode> ModeList => modeList;

        public LightModeList(Profile profile)
        {

            List<LightMode> init = new List<LightMode>();

            init.Add(LightMode.Empty);
            init.AddRange(LightMode.LightModes.Values);

            foreach (var h in profile.CustomLightModes)
            {
                init.Add(h);
            }

            modeList = new ObservableCollection<LightMode>(init);

        }

        public LightMode SelectedMode
        {
            get
            {
                if (selection == null) return LightMode.Empty;

                LightMode lm = LightMode.Empty; 
                
                foreach (var bulb in selection)
                {
                    if (lm == LightMode.Empty)
                    {
                        lm = bulb.LightMode;
                    }
                    else
                    {
                        if (!lm.Equals(bulb.LightMode))
                        {
                            return LightMode.Empty;
                        }
                    }
                }

                return lm;
            }
            set
            {
                foreach (var bulb in selection)
                {
                    bulb.LightMode = value;
                }
            }
        }

        public IEnumerable<Bulb> BulbSelection
        {
            get => BulbSelection;
            set
            {
                if (SetProperty(ref selection, value))
                {
                    OnPropertyChanged(nameof(SelectedMode));
                }
            }
        }


    }
}
