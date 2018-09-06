using SCG.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SCG
{
	public interface ISaveCommandData
	{
		Window TargetWindow { get; set; }

		string FilePath { get; set; }
		string DefaultFileName { get; set; }
		string InitialDirectory { get; set; }
		string Content { get; }
		string SaveAsText { get; set; }

		FileBrowserFilter FileTypes { get; set; }
	}
}
