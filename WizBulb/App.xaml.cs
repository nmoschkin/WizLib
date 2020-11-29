using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;

namespace WizBulb
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

       

        public new static App Current
        {
            get => (App)Application.Current;
        }

        public App() : base()
        {
           
        }

    }
}
