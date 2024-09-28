using System.Windows.Data;
using System.Windows.Media;

namespace SCG.Converters;

public class FooterOutputColorConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		var logType = (LogType)value;
		switch (logType)
		{
			case LogType.Important:
				return Colors.Green;
			case LogType.Error:
				return Colors.Red;
			default:
				return Colors.Black;
		}
	}

	public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
