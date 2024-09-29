using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace SCG.Converters;

public class LogTypeColorConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		var logType = (LogType)value;
		Log.Here().Important($"Converting log type {logType.ToString()} to color.");
		switch (logType)
		{
			case LogType.Important:
				return new SolidColorBrush(Colors.Azure);
			case LogType.Error:
				return new SolidColorBrush(Colors.Salmon);
			case LogType.Warning:
				return new SolidColorBrush(Colors.Khaki);
			case LogType.Activity:
			default:
				return Brushes.Transparent;
		}
	}

	public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
