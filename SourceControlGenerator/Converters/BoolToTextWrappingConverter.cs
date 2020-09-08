using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace SCG.Converters
{
	public class BoolToTextWrappingConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is bool enabled)
			{
				return enabled ? TextWrapping.Wrap : TextWrapping.NoWrap;
			}
			return TextWrapping.NoWrap;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is TextWrapping wrap)
			{
				return wrap == TextWrapping.Wrap ? true : false;
			}
			return false;
		}
	}
}
