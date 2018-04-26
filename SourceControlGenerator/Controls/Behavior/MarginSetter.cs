using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace LL.SCG.Controls.Behavior
{
	/// <summary>
	/// Attached behavior for setting child margins within panels.
	/// Source: http://blogs.microsoft.co.il/eladkatz/2011/05/29/what-is-the-easiest-way-to-set-spacing-between-items-in-stackpanel/
	/// </summary>
	public class MarginSetter
	{
		public static Thickness GetMargin(DependencyObject obj)
		{
			return (Thickness)obj.GetValue(MarginProperty);
		}

		public static void SetMargin(DependencyObject obj, Thickness value)
		{
			obj.SetValue(MarginProperty, value);
		}

		// Using a DependencyProperty as the backing store for Margin.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty MarginProperty = DependencyProperty.RegisterAttached("Margin", typeof(Thickness), typeof(MarginSetter), new UIPropertyMetadata(new Thickness(), MarginChangedCallback));

		public static void MarginChangedCallback(object sender, DependencyPropertyChangedEventArgs e)
		{
			// Make sure this is put on a panel
			var panel = sender as Panel;

			if (panel == null) return;

			panel.Loaded += new RoutedEventHandler(Panel_Loaded);
		}

		private static void Panel_Loaded(object sender, RoutedEventArgs e)
		{
			var panel = sender as Panel;

			// Go over the children and set margin for them:

			foreach (var child in panel.Children)
			{
				var fe = child as FrameworkElement;

				if (fe == null) continue;

				fe.Margin = MarginSetter.GetMargin(panel);
			}
		}
	}
}
