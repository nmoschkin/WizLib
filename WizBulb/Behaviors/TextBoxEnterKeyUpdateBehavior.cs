using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WizBulb.Behaviors
{
    public class TextBoxEnterKeyUpdateBehavior : Behavior<TextBox>
    {
        string stxt;


        private static bool ValidateUpdateTarget(object value)
        {
            return true;
        }

        public static readonly DependencyProperty ObservableUpdatePathProperty
            = DependencyProperty.Register(nameof(ObservableUpdatePath), typeof(string), typeof(TextBoxEnterKeyUpdateBehavior), new PropertyMetadata(defaultValue: null), ValidateUpdateTarget);

        public string ObservableUpdatePath
        {
            get
            {
                return (string)GetValue(ObservableUpdatePathProperty);
            }
            set
            {
                SetValue(ObservableUpdatePathProperty, value);
            }
        }


        public static readonly DependencyProperty ObservableUpdatePropertyProperty
            = DependencyProperty.Register(nameof(ObservableUpdateProperty), typeof(string), typeof(TextBoxEnterKeyUpdateBehavior), new PropertyMetadata(defaultValue: null), ValidateUpdateTarget);

        public string ObservableUpdateProperty
        {
            get
            {
                return (string)GetValue(ObservableUpdatePropertyProperty);
            }
            set
            {
                SetValue(ObservableUpdatePropertyProperty, value);
            }
        }



        private static bool ValidateTarget(object value)
        {
            if (value == null) return true;

            if (typeof(FrameworkElement).IsAssignableFrom(value.GetType())) return true;
            return false;
        }

        public static readonly DependencyProperty CancelFocusReceiverProperty
            = DependencyProperty.Register(nameof(CancelFocusReceiver), typeof(object), typeof(TextBoxEnterKeyUpdateBehavior), new PropertyMetadata(defaultValue: null), ValidateTarget);

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
            = DependencyProperty.Register(nameof(ReturnFocusReceiver), typeof(object), typeof(TextBoxEnterKeyUpdateBehavior), new PropertyMetadata(defaultValue: null), ValidateTarget);

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



        private object GetTarget(object item, string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return item;
            }

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

        protected override void OnAttached()
        {
            if (this.AssociatedObject != null)
            {
                base.OnAttached();
                this.AssociatedObject.GotFocus += AssociatedObject_GotFocus;
                this.AssociatedObject.LostFocus += AssociatedObject_LostFocus;
                this.AssociatedObject.KeyDown += AssociatedObject_KeyDown;
            }
        }

        private void AssociatedObject_LostFocus(object sender, RoutedEventArgs e)
        {
            TriggerUpdateTarget((TextBox)sender);
        }

        private void AssociatedObject_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            stxt = this.AssociatedObject.Text;
        }

        protected override void OnDetaching()
        {
            if (this.AssociatedObject != null)
            {
                this.AssociatedObject.KeyDown -= AssociatedObject_KeyDown;
                this.AssociatedObject.LostFocus -= AssociatedObject_LostFocus;
                this.AssociatedObject.GotFocus -= AssociatedObject_GotFocus;
                base.OnDetaching();
            }
        }

        private void TriggerUpdateTarget(TextBox sender)
        {
            if (sender.Text == stxt) return;

            var o = GetTarget(sender.DataContext, ObservableUpdatePath);

            if (o is WiZ.Observable.ObservableBase ob)
            {
                ob.OnPropertyChanged(ObservableUpdateProperty);
            }
        }

        private void AssociatedObject_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                if (e.Key == Key.Return)
                {
                    if (e.Key == Key.Enter)
                    {
                        e.Handled = true;

                        if (ReturnFocusReceiver != null)
                        {
                            ((FrameworkElement)ReturnFocusReceiver).Focus();
                        }
                        else
                        {
                            textBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                        }
                    }
                }
                else if (e.Key == Key.Escape)
                {
                    e.Handled = true;                    
                    
                    textBox.Text = stxt;

                    if (CancelFocusReceiver != null)
                    {
                        ((FrameworkElement)CancelFocusReceiver).Focus();
                    }
                    else
                    {
                        textBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
                    }
                }
            }
        }
    }
}
