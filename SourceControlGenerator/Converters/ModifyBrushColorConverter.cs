using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace LL.SCG.Converters
{
	public class ModifyBrushColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if(value is SolidColorBrush brush)
			{
				if(parameter is float val)
				{
					brush.Color = Color.FromRgb((byte)(brush.Color.R * val), (byte)(brush.Color.G * val), (byte)(brush.Color.B * val));
					return brush;
				}
			}
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}
	}
}