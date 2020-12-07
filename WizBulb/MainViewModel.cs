using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Forms;
using System.IO;

using DataTools.Hardware.Network;

using WizBulb.Localization.Resources;

using WizLib;
using WizLib.Profiles;
using System.Net.NetworkInformation;
using WizLib.Observable;

namespace WizBulb
{
    public delegate void LightModeClickEvent(object sender, LightModeClickEventArgs e);

    public delegate void ScanCompleteEvent(object sender, EventArgs e);

    public class LightModeClickEventArgs : EventArgs
    {
        #region Public Constructors

        public LightModeClickEventArgs(LightMode lm, UIElement el)
        {
            LightMode = lm;
            Element = el;
        }

        #endregion Public Constructors

        #region Public Properties

        public UIElement Element { get; private set; }
        public LightMode LightMode { get; private set; }

        #endregion Public Properties
    }
    public class MainViewModel : ObservableBase
    {

        #region Private Fields

        private AdaptersCollection adapters;

        private ObservableDictionary<MACAddress, Bulb> allBulbs = new ObservableDictionary<MACAddress, Bulb>(nameof(Bulb.MACAddress));

        private bool autoChangeBulb = true;

        private bool autoWatch = false;

        private bool btnsEnabled = true;

        private ObservableDictionary<MACAddress, Bulb> bulbs = new ObservableDictionary<MACAddress, Bulb>(nameof(Bulb.MACAddress));

        private bool changed = false;

        private CancellationTokenSource cts;

        private ObservableDictionary<int, Home> homes = new ObservableDictionary<int, Home>(nameof(Home.HomeId));

        private int interval = 2500;

        private string networkStatus;

        private Profile profile = new Profile();

        private string projFile;

        private ObservableDictionary<int, Room> rooms = new ObservableDictionary<int, Room>(nameof(Room.RoomId));

        private NetworkAdapter selAdapter;

        private Bulb selBulb;

        private IList<Bulb> selBulbs;

        private Home selHome;

        private Room selRoom;

        private Visibility showns = Visibility.Hidden;

        private Visibility showts = Visibility.Hidden;

        private string statusMessage;

        private int timeout = 1;

        private string timeoutStatus;

        private Task watchTask;

        #endregion Private Fields

        #region Public Constructors

        public MainViewModel(bool populate)
        {

            if (populate)
            {
                _ = RefreshNetworks().ContinueWith(async (t) => await RefreshAll());
            }
        }

        #endregion Public Constructors

        #region Public Events

        public event LightModeClickEvent LightModeClick;

        public event ScanCompleteEvent ScanComplete;

        #endregion Public Events

        #region Public Properties

        public ObservableCollection<NetworkAdapter> Adapters
        {
            get => adapters?.Adapters;
        }

        public bool AutoChangeBulb
        {
            get => autoChangeBulb;
            set
            {
                SetProperty(ref autoChangeBulb, value);
            }
        }

        public bool AutoWatch
        {
            get => autoWatch;
            set
            {
                if (autoWatch == value) return;


                if (value)
                {
                    WatchBulbs();
                }
                else
                {
                    WatchAbort();
                }
            }
        }

        public ObservableDictionary<MACAddress, Bulb> Bulbs
        {
            get => bulbs;
            protected set
            {
                SetProperty(ref bulbs, value);
            }
        }

        public bool ButtonsEnabled
        {
            get => btnsEnabled;
            set
            {
                SetProperty(ref btnsEnabled, value);
            }
        }

        public bool Changed
        {
            get => changed;
            set
            {
                SetProperty(ref changed, value);
            }
        }

        public ObservableDictionary<int, Home> Homes
        {
            get => homes;
            set
            {
                SetProperty(ref homes, value);
            }
        }


        public int Interval
        {
            get => interval;
            set
            {
                SetProperty(ref interval, value);
            }

        }

        public string NetworkStatus
        {
            get => networkStatus;
            set
            {
                SetProperty(ref networkStatus, value);
            }
        }

        public Profile Profile
        {
            get => profile;
            set
            {
                SetProperty(ref profile, value);
            }
        }

        public string ProjectFile
        {
            get => projFile;
            set
            {
                projFile = value;
            }
        }

        public ObservableDictionary<int, Room> Rooms
        {
            get => rooms;
            set
            {
                SetProperty(ref rooms, value);
            }
        }

        public NetworkAdapter SelectedAdapter
        {
            get => selAdapter;
            set
            {
                SetProperty(ref selAdapter, value);
            }
        }

        public Bulb SelectedBulb
        {
            get => selBulb;
            set
            {
                SetProperty(ref selBulb, value);
            }
        }

        public IList<Bulb> SelectedBulbs
        {
            get => selBulbs;
            set
            {
                SetProperty(ref selBulbs, value);
            }
        }

        public Home SelectedHome
        {
            get => selHome;
            set
            {
                if (SetProperty(ref selHome, value))
                {
                    Rooms = value?.Rooms;
                }
            }
        }

        public Room SelectedRoom
        {
            get => selRoom;
            set
            {
                if (SetProperty(ref selRoom, value))
                {
                    Task.Run(async () =>
                    {
                        if (selRoom == null)
                        {
                            Bulbs = allBulbs;
                        }
                        else
                        {
                            Bulbs = new ObservableDictionary<MACAddress, Bulb>(
                                            nameof(Bulb.MACAddress),
                                            await BulbItem.CreateBulbsFromInterfaceList(selRoom.Bulbs)
                                            );
                        }

                    });
                }
            }
        }

        public Visibility ShowNetworkStatus
        {
            get => showns;
            set
            {
                SetProperty(ref showns, value);
            }
        }

        public Visibility ShowTimeoutStatus
        {
            get => showts;
            set
            {
                SetProperty(ref showts, value);
            }
        }

        public string StatusMessage
        {
            get => statusMessage;
            set
            {
                SetProperty(ref statusMessage, value);
            }
        }

        public int Timeout
        {
            get => timeout;
            set
            {
                SetProperty(ref timeout, value);
            }
        }

        public string TimeoutStatus
        {
            get => timeoutStatus;
            set
            {
                SetProperty(ref timeoutStatus, value);
            }
        }

        public string WindowTitle
        {
            get
            {
                if (!string.IsNullOrEmpty(ProjectFile))
                {
                    return "WiZBulb - " + Path.GetFileNameWithoutExtension(ProjectFile);
                }
                else
                {
                    return "WiZBulb";
                }
            }
        }

        #endregion Public Properties

        #region Public Methods

        public virtual bool CheckTimeout()
        {
            if (Timeout < 1 || Timeout > 360)
            {
                StatusMessage = string.Format(AppResources.WarnEnterNumberRange, 1, 360);
                return false;
            }

            return true;
        }

        public async Task<bool> LoadLastProject()
        {
            var recents = Settings.RecentFiles;
            if (recents == null || recents.Length == 0) return false;

            string s = recents[0].FileName;

            return await OpenProject(s);
        }

        public virtual void NewProject()
        {
            ProjectFile = null;
            Profile = new Profile();

            Bulb.BulbCache.Clear();
            ScanForBulbs();
        }

        public virtual async Task<bool> OpenProject()
        {
            var dlg = new OpenFileDialog()
            {
                InitialDirectory = Settings.LastDirectory,
                Filter = $"{AppResources.ProfileFilterEntry}",
                Title = AppResources.LoadProfile,
                CheckFileExists = true
            };

            if (dlg.ShowDialog() != DialogResult.OK) return false;

            Settings.LastBrowseFolder = Path.GetDirectoryName(dlg.FileName);
            return await OpenProject(dlg.FileName);
        }

        public virtual async Task<bool> OpenProject(string fileName)
        {
            try
            {

                if (!File.Exists(fileName)) return false;

                try
                {
                    var j = new JsonProfileSerializer(fileName);

                    SelectedRoom = null;
                    SelectedHome = null;

                    var prof = (Profile)j.Deserialize();

                    if (prof == null) return false;

                    Profile = prof;
                    
                    Homes = new ObservableDictionary<int, Home>(nameof(Home.HomeId), Profile.Homes);

                    allBulbs = Bulbs = new ObservableDictionary<MACAddress, Bulb>(
                                    nameof(Bulb.MACAddress),
                                    await BulbItem.CreateBulbsFromInterfaceList(Profile.Bulbs)
                                    );

                    allBulbs.Sort((a, b) =>
                    {
                        int x = (a.Settings.RoomId ?? 0) - (b.Settings.RoomId ?? 0);
                        if (x == 0)
                        {
                            x = string.Compare(a.Name, b.Name);
                        }
                        return x;
                    });

                    Settings.AddRecentFile(fileName, Profile.ProjectId);
                    ProjectFile = fileName;

                    OnPropertyChanged(nameof(WindowTitle));

                    return true;
                }
                catch
                {
                    return false;
                }

            }
            catch
            {
                return false;
            }
        }


        public virtual bool SaveProject()
        {
            if (string.IsNullOrEmpty(ProjectFile)) return SaveProjectAs();
            var fileName = ProjectFile;

            var j = new JsonProfileSerializer(fileName);

            Profile.AddUpdateBulbs(allBulbs, true);
            Profile.Homes = new ObservableDictionary<int, Home>(Homes);

            j.Serialize(Profile);

            Settings.AddRecentFile(fileName, Profile.ProjectId);
            OnPropertyChanged(nameof(WindowTitle));

            return true;
        }

        public virtual bool SaveProjectAs()
        {
            var dlg = new SaveFileDialog()
            {
                InitialDirectory = Settings.LastDirectory,
                Filter = $"{AppResources.ProfileFilterEntry}",
                Title = AppResources.SaveProfileAs,
                OverwritePrompt = true
            };

            Settings.LastBrowseFolder = Path.GetDirectoryName(dlg.FileName);

            if (dlg.ShowDialog() != DialogResult.OK) return false;

            var fileName = dlg.FileName;
            var j = new JsonProfileSerializer(fileName);

            Profile.AddUpdateBulbs(allBulbs, true);
            Profile.Homes = new ObservableDictionary<int, Home>(Homes);

            j.Serialize(Profile);

            ProjectFile = fileName;
            Settings.AddRecentFile(fileName, Profile.ProjectId);
            OnPropertyChanged(nameof(WindowTitle));

            return true;
        }

        public virtual void PopulateLightModesMenu(MenuItem mi)
        {
            MenuItem mis;
            ObservableCollection<MenuItem> miln = new ObservableCollection<MenuItem>();

            var lms = LightMode.LightModes.Values;
            var lmts = LightMode.AllLightModeTypes;


            ObservableCollection<MenuItem> submnu = new ObservableCollection<MenuItem>();

            foreach (var lmt in lmts)
            {
                mis = new MenuItem()
                {
                    Header = lmt.Value,
                    Tag = lmt.Key
                };

                mis.ItemsSource = new ObservableCollection<MenuItem>();
                miln.Add(mis);
            }

            foreach (var lm in lms)
            {
                foreach (var sm in miln)
                {
                    if ((LightModeType)sm.Tag == lm.Type)
                    {
                        mis = new MenuItem()
                        {
                            Header = lm.Name,
                            Tag = lm
                        };

                        mis.Click += LightModeItemClicked;

                        if (sm.ItemsSource is ObservableCollection<MenuItem> lsm)
                        {
                            lsm.Add(mis);
                        }

                        break;
                    }
                }
            }

            mi.ItemsSource = miln;
        }

        public void PopulateRecentFiles(MenuItem mi)
        {
            var recents = Settings.RecentFiles;
            var l = new List<MenuItem>();

            foreach (var r in recents)
            {
                var mis = new MenuItem()
                {
                    Header = Path.GetFileName(r.FileName),
                    ToolTip = r.FileName,
                    Tag = r
                };


                mis.Click += RecentItemClicked;
                l.Add(mis);
            }

            mi.ItemsSource = l;
        }

        public virtual async Task RefreshAll()
        {
            await RefreshNetworks();
            ScanForBulbs();
        }

        public virtual async Task RefreshNetworks()
        {
            var disp = App.Current.Dispatcher;

            await disp.BeginInvoke(() =>
            {
                adapters = new AdaptersCollection();
                OnPropertyChanged("Adapters");

                SelectedAdapter = null;
            });

            foreach (var net in Adapters)
            {
                if (net.HasInternet == InternetStatus.HasInternet)
                {
                    await disp.BeginInvoke(() => SelectedAdapter = net);
                    return;
                }
            }
            
        }

        public virtual async Task RefreshOnce()
        {
            if (App.Current == null || App.Current.Dispatcher == null) return;

            var disp = App.Current.Dispatcher;

            var bulbs = await Bulb.ScanForBulbs(selAdapter.IPV4Address, (MACAddress)(PhysicalAddress)selAdapter.PhysicalAddress, ScanMode.GetSystemConfig, Timeout * 1000,
            async (b) =>
            {
                try
                {
                    await disp.Invoke(async () =>
                    {
                        try
                        {
                            if (!Bulbs.ContainsKey(b.MACAddress))
                            {
                                int? selRoom = null;

                                if (SelectedRoom != null)
                                {
                                    selRoom = SelectedRoom.RoomId;
                                    if (b.Settings.RoomId != selRoom) return;
                                }

                                Bulbs.Add(b);
                            }

                            if (allBulbs != Bulbs && !allBulbs.ContainsKey(b.MACAddress))
                            {
                                allBulbs.Add(b);
                            }

                            await b.GetPilot();
                        }
                        catch
                        {
                            return;
                        }
                    });
                }
                catch
                {
                    return;
                }
            });
        }

        public virtual async Task RefreshSelected()
        {
            if (SelectedBulb != null && (SelectedBulbs == null || SelectedBulbs.Count == 0))
            {

                GC.Collect(0);
                StatusMessage = string.Format(AppResources.GettingBulbInfoForX, SelectedBulb.ToString());

                for (int re = 0; re < 3; re++)
                {
                    if (await SelectedBulb.GetPilot()) break;

                    StatusMessage = string.Format(AppResources.RetryingX, re);
                    await Task.Delay(1000);
                }
            }
            else if (SelectedBulbs != null && SelectedBulbs.Count > 0)
            {
                foreach (var bulb in SelectedBulbs)
                {
                    GC.Collect(0);
                    StatusMessage = string.Format(AppResources.GettingBulbInfoForX, bulb.ToString());

                    await bulb.GetPilot();
                }
            }

            StatusMessage = "";
        }

        public virtual bool ScanForBulbs()
        {
            var disp = Dispatcher.CurrentDispatcher;

            if (ButtonsEnabled == false) return false;

            ButtonsEnabled = false;

            if (selAdapter == null)
            {
                StatusMessage = AppResources.WarnSelectAdapter;
                ButtonsEnabled = true;
                return false;
            }

            if (!CheckTimeout())
            {
                ButtonsEnabled = true;
                return false;
            }

            if (Bulbs == null)
            {
                Bulbs = new ObservableDictionary<MACAddress, Bulb>(nameof(Bulb.MACAddress));
            }
            else
            {
                Bulbs.Clear();
            }

            string prevStat = ""; // StatusMessage;
            StatusMessage = AppResources.ScanningBulbs;

            _ = Task.Run(async () =>
            {
                var aw = AutoWatch;
                if (aw) WatchAbort();

                await Bulb.ScanForBulbs(selAdapter.IPV4Address, (MACAddress)(PhysicalAddress)selAdapter.PhysicalAddress, ScanMode.GetSystemConfig, Timeout * 1000,
                (b) =>
                {
                    disp.Invoke(() =>
                    {
                        if (!Bulbs.ContainsKey(b.MACAddress))
                        {
                            Bulbs.Add(b);
                            StatusMessage = string.Format(AppResources.ScanningBulbsXBulbsFound, Bulbs.Count);
                        }
                    });
                });

                allBulbs = bulbs;
                Homes = Home.GenerateHomes(Bulbs);

                StatusMessage = AppResources.ScanComplete;

                ScanComplete?.Invoke(this, new EventArgs());

                ButtonsEnabled = true;
                if (aw) WatchBulbs();

                App.Current.Dispatcher.Invoke(() =>
                {
                    allBulbs.Sort((a, b) =>
                    {
                        if (a.Settings?.RoomId == null && b.Settings?.RoomId == null) return 0;
                        else if (a.Settings?.RoomId == null) return 1;
                        else if (b.Settings?.RoomId == null) return -1;

                        return (int)a.Settings.RoomId - (int)b.Settings.RoomId;
                    });
                });

                _ = Task.Run(async () =>
                {

                    await Task.Delay(5000);

                    if (ButtonsEnabled)
                        StatusMessage = prevStat ?? "";

                });

            });

            return true;
        }

        public void WatchAbort()
        {
            if (cts != null) 
            {
                cts.Cancel();
                watchTask?.Wait();

                autoWatch = false;
                watchTask = null;
                cts = null;

                OnPropertyChanged(nameof(AutoWatch));
            }
        }

        public bool WatchBulbs()
        {
            if (autoWatch) return false;

            cts = new CancellationTokenSource();
            int ival = interval / 100;

            watchTask = Task.Run(async () =>
            {
                try
                {
                    while (cts != null && !cts.IsCancellationRequested)
                    {
                        await RefreshOnce();

                        for (int i = 0; i < ival; i++)
                        {
                            if (cts?.IsCancellationRequested ?? true) break;
                            await Task.Delay(100);
                        }
                        
                    }
                }
                catch
                {
                    return;
                }

            }, cts.Token);

            autoWatch = true;
            OnPropertyChanged(nameof(AutoWatch));

            return true;
        }

        #endregion Public Methods

        #region Protected Methods

        protected virtual async void LightModeItemClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (e.Source is MenuItem mi && mi.Tag is LightMode lm)
            {

                if (autoChangeBulb)
                {
                    if (SelectedBulb != null && (SelectedBulbs == null || SelectedBulbs.Count == 0))
                    {
                        await SelectedBulb.SetLightMode(lm);
                    }
                    else if (SelectedBulbs != null && SelectedBulbs.Count > 0)
                    {
                        await Bulb.SetLights(SelectedBulbs, lm);

                        foreach (Bulb bulb in SelectedBulbs)
                        {
                            await bulb.GetPilot();
                        }
                    }
                }

                LightModeClick?.Invoke(this, new LightModeClickEventArgs(lm, mi));
            }
        }

        protected virtual async void RecentItemClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (e.Source is MenuItem mi && mi.Tag is RecentFile r)
            {
                await OpenProject(r.FileName);
            }
        }

        #endregion Protected Methods
    }
}
