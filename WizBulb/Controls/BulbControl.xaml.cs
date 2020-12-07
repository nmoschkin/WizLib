using System;
using System.Collections.Generic;
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

using WizLib;

namespace WizBulb.Controls
{
    /// <summary>
    /// Interaction logic for BulbControl.xaml
    /// </summary>
    public partial class BulbControl : UserControl
    {

        public Bulb ItemSource
        {
            get { return (Bulb)GetValue(ItemSourceProperty); }
            set { SetValue(ItemSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemSourceProperty =
            DependencyProperty.Register("ItemSource", typeof(Bulb), typeof(BulbControl), new PropertyMetadata(null, OnBulbChange));


        private static void OnBulbChange(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is BulbControl ctrl) 
            {

                if (!e.OldValue.Equals(e.NewValue))
                {
                    if (e.OldValue is Bulb oldBulb)
                    {
                        oldBulb.PropertyChanged -= NewBulb_PropertyChanged;
                    }

                    if (e.NewValue is Bulb newBulb)
                    {
                        newBulb.PropertyChanged += NewBulb_PropertyChanged;
                    }

                    ctrl.RefreshBulb();
                }

            }
        }

        private static void NewBulb_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch(e.PropertyName)
            {
                
            }
        }

        private void RefreshBulb()
        {
            

        }


        public BulbControl()
        {
            InitializeComponent();
        }
    }
}
