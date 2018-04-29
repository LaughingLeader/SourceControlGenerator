using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkdownConverter
{
	public class HTMLFormatter
	{
		public ITagReplacer Link { get; set; }
		public ITagReplacer Header { get; set; }
		public ITagReplacer Bold { get; set; }
		public ITagReplacer Underline { get; set; }
		public ITagReplacer Italic { get; set; }
		public ITagReplacer Strikethrough { get; set; }
		public ITagReplacer List { get; set; }
		public ITagReplacer OrderedList { get; set; }
		public ITagReplacer Quote { get; set; }
		public ITagReplacer Code { get; set; }

		public HTMLFormatter()
		{
			Link = new TagReplacerData("<a>");
			Header = new TagReplacerCollection
			(
				new TagReplacerData("<h1>"), 
				new TagReplacerData("<h2>"),
				new TagReplacerData("<h3>"),
				new TagReplacerData("<h4>"),
				new TagReplacerData("<h5>"),
				new TagReplacerData("<h6>")
			);
		}
	}

	public interface ITagReplacer
	{
		string Replace(string input);
		void SetReplacements(string startTagReplacement, string endTagReplacement = "");
	}

	public class TagReplacerCollection : ITagReplacer
	{
		public List<ITagReplacer> TagReplacers { get; set; }

		public TagReplacerCollection()
		{
			TagReplacers = new List<ITagReplacer>();
		}

		public TagReplacerCollection(params ITagReplacer[] tagReplacers)
		{
			TagReplacers = tagReplacers.ToList();
		}

		public void SetReplacements(string startTagReplacement, string endTagReplacement = "")
		{
			foreach (var tagReplacer in TagReplacers)
			{
				tagReplacer.SetReplacements(startTagReplacement, endTagReplacement);
			}
		}

		public string Replace(string input)
		{
			var output = input;
			foreach (var tagReplacer in TagReplacers)
			{
				output = tagReplacer.Replace(output);
			}
			return output;
		}
	}

	public class TagReplacerData : ITagReplacer
	{
		public string StartTag { get; set; }
		public string EndTag { get; set; }
		public string StartTagReplacement { get; set; }
		public string EndTagReplacement { get; set; }

		public void SetReplacements(string startTagReplacement, string endTagReplacement = "")
		{
			StartTagReplacement = startTagReplacement;

			if (String.IsNullOrEmpty(endTagReplacement))
			{
				EndTagReplacement = StartTagReplacement.Insert(1, "/");
			}
			else
			{
				EndTagReplacement = endTagReplacement;
			}
		}

		public TagReplacerData(string startTag, string endTag = "", string startTagReplacement = "", string endTagReplacement = "")
		{
			StartTag = startTag;
			if(String.IsNullOrEmpty(endTag))
			{
				EndTag = StartTag.Insert(1, "/");
			}
			else
			{
				EndTag = endTag;
			}

			StartTagReplacement = startTagReplacement;

			if (String.IsNullOrEmpty(endTagReplacement) && !String.IsNullOrEmpty(startTagReplacement))
			{
				EndTagReplacement = StartTagReplacement.Insert(1, "/");
			}
			else
			{
				EndTagReplacement = endTagReplacement;
			}
		}

		public string Replace(string input)
		{
			var output = input;
			if(!String.IsNullOrEmpty(StartTagReplacement))
			{
				output = output.Replace(StartTag, StartTagReplacement);
			}
			if (!String.IsNullOrEmpty(EndTagReplacement))
			{
				output = output.Replace(EndTag, EndTagReplacement);
			}
			return output;
		}
	}
}
