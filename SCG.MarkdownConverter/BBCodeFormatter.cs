using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Dom.Html;
using AngleSharp.Html;
using AngleSharp.Parser.Html;
using CodeKicker.BBCode;
using Markdig;

namespace LL.SCG.Markdown
{
	public class BBCodeFormatter : IMarkdownFormatter
	{
		public string Name { get; set; }

		public string DefaultFileExtension { get; set; }

		public List<string> BBIgnoredTags { get; set; }

		public List<Type> BBIgnoredElements { get; set; }

		private bool BBIgnoringLinks
		{
			get
			{
				return BBIgnoredTags.IndexOf(TagNames.Link) > 0 || BBIgnoredTags.IndexOf(TagNames.A) > 0 || BBIgnoredElements.IndexOf(typeof(IHtmlAnchorElement)) > 0;
			}
		}

		private bool BBIgnoringImages
		{
			get
			{
				return BBIgnoredTags.IndexOf(TagNames.Image) > 0 || BBIgnoredElements.IndexOf(typeof(IHtmlImageElement)) > 0;
			}
		}

		private bool BBIgnoringLists
		{
			get
			{
				return BBIgnoredElements.IndexOf(typeof(IHtmlOrderedListElement)) > 0 || BBIgnoredElements.IndexOf(typeof(IHtmlUnorderedListElement)) > 0 || BBIgnoredElements.IndexOf(typeof(IHtmlListItemElement)) > 0;
			}
		}

		public IHtmlDocument BBCodeConversion(IHtmlDocument doc)
		{
			if(!BBIgnoringLinks)
			{
				foreach (var element in doc.All.OfType<IHtmlAnchorElement>())
				{
					element.OuterHtml = $"[url={element.Href}]{element.InnerHtml}[/url]";
				}
			}
			

			if(!BBIgnoringImages)
			{
				foreach (var element in doc.All.OfType<IHtmlImageElement>())
				{
					element.OuterHtml = $"[img]{element.Source}[/img]";
				}
			}
			
			if(!BBIgnoringLists)
			{
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
			}

			if(BBIgnoredTags.IndexOf(TagNames.Strong) < 0)
			{
				foreach (var element in doc.GetElementsByTagName(TagNames.Strong))
				{
					element.OuterHtml = $"[b]{element.InnerHtml}[/b]";
				}
			}

			if (BBIgnoredTags.IndexOf(TagNames.Em) < 0)
			{
				foreach (var element in doc.GetElementsByTagName(TagNames.Em))
				{
					element.OuterHtml = $"[i]{element.InnerHtml}[/i]";
				}
			}

			if (BBIgnoredTags.IndexOf(TagNames.U) < 0)
			{
				foreach (var element in doc.GetElementsByTagName(TagNames.U))
				{
					element.OuterHtml = $"[u]{element.InnerHtml}[/u]";
				}
			}

			if (BBIgnoredTags.IndexOf(TagNames.P) < 0 && BBIgnoredElements.IndexOf(typeof(IHtmlParagraphElement)) < 0)
			{
				foreach (var element in doc.All.OfType<IHtmlParagraphElement>())
				{
					element.OuterHtml = element.InnerHtml + Environment.NewLine;
				}
			}

			if (BBIgnoredTags.IndexOf(TagNames.Code) < 0)
			{
				foreach (var element in doc.GetElementsByTagName(TagNames.Code))
				{
					element.OuterHtml = $"[code]{element.InnerHtml}[/code]";
				}
			}

			if (BBIgnoredTags.IndexOf(TagNames.Pre) < 0)
			{
				foreach (var element in doc.GetElementsByTagName(TagNames.Pre))
				{
					element.OuterHtml = $"[noparse]{element.InnerHtml}[/noparse]";
				}
			}

			if (BBIgnoredTags.IndexOf(TagNames.Strike) < 0)
			{
				foreach (var element in doc.GetElementsByTagName(TagNames.Strike))
				{
					element.OuterHtml = $"[strike]{element.InnerHtml}[/strike]";
				}
			}

			/*
			foreach (var element in doc.All)
			{
				Log.Here().Activity($"Element | Type {element.GetType()} Content: {element.OuterHtml}");
			}
			*/

			return doc;
		}

		public virtual string ConvertHTML(string input)
		{
			try
			{
				var parser = new HtmlParser(new HtmlParserOptions() { IsStrictMode = false});
				var doc = parser.Parse(input);

				doc = BBCodeConversion(doc);

				//AngleSharp adds html, head, and body tags, so we use the body's InnerHtml here.
				//var output = doc.Body.InnerHtml.Replace("<p>", "").Replace("</p>", Environment.NewLine);
				var output = doc.Body.InnerHtml;
				return output;
			}
			catch(Exception ex)
			{
				Log.Here().Error($"Error converting markdown to {Name}: {ex.ToString()}");
			}
			return "";
		}

		public void AddTagToIgnoreList(params string[] tagNames)
		{
			BBIgnoredTags.AddRange(tagNames);
		}

		public void AddElementToIgnoreList(params Type[] elementTypes)
		{
			BBIgnoredElements.AddRange(elementTypes);
		}

		public BBCodeFormatter()
		{
			Name = "BBCode";
			DefaultFileExtension = ".txt";

			BBIgnoredTags = new List<string>();
			BBIgnoredElements = new List<Type>();
		}
	}
}
