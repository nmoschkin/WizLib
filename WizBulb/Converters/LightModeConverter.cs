using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using WiZ;

namespace WizBulb.Converters
{
    public class LightModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is LightMode lm)
            {
                if (targetType == typeof(string))
                {
                    return lm.Name;
                }
                else if (targetType == typeof(int))
                {
                    return lm.Code;
                }
            }
            else if (value is LightModeType t)
            {
                return LightMode.GetLightModeTypeDescription(t);
            }
            else if (targetType == typeof(LightModeType))
            {
                if (value is int lmi)
                {
                    return (LightModeType)lmi;
                }
                else if (value is string lms)
                {
                    int lmc;

                    if (int.TryParse(lms, out lmc))
                    {
                        return (LightModeType)lmc;
                    }
                    else
                    {
                        return LightMode.GetLightModeTypeByDescription(lms);
                    }
                }
                else
                {
                    return null;
                }

            }
            else if (targetType == typeof(LightMode))
            {
                if (value is int lmi)
                {
                    return LightMode.GetLightMode(lmi);
                }
                else if (value is string lms)
                {
                    int lmc;

                    if (int.TryParse(lms, out lmc))
                    {
                        return LightMode.GetLightMode(lmc);
                    }
                    else
                    {
                        return LightMode.GetLightModeByName(lms);
                    }
                }
                else
                {
                    return null;
                }
            }

            return value;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Convert(value, targetType, parameter, culture);

    }
}
