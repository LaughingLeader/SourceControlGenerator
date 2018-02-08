using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LL.DOS2.SourceControl.Data
{
	public class AppSettingsData : PropertyChangedBase
	{
		public string DOS2DataDirectory { get; set; }

		private string directoryLayoutFile;

		public string DirectoryLayoutFile
		{
			get { return directoryLayoutFile; }
			set
			{
				directoryLayoutFile = value;
				RaisePropertyChanged("DirectoryLayoutFile");
			}
		}

		private string gitRootDirectory;

		public string GitRootDirectory
		{
			get { return gitRootDirectory; }
			set { gitRootDirectory = value; RaisePropertyChanged("GitRootDirectory"); }
		}

		private string backupRootDirectory;

		public string BackupRootDirectory
		{
			get { return backupRootDirectory; }
			set
			{
				backupRootDirectory = value;
				RaisePropertyChanged("BackupRootDirectory");
			}
		}

		private string gitIgnoreFile;

		public string GitIgnoreFile
		{
			get { return gitIgnoreFile; }
			set
			{
				gitIgnoreFile = value;
				RaisePropertyChanged("GitIgnoreFile");
			}
		}

		private string readmeTemplateFile;

		public string ReadmeTemplateFile
		{
			get { return readmeTemplateFile; }
			set
			{
				readmeTemplateFile = value;
				RaisePropertyChanged("ReadmeTemplateFile");
			}
		}

		private string changelogTemplateFile;

		public string ChangelogTemplateFile
		{
			get { return changelogTemplateFile; }
			set
			{
				changelogTemplateFile = value;
				RaisePropertyChanged("ChangelogTemplateFile");
			}
		}

		private string customLicenseFile;

		public string CustomLicenseFile
		{
			get { return customLicenseFile; }
			set
			{
				customLicenseFile = value;
				RaisePropertyChanged("CustomLicenseFile");
			}
		}

		private string keywordsFile;

		public string KeywordsFile
		{
			get { return keywordsFile; }
			set
			{
				keywordsFile = value;
				RaisePropertyChanged("KeywordsFile");
			}
		}

		private string projectsAppData;

		public string ProjectsAppData
		{
			get { return projectsAppData; }
			set
			{
				projectsAppData = value;
				RaisePropertyChanged("ProjectsAppData");
			}
		}

		private string gitGenerationSettings;

		public string GitGenSettingsFile
		{
			get { return gitGenerationSettings; }
			set
			{
				gitGenerationSettings = value;
				RaisePropertyChanged("GitGenSettingsFile");
			}
		}


		public AppSettingsData()
		{
			DOS2DataDirectory = "";
			BackupRootDirectory = DefaultPaths.Backups;
			GitRootDirectory = DefaultPaths.GitRoot;
			ProjectsAppData = DefaultPaths.ProjectsAppData;
			GitIgnoreFile = DefaultPaths.GitIgnore;
			ReadmeTemplateFile = DefaultPaths.ReadmeTemplate;
			ChangelogTemplateFile = DefaultPaths.ChangelogTemplate;
			CustomLicenseFile = DefaultPaths.CustomLicense;
			KeywordsFile = DefaultPaths.Keywords;
			GitGenSettingsFile = DefaultPaths.GitGenSettings;
		}
	}

	

}
