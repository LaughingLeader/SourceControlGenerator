using System.Windows;
using System.Windows.Controls;

namespace SCG.Controls;

public class CrossButton : Button
{
	/// <summary>
	/// Initializes the <see cref="CrossButton"/> class.
	/// </summary>
	static CrossButton()
	{
		DefaultStyleKeyProperty.OverrideMetadata(typeof(CrossButton), new FrameworkPropertyMetadata(typeof(CrossButton)));
	}
}
