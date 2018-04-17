using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace LL.SCG.Converters
{
	public class ObjectColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			Color? color = null;
			SolidColorBrush brush = null;
			string[] parameters = null;

			//Log.Here().Activity($"Attempting to parse {value.GetType()} and param {parameter.ToString()}");

			if (parameter is string strParam)
			{
				parameters = strParam.Split(';');
			}
			if (value is Control control && parameters != null)
			{
				if (parameters[0].IndexOf("background", StringComparison.OrdinalIgnoreCase) > 0)
				{
					brush = (SolidColorBrush)control.Background;
					color = brush.Color;
				}
				else if (parameters[0].IndexOf("foreground", StringComparison.OrdinalIgnoreCase) > 0)
				{
					brush = (SolidColorBrush)control.Foreground;
					color = brush.Color;
				}
				else if (parameters[0].IndexOf("background", StringComparison.OrdinalIgnoreCase) > 0)
				{
					brush = (SolidColorBrush)control.BorderBrush;
					color = brush.Color;
				}
			}
			else if(value is SolidColorBrush)
			{
				brush = (SolidColorBrush)value;
				color = brush.Color;
			}
			else
			{
				Log.Here().Error($"Error with ObjectColorConverter: value passed in is of the type {value.GetType()}");
			}

			if (color != null)
			{
				if (parameters != null && parameters.Length > 1 && parameters[1] is string secondVal && !String.IsNullOrEmpty(secondVal))
				{
					double colorModVal = 1;

					var parsed = double.TryParse(secondVal, out colorModVal);
					if (parsed)
					{
						var finalColor = Color.FromRgb((byte)(color.Value.R * colorModVal), (byte)(color.Value.G * colorModVal), (byte)(color.Value.B * colorModVal));
						//Log.Here().Activity($"Final color is {finalColor} | colorModVal is {colorModVal}");
						return finalColor;
					}
				}
			}
			else
			{
				if (brush != null) return brush.Color;
			}

			return Colors.Red;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}
	}
}