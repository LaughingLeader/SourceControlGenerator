using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace SCG.Converters
{
	public class TextWrapToScrollBarVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if(value is TextWrapping textWrapping)
			{
				if (textWrapping == TextWrapping.Wrap || textWrapping == TextWrapping.WrapWithOverflow) return ScrollBarVisibility.Disabled;
			}
			return ScrollBarVisibility.Auto;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}
	}
}