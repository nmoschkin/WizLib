using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;
using System.Windows.Controls;
using System.Reflection;

namespace WizBulb.Converters
{
    /// <summary>
    /// Provides a visibility expression based on the value of a selection of an item or items.
    /// </summary>
    class SelectionConverter : IValueConverter
    {

        /// <summary>
        /// Text binding for string return results
        /// </summary>
        public string TargetPath { get; set; }

        /// <summary>
        /// Text to display for multiple selections
        /// </summary>
        public string MultiSelectText { get; set; }

        /// <summary>
        /// The visibility to return if an item is selected
        /// </summary>
        public Visibility Visibility { get; set; } = Visibility.Visible;

        /// <summary>
        /// The visibility to return if no items are selected
        /// </summary>
        public Visibility NonVisibility { get; set; } = Visibility.Collapsed;

        /// <summary>
        /// The visibility to return if multiple items are selected
        /// </summary>
        public Visibility MultiSelectVisibility { get; set; } = Visibility.Visible;


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            
            if (targetType == typeof(object))
            {

                if (value == null)
                {
                    return null;
                }
                else if (value is IEnumerable<object> items)
                {

                    int ic = items.Count();

                    if (ic == 0)
                    {
                        return null;
                    }
                    else if (ic == 1)
                    {
                        var x = items.First();

                        return GetTarget(x, TargetPath)?.ToString();
                    }
                    else
                    {
                        return MultiSelectText;
                    }
                } 
                else
                {
                    return value;
                }

            }
            else if (targetType == typeof(bool))
            {
                if (value == null)
                {
                    return false;
                }
                else if (value is IEnumerable<object> items)
                {
                    int ic = items.Count();

                    if (ic == 0)
                    {
                        return false;
                    }
                    else if (ic == 1)
                    {
                        return true;
                    }
                    else
                    {
                        if (MultiSelectVisibility == Visibility.Visible)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return true;
                }

            }
            else if (targetType == typeof(Visibility))
            {
                if (value == null)
                {
                    return NonVisibility;
                }
                else if (value is IEnumerable<object> items)
                {
                    int ic = items.Count();

                    if (ic == 0)
                    {
                        return NonVisibility;
                    }
                    else if (ic == 1)
                    {
                        return Visibility;
                    }
                    else
                    {
                        return MultiSelectVisibility;
                    }
                }
                else
                {
                    return Visibility;
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


        private object GetTarget(object item, string path)
        {
            string[] s = path.Split('.');
            object ini = item;

            PropertyInfo p;

            foreach (var si in s)
            {
                p = ini.GetType().GetProperty(si);
                if (p != null)
                {
                    ini = p.GetValue(ini);
                }

            }

            return ini;
        }

    }
}
