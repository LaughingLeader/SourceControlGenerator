using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LL.DOS2.SourceControl.Data
{
	public class AppSettingsData
	{
		public string DOS2DataDirectory { get; set; }
		public string GitRootDirectory { get; set; }
		public string BackupRootDirectory { get; set; }
		public string GitIgnoreFile { get; set; }
		public string ReadmeTemplateFile { get; set; }
		public string ChangelogTemplateFile { get; set; }
		public string CustomLicenseFile { get; set; }

		public AppSettingsData()
		{
			DOS2DataDirectory = "";
			BackupRootDirectory = DefaultPaths.Backups;
			GitRootDirectory = DefaultPaths.GitRoot;
			GitIgnoreFile = DefaultPaths.GitIgnore;
			ReadmeTemplateFile = DefaultPaths.ReadmeTemplate;
			ChangelogTemplateFile = DefaultPaths.ChangelogTemplate;
			CustomLicenseFile = DefaultPaths.CustomLicense;
		}
	}

	

}
