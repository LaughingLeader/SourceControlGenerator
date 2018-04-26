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
	public class IntToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if(value is int count)
			{
				int paramval = 1;

				if (parameter is string param)
				{
					int.TryParse(param, out paramval);
				}
				if(paramval == 1)
				{
					if (count > 0)
					{
						return Visibility.Visible;
					}
					else
					{
						return Visibility.Collapsed;
					}
				}
				else
				{
					if (count > 0)
					{
						return Visibility.Collapsed;
					}
					else
					{
						return Visibility.Visible;
					}
				}
			}

			return Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}
	}
}