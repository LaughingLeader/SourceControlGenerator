using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace LL.SCG.Controls.Behavior
{
	/// <summary>
	/// Allows a ComboBox to size to the largest item within.
	/// Source: https://stackoverflow.com/a/4414372
	/// </summary>
	public static class ComboBoxWidthFromItemsBehavior
	{
		public static readonly DependencyProperty ComboBoxWidthFromItemsProperty =
			DependencyProperty.RegisterAttached
			(
				"ComboBoxWidthFromItems",
				typeof(bool),
				typeof(ComboBoxWidthFromItemsBehavior),
				new UIPropertyMetadata(false, OnComboBoxWidthFromItemsPropertyChanged)
			);
		public static bool GetComboBoxWidthFromItems(DependencyObject obj)
		{
			return (bool)obj.GetValue(ComboBoxWidthFromItemsProperty);
		}
		public static void SetComboBoxWidthFromItems(DependencyObject obj, bool value)
		{
			obj.SetValue(ComboBoxWidthFromItemsProperty, value);
		}
		private static void OnComboBoxWidthFromItemsPropertyChanged(DependencyObject dpo,
																	DependencyPropertyChangedEventArgs e)
		{
			ComboBox comboBox = dpo as ComboBox;
			if (comboBox != null)
			{
				if ((bool)e.NewValue == true)
				{
					comboBox.Loaded += OnComboBoxLoaded;
				}
				else
				{
					comboBox.Loaded -= OnComboBoxLoaded;
				}
			}
		}
		private static void OnComboBoxLoaded(object sender, RoutedEventArgs e)
		{
			ComboBox comboBox = sender as ComboBox;
			Action action = () => { comboBox.SetWidthFromItems(); };
			comboBox.Dispatcher.BeginInvoke(action, DispatcherPriority.ContextIdle);
		}
	}
}
