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
		public AppSettingsData AppSettings { get; set; }

		//Visible Data

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

		public MainAppData()
		{
			ManageButtonsText = "Select a Project";
		}
	}
}
