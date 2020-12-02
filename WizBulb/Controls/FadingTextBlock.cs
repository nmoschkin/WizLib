using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WizBulb.Controls
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:WizBulb.Controls"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:WizBulb.Controls;assembly=WizBulb.Controls"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:FadingTextBlock/>
    ///
    /// </summary>
    public class FadingTextBlock : TextBlock
    {
        static FadingTextBlock()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FadingTextBlock), new FrameworkPropertyMetadata(typeof(FadingTextBlock)));
        }

        public FadingTextBlock()
        {
        }

        public new string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public new static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(FadingTextBlock), new PropertyMetadata("", TextPropertyChanged));

        protected static void TextPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is FadingTextBlock b)
            {
                if (string.Compare((string)e.OldValue, (string)e.NewValue) != 0)
                {
                    if (b.Visibility != Visibility.Collapsed)
                    {
                        DoubleAnimation a;
                        
                        if (string.IsNullOrEmpty((string)e.NewValue))
                        {
                            a = new DoubleAnimation()
                            {
                                Duration = new Duration(new TimeSpan(0, 0, 0, 0, 200)),
                                From = 1,
                                To = 0
                            };
                        }
                        else
                        {
                            a = new DoubleAnimation()
                            {
                                Duration = new Duration(new TimeSpan(0, 0, 0, 0, 200)),
                                From = 0,
                                To = 1
                            };
                        }

                        b.BeginAnimation(OpacityProperty, a);
                    }

                    if (string.IsNullOrEmpty((string)e.NewValue))
                    {
                        var disp = App.Current.Dispatcher;

                        _ = Task.Run(async () =>
                        {
                            await Task.Delay(200);
                            disp.Invoke(() => b.SetValue(TextBlock.TextProperty, e.NewValue));
                        });
                    }
                    else
                    {
                        b.SetValue(TextBlock.TextProperty, e.NewValue);
                    }
                }
            }
        }

    }
}
