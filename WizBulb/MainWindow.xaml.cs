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
using WizLib;
using System.Net.Sockets;
using System.Net;
using DataTools.Desktop.Unified;
using DataTools.Text;
using DataTools.ColorControls;
using DataTools.Hardware.Network;
using WizBulb.Localization.Resources;
using System.ComponentModel;
using System.Collections;

namespace WizBulb
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public delegate void BulbDoer();

        [DllImport("kernel32.dll")]
        static extern int AllocConsole();

        private MainViewModel vm;       
        
        public AdaptersCollection Networks { get; set; }

        public ObservableCollection<Bulb> Bulbs { get; set; }

        public MainWindow()
        {
            Bulb.HasConsole = AllocConsole() != 0;

            InitializeComponent();

            var loc = Settings.LastWindowLocation;
            var size = Settings.LastWindowSize;

            Left = loc.X;
            Top = loc.Y;
            
            Width = size.Width;
            Height = size.Height;

            //var b = new Bulb("192.168.1.12");
            //var b2 = new Bulb("192.168.1.8");

            //PredefinedScene p = PredefinedScene.GoldenWhite;

            //List<Action> paras = new List<Action>();

            //paras.Add(() => { b.SetScene(p, 64); });
            //paras.Add(() => { b2.SetScene(p, 64); });
            //paras.Add(() => { b.SetScene(p, System.Drawing.Color.Aqua, 255); });
            //paras.Add(() => { b2.SetScene(p, System.Drawing.Color.Violet, 255); });

            //Parallel.Invoke(paras.ToArray());

            //Picker.ColorHit += Picker_ColorHit;
            
            this.Loaded += MainWindow_Loaded;
            this.LocationChanged += MainWindow_LocationChanged;
            this.SizeChanged += MainWindow_SizeChanged;

            var iconv = (WizBulb.Converters.IntDisplayConverter)this.Resources["intConv"];

            iconv.ConverterError += Iconv_ConverterError;

            vm = new MainViewModel();

            vm.PopulateLightModesMenu(mnuModes);
            vm.LightModeClick += Vm_LightModeClick;
            vm.PropertyChanged += Vm_PropertyChanged;
            //vm.AutoWatch = true;

            DataContext = vm;
        }

        bool notch = false;

        private void Vm_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.SelectedBulb) && vm.SelectedBulb != null)
            {
                notch = true;
                Slide.Value = (double)vm.SelectedBulb.Brightness;
                notch = false;
            }
        }

        private void Vm_LightModeClick(object sender, LightModeClickEventArgs e)
        {
            // throw new NotImplementedException();
        }

        private void Iconv_ConverterError(object sender, Converters.ConverterErrorEventArgs e)
        {
            vm.TimeoutStatus = e.Message;
            vm.ShowTimeoutStatus = Visibility.Visible;
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

            ColorSwatch.Background = new SolidColorBrush((Color)uc);
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Settings.LastWindowSize = e.NewSize;
        }

        private void MainWindow_LocationChanged(object sender, EventArgs e)
        {
            Settings.LastWindowLocation = new Point(Left, Top);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var loc = Settings.LastWindowLocation;
            var size = Settings.LastWindowSize;

            Left = loc.X;
            Top = loc.Y;

            Width = size.Width;
            Height = size.Height;
        }

        private void ValueSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //Picker.ColorValue = ValueSlider.Value / 100;
        }

        private void BtnScan_Click(object sender, RoutedEventArgs e)
        {
            vm.ScanForBulbs();
        }


        GridViewColumnHeader _lastHeaderClicked = null;
        ListSortDirection _lastDirection = ListSortDirection.Ascending;

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

            //if (sortBy == "Scene")
            //{
            //    dataView.CustomSort = new BulbComparer();
            //}
            dataView.SortDescriptions.Clear();
            SortDescription sd = new SortDescription(sortBy, direction);
            dataView.SortDescriptions.Add(sd);
            dataView.Refresh();
        }

        private void mnuPing_Click(object sender, RoutedEventArgs e)
        {
            if (BulbList.SelectedValue is Bulb b)
            {
                b.Pulse();
            }
        }

        private void mnuRename_Click(object sender, RoutedEventArgs e)
        {
            if (BulbList.SelectedValue is Bulb b)
            {
                b.Renaming = true;
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                tb.SelectAll();
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

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                tb.IsEnabled = false;
            }
        }

        private void mnuOpenProject_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuNewProject_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuSaveProject_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuSaveAs_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuLoadLast_Click(object sender, RoutedEventArgs e)
        {

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
        }

        private async void ColorPicker_ColorHit(object sender, ColorHitEventArgs e)
        {
            if ((BulbList.SelectedItems?.Count ?? 0) > 0)
            {

                foreach (Bulb bulb in BulbList.SelectedItems)
                {
                    await bulb.SetLightMode((UniColor)e.Color, 100);
                }
            }
            else if (vm.SelectedBulb != null)
            {
                await vm.SelectedBulb.SetLightMode((UniColor)e.Color, 100);
            }
        }

        private async void mnuRefresh_Click(object sender, RoutedEventArgs e)
        {
            await vm.RefreshSelected();
        }

        private async void mnuRefreshAll_Click(object sender, RoutedEventArgs e)
        {
            await vm.RefreshAll();
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
        }

        private void Slider_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            _ = vm.RefreshSelected();
        }

        private void HomeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void HomeList_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RoomList_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RoomList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void mnuRoomRefresh_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuRoomRefreshAll_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (notch) return;

            byte i = (byte)e.NewValue;
            Bulb.SetLights(vm.SelectedBulbs, i);

        }
    }

}

