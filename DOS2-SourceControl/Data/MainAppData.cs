using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using LL.DOS2.SourceControl.Data;
using Newtonsoft.Json;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml;
using System.ComponentModel;
using System.Collections.ObjectModel;
using LL.DOS2.SourceControl.Data.View;

namespace LL.DOS2.SourceControl.Data
{
	public class MainAppData : PropertyChangedBase
	{
		public List<string> ProjectDirectoryLayouts { get; set; }
		public List<ModProjectData> ModProjects { get; set; }

		//Visible Data
		private AppSettingsData appSettings;

		public AppSettingsData AppSettings
		{
			get { return appSettings; }
			set
			{
				appSettings = value;
				RaisePropertyChanged("AppSettings");
			}
		}


		private string defaultGitIgnoreText;

		public string DefaultGitIgnoreText
		{
			get { return defaultGitIgnoreText; }
			set
			{
				defaultGitIgnoreText = value;
				RaisePropertyChanged("DefaultGitIgnoreText");
			}
		}

		private string defaultReadmeText;

		public string DefaultReadmeText
		{
			get { return defaultReadmeText; }
			set
			{
				defaultReadmeText = value;
				RaisePropertyChanged("DefaultReadmeText");
			}
		}

		private string defaultChangelogText;

		public string DefaultChangelogText
		{
			get { return defaultChangelogText; }
			set
			{
				defaultChangelogText = value;
				RaisePropertyChanged("DefaultChangelogText");
			}
		}

		private string customLicenseText;

		public string CustomLicenseText
		{
			get { return customLicenseText; }
			set
			{
				customLicenseText = value;
				RaisePropertyChanged("CustomLicenseText");
			}
		}


		private ObservableCollection<AvailableProjectViewData> availableProjects;

		public ObservableCollection<AvailableProjectViewData> AvailableProjects
		{
			get { return availableProjects; }
			set
			{
				availableProjects = value;
				RaisePropertyChanged("AvailableProjects");
			}
		}

		private ObservableCollection<SourceControlData> managedProjects;

		public ObservableCollection<SourceControlData> ManagedProjects
		{
			get { return managedProjects; }
			set
			{
				managedProjects = value;
				RaisePropertyChanged("ManagedProjects");
			}
		}

		private ObservableCollection<KeywordData> keywordList;

		public ObservableCollection<KeywordData> KeywordList
		{
			get { return keywordList; }
			set
			{
				keywordList = value;
				RaisePropertyChanged("KeywordList");
			}
		}

		private string manageButtonsText;

		public string ManageButtonsText
		{
			get { return manageButtonsText; }
			set
			{
				manageButtonsText = value;
				RaisePropertyChanged("ManageButtonsText");
			}
		}

		public string KeywordListText
		{
			get
			{
				if(KeywordList.Count > 0)
				{
					string json = JsonConvert.SerializeObject(KeywordList, Newtonsoft.Json.Formatting.Indented);
					return json;
				}
				return "";
			}
		}

		public MainAppData()
		{
			ManageButtonsText = "Select a Project";
		}
	}
}
