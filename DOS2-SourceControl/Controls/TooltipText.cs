using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LL.DOS2.SourceControl.Controls
{
	static public class TooltipText
	{
		static public string GitIgnore => ".gitignore files are used to prevent certain files from being tracked.\nThis is useful for preventing large asset or binary files from slowing down the git source control.";
		static public string Readme => "A basic readme file, detailing relevant information about the project.\nDisplays on the repository main page.";
		static public string Changelog => "A changelog file with details on each update.";
		static public string CustomLicense => "A custom license to use instead of the more common three (MIT, Apache, GNU).";
		static public string TemplateKeywords => "Words that get replaced when generating templates.";
	}
}
