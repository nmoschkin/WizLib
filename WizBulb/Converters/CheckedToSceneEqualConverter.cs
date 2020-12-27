using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;


namespace WizBulb.Converters
{
    public class CheckedToSceneEqualConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int a && parameter is int b)
            {
                return (a == b);
            }
            else if (value is IEnumerable<object> lbulbs && parameter is int b2)
            {
                bool pass = true;


                foreach (WiZ.Bulb bulb in lbulbs)
                {
                    if ((bulb.Settings.Scene ?? -4) != b2)
                    {
                        pass = false;
                        break;
                    }
                }

                return pass;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
