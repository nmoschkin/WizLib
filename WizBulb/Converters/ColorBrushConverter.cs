using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WizBulb.Converters
{
    public class ColorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            
            if (targetType.IsAssignableTo(typeof(System.Windows.Media.Brush)))
            {

                if (value is System.Drawing.Color c)
                {
                    DataTools.Desktop.Unified.UniColor unc = c;
                    var mcc = (System.Windows.Media.Color)unc;
                    return new System.Windows.Media.SolidColorBrush(mcc);
                }
                else if (value is System.Windows.Media.Color mc)
                {
                    return new System.Windows.Media.SolidColorBrush(mc);
                }
                else
                {
                    return null;
                }

            }
            else if (targetType == typeof(string))
            {
                if (value is System.Drawing.Color c)
                {
                    DataTools.Desktop.Unified.UniColor unc = c;
                    return unc.ToString();
                }
                else if (value is System.Windows.Media.Color mc)
                {
                    DataTools.Desktop.Unified.UniColor unc = mc;
                    return unc.ToString();
                }
                else
                {
                    return value?.ToString();
                } 
            }
            else
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is System.Windows.Media.SolidColorBrush br)
            {
                return br.Color;
            }
            else
            {
                return new System.Windows.Media.Color();
            }
        }
    }
}
