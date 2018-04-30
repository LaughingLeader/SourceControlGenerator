using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using CodeKicker.BBCode;
using Markdig;

namespace LL.SCG.Markdown
{
	public class BBCodeFormatter : IMarkdownFormatter
	{
		public string Name { get; set; }

		public IHtmlDocument BBCodeConversion(IHtmlDocument doc)
		{
			foreach (var element in doc.All.OfType<IHtmlAnchorElement>())
			{
				element.OuterHtml = $"[url={element.Href}]{element.InnerHtml}[/url]";
			}

			foreach (var element in doc.All.OfType<IHtmlImageElement>())
			{
				element.OuterHtml = $"[img]{element.Source}[/img]";
			}

			foreach (var element in doc.All.OfType<IHtmlOrderedListElement>())
			{
				element.OuterHtml = $"[olist]{element.InnerHtml}[/olist]";
			}

			foreach (var element in doc.All.OfType<IHtmlUnorderedListElement>())
			{
				element.OuterHtml = $"[list]{element.InnerHtml}[/list]";
			}

			foreach (var element in doc.All.OfType<IHtmlListItemElement>())
			{
				element.OuterHtml = $"[*]{element.InnerHtml}";
			}

			return doc;
		}

		public virtual string Convert(string input)
		{
			try
			{
				var pipeline = new MarkdownPipelineBuilder().Build();
				var html = Markdig.Markdown.ToHtml(input, pipeline);

				var parser = new HtmlParser(new HtmlParserOptions() { IsStrictMode = false});
				var doc = parser.Parse(html);

				doc = BBCodeConversion(doc);

				//AngleSharp adds html, head, and body tags, so we use the body's InnerHtml here.
				var output = doc.Body.InnerHtml.Replace("<p>", "").Replace("</p>", Environment.NewLine);
				return output;
			}
			catch(Exception ex)
			{
				Log.Here().Error($"Error converting markdown to {Name}: {ex.ToString()}");
			}
			return "";
		}

		public BBCodeFormatter()
		{
			Name = "BBCode";
		}
	}
}
