using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using LL.SCG.Data.View;
using LL.SCG.Markdown;

namespace LL.SCG.Converters
{
	public class FormatterToFormatterDataConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if(value is IMarkdownFormatter formatter)
			{
				return new MarkdownFormatterData()
				{
					Formatter = formatter
				};
			}
			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}
	}
}