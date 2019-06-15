using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alphaleonis.Win32.Filesystem;
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
				Update(ref appSettings, value);
			}
		}

		public void OnSettingsLoaded()
		{
			if(AppSettings != null)
			{
				AppSettings.PropertyChanged += AppSettings_PropertyChanged;
			}
		}

		private void AppSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == "GitInstallPath")
			{
				GitDetected = FileCommands.IsValidPath(AppSettings.GitInstallPath);
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
				Update(ref portable, value);
			}
		}

		private bool clipboardPopulated = false;

		public bool ClipboardPopulated
		{
			get { return clipboardPopulated; }
			set
			{
				Update(ref clipboardPopulated, value);
			}
		}

		private string windowTitle = "Source Control Generator";

		public string WindowTitle
		{
			get { return windowTitle; }
			set
			{
				Update(ref windowTitle, value);
			}
		}

		private MenuBarData menuBarData;

		public MenuBarData MenuBarData
		{
			get { return menuBarData; }
			set
			{
				Update(ref menuBarData, value);
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
				Update(ref selectedModuleData, value);
				Notify("CanLoadModule");
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
				Update(ref currentModuleData, value);
				Notify("CurrentModuleName");
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
				Update(ref moduleIsLoaded, value);
			}
		}

		private bool gitDetected = false;

		public bool GitDetected
		{
			get { return gitDetected; }
			set { Update(ref gitDetected, value); }
		}

		private ICommand saveKeywordsCommand;

		public ICommand SaveKeywordsCommand
		{
			get { return saveKeywordsCommand; }
			set
			{
				Update(ref saveKeywordsCommand, value);
			}
		}

		private ICommand saveKeywordsAsCommand;

		public ICommand SaveKeywordsAsCommand
		{
			get { return saveKeywordsAsCommand; }
			set
			{
				Update(ref saveKeywordsAsCommand, value);
			}
		}

		#region ProgressBar

		private bool progressActive = false;

		public bool ProgressActive
		{
			get { return progressActive; }
			set
			{
				Update(ref progressActive, value);
			}
		}


		private string progressTitle = "Processing...";

		public string ProgressTitle
		{
			get { return progressTitle; }
			set
			{
				Update(ref progressTitle, value);
			}
		}

		private string progressMessage = "";

		public string ProgressMessage
		{
			get { return progressMessage; }
			set
			{
				Update(ref progressMessage, value);
			}
		}

		private string progressLog = "";

		public string ProgressLog
		{
			get { return progressLog; }
			set
			{
				Update(ref progressLog, value);
			}
		}


		private int progressValue = 0;

		public int ProgressValue
		{
			get { return progressValue; }
			set
			{
				Update(ref progressValue, value);
			}
		}

		private int progressValueMax = 1000;

		public int ProgressValueMax
		{
			get { return progressValueMax; }
			set
			{
				Update(ref progressValueMax, value);
			}
		}


		private Visibility progressVisiblity = Visibility.Collapsed;

		public Visibility ProgressVisiblity
		{
			get { return progressVisiblity; }
			set
			{
				Update(ref progressVisiblity, value);
			}
		}

		private Visibility progressCancelButtonVisibility = Visibility.Collapsed;

		public Visibility ProgressCancelButtonVisibility
		{
			get { return progressCancelButtonVisibility; }
			set
			{
				Update(ref progressCancelButtonVisibility, value);
			}
		}

		private bool isIndeterminate = false;

		public bool IsIndeterminate
		{
			get { return isIndeterminate; }
			set
			{
				Update(ref isIndeterminate, value);
			}
		}


		private ActionCommand progressCancelCommand;

		public ActionCommand ProgressCancelCommand
		{
			get { return progressCancelCommand; }
			set
			{
				Update(ref progressCancelCommand, value);
			}
		}

		#endregion

		private Visibility moduleSelectionVisibility = Visibility.Visible;

		public Visibility ModuleSelectionVisibility
		{
			get { return moduleSelectionVisibility; }
			set
			{
				Update(ref moduleSelectionVisibility, value);
			}
		}

		private Visibility lockScreenVisibility = Visibility.Collapsed;

		public Visibility LockScreenVisibility
		{
			get { return lockScreenVisibility; }
			set
			{
				Update(ref lockScreenVisibility, value);
			}
		}

		//Footer
		private string footerOutputDate;

		public string FooterOutputDate
		{
			get { return footerOutputDate; }
			set
			{
				Update(ref footerOutputDate, value);
			}
		}


		private string footerOutputText;

		public string FooterOutputText
		{
			get { return footerOutputText; }
			set
			{
				Update(ref footerOutputText, value);
			}
		}

		private LogType footerOutputType;

		public LogType FooterOutputType
		{
			get { return footerOutputType; }
			set
			{
				Update(ref footerOutputType, value);
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
