using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LL.SCG.Data.Command
{
	public class SaveFileCommandData
	{
		public string FileName { get; set; }
		public string FilePath { get; set; }
		public string DefaultFilePath { get; set; }
		public string Content { get; set; }
		public string DialogTitle { get; set; }
	}
}
