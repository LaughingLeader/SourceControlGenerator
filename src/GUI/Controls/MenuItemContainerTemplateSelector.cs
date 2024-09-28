using System.Windows;
using System.Windows.Controls;

namespace SCG.Controls;

public class MenuItemContainerTemplateSelector : ItemContainerTemplateSelector
{
	public override DataTemplate SelectTemplate(object item, ItemsControl parentItemsControl)
	{
		var key = new DataTemplateKey(item.GetType());
		if (key != null)
		{
			var template = (DataTemplate)parentItemsControl.FindResource(key);
			if (template != null) return template;
		}
		return base.SelectTemplate(item, parentItemsControl);
	}
}
