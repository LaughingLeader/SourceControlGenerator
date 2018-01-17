using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LL.DOS2.SourceControl.Data
{
	public class AppData
	{
		public string DOS2DataDirectory { get; set; }
		public string GitRootDirectory { get; set; }
		public string BackupRootDirectory { get; set; }

		public AppData()
		{
			DOS2DataDirectory = "";
			GitRootDirectory = "Git_Projects";
			BackupRootDirectory = "Backups";
		}
	}
}
