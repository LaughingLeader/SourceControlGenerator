using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCG.Markdown
{
	public interface IMarkdownFormatter
	{
		string Name { get; set; }
		string DefaultFileExtension { get; set; }

		string ConvertHTML(string input);
	}
}
