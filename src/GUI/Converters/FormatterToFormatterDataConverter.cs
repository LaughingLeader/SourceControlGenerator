using SCG.Data.View;
using SCG.Markdown;

using System.Windows.Data;

namespace SCG.Converters;

public class FormatterToFormatterDataConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		if (value is IMarkdownFormatter formatter)
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