using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SynoDL8
{
    public class StateHelper : DependencyObject
    {
        public static readonly DependencyProperty StateProperty = DependencyProperty.RegisterAttached(
            "State", typeof(String), typeof(StateHelper), new PropertyMetadata("Normal", StateChanged));

        internal static void StateChanged(DependencyObject target, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue != null)
            {
                VisualStateManager.GoToState((Control)target, (string)args.NewValue, true);
            }
        }

        public static void SetState(DependencyObject obj, string value)
        {
            obj.SetValue(StateProperty, value);
        }

        public static string GetState(DependencyObject obj)
        {
            return (string)obj.GetValue(StateProperty);
        }
    }
}
