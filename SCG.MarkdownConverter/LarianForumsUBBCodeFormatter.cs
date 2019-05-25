using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Markdig;

namespace SCG.Markdown
{
	public class LarianForumsUBBCodeFormatter : BBCodeFormatter
	{
		public bool DisableListTag { get; set; } = false;

		public LarianForumsUBBCodeFormatter() : base()
		{
			Name = "Larian Forums";

			AddTagToIgnoreList(TagNames.Strike, TagNames.Header);
			AddElementToIgnoreList(typeof(IHtmlOrderedListElement));
		}

		private string HeaderTag(int size)
		{
			return $"[size: {size}pt][b]";
		}

		public override string ConvertHTML(string input)
		{
			try
			{
				var parser = new HtmlParser(new HtmlParserOptions() { IsStrictMode = false });
				var doc = parser.ParseDocument(input);

				doc = BBCodeConversion(doc);

				//foreach (var element in doc.All.OfType<IHtmlHeadingElement>())
				//{
				//	element.OuterHtml = $"[b][u]{element.InnerHtml}[/u][/b]";
				//}

				foreach (var element in doc.All.OfType<IHtmlHeadingElement>())
				{
					var comparer = StringComparison.OrdinalIgnoreCase;
					var text = element.OuterHtml;

					var headerEndTag = "[/b][/size]";

					text = text.Replace("<h1>", HeaderTag(23), comparer).Replace("<h2>", HeaderTag(20), comparer).Replace("<h3>", HeaderTag(17), comparer).Replace("<h4>", HeaderTag(14), comparer).Replace("<h5>", HeaderTag(11), comparer).Replace("<h6>", HeaderTag(8), comparer);
					text = text.Replace("</h1>", headerEndTag, comparer).Replace("</h2>", headerEndTag, comparer).Replace("</h3>", headerEndTag, comparer).Replace("</h4>", headerEndTag, comparer).Replace("</h5>", headerEndTag, comparer).Replace("</h6>", headerEndTag, comparer);
					element.OuterHtml = text;
				}

				foreach (var element in doc.GetElementsByTagName(TagNames.Strike))
				{
					element.OuterHtml = $"[s]{element.InnerHtml}[/s]";
				}

				foreach (var element in doc.All.OfType<IHtmlOrderedListElement>())
				{
					if (!DisableListTag)
						element.OuterHtml = $"[list=1]{element.InnerHtml}[/list]";
					else
						element.OuterHtml = element.InnerHtml;
				}

				foreach (var element in doc.All.OfType<IHtmlUnorderedListElement>())
				{
					if (!DisableListTag)
						element.OuterHtml = $"[list]{element.InnerHtml}[/list]";
					else
						element.OuterHtml = element.InnerHtml;
				}

				foreach (var element in doc.All.OfType<IHtmlListItemElement>())
				{
					if (!DisableListTag)
						element.OuterHtml = $"[*] {element.InnerHtml}";
					else
						element.OuterHtml = $"• {element.InnerHtml}";
				}

				foreach (var element in doc.All.OfType<IHtmlHrElement>())
				{
					element.OuterHtml = $"___________________________{element.InnerHtml}";
				}

				//AngleSharp adds html, head, and body tags.
				var output = doc.Body.InnerHtml;

				output.Replace("\t", "&nbsp;&nbsp;");

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
