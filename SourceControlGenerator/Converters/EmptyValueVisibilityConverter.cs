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
	public class EmptyValueVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if(value != null)
			{
				var type = value.GetType();
				if(value is string stringValue)
				{
					if(!String.IsNullOrEmpty(stringValue)) return Visibility.Visible;
				}
				else
				{
					return Visibility.Visible;
				}
			}

			return Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}
	}
}