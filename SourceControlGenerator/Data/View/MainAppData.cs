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
using ReactiveUI;
using Reactive.Bindings.Extensions;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;

namespace SCG.Data.View
{
	public class MainAppData : ReactiveObject
	{
		public ManagedProjectsData AppProjects { get; set; }

		private AppSettingsData appSettings;

		public AppSettingsData AppSettings
		{
			get { return appSettings; }
			set
			{
				this.RaiseAndSetIfChanged(ref appSettings, value);
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
				this.RaiseAndSetIfChanged(ref portable, value);
			}
		}

		private bool clipboardPopulated = false;

		public bool ClipboardPopulated
		{
			get { return clipboardPopulated; }
			set
			{
				this.RaiseAndSetIfChanged(ref clipboardPopulated, value);
			}
		}

		private string windowTitle = "Source Control Generator";

		public string WindowTitle
		{
			get { return windowTitle; }
			set
			{
				this.RaiseAndSetIfChanged(ref windowTitle, value);
			}
		}

		private MenuBarData menuBarData;

		public MenuBarData MenuBarData
		{
			get { return menuBarData; }
			set
			{
				this.RaiseAndSetIfChanged(ref menuBarData, value);
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
				this.RaiseAndSetIfChanged(ref selectedModuleData, value);
				this.RaisePropertyChanged("CanLoadModule");
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
				this.RaiseAndSetIfChanged(ref currentModuleData, value);
				this.RaisePropertyChanged("CurrentModuleName");
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
				this.RaiseAndSetIfChanged(ref moduleIsLoaded, value);
			}
		}

		private bool gitDetected = false;

		public bool GitDetected
		{
			get { return gitDetected; }
			set { this.RaiseAndSetIfChanged(ref gitDetected, value); }
		}

		private ICommand saveKeywordsCommand;

		public ICommand SaveKeywordsCommand
		{
			get { return saveKeywordsCommand; }
			set
			{
				this.RaiseAndSetIfChanged(ref saveKeywordsCommand, value);
			}
		}

		private ICommand saveKeywordsAsCommand;

		public ICommand SaveKeywordsAsCommand
		{
			get { return saveKeywordsAsCommand; }
			set
			{
				this.RaiseAndSetIfChanged(ref saveKeywordsAsCommand, value);
			}
		}

		#region ProgressBar

		private bool progressActive = false;

		public bool ProgressActive
		{
			get { return progressActive; }
			set
			{
				this.RaiseAndSetIfChanged(ref progressActive, value);
			}
		}


		private string progressTitle = "Processing...";

		public string ProgressTitle
		{
			get { return progressTitle; }
			set
			{
				this.RaiseAndSetIfChanged(ref progressTitle, value);
			}
		}

		private string progressMessage = "";

		public string ProgressMessage
		{
			get { return progressMessage; }
			set
			{
				this.RaiseAndSetIfChanged(ref progressMessage, value);
			}
		}

		private string progressLog = "";

		public string ProgressLog
		{
			get { return progressLog; }
			set
			{
				this.RaiseAndSetIfChanged(ref progressLog, value);
			}
		}


		private int progressValue = 0;

		public int ProgressValue
		{
			get { return progressValue; }
			set
			{
				this.RaiseAndSetIfChanged(ref progressValue, value);
				ProgressValueTaskBar = ProgressValue > 0 ? ((double)ProgressValue / (double)ProgressValueMax) : 0d;
				Log.Here().Activity($"Progress Taskbar: {ProgressValueTaskBar}");
			}
		}

		private int progressValueMax = 100;

		public int ProgressValueMax
		{
			get { return progressValueMax; }
			set
			{
				this.RaiseAndSetIfChanged(ref progressValueMax, value);
				ProgressValueTaskBar = ProgressValue > 0 ? ((double)ProgressValue / (double)ProgressValueMax) : 0d;
				Log.Here().Activity($"Progress Taskbar: {ProgressValueTaskBar}");
			}
		}

		private double progressValueTaskBar = 0d;

		public double ProgressValueTaskBar
		{
			get => progressValueTaskBar;
			set
			{
				this.RaiseAndSetIfChanged(ref progressValueTaskBar, value);
			}
		}

		private Visibility progressVisiblity = Visibility.Collapsed;

		public Visibility ProgressVisiblity
		{
			get { return progressVisiblity; }
			set
			{
				this.RaiseAndSetIfChanged(ref progressVisiblity, value);
				IsUnlocked = value != Visibility.Visible;
				if (this.ModuleIsLoaded)
				{
					CurrentModuleData.OnLockScreenChanged(value, IsUnlocked);
				}
			}
		}

		private Visibility progressCancelButtonVisibility = Visibility.Collapsed;

		public Visibility ProgressCancelButtonVisibility
		{
			get { return progressCancelButtonVisibility; }
			set
			{
				this.RaiseAndSetIfChanged(ref progressCancelButtonVisibility, value);
			}
		}

		private bool isIndeterminate = false;

		public bool IsIndeterminate
		{
			get { return isIndeterminate; }
			set
			{
				this.RaiseAndSetIfChanged(ref isIndeterminate, value);
			}
		}


		private ActionCommand progressCancelCommand;

		public ActionCommand ProgressCancelCommand
		{
			get { return progressCancelCommand; }
			set
			{
				this.RaiseAndSetIfChanged(ref progressCancelCommand, value);
			}
		}

		#endregion

		private Visibility moduleSelectionVisibility = Visibility.Visible;

		public Visibility ModuleSelectionVisibility
		{
			get { return moduleSelectionVisibility; }
			set
			{
				this.RaiseAndSetIfChanged(ref moduleSelectionVisibility, value);
			}
		}

		private bool isUnlocked = false;

		public bool IsUnlocked
		{
			get => isUnlocked;
			set { this.RaiseAndSetIfChanged(ref isUnlocked, value); }
		}

		private Visibility lockScreenVisibility = Visibility.Collapsed;

		public Visibility LockScreenVisibility
		{
			get { return lockScreenVisibility; }
			set
			{
				this.RaiseAndSetIfChanged(ref lockScreenVisibility, value);
				IsUnlocked = value != Visibility.Visible;
				if(this.ModuleIsLoaded)
				{
					CurrentModuleData.OnLockScreenChanged(value, IsUnlocked);
				}
			}
		}

		//Footer
		private string footerOutputDate;

		public string FooterOutputDate
		{
			get { return footerOutputDate; }
			set
			{
				this.RaiseAndSetIfChanged(ref footerOutputDate, value);
			}
		}


		private string footerOutputText;

		public string FooterOutputText
		{
			get { return footerOutputText; }
			set
			{
				this.RaiseAndSetIfChanged(ref footerOutputText, value);
			}
		}

		private LogType footerOutputType;

		public LogType FooterOutputType
		{
			get { return footerOutputType; }
			set
			{
				this.RaiseAndSetIfChanged(ref footerOutputType, value);
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

			this.WhenAnyValue(d => d.AppSettings.GitInstallPath).ToObservableChangeSet().
				Buffer(TimeSpan.FromMilliseconds(250)).FlattenBufferResult().Subscribe((pathChange) =>
			{
				string path = pathChange.First().Item.Current;
				//Log.Here().Activity($"Checking for git path at {path}");
				GitDetected = !String.IsNullOrEmpty(path) && Directory.Exists(path);
			});
		}
	}
}
