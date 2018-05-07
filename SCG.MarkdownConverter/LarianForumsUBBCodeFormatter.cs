using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Dom.Html;
using AngleSharp.Html;
using AngleSharp.Parser.Html;
using Markdig;

namespace LL.SCG.Markdown
{
	public class LarianForumsUBBCodeFormatter : BBCodeFormatter
	{
		public LarianForumsUBBCodeFormatter() : base()
		{
			Name = "Larian Forums";

			AddTagToIgnoreList(TagNames.Strike, TagNames.Header);
			AddElementToIgnoreList(typeof(IHtmlOrderedListElement));
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
					element.OuterHtml = $"[b][u]{element.InnerHtml}[/u][/b]";
				}

				foreach (var element in doc.GetElementsByTagName(TagNames.Strike))
				{
					element.OuterHtml = $"[s]{element.InnerHtml}[/s]";
				}

				foreach (var element in doc.All.OfType<IHtmlOrderedListElement>())
				{
					element.OuterHtml = $"[list=1]{element.InnerHtml}[/list]";
				}

				foreach (var element in doc.All.OfType<IHtmlUnorderedListElement>())
				{
					element.OuterHtml = $"[list]{element.InnerHtml}[/list]";
				}

				foreach (var element in doc.All.OfType<IHtmlListItemElement>())
				{
					element.OuterHtml = $"[*]{element.InnerHtml}";
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
