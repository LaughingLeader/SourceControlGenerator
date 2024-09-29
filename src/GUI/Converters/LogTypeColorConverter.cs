using SCG.Data.View;

using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace SCG.Converters;

public class LogTypeColorConverter : IValueConverter
{
	public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
	{
		if(value is LogType logType)
		{
			return logType switch
			{
				LogType.Important => LogData.ImportantBrush,
				LogType.Error => LogData.ErrorBrush,
				LogType.Warning => LogData.WarningBrush,
				_ => Brushes.Transparent,
			};
		}
		return null;
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture) => null;
}
