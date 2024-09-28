using System.Windows.Data;

namespace SCG.Converters;

public class StringToBoolConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		if (value is string str)
		{
			return !String.IsNullOrEmpty(str);
		}
		return false;
	}

	public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		return null;
	}
}