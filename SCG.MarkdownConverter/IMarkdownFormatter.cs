using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LL.SCG.Markdown
{
	public interface IMarkdownFormatter
	{
		string Name { get; set; }

		string Convert(string input);
	}
}
