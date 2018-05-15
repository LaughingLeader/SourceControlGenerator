using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SCG.Controls
{
	public class MenuItemContainerTemplateSelector : ItemContainerTemplateSelector
	{
		public override DataTemplate SelectTemplate(object item, ItemsControl parentItemsControl)
		{
			var key = new DataTemplateKey(item.GetType());
			return (DataTemplate)parentItemsControl.FindResource(key);
		}
	}
}
