using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SCG.Controls.Behavior
{
	public static class TextBoxEnterKeyBehavior
	{
		public static readonly DependencyProperty EnterLosesFocusProperty = DependencyProperty.RegisterAttached("EnterLosesFocus", typeof(bool), typeof(TextBoxEnterKeyBehavior), new UIPropertyMetadata(false, OnEnterBehaviorEnabled));

		public static bool GetEnterLosesFocus(DependencyObject obj)
		{
			return (bool)obj.GetValue(EnterLosesFocusProperty);
		}

		public static void SetEnterLosesFocus(DependencyObject obj, bool value)
		{
			obj.SetValue(EnterLosesFocusProperty, value);
		}

		private static void OnEnterBehaviorEnabled(DependencyObject dpo, DependencyPropertyChangedEventArgs e)
		{
			TextBox control = dpo as TextBox;
			if (control != null)
			{
				if ((bool)e.NewValue == true)
				{
					control.KeyDown += TextBox_OnKeyDown;
				}
				else
				{
					control.KeyDown -= TextBox_OnKeyDown;
				}
			}
		}

		private static void TextBox_OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			TextBox textBox = sender as TextBox;
			if (textBox != null)
			{
				if (e.Key == Key.Return)
				{
					if (e.Key == Key.Enter)
					{
						textBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
					}
				}
			}
		}
	}
}
