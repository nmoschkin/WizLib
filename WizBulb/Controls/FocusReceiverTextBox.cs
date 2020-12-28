using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using System.Windows.Input;
using System.Windows.Interactivity;


namespace WizBulb.Controls
{
    public class FocusReceiverTextBox : TextBox
    {

        private static bool ValidateTarget(object value)
        {
            if (value == null) return true;

            if (typeof(FrameworkElement).IsAssignableFrom(value.GetType())) return true;
            return false;
        }

        public static readonly DependencyProperty CancelFocusReceiverProperty
            = DependencyProperty.Register(nameof(CancelFocusReceiver), typeof(object), typeof(FocusReceiverTextBox), new PropertyMetadata(defaultValue: null), ValidateTarget);

        public object CancelFocusReceiver
        {
            get
            {
                return GetValue(CancelFocusReceiverProperty);
            }
            set
            {
                SetValue(CancelFocusReceiverProperty, value);
            }
        }

        public static readonly DependencyProperty ReturnFocusReceiverProperty
            = DependencyProperty.Register(nameof(ReturnFocusReceiver), typeof(object), typeof(FocusReceiverTextBox), new PropertyMetadata(defaultValue: null), ValidateTarget);

        public object ReturnFocusReceiver
        {
            get
            {
                return GetValue(ReturnFocusReceiverProperty);
            }
            set
            {
                SetValue(ReturnFocusReceiverProperty, value);
            }
        }






    }
}
