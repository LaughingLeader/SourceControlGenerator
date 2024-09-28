using SCG.Data.View;

using System.Windows;

namespace SCG.Data.Proxy;

public class IconProxy : Freezable
{
	#region Overrides of Freezable

	protected override Freezable CreateInstanceCore()
	{
		return new IconProxy();
	}

	#endregion

	public object Data
	{
		get { return (object)GetValue(DataProperty); }
		set { SetValue(DataProperty, value); }
	}

	// Using a DependencyProperty as the backing store for Data.
	// This enables animation, styling, binding, etc...
	public static readonly DependencyProperty DataProperty =
		DependencyProperty.Register("Data",
									typeof(IconPathData),
									typeof(IconProxy),
									new UIPropertyMetadata(null));
}
