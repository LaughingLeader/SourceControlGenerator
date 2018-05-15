using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using SCG.Util;

namespace SCG.Converters
{
	public class LogTypeColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			LogType logType = (LogType)value;
			Log.Here().Important($"Converting log type {logType.ToString()} to color.");
			switch (logType)
			{
				case LogType.Activity:
					return SystemColors.WindowBrush;
				case LogType.Important:
					return new SolidColorBrush(Colors.Azure);
				case LogType.Error:
					return new SolidColorBrush(Colors.Salmon);
				case LogType.Warning:
					return new SolidColorBrush(Colors.Khaki);
				default:
					return SystemColors.WindowBrush;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
