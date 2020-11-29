using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Windows.Data;
using System.Globalization;

namespace WizBulb.Converters
{
    public class MacAddrConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s)
            {
                return PhysicalAddress.Parse(s);
            }
            else if (value is PhysicalAddress p)
            {
                return p.ToString();
            }
            else
            {
                return value?.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s)
            {
                return PhysicalAddress.Parse(s);
            }
            else if (value is PhysicalAddress p)
            {
                return p.ToString();
            }
            else
            {
                return null;
            }

        }
    }
}
