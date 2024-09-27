using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using SCG.SCGEnum;

namespace SCG.Converters
{
	public class FileValidationColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			FileValidation logType = (FileValidation)value;
			switch (logType)
			{
				case FileValidation.Error:
					return new SolidColorBrush(Colors.Red);
				case FileValidation.Warning:
					var color = Colors.Orange;
					color.A = 40;
					return new SolidColorBrush(color); // Transparent orange
			}

			return new SolidColorBrush(Colors.Transparent);
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}
	}
}