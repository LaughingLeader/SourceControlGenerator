using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace SCG.Converters
{
	public class BindingFallbackConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if(values.Length == 2)
			{
				if (values[0] == null && values[1] != null)
				{
					Log.Here().Activity($"Using tooltip 2: {values[1]}");
					return (string)values[1];
				}
			}
			Log.Here().Activity($"Using tooltip 1: {values[0]}");
			return (string)values[0];
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}