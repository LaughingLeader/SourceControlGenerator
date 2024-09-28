using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SCG.Converters;

public class TextWrapToScrollBarVisibilityConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		if (value is TextWrapping textWrapping)
		{
			if (textWrapping == TextWrapping.Wrap || textWrapping == TextWrapping.WrapWithOverflow) return ScrollBarVisibility.Disabled;
		}
		return ScrollBarVisibility.Auto;
	}

	public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		return null;
	}
}