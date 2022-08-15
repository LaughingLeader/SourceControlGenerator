using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

using System;
using System.Linq;

namespace SCG.Markdown
{
	public class NexusFormatter : BBCodeFormatter
	{
		public NexusFormatter() : base()
		{
			Name = "Nexus Mods";

			AddTagToIgnoreList(TagNames.Strike, TagNames.Header);
			AddElementToIgnoreList(typeof(IHtmlOrderedListElement));
		}

		public virtual IHtmlDocument NexusBBCodeConversion(IHtmlDocument doc)
		{
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

			foreach (var element in doc.GetElementsByTagName(TagNames.Strike))
			{
				element.OuterHtml = $"[s]{element.InnerHtml}[/s]";
			}

			foreach(var element in doc.All.OfType<IHtmlHeadingElement>())
			{
				element.OuterHtml = $"[heading]{element.InnerHtml}[/heading]";
			}

			foreach (var element in doc.All.OfType<IHtmlHrElement>())
			{
				element.OuterHtml = $"[line]{element.InnerHtml}";
			}

			return doc;
		}

		public override string ConvertHTML(string input)
		{
			try
			{
				var parser = new HtmlParser(new HtmlParserOptions() { IsStrictMode = false });
				var doc = parser.ParseDocument(input);

				doc = BBCodeConversion(doc);
				doc = NexusBBCodeConversion(doc);
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
