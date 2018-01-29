using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using LL.DOS2.SourceControl.Util;

namespace LL.DOS2.SourceControl.Controls
{
	public class LogTypeColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			LogType logType = (LogType)value;			
			switch (logType)
			{
				case LogType.Activity:
					return SystemColors.WindowBrush;
				case LogType.Important:
					return Colors.Azure;
				case LogType.Error:
					return Colors.Salmon;
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
