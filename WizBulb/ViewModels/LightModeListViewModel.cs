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
    public class LightModeListViewModel : ObservableBase
    {

        private ObservableCollection<LightMode> modeList;
        private ObservableCollection<Bulb> selection;

        public ObservableCollection<LightMode> ModeList => modeList;

        public LightModeListViewModel(Profile profile)
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

        ~LightModeListViewModel()
        {
            if (selection != null)
            {
                selection.CollectionChanged -= SelectionChanged;
            }
        }

        public LightMode SelectedMode
        {
            get
            {
                if (selection == null || selection.Count == 0) return LightMode.Empty;

                LightMode lm = LightMode.Empty; 
                
                foreach (var bulb in selection)
                {
                    if (lm == LightMode.Empty && bulb.LightMode != null)
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
                    _ = bulb.GetPilot();
                }
            }
        }

        public ObservableCollection<Bulb> BulbSelection
        {
            get => BulbSelection;
            set
            {
                var oldval = selection;

                if (SetProperty(ref selection, value))
                {
                    if (oldval != null) oldval.CollectionChanged -= SelectionChanged;
                    value.CollectionChanged += SelectionChanged;
                    
                    OnPropertyChanged(nameof(SelectedMode));
                }
            }
        }

        private void SelectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(SelectedMode));
        }
    }
}
