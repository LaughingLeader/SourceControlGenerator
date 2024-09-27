using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SCG.Controls.Behavior
{
    public class InputBindingsBehavior
    {
        public static readonly DependencyProperty TakesInputBindingPrecedenceProperty =
            DependencyProperty.RegisterAttached("TakesInputBindingPrecedence", typeof(bool), typeof(InputBindingsBehavior), new UIPropertyMetadata(false, OnTakesInputBindingPrecedenceChanged));

        public static bool GetTakesInputBindingPrecedence(UIElement obj)
        {
            return (bool)obj.GetValue(TakesInputBindingPrecedenceProperty);
        }

        public static void SetTakesInputBindingPrecedence(UIElement obj, bool value)
        {
            obj.SetValue(TakesInputBindingPrecedenceProperty, value);
        }

        private static void OnTakesInputBindingPrecedenceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((UIElement)d).PreviewKeyDown += new KeyEventHandler(InputBindingsBehavior_PreviewKeyDown);
        }

        private static void InputBindingsBehavior_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var uielement = (UIElement)sender;

            //Log.Here().Activity($"InputBindingsBehavior_PreviewKeyDown: {e.Key}");

            KeyBinding foundBinding = ((UIElement)uielement).InputBindings
            .OfType<KeyBinding>()
            .FirstOrDefault(inputBinding => inputBinding.Gesture.Matches(sender, e));

            if (foundBinding != null)
            {
                e.Handled = true;
                if (foundBinding.Command.CanExecute(foundBinding.CommandParameter))
                {
                    foundBinding.Command.Execute(foundBinding.CommandParameter);
                }
            }
        }
    }
}
