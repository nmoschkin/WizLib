using DataTools.Graphics;

using System;
using System.Globalization;
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
                    UniColor unc = c;
                    var mcc = unc.GetWPFColor();
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
                    UniColor unc = c;
                    return unc.ToString();
                }
                else if (value is System.Windows.Media.Color mc)
                {
                    UniColor unc = mc.GetUniColor();
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