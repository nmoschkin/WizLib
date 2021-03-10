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
using System.Windows.Data;
using System.ComponentModel;

namespace WizBulb.ViewModels
{
    public class LightModeListViewModel : ObservableBase
    {

        private ListCollectionView modeList;
        private ObservableCollection<Bulb> selection;

        public ListCollectionView ModeList
        {
            get => modeList;
            set
            {
                SetProperty(ref modeList, value);
            }
        }


        public LightModeListViewModel(Profile profile)
        {

            List<LightMode> init = new List<LightMode>();

            init.Add(LightMode.Empty);
            init.AddRange(LightMode.LightModes.Values);

            foreach (var h in profile.CustomLightModes)
            {
                init.Add(h);
            }

            init.Sort((a, b) =>
            {
                
                if (a.Type == LightModeType.CustomColor && b.Type == LightModeType.CustomColor)
                {
                    if (a.IsEmpty && b.IsEmpty) return 0;
                    else if (a.IsEmpty) return -1;
                    else if (b.IsEmpty) return 1;

                }
                else if (a.Type == LightModeType.CustomColor)
                {
                    return -1;
                }
                else if (b.Type == LightModeType.CustomColor)
                {
                    return 1;
                }

                int t = string.Compare(a.TypeDescription, b.TypeDescription);
                return t == 0 ? string.Compare(a.Name, b.Name) : t;
            });

            var ml = new ListCollectionView(init);
            ml.GroupDescriptions.Add(new PropertyGroupDescription("TypeDescription"));

            ModeList = ml;
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
