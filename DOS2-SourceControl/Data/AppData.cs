using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LL.DOS2.SourceControl.Data
{
	public class AppData
	{
		public string DOS2DataDirectory { get; set; }
		public string GitRootDirectory { get; set; }
		public string BackupRootDirectory { get; set; }
		public string GitIgnoreFile { get; set; }

		static public string DefaultGitRootPath()
		{
			return @"Git_Projects";
		}

		static public string DefaultBackupsPath()
		{
			return @"Backups";
		}

		static public string DefaultGitIgnorePath()
		{
			return @"Settings/.gitignore.default";
		}

		public AppData()
		{
			DOS2DataDirectory = "";
			GitRootDirectory = DefaultGitRootPath();
			BackupRootDirectory = DefaultBackupsPath();
			GitIgnoreFile = DefaultGitIgnorePath();
		}
	}
}
