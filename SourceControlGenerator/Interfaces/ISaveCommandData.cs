using SCG.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCG
{
	public interface ISaveCommandData
	{
		string Filename { get; set; }
		string FilePath { get; set; }
		string DefaultFilePath { get; set; }
		string Content { get; }
		string SaveAsText { get; set; }

		FileBrowserFilter FileTypes { get; set; }
	}
}
