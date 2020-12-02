using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

using WizBulb.Localization.Resources;

namespace WizBulb.Converters 
{
    public class DistanceConverter : IValueConverter
    {
        public bool AsFeet { get; set; } = true;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d)
            {
                string s;
                if (AsFeet)
                {
                    d *= 3.28084; // get feet

                    var f = Math.Floor(d);
                    var i = (d - f) * 12;

                    s = string.Format(AppResources.XFeetInches, (int)f, (int)i);
                }
                else
                {
                    if (d < 1d)
                    {
                        d = Math.Round(d * 100, 0);
                    }
                    else
                    {
                        d = Math.Round(d, 1);
                    }

                    s = string.Format(AppResources.XCentimetersShort, d);

                }

                return s;
            }
            else if (value == null)
            {
                return "";
            }
            else
            {
                return value?.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
