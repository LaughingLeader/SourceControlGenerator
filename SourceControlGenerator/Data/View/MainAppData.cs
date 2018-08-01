using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SCG.Data;
using Newtonsoft.Json;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml;
using System.ComponentModel;
using System.Collections.ObjectModel;
using SCG.Data.View;
using SCG.Core;
using SCG.Commands;
using System.Windows.Input;
using SCG.Data.App;
using SCG.Interfaces;
using System.Windows;
using SCG.Collections;
using System.Windows.Data;

namespace SCG.Data.View
{
	public class MainAppData : PropertyChangedBase
	{
		public ManagedProjectsData AppProjects { get; set; }

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
		
		public ObservableImmutableList<KeywordData> GlobalKeyList { get; set; }
		public ObservableImmutableList<KeywordData> DateKeyList { get; set; }

		/// <summary>
		/// The merged KeyList visible in the main window.
		/// </summary>
		public ObservableImmutableList<KeywordData> AppKeyList { get; private set; }

		public ObservableCollection<ModuleSelectionData> Modules { get; private set; }

		private bool portable = false;

		public bool Portable
		{
			get { return portable; }
			set
			{
				portable = value;
				RaisePropertyChanged("Portable");
			}
		}

		private bool clipboardPopulated = false;

		public bool ClipboardPopulated
		{
			get { return clipboardPopulated; }
			set
			{
				clipboardPopulated = value;
				RaisePropertyChanged("ClipboardPopulated");
			}
		}

		private string windowTitle = "Source Control Generator";

		public string WindowTitle
		{
			get { return windowTitle; }
			set
			{
				windowTitle = value;
				RaisePropertyChanged("WindowTitle");
			}
		}


		public static IconPathData IconData { get; set; }

		private MenuBarData menuBarData;

		public MenuBarData MenuBarData
		{
			get { return menuBarData; }
			set
			{
				menuBarData = value;
				RaisePropertyChanged("MenuBarData");
			}
		}

		//public static ThemeColorData ThemeColorData { get; set; }

		private IModuleData selectedModuleData = null;

		/// <summary>
		/// The selected module (on the selection screen)
		/// </summary>
		public IModuleData SelectedModuleData
		{
			get { return selectedModuleData; }
			set
			{
				selectedModuleData = value;
				RaisePropertyChanged("SelectedModuleData");
				RaisePropertyChanged("CanLoadModule");
			}
		}

		public bool CanLoadModule
		{
			get
			{
				return selectedModuleData != null;
			}
		}

		private IModuleData currentModuleData;

		public IModuleData CurrentModuleData
		{
			get { return currentModuleData; }
			set
			{
				currentModuleData = value;
				RaisePropertyChanged("CurrentModuleData");
				RaisePropertyChanged("CurrentModuleName");
			}
		}

		public string CurrentModuleName
		{
			get
			{
				return CurrentModuleData != null ? CurrentModuleData.ModuleName : "";
			}
		}

		private bool moduleIsLoaded;

		public bool ModuleIsLoaded
		{
			get { return moduleIsLoaded; }
			set
			{
				moduleIsLoaded = value;
				RaisePropertyChanged("ModuleIsLoaded");
			}
		}

		private ICommand saveKeywordsCommand;

		public ICommand SaveKeywordsCommand
		{
			get { return saveKeywordsCommand; }
			set
			{
				saveKeywordsCommand = value;
				RaisePropertyChanged("SaveKeywordsCommand");
			}
		}

		private ICommand saveKeywordsAsCommand;

		public ICommand SaveKeywordsAsCommand
		{
			get { return saveKeywordsAsCommand; }
			set
			{
				saveKeywordsAsCommand = value;
				RaisePropertyChanged("SaveKeywordsAsCommand");
			}
		}


		#region ProgressBar

		private bool progressActive = false;

		public bool ProgressActive
		{
			get { return progressActive; }
			set
			{
				progressActive = value;
				RaisePropertyChanged("ProgressActive");
			}
		}


		private string progressTitle = "Processing...";

		public string ProgressTitle
		{
			get { return progressTitle; }
			set
			{
				progressTitle = value;
				RaisePropertyChanged("ProgressTitle");
			}
		}

		private string progressMessage = "";

		public string ProgressMessage
		{
			get { return progressMessage; }
			set
			{
				progressMessage = value;
				RaisePropertyChanged("ProgressMessage");
			}
		}

		private string progressLog = "";

		public string ProgressLog
		{
			get { return progressLog; }
			set
			{
				progressLog = value;
				RaisePropertyChanged("ProgressLog");
			}
		}


		private int progressValue = 0;

		public int ProgressValue
		{
			get { return progressValue; }
			set
			{
				progressValue = value;
				RaisePropertyChanged("ProgressValue");
			}
		}

		private int progressValueMax = 1000;

		public int ProgressValueMax
		{
			get { return progressValueMax; }
			set
			{
				progressValueMax = value;
				RaisePropertyChanged("ProgressValueMax");
			}
		}


		private Visibility progressVisiblity = Visibility.Collapsed;

		public Visibility ProgressVisiblity
		{
			get { return progressVisiblity; }
			set
			{
				progressVisiblity = value;
				RaisePropertyChanged("ProgressVisiblity");
			}
		}

		private Visibility progressCancelButtonVisibility = Visibility.Collapsed;

		public Visibility ProgressCancelButtonVisibility
		{
			get { return progressCancelButtonVisibility; }
			set
			{
				progressCancelButtonVisibility = value;
				RaisePropertyChanged("ProgressCancelButtonVisibility");
			}
		}

		private ActionCommand progressCancelCommand;

		public ActionCommand ProgressCancelCommand
		{
			get { return progressCancelCommand; }
			set
			{
				progressCancelCommand = value;
				RaisePropertyChanged("ProgressCancelCommand");
			}
		}

		#endregion

		private Visibility moduleSelectionVisibility = Visibility.Visible;

		public Visibility ModuleSelectionVisibility
		{
			get { return moduleSelectionVisibility; }
			set
			{
				moduleSelectionVisibility = value;
				RaisePropertyChanged("ModuleSelectionVisibility");
			}
		}

		private Visibility lockScreenVisibility = Visibility.Collapsed;

		public Visibility LockScreenVisibility
		{
			get { return lockScreenVisibility; }
			set
			{
				lockScreenVisibility = value;
				RaisePropertyChanged("LockScreenVisibility");
			}
		}

		public void MergeKeyLists()
		{
			if(CurrentModuleData != null && CurrentModuleData.KeyList != null)
			{
				ModuleNameKeyword.KeywordValue = CurrentModuleName;
				AppKeyList.DoOperation(list => list.Clear().AddRange(GlobalKeyList).AddRange(CurrentModuleData.KeyList));
				Log.Here().Activity("Merged keyword lists.");
			}
		}

		private KeywordData ModuleNameKeyword { get; set; }

		public MainAppData()
		{
			ModuleIsLoaded = false;

			IconData = new IconPathData();
			MenuBarData = new MenuBarData();

			ProgressCancelCommand = new ActionCommand();

			//ThemeColorData = ThemeColorData.Default();
			//ThemeColorData.RefreshTheme();

			Modules = new ObservableCollection<ModuleSelectionData>();
			GlobalKeyList = new ObservableImmutableList<KeywordData>();
			DateKeyList = new ObservableImmutableList<KeywordData>();
			AppKeyList = new ObservableImmutableList<KeywordData>();

			//BindingOperations.EnableCollectionSynchronization(GlobalKeyList, GlobalKeyList);
			//BindingOperations.EnableCollectionSynchronization(DateKeyList, DateKeyListLock);
			//BindingOperations.EnableCollectionSynchronization(AppKeyList, AppKeyListLock);

			ModuleNameKeyword = new KeywordData()
			{
				KeywordName = "$ModuleName",
				KeywordValue = CurrentModuleName
			};

			GlobalKeyList.Add(ModuleNameKeyword);

			DateKeyList.Add(new KeywordData()
			{
				KeywordName = "$Date",
				KeywordValue =  DateTime.Now.ToString("d"),
				Replace = (o) => { return DateTime.Now.ToString("d"); }
			});
			DateKeyList.Add(new KeywordData()
			{
				KeywordName = "$DateYear",
				KeywordValue = DateTime.Now.Year.ToString(),
				Replace = (o) => { return DateTime.Now.Year.ToString(); }
			});
			DateKeyList.Add(new KeywordData()
			{
				KeywordName = "$DateMonth",
				KeywordValue = DateTime.Now.Month.ToString(),
				Replace = (o) => { return DateTime.Now.Month.ToString(); }
			});
			DateKeyList.Add(new KeywordData()
			{
				KeywordName = "$DateDay",
				KeywordValue = DateTime.Now.Day.ToString(),
				Replace = (o) => { return DateTime.Now.Day.ToString(); }
			});
			DateKeyList.Add(new KeywordData()
			{
				KeywordName = "$DateShortLong",
				KeywordValue = DateTime.Now.ToString("D"),
				Replace = (o) => { return DateTime.Now.ToString("D"); }
			});
			DateKeyList.Add(new KeywordData()
			{
				KeywordName = "$DateFullShort",
				KeywordValue = DateTime.Now.ToString("f"),
				Replace = (o) => { return DateTime.Now.ToString("f"); }
			});
			DateKeyList.Add(new KeywordData()
			{
				KeywordName = "$DateFullLong",
				KeywordValue = DateTime.Now.ToString("F"),
				Replace = (o) => { return DateTime.Now.ToString("F"); }
			});
			DateKeyList.Add(new KeywordData()
			{
				KeywordName = "$DateGeneralShort",
				KeywordValue = DateTime.Now.ToString("g"),
				Replace = (o) => { return DateTime.Now.ToString("g"); }
			});
			DateKeyList.Add(new KeywordData()
			{
				KeywordName = "$DateGeneralLong",
				KeywordValue = DateTime.Now.ToString("G"),
				Replace = (o) => { return DateTime.Now.ToString("G"); }
			});
			DateKeyList.Add(new KeywordData()
			{
				KeywordName = "$DateMonthDay",
				KeywordValue = DateTime.Now.ToString("M"),
				Replace = (o) => { return DateTime.Now.ToString("M"); }
			});
			DateKeyList.Add(new KeywordData()
			{
				KeywordName = "$DateMonthYear",
				KeywordValue = DateTime.Now.ToString("Y"),
				Replace = (o) => { return DateTime.Now.ToString("Y"); }
			});
			DateKeyList.Add(new KeywordData()
			{
				KeywordName = "$DateTimeShort",
				KeywordValue = DateTime.Now.ToString("t"),
				Replace = (o) => { return DateTime.Now.ToString("t"); }
			});
			DateKeyList.Add(new KeywordData()
			{
				KeywordName = "$DateTimeLong",
				KeywordValue = DateTime.Now.ToString("T"),
				Replace = (o) => { return DateTime.Now.ToString("T"); }
			});
			DateKeyList.Add(new KeywordData()
			{
				KeywordName = "$DateRoundTrip",
				KeywordValue = DateTime.Now.ToString("O"),
				Replace = (o) => { return DateTime.Now.ToString("O"); }
			});
			DateKeyList.Add(new KeywordData()
			{
				KeywordName = "$DateRFC1123",
				KeywordValue = DateTime.Now.ToString("R"),
				Replace = (o) => { return DateTime.Now.ToString("R"); }
			});
			DateKeyList.Add(new KeywordData()
			{
				KeywordName = "$DateUniversal",
				KeywordValue = DateTime.Now.ToString("U"),
				Replace = (o) => { return DateTime.Now.ToString("U"); }
			});
		}
	}
}
