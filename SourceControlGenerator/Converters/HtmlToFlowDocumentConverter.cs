using Markdig;
using Markdig.Extensions;
using Markdig.Extensions.AutoLinks;
using Neo.Markdig.Xaml;
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
	public class HtmlToFlowDocumentConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var pipeline = new MarkdownPipelineBuilder().Configure(".html").Build();
			var doc = MarkdownXaml.ToFlowDocument((string)value, pipeline);
			return doc;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}