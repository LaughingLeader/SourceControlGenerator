using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SCG.Controls.Behavior
{
	public static class ListBoxSelectionBehavior
	{
		public static readonly DependencyProperty ClickSelectionProperty =
			DependencyProperty.RegisterAttached("ClickSelection",
												typeof(bool),
												typeof(ListBoxSelectionBehavior),
												new UIPropertyMetadata(false, OnClickSelectionChanged));
		public static bool GetClickSelection(DependencyObject obj)
		{
			return (bool)obj.GetValue(ClickSelectionProperty);
		}
		public static void SetClickSelection(DependencyObject obj, bool value)
		{
			obj.SetValue(ClickSelectionProperty, value);
		}
		private static void OnClickSelectionChanged(DependencyObject dpo,
																 DependencyPropertyChangedEventArgs e)
		{
			ListBox listBox = dpo as ListBox;
			if (listBox != null)
			{
				if ((bool)e.NewValue == true)
				{
					listBox.SelectionMode = SelectionMode.Extended;
					listBox.SelectionChanged += OnSelectionChanged;
				}
				else
				{
					listBox.SelectionChanged -= OnSelectionChanged;
				}
			}
		}

		static void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
			{
				ListBox listBox = sender as ListBox;
				if(e.AddedItems.Count > 0)
				{
					var valid = e.AddedItems[0];
					foreach (var item in new ArrayList(listBox.SelectedItems))
					{
						if (item != valid)
						{
							listBox.SelectedItems.Remove(item);
						}
					}
				}
			}
		}
	}
}
