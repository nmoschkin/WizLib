using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using WiZ;
using System.Net.Sockets;
using System.Net;
using DataTools.Text;
using DataTools.ColorControls;
using DataTools.Hardware.Network;
using WizBulb.Localization.Resources;
using System.ComponentModel;
using System.Collections;
using System.Runtime.CompilerServices;
using DataTools.Win32;
using WiZ.Observable;
using DataTools.Graphics;

namespace WizBulb
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private Fields

        ListSortDirection _lastDirection = ListSortDirection.Ascending;

        GridViewColumnHeader _lastHeaderClicked = null;

        bool notch = false;

        private MainViewModel vm;

        #endregion Private Fields

        #region Public Constructors

        class TestClass : ObservableBase
        {
            private string title;

            public string Title
            {
                get => title;
                set
                {
                    SetProperty(ref title, value);
                }
            }

            private string key;

            [KeyProperty]
            public string Key
            {
                get => key;
                set
                {
                    SetProperty(ref key, value);
                }
            }

            public TestClass(string key, string value)
            {
                Title = value;
                Key = key;
            }
           
            public override string ToString()
            {
                return $"{Key}: {Title}";
            }

        }

        public MainWindow()
        {
            //WiZ.Helpers.ConsoleHelper.AllocConsole();

            InitializeComponent();

            //var lonc = new List<NamedColor>(NamedColor.Catalog);

            //lonc.Sort((a, b) =>
            //{
            //    return (a.Color.IntValue - b.Color.IntValue);
            //});

            //int c = lonc.Count - 1;
            //int i;

            //for (i = c; i >= 0; i--)
            //{
            //    if (i > 0)
            //    {
            //        if (lonc[i].Color.IntValue == lonc[i - 1].Color.IntValue)
            //        {
            //            lonc.RemoveAt(i);
            //        }
            //    }
            //}

            //var testing = new ObservableDictionary<UniColor, NamedColor>(nameof(NamedColor.Color), new Comparison<UniColor>((a, b) => {
            //        return (a.IntValue - b.IntValue);
            //        }));
            //foreach (var ck in lonc)
            //{
            //    testing.Add(ck);
            //}

            //int ecount = 0;
            //foreach (var k in testing.Keys)
            //{

            //    if (k.Value == 0xff000000)
            //    {
            //        ecount++;
                    
            //        var erl = "Break";

            //    }
            //}

            //var nc1 = lonc[343];

            //HSVDATA h;
            //HSVDATA h2;

            //h = DataTools.Desktop.ColorMath.ColorToHSV(nc1);
            //UniColor clr = DataTools.Desktop.ColorMath.HSVToColorRaw(h);

            //h2 = DataTools.Desktop.ColorMath.ColorToHSV(clr);
            //NamedColor nc2 = NamedColor.FindColor(clr);

            //nc1 = testing[clr];


            //ObservableDictionary<string, TestClass> testing = new ObservableDictionary<string, TestClass>();

            //testing.Add(new TestClass("A1", "Bob"));
            //testing.Add(new TestClass("B1", "Sally"));
            //testing.Add(new TestClass("B3", "Janice"));
            //testing.Add(new TestClass("A3", "Doug"));
            //testing.Add(new TestClass("E2", "Michael"));
            //testing.Add(new TestClass("C1", "Henry"));
            //testing.Add(new TestClass("D1", "Jeff"));
            //testing.Add(new TestClass("E1", "Nate"));
            //testing.Add(new TestClass("A2", "Gina"));
            //testing.Add(new TestClass("F2", "Philip"));
            //testing.Add(new TestClass("F1", "Martha"));
            //testing.Add(new TestClass("D3", "Gilbert"));
            //testing.Add(new TestClass("E3", "Robert"));
            //testing.Add(new TestClass("F3", "Daniel"));
            //testing.Add(new TestClass("B2", "Frank"));
            //testing.Add(new TestClass("C2", "Delilah"));
            //testing.Add(new TestClass("C3", "Adrienne"));
            //testing.Add(new TestClass("D2", "Sean"));
            //testing.Add(new TestClass("F4", "Elaine"));

            //var v = testing.Keys;

            //testing.Sort((a, b) =>
            //{
            //    return string.Compare(a.Title, b.Title);
            //});

            //var f4 = testing["F4"];

            //f4.Key = "CF2";

            //f4 = testing["CF2"];


            var loc = Settings.LastWindowLocation;
            var size = Settings.LastWindowSize;

            Left = loc.X;
            Top = loc.Y;

            Width = size.Width;
            Height = size.Height;
            var ip = NetworkHelper.DefaultLocalIP;

            this.Loaded += MainWindow_Loaded;
            this.LocationChanged += MainWindow_LocationChanged;
            this.SizeChanged += MainWindow_SizeChanged;

            var iconv = (WizBulb.Converters.IntDisplayConverter)this.Resources["intConv"];

            iconv.ConverterError += Iconv_ConverterError;

            vm = new MainViewModel(false);
            vm.PropertyChanged += Vm_PropertyChanged;

            vm.PopulateLightModesMenu(mnuModes);
            vm.PopulateRecentFiles(mnuRecents);

            DataContext = vm;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var loc = Settings.LastWindowLocation;
            var size = Settings.LastWindowSize;

            Left = loc.X;
            Top = loc.Y;

            Width = size.Width;
            Height = size.Height;
            mnuLoadLast.IsChecked = Settings.OpenLastOnStartup;

            //vm.RefreshNetworks();
            //_ = vm.LoadLastProject();

            _ = vm.RefreshAll().ContinueWith(async (t) =>
            {
                await App.Current.Dispatcher.Invoke(async () =>
                {
                    if (Settings.OpenLastOnStartup && await vm.LoadLastProject())
                    {
                        await Task.Delay(vm.Interval);
                        vm.WatchBulbs();
                    }
                });
            });

        }

        #endregion Public Constructors

        #region Public Delegates

        public delegate void BulbDoer();

        #endregion Public Delegates

        #region Public Properties

        public ObservableCollection<Bulb> Bulbs { get; set; }
        public AdaptersCollection Networks { get; set; }

        #endregion Public Properties

        #region Private Methods

        private void BtnScan_Click(object sender, RoutedEventArgs e)
        {

            vm.ScanForBulbs();
        }

        private void BulbList_Click(object sender, RoutedEventArgs e)
        {
            var headerClicked = e.OriginalSource as GridViewColumnHeader;
            ListSortDirection direction;

            if (headerClicked != null)
            {
                if (headerClicked.Role != GridViewColumnHeaderRole.Padding)
                {
                    if (headerClicked != _lastHeaderClicked)
                    {
                        direction = ListSortDirection.Ascending;
                    }
                    else
                    {
                        if (_lastDirection == ListSortDirection.Ascending)
                        {
                            direction = ListSortDirection.Descending;
                        }
                        else
                        {
                            direction = ListSortDirection.Ascending;
                        }
                    }

                    var columnBinding = headerClicked.Column.DisplayMemberBinding as Binding;
                    var sortBy = columnBinding?.Path.Path ?? headerClicked.Column.Header as string;

                    Sort(sortBy, direction);

                    if (direction == ListSortDirection.Ascending)
                    {
                        headerClicked.Column.HeaderTemplate =
                          Resources["HeaderTemplateArrowUp"] as DataTemplate;
                    }
                    else
                    {
                        headerClicked.Column.HeaderTemplate =
                          Resources["HeaderTemplateArrowDown"] as DataTemplate;
                    }

                    // Remove arrow from previously sorted header
                    if (_lastHeaderClicked != null && _lastHeaderClicked != headerClicked)
                    {
                        _lastHeaderClicked.Column.HeaderTemplate = null;
                    }

                    _lastHeaderClicked = headerClicked;
                    _lastDirection = direction;
                }
            }
        }

        private void Sort(string sortBy, ListSortDirection direction)
        {
            var dataView =
              CollectionViewSource.GetDefaultView(BulbList.ItemsSource);

            dataView.SortDescriptions.Clear();

            if (sortBy == "Bulbs")
            {
                if (direction == ListSortDirection.Descending)
                {
                    if (BulbList.ItemsSource is ObservableDictionary<MACAddress, Bulb> c)
                    {
                        c.Sort((a, b) =>
                        {
                            return -string.Compare(a.Name?.ToString(), b.Name?.ToString());
                        });

                    }
                }
                else
                {
                    if (BulbList.ItemsSource is ObservableDictionary<MACAddress, Bulb> c)
                    {
                        c.Sort((a, b) =>
                        {
                            return string.Compare(a.Name?.ToString(), b.Name?.ToString());
                        });

                    }
                }
            }
            else if (sortBy == "IPAddress")
            {
                if (direction == ListSortDirection.Descending)
                {
                    if (BulbList.ItemsSource is ObservableDictionary<MACAddress, Bulb> c)
                    {
                        c.Sort((a, b) =>
                        {
                            return -string.Compare(a.IPAddress?.ToString(), b.IPAddress?.ToString());
                        });

                    }
                }
                else
                {
                    if (BulbList.ItemsSource is ObservableDictionary<MACAddress, Bulb> c)
                    {
                        c.Sort((a, b) =>
                        {
                            return string.Compare(a.IPAddress?.ToString(), b.IPAddress?.ToString());
                        });

                    }
                }
            }
            else if (sortBy == "MACAddress")
            {
                if (direction == ListSortDirection.Descending)
                {
                    if (BulbList.ItemsSource is ObservableDictionary<MACAddress, Bulb> c)
                    {
                        c.Sort((a, b) =>
                        {
                            return -a.MACAddress.CompareTo(b.MACAddress);
                        });

                    }
                }
                else
                {
                    if (BulbList.ItemsSource is ObservableDictionary<MACAddress, Bulb> c)
                    {
                        c.Sort((a, b) =>
                        {
                            return a.MACAddress.CompareTo(b.MACAddress);
                        });

                    }
                }
            }
            else
            {
                SortDescription sd = new SortDescription(sortBy, direction);
                dataView.SortDescriptions.Add(sd);
            }

            dataView.Refresh();
        }

        private async void BulbList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                if ((e.KeyboardDevice.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                {
                    await vm.RefreshAll();
                }
                else
                {
                    await vm.RefreshSelected();
                }
            }
            else if ((e.KeyboardDevice.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (e.Key == Key.S)
                {
                    if ((e.KeyboardDevice.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                    {
                        vm.SaveProjectAs();
                    }
                    else
                    {
                        vm.SaveProject();
                    }
                }
                else if (e.Key == Key.O)
                {
                    await vm.OpenProject();
                }
                else if (e.Key == Key.Q)
                {
                    Close();
                }

            }

        }

        private void BulbList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.RemovedItems != null && e.RemovedItems.Count > 0)
            {
                var b = e.RemovedItems[0] as Bulb;
                b.Renaming = false;
            }

            List<Bulb> lb = new List<Bulb>();

            foreach (Bulb sel in BulbList.SelectedItems)
            {
                lb.Add(sel);
            }

            vm.SelectedBulbs = lb;
        }

        private async void ColorPicker_ColorHit(object sender, ColorHitEventArgs e)
        {
            try
            {
                if ((BulbList.SelectedItems?.Count ?? 0) > 0)
                {

                    foreach (Bulb bulb in BulbList.SelectedItems)
                    {
                        await bulb.SetLightMode(e.Color);
                    }
                }
                else if (vm.SelectedBulb != null)
                {
                    await vm.SelectedBulb.SetLightMode(e.Color, 100);
                }
            }
            catch { }
        }

        private void HomeList_Click(object sender, RoutedEventArgs e)
        {

        }

        private void HomeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Iconv_ConverterError(object sender, Converters.ConverterErrorEventArgs e)
        {
            vm.TimeoutStatus = e.Message;
            vm.ShowTimeoutStatus = Visibility.Visible;
        }

        private void MainWindow_LocationChanged(object sender, EventArgs e)
        {
            Settings.LastWindowLocation = new Point(Left, Top);
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Settings.LastWindowSize = e.NewSize;
        }

        private void mnuLoadLast_Checked(object sender, RoutedEventArgs e)
        {
            Settings.OpenLastOnStartup = mnuLoadLast.IsChecked;
        }

        private void mnuLoadLast_Click(object sender, RoutedEventArgs e)
        {
            Settings.OpenLastOnStartup = mnuLoadLast.IsChecked;
        }

        private void mnuNewProject_Click(object sender, RoutedEventArgs e)
        {
            vm.NewProject();
        }

        private async void mnuOpenProject_Click(object sender, RoutedEventArgs e)
        {
            await vm.OpenProject();
        }

        private void mnuPing_Click(object sender, RoutedEventArgs e)
        {
            if (BulbList.SelectedValue is Bulb b)
            {
                b.Pulse();
            }
        }

        private void mnuQuit_Click(object sender, RoutedEventArgs e)
        {
            if (vm.Changed)
            {
                var ret = MessageBox.Show(AppResources.AskSaveExit, AppResources.SaveChangesTitle, MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                if (ret == MessageBoxResult.No)
                {
                    Environment.Exit(0);
                }
                else if (ret == MessageBoxResult.Yes)
                {

                    // if (vm.SaveProfile()) Environment.Exit(0);
                    Environment.Exit(0);
                }
                else
                {
                    return;
                }
            }

            Close();
        }

        private async void mnuRefresh_Click(object sender, RoutedEventArgs e)
        {
            await vm.RefreshSelected();
        }

        private async void mnuRefreshAll_Click(object sender, RoutedEventArgs e)
        {
            await vm.RefreshAll();
        }

        private void mnuRename_Click(object sender, RoutedEventArgs e)
        {
            if (BulbList.SelectedValue is Bulb b)
            {
                b.Renaming = true;
            }
        }

        private void mnuRoomRefresh_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuRoomRefreshAll_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuSaveAs_Click(object sender, RoutedEventArgs e)
        {
            vm.SaveProjectAs();
        }

        private void mnuSaveProject_Click(object sender, RoutedEventArgs e)
        {
            vm.SaveProject();
        }

        private void Picker_ColorHit(object sender, ColorHitEventArgs e)
        {
            UniColor uc = e.Color;
            var nc = NamedColor.FindColor(uc, true);

            if (nc != null)
            {
                ColorText.Text = nc.Name;
                uc = nc.Color;
            }
            else
            {
                ColorText.Text = "";
            }

            ColorSwatch.Background = new SolidColorBrush(uc.GetWPFColor());
        }

        private void RoomList_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RoomList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Slider_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            _ = vm.RefreshSelected();
        }

        private async void Speed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (notch || vm == null) return;

            byte i = (byte)e.NewValue;
            await Bulb.SetLights(vm.SelectedBulbs, speed: i);

        }

        private async void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (notch || vm == null) return;

            byte i = (byte)e.NewValue;
            await Bulb.SetLights(vm.SelectedBulbs, brightness: i);
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                tb.SelectAll();
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                tb.IsEnabled = false;
            }
        }

        private void Vm_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.Bulbs))
            {
                //GridView view = (GridView)BulbList.View;
                //RoutedEventArgs v;
                //var header = GetHeader(AppResources.RoomId);

                //v = new RoutedEventArgs(GridViewColumnHeader.ClickEvent, header);
                //this.BulbList_Click(BulbList, v);
            }

            if (e.PropertyName == nameof(MainViewModel.SelectedBulb) && vm.SelectedBulb != null && vm.SelectedBulb.Brightness != null)
            {
                notch = true;
                Slide.Value = vm.SelectedBulb?.Brightness ?? 10;
                Speed.Value = vm.SelectedBulb?.Speed ?? 10;
                notch = false;
            }
        }

        private GridViewColumnHeader GetHeader(string text)
        {

            GridViewHeaderRowPresenter presenter = GetDescendantByType(BulbList, typeof(GridViewHeaderRowPresenter)) as GridViewHeaderRowPresenter;

            GridView gridView = BulbList.View as GridView;

            for (int i = 0; i < gridView.Columns.Count; i++)

            {

                GridViewColumnHeader header = VisualTreeHelper.GetChild(presenter, i) as GridViewColumnHeader;
                GridViewColumn colunmn = header.Column;

                if (colunmn != null && colunmn.Header.ToString() == text)
                {
                    return header;
                }

            }

            return null;

        }

        static Visual GetDescendantByType(Visual element, Type type)
        {

            if (element == null) return null;

            if (element.GetType() == type) return element;

            Visual foundElement = null;

            if (element is FrameworkElement)

                (element as FrameworkElement).ApplyTemplate();

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)

            {

                Visual visual = VisualTreeHelper.GetChild(element, i) as Visual;

                foundElement = GetDescendantByType(visual, type);

                if (foundElement != null)

                    break;

            }

            return foundElement;

        }



        #endregion Private Methods

        private void BulbList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (BulbList.SelectedItem is Bulb b)
            {
                b.Pulse();
            }
        }

        private void mnuTestCycle_Click(object sender, RoutedEventArgs e)
        {
            if (BulbList.SelectedItem is Bulb b)
            {
                _ = Task.Run(async () =>
                {

                    DateTime t = DateTime.Now;

                    DateTime end = DateTime.Now.AddSeconds(30);

                    System.Drawing.Color[] cycles = new System.Drawing.Color[] { System.Drawing.Color.Red, System.Drawing.Color.Orange, System.Drawing.Color.Yellow, System.Drawing.Color.Green, System.Drawing.Color.Cyan, System.Drawing.Color.Blue, System.Drawing.Color.Purple };


                    int d = 30000 / cycles.Length;

                    for (int i = 0; i < cycles.Length; i++)
                    {
                        b.Color = cycles[i];
                        await Task.Delay(d);
                    }

                });

            }
        }
    }

}

