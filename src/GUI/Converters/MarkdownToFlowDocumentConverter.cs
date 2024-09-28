using System.Globalization;
using System.Windows.Data;

namespace SCG.Converters;

public class MarkdownToFlowDocumentConverter : IValueConverter
{
	private static MdXaml.Markdown? _markdownEngine;

	public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is string htmlText)
		{
			_markdownEngine ??= new MdXaml.Markdown();

			var doc = _markdownEngine.Transform(htmlText);
			return doc;
		}
		return null;
	}

	public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return null;
	}
}