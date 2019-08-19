using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SCG.Controls
{
	public class MonitoredContentPresenter : System.Windows.Controls.ContentPresenter
	{
		#region RE: ContentChanged
		public static RoutedEvent ContentChangedEvent = EventManager.RegisterRoutedEvent("ContentChanged", 
			RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MonitoredContentPresenter));
		public event RoutedEventHandler ContentChanged
		{
			add { AddHandler(ContentChangedEvent, value); }
			remove { RemoveHandler(ContentChangedEvent, value); }
		}
		public static void AddContentChangedHandler(UIElement el, RoutedEventHandler handler)
		{
			el.AddHandler(ContentChangedEvent, handler);
		}
		public static void RemoveContentChangedHandler(UIElement el, RoutedEventHandler handler)
		{
			el.RemoveHandler(ContentChangedEvent, handler);
		}
		#endregion

		protected override void OnVisualChildrenChanged(System.Windows.DependencyObject visualAdded, System.Windows.DependencyObject visualRemoved)
		{
			base.OnVisualChildrenChanged(visualAdded, visualRemoved);
			RaiseEvent(new RoutedEventArgs(ContentChangedEvent, this));
		}
	}
}
