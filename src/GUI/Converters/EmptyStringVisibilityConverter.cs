using System.Windows;
using System.Windows.Data;

namespace SCG.Converters;

public class EmptyStringVisibilityConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		if (value is string str && String.IsNullOrEmpty(str)) return Visibility.Collapsed;

		return Visibility.Visible;
	}

	public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		return null;
	}
}