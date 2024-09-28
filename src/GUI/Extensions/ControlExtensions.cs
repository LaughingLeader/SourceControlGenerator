using System.Windows;
using System.Windows.Media;

namespace SCG.Extensions;

public static class ControlExtensions
{
	public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject depObj) where T : DependencyObject
	{
		if (depObj != null)
		{
			for (var i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
			{
				var child = VisualTreeHelper.GetChild(depObj, i);
				if (child != null && child is T)
				{
					yield return (T)child;
				}

				foreach (var childOfChild in FindVisualChildren<T>(child))
				{
					yield return childOfChild;
				}
			}
		}
	}

	public static T FindParent<T>(this DependencyObject child) where T : DependencyObject
	{
		//get parent item
		var parentObject = VisualTreeHelper.GetParent(child);

		//we've reached the end of the tree
		if (parentObject == null) return null;

		//check if the parent matches the type we're looking for
		var parent = parentObject as T;
		if (parent != null)
			return parent;
		else
			return FindParent<T>(parentObject);
	}
}
