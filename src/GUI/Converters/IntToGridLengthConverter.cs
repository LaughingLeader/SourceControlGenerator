using System.Windows;
using System.Windows.Data;

namespace SCG.Converters;

public class IntToGridLengthConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		if (value is int count)
		{
			if (count > 0)
			{
				return GridLength.Auto;
			}
		}

		return GridLength.Auto;
	}

	public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		return null;
	}
}