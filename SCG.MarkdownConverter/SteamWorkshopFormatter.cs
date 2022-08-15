using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

using System;
using System.Linq;

namespace SCG.Markdown
{
	public class SteamWorkshopFormatter : BBCodeFormatter
	{
		public SteamWorkshopFormatter() : base()
		{
			Name = "Steam Workshop";

			AddTagToIgnoreList(TagNames.Header);
		}

		public override string ConvertHTML(string input)
		{
			try
			{
				var parser = new HtmlParser(new HtmlParserOptions() { IsStrictMode = false });
				var doc = parser.ParseDocument(input);

				doc = BBCodeConversion(doc);

				foreach (var element in doc.All.OfType<IHtmlHeadingElement>())
				{
					element.OuterHtml = $"[h1]{element.InnerHtml}[/h1]";
				}

				//AngleSharp adds html, head, and body tags.
				var output = doc.Body.InnerHtml;
				return output;
			}
			catch (Exception ex)
			{
				Log.Here().Error($"Error converting markdown to {Name}: {ex.ToString()}");
			}
			return "";
		}
	}
}
