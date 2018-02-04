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
using LL.DOS2.SourceControl.Core;
using LL.DOS2.SourceControl.Core.Commands;

namespace LL.DOS2.SourceControl.Data.View
{
	public class MainAppData : PropertyChangedBase
	{
		public List<string> ProjectDirectoryLayouts { get; set; }
		public List<ModProjectData> ModProjects { get; set; }
		public List<SourceControlData> GitProjects { get; set; }

		//Visible Data

		private bool projectSelected;

		public bool ProjectSelected
		{
			get { return projectSelected; }
			set
			{
				projectSelected = value;
				RaisePropertyChanged("ProjectSelected");
			}
		}


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

		private ObservableCollection<ModProjectData> managedProjects;

		public ObservableCollection<ModProjectData> ManagedProjects
		{
			get { return managedProjects; }
			set
			{
				managedProjects = value;
				RaisePropertyChanged("ManagedProjects");
			}
		}

		public ObservableCollection<TemplateEditorData> Templates { get; set; }
		public ObservableCollection<KeywordData> AppKeyList { get; set; }
		public ObservableCollection<KeywordData> DateKeyList { get; set; }


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
				if(UserKeywords != null)
				{
					string json = JsonConvert.SerializeObject(UserKeywords, Newtonsoft.Json.Formatting.Indented);
					return json;
				}
				return "";
			}
		}

		private UserKeywordData userKeywordData;

		public UserKeywordData UserKeywords
		{
			get { return userKeywordData; }
			set
			{
				userKeywordData = value;
				RaisePropertyChanged("UserKeywords");
			}
		}

		private CallbackCommand loadKeywords;

		public CallbackCommand LoadKeywords
		{
			get { return loadKeywords; }
			set
			{
				loadKeywords = value;
				RaisePropertyChanged("LoadKeywords");
			}
		}


		public MainAppData()
		{
			ManageButtonsText = "Select a Project";

			LoadKeywords = new CallbackCommand(FileCommands.Load.LoadUserKeywords);

			Templates = new ObservableCollection<TemplateEditorData>();
			
			AppKeyList = new ObservableCollection<KeywordData>();
			AppKeyList.Add(new KeywordData()
			{
				KeywordName = "$Author",
				KeywordValue = "Mod Data: Author",
				Replace = (modProjectData) => { return modProjectData.ModuleInfo.Author; }
			});
			AppKeyList.Add(new KeywordData()
			{
				KeywordName = "$Description",
				KeywordValue = "Mod Data: Description",
				Replace = (modProjectData) => { return modProjectData.ModuleInfo.Description; }
			});
			AppKeyList.Add(new KeywordData()
			{
				KeywordName = "$ModType",
				KeywordValue = "Mod Data: Type",
				Replace = (modProjectData) => { return modProjectData.ModuleInfo.Type; }
			});
			AppKeyList.Add(new KeywordData()
			{
				KeywordName = "$ProjectName",
				KeywordValue = "Mod Data: Name",
				Replace = (modProjectData) => { return modProjectData.ModuleInfo.Name; }
			});
			AppKeyList.Add(new KeywordData()
			{
				KeywordName = "$Version",
				KeywordValue = "Mod Data: Version",
				Replace = (modProjectData) => { return modProjectData.Version; }
			});
			AppKeyList.Add(new KeywordData()
			{
				KeywordName = "$Targets",
				KeywordValue = "Mod Data: Targets",
				Replace = (modProjectData) => { return String.Join(",", modProjectData.ModuleInfo.TargetModes.ToArray()); }
			});
			AppKeyList.Add(new KeywordData()
			{
				KeywordName = "$DependencyProjects",
				KeywordValue = "Mod Data: Dependencies",
				Replace = (modProjectData) => { return modProjectData.Dependencies.ToDelimitedString(d => d.Name); }
			});

			DateKeyList = new ObservableCollection<KeywordData>();
			DateKeyList.Add(new KeywordData()
			{
				KeywordName = "$Date",
				KeywordValue = "Current Date (Short Format = mm/dd/yyyy)",
				Replace = (modProjectData) => { return DateTime.Now.ToString("d"); }
			});
			DateKeyList.Add(new KeywordData()
			{
				KeywordName = "$DateShortLong",
				KeywordValue = DateTime.Now.ToString("D"),
				Replace = (modProjectData) => { return DateTime.Now.ToString("D"); }
			});
			DateKeyList.Add(new KeywordData()
			{
				KeywordName = "$DateFullShort",
				KeywordValue = DateTime.Now.ToString("f"),
				Replace = (modProjectData) => { return DateTime.Now.ToString("f"); }
			});
			DateKeyList.Add(new KeywordData()
			{
				KeywordName = "$DateFullLong",
				KeywordValue = DateTime.Now.ToString("F"),
				Replace = (modProjectData) => { return DateTime.Now.ToString("F"); }
			});
			DateKeyList.Add(new KeywordData()
			{
				KeywordName = "$DateGeneralShort",
				KeywordValue = DateTime.Now.ToString("g"),
				Replace = (modProjectData) => { return DateTime.Now.ToString("g"); }
			});
			DateKeyList.Add(new KeywordData()
			{
				KeywordName = "$DateGeneralLong",
				KeywordValue = DateTime.Now.ToString("G"),
				Replace = (modProjectData) => { return DateTime.Now.ToString("G"); }
			});
			DateKeyList.Add(new KeywordData()
			{
				KeywordName = "$DateMonthDay",
				KeywordValue = DateTime.Now.ToString("M"),
				Replace = (modProjectData) => { return DateTime.Now.ToString("M"); }
			});
			DateKeyList.Add(new KeywordData()
			{
				KeywordName = "$DateMonthYear",
				KeywordValue = DateTime.Now.ToString("Y"),
				Replace = (modProjectData) => { return DateTime.Now.ToString("Y"); }
			});
			DateKeyList.Add(new KeywordData()
			{
				KeywordName = "$DateTimeShort",
				KeywordValue = DateTime.Now.ToString("t"),
				Replace = (modProjectData) => { return DateTime.Now.ToString("t"); }
			});
			DateKeyList.Add(new KeywordData()
			{
				KeywordName = "$DateTimeLong",
				KeywordValue = DateTime.Now.ToString("T"),
				Replace = (modProjectData) => { return DateTime.Now.ToString("T"); }
			});
			DateKeyList.Add(new KeywordData()
			{
				KeywordName = "$DateRoundTrip",
				KeywordValue = DateTime.Now.ToString("O"),
				Replace = (modProjectData) => { return DateTime.Now.ToString("O"); }
			});
			DateKeyList.Add(new KeywordData()
			{
				KeywordName = "$DateRFC1123",
				KeywordValue = DateTime.Now.ToString("R"),
				Replace = (modProjectData) => { return DateTime.Now.ToString("R"); }
			});
			DateKeyList.Add(new KeywordData()
			{
				KeywordName = "$DateUniversal",
				KeywordValue = DateTime.Now.ToString("U"),
				Replace = (modProjectData) => { return DateTime.Now.ToString("U"); }
			});
		}
	}
}
