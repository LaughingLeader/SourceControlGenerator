using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Markdig;

namespace LL.SCG.Markdown
{
	public class HTMLFormatterInterface : IMarkdownFormatter
	{
		public string Name { get; set; }

		public string Convert(string input)
		{
			var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
			return Markdig.Markdown.ToHtml(input, pipeline);
		}

		public HTMLFormatterInterface()
		{
			Name = "HTML";
		}
	}

	public static class MarkdownConverter
	{
		public static List<IMarkdownFormatter> InitFormatters()
		{
			List<IMarkdownFormatter> formatters = new List<IMarkdownFormatter>();

			formatters.Add(new SteamWorkshopFormatter());
			formatters.Add(new BBCodeFormatter());
			formatters.Add(new HTMLFormatterInterface());

			return formatters;
		}
	}
}
