using Markdig;

namespace SCG.Markdown;

public class HTMLFormatterInterface : IMarkdownFormatter
{
	public string Name { get; set; }

	public string DefaultFileExtension { get; set; }

	public string ConvertHTML(string input)
	{
		return input;
	}

	public HTMLFormatterInterface()
	{
		Name = "HTML";
		DefaultFileExtension = ".html";
	}
}

public static class MarkdownConverter
{
	public static List<IMarkdownFormatter> InitFormatters()
	{
		List<IMarkdownFormatter> formatters =
		[
			new SteamWorkshopFormatter(),
			new NexusFormatter(),
			new LarianForumsUBBCodeFormatter(),
			new BBCodeFormatter(),
			new HTMLFormatterInterface(),
		];

		return formatters;
	}

	public static string ConvertMarkdownToHTML(string input)
	{
		try
		{
			var pipeline = new MarkdownPipelineBuilder().Build();
			var html = Markdig.Markdown.ToHtml(input, pipeline);
			return html;
		}
		catch (Exception ex)
		{
			Log.Here().Error($"Error converting markdown to HTML: {ex.ToString()}");
		}
		return input;
	}
}
