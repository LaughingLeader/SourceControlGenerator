using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonMark;
using CommonMark.Syntax;

namespace MarkdownConverter
{
	public class SteamWorkshopFormatter : HTMLFormatter
	{
		public SteamWorkshopFormatter () : base()
		{
			Header.SetReplacements("[h1]");
			Bold.SetReplacements("[b]");
			Underline.SetReplacements("[u]");
			Italic.SetReplacements("[i]");
			Strikethrough.SetReplacements("[strike]");
			Link.SetReplacements("[url=*]");
			List.SetReplacements("[list]");
			OrderedList.SetReplacements("[olist]");
			ListItem.SetReplacements("[*]");
			Quote.SetReplacements("[quote=*]");
			Code.SetReplacements("[code]");
		}

		public string Parse(string input)
		{
			string output = input;

			return output;
		}
	}
}
