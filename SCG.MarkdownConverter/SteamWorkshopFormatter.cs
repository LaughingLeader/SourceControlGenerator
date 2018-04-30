using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using Markdig;

namespace LL.SCG.Markdown
{
	public class SteamWorkshopFormatter : BBCodeFormatter
	{
		public SteamWorkshopFormatter() : base()
		{
			Name = "Steam Workshop";
		}

		public override string ConvertHTML(string input)
		{
			try
			{
				var parser = new HtmlParser(new HtmlParserOptions() { IsStrictMode = false });
				var doc = parser.Parse(input);

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
