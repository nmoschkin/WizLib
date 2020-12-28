using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

using WiZ.Profiles;

namespace WizBulb.Converters
{

    public enum ProfilePart
    {
        Home,
        Room,
        //Bulb,
        //Other,
        //None
    }

    public class ProfilePartsConverter : IValueConverter
    {

        public ProfilePart Part { get; set; } = ProfilePart.Home;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int i)
            {

                if (Part == ProfilePart.Home)
                {
                    Home h;

                    Home.HomeCache.TryGetValue(i, out h);
                    return h;
                }
                else
                {
                    Room r;
                    Room.RoomCache.TryGetValue(i, out r);
                    return r;
                }

            }
            else
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
