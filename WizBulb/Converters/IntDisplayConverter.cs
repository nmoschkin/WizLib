using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

using WizBulb.Localization.Resources;
using WizLib;

namespace WizBulb.Converters
{

    public class ConverterErrorEventArgs : EventArgs
    {

        public string Message { get; private set; }

        public int Code { get; private set; }

        public ConverterErrorEventArgs(string msg, int code = 0)
        {
            Message = msg;
            Code = code;
        }
    }


    public delegate void ConverterErrorEvent(object sender, ConverterErrorEventArgs e);

    public class IntDisplayConverter : ViewModelBase, IValueConverter
    {
        private bool error;
        private string errorMsg;

        public event ConverterErrorEvent ConverterError;

        public int MinValue { get; set; } = 5;

        public int MaxValue { get; set; } = 60;

        public bool ErrorState
        {
            get => error;
            set
            {
                SetProperty(ref error, value);
            }
        }

        public string ErrorMessage
        {
            get => errorMsg;
            set
            {
                SetProperty(ref errorMsg, value);
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int i)
            {
                return i.ToString();
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
                int i = 0;

                if (int.TryParse(s, out i))
                {
                    if (i >= MinValue && i <= MaxValue)
                    {
                        ErrorState = false;
                        ErrorMessage = AppResources.Success;

                        return i;
                    }
                    else
                    {
                        ErrorMessage = AppResources.ErrorValueOutOfRange;
                    }
                }
                else
                {
                    ErrorMessage = AppResources.ErrorValueNotNumber;
                }
            }
            else
            {
                ErrorMessage = AppResources.ErrorValueNotNumber;
            }


            ErrorState = true;
            ConverterError?.Invoke(this, new ConverterErrorEventArgs(ErrorMessage));

            return value; 

        }
    }
}
