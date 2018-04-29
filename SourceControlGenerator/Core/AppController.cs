using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using LL.SCG.Data;
using Newtonsoft.Json;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml;
using System.ComponentModel;
using System.Collections.ObjectModel;
using LL.SCG.Data.View;
using LL.SCG.FileGen;
using LL.SCG.Windows;
using LL.SCG.Interfaces;
using LL.SCG.Data.App;
using System.Windows;
using System.Windows.Controls;
using LL.SCG.Commands;
using System.Globalization;
using System.Windows.Input;
using LL.SCG.Util;
using System.Windows.Threading;

namespace LL.SCG.Core
{
   public class AppController : PropertyChangedBase
	{
		private static AppController _instance;

		public static AppController Main => _instance;

		private MainWindow mainWindow;

		public MainAppData Data { get; set; }

		public Dictionary<string, IProjectController> ProjectControllers { get; set; }

		private IProjectController currentModule;

		public IProjectController CurrentModule
		{
			get { return currentModule; }
			private set
			{
				currentModule = value;
				RaisePropertyChanged("CurrentModule");
			}

		}

		public event EventHandler OnModuleSet;

		public void SetSelectedModule(string moduleName = "")
		{
			if (!String.IsNullOrWhiteSpace(moduleName))
			{
				if (ProjectControllers.ContainsKey(moduleName))
				{
					Data.SelectedModuleData = ProjectControllers[moduleName].ModuleData;
				}
			}
			else
			{
				Data.SelectedModuleData = null;
				CurrentModule = null;
			}
		}

		public bool SetModuleToSelected()
		{
			if (Data.SelectedModuleData != null) return SetModule(Data.SelectedModuleData);
			return false;
		}

		public bool SetModule(IModuleData moduleData)
		{
			var projectControllerEntry = ProjectControllers.FirstOrDefault(p => p.Value.ModuleData == moduleData);

			if (projectControllerEntry.Value != null)
			{
				return SetModule(projectControllerEntry.Value);
			}
			return false;
		}

		public bool SetModule(string moduleName = "")
		{
			IProjectController nextProjectController = null;

			Log.Here().Activity($"Attempting to set module to {moduleName}");

			if (!String.IsNullOrWhiteSpace(moduleName))
			{
				if (ProjectControllers.ContainsKey(moduleName))
				{
					nextProjectController = ProjectControllers[moduleName];
				}
			}

			if(nextProjectController != null)
			{
				return SetModule(nextProjectController);
			}

			return false;
		}

		public bool SetModule(IProjectController projectController)
		{
			CurrentModule = projectController;
			CurrentModule.Initialize(Data);
			FileCommands.Load.LoadAll(CurrentModule.ModuleData);
			CurrentModule.Start();

			Data.CurrentModuleData = CurrentModule.ModuleData;
			Data.ModuleIsLoaded = true;

			Log.Here().Activity("Module set to {0}", CurrentModule.ModuleData.ModuleName);

			if(Data.AppSettings.LastModule != CurrentModule.ModuleData.ModuleName)
			{
				Data.AppSettings.LastModule = CurrentModule.ModuleData.ModuleName;
				SaveAppSettings();
			}

			OnModuleSet?.Invoke(this, EventArgs.Empty);

			RegisterMenuShortcuts();

			if(CurrentModule.ModuleData != null)
			{
				CurrentModule.ModuleData.OnSettingsReverted += OnSettingsReverted;

				if(mainWindow.IsLoaded)
				{
					if (CurrentModule.ModuleData.ModuleSettings.FirstTimeSetup)
					{
						Data.LockScreenVisibility = Visibility.Visible;
						CurrentModule.OpenSetup(OnSetupComplete);
					}
				}
			}

			return true;
		}

		private void OnSetupComplete()
		{
			Data.LockScreenVisibility = Visibility.Collapsed;
			CurrentModule.ModuleData.ModuleSettings.FirstTimeSetup = false;
			FileCommands.Save.SaveModuleSettings(CurrentModule.ModuleData);
		}

		public void UnloadCurrentModule()
		{
			if(CurrentModule != null)
			{
				if (CurrentModule.ModuleData != null)
				{
					CurrentModule.ModuleData.OnSettingsReverted -= OnSettingsReverted;
				}

				Log.Here().Activity("Unloading module {0}.", CurrentModule.ModuleData.ModuleName);
				Data.AppSettings.LastModule = CurrentModule.ModuleData.ModuleName;
				UnregisterMenuShortcuts(CurrentModule.ModuleData.ModuleName);
				CurrentModule.Unload();
				SetSelectedModule();
			}

			Data.ModuleIsLoaded = false;
			Data.ModuleSelectionVisibility = Visibility.Visible;
		}
		#region Progress

		public Action OnProgressLoaded { get; set; }

		private BackgroundWorker progressWorker { get; set; }

		public DoWorkEventHandler ProgressWorkvent { get; set; }
		public RunWorkerCompletedEventHandler ProgressCompleteEvent { get; set; }

		public void StartProgress(string Title, DoWorkEventHandler WorkEvent, RunWorkerCompletedEventHandler CompleteEvent = null, int StartValue = 0, Action OnStarted = null)
		{
			if (progressWorker == null)
			{
				progressWorker = new BackgroundWorker();
				progressWorker.WorkerReportsProgress = true;
				progressWorker.WorkerSupportsCancellation = true;
				progressWorker.ProgressChanged += progressWorker_ProgressChanged;
				progressWorker.RunWorkerCompleted += progressWorker_ProgressFinished;
			}

			OnProgressLoaded = OnStarted;
			Data.ProgressTitle = Title;
			Data.ProgressValue = StartValue;
			Data.ProgressVisiblity = System.Windows.Visibility.Visible;
			mainWindow.IsEnabled = false;

			if (ProgressWorkvent != null) progressWorker.DoWork -= ProgressWorkvent;
			if (ProgressCompleteEvent != null) progressWorker.RunWorkerCompleted -= ProgressCompleteEvent;
			progressWorker.DoWork += WorkEvent;
			if(CompleteEvent != null) progressWorker.RunWorkerCompleted += CompleteEvent;
			
			ProgressWorkvent = WorkEvent;
			if (CompleteEvent != null) ProgressCompleteEvent = CompleteEvent;

			progressWorker.RunWorkerAsync();
		}
		
		public void UpdateProgress(int Value = 1, string Message = null)
		{
			if (Message != null) Data.ProgressMessage = Message;
			Data.ProgressValue += Value;
			progressWorker.ReportProgress(Data.ProgressValue);
		}

		public void SetProgress(int Value = 1, string Message = null)
		{
			//Log.Here().Activity($"Setting progress to {Value} with message {Message}.");
			if (Message != null) Data.ProgressMessage = Message;
			Data.ProgressValue = Value;
			progressWorker.ReportProgress(Data.ProgressValue);
		}

		public void UpdateProgressMessage(string Message)
		{
			Data.ProgressMessage = Message;
			progressWorker.ReportProgress(Data.ProgressValue);
		}

		public void UpdateProgressTitle(string Title)
		{
			Data.ProgressTitle = Title;
			progressWorker.ReportProgress(Data.ProgressValue);
		}

		public void FinishProgress()
		{
			progressWorker.ReportProgress(Data.ProgressValueMax);
		}

		private void progressWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			//Data.ProgressValue = e.ProgressPercentage;
			//Log.Here().Activity($"#Progress set to {Data.ProgressValue}");
		}

		private void progressWorker_ProgressFinished(object sender, RunWorkerCompletedEventArgs e)
		{
			Data.ProgressValue = Data.ProgressValueMax;
			OnProgressCompleteAsync();
		}

		private async void OnProgressCompleteAsync()
		{
			await Task.Delay(500);
			await HideProgressBar();
		}

		private async Task HideProgressBar()
		{
			mainWindow.IsEnabled = true;
			Data.ProgressVisiblity = System.Windows.Visibility.Collapsed;
		}
		#endregion

		public void LoadAppSettings()
		{
			if (File.Exists(DefaultPaths.MainAppSettings))
			{
				try
				{
					Log.Here().Activity($"Loading main app settings from {DefaultPaths.MainAppSettings}");
					Data.AppSettings = JsonConvert.DeserializeObject<AppSettingsData>(File.ReadAllText(DefaultPaths.MainAppSettings));
				}
				catch(Exception ex)
				{
					Log.Here().Error($"Error loading main app settings from {DefaultPaths.MainAppSettings}: {ex.ToString()}");
				}
			}
			else
			{
				Log.Here().Warning($"Main app settings file at {DefaultPaths.MainAppSettings} not found. Creating new file.");
				Data.AppSettings = new AppSettingsData();
			}
		}

		public void SaveAppSettings()
		{
			try
			{
				Log.Here().Activity($"Saving main app settings to {DefaultPaths.MainAppSettings}");
				var json = JsonConvert.SerializeObject(Data.AppSettings);
				if (FileCommands.WriteToFile(DefaultPaths.MainAppSettings, json))
				{
					Log.Here().Activity($"Main app settings saved.");
				}
				else
				{
					Log.Here().Error($"Error saving main app settings to {DefaultPaths.MainAppSettings}.");
				}
			}
			catch (Exception ex)
			{
				Log.Here().Error($"Error loading main app settings from {DefaultPaths.MainAppSettings}: {ex.ToString()}");
			}
		}

		public static void RegisterController(string Name, IProjectController projectController, string Logo = null, string DisplayName = null)
		{
			_instance.ProjectControllers.Add(Name, projectController);

			var selectionData = new ModuleSelectionData()
			{
				ModuleName = Name,
				Logo = Path.GetFullPath(Logo),
				DisplayName = DisplayName
			};

			if(!String.IsNullOrWhiteSpace(Logo) && File.Exists(Logo))
			{
				selectionData.LogoExists = Visibility.Visible;
			}

			if (DisplayName == null && Logo == null) selectionData.DisplayName = Name;

			_instance.Data.Modules.Add(selectionData);

			projectController.MainAppData = _instance.Data;

			Log.Here().Important("Registered controller for module {0}.", Name);
		}

		#region  Menu Commands

		public void MenuAction_AddNewTemplate()
		{
			if (CurrentModule != null)
			{
				var data = CurrentModule.ModuleData;
				if (!data.AddTemplateControlVisible)
				{
					TabControl mainTabs = (TabControl)mainWindow.FindName("MainTabsControl");

					data.CreateNewTemplateData();

					mainWindow.Dispatcher.BeginInvoke((Action)(() =>
					{
						data.AddTemplateControlVisible = true;
						if (mainTabs != null) mainTabs.SelectedIndex = 1;
					}));
				}
			}
		}

		public void MenuAction_OpenModuleSelectScreen()
		{
			UnloadCurrentModule();
			var projectsGrid = (Grid)mainWindow.FindName("ProjectsViewGrid");
			if (projectsGrid != null)
			{
				projectsGrid.Children.Clear();
			}
		}

		public void MenuAction_ToggleLogWindow()
		{
			if (mainWindow.LogWindowShown)
			{
				mainWindow.LogWindow.Hide();
			}
			else
			{
				mainWindow.LogWindow.Owner = mainWindow;
				mainWindow.LogWindow.Show();
			}

			mainWindow.RaisePropertyChanged("LogVisibleText");
		}

		public void MenuAction_SaveLog()
		{
			string logContent = "";
			foreach(var data in mainWindow.LogWindow.Data.Logs)
			{
				logContent += data.Output + Environment.NewLine;
			}

			string sysFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.Replace("/", "-");
			string fileName = "SourceControlGenerator_Log_" + DateTime.Now.ToString(sysFormat + "_HH-mm") + ".txt";

			FileCommands.Save.OpenDialog(mainWindow, "Save Log File...", Data.AppSettings.LastLogPath, logContent, (string logPath) => {
				if (FileCommands.WriteToFile(logPath, logContent))
				{
					Log.Here().Activity($"Saved log file to {logPath}.");
				}
				else
				{
					Log.Here().Error($"Error saving log file to {logPath}.");
				}
				Data.AppSettings.LastLogPath = logPath;
			}, fileName);
		}

		public void MenuAction_OpenAbout()
		{
			if(!mainWindow.AboutWindow.IsVisible)
			{
				mainWindow.AboutWindow.Owner = mainWindow;
				mainWindow.AboutWindow.Show();
			}
			else
			{
				mainWindow.AboutWindow.Hide();
			}
		}

		public void MenuAction_OpenIssuesLink()
		{
			Helpers.Web.OpenUri("https://github.com/LaughingLeader-DOS2-Mods/LeaderLib/issues/new");
		}

		public void MenuAction_OpenRepoLink()
		{
			Helpers.Web.OpenUri("https://github.com/LaughingLeader-DOS2-Mods/LeaderLib");
		}

		public void MenuAction_NotImplemented() { }

		#endregion

		public void RegisterMenuShortcuts()
		{
			if(Data.MenuBarData != null)
			{
				foreach(var menu in Data.MenuBarData.Menus)
				{
					menu.RegisterInputBinding(mainWindow);
				}
			}
		}

		public void UnregisterMenuShortcuts(string ModuleName)
		{
			if (Data.MenuBarData != null)
			{
				foreach (var menu in Data.MenuBarData.Menus.Where(m => m.Module == ModuleName))
				{
					menu.UnregisterInputBinding(mainWindow);
				}
			}
		}

		//Workaround for converted settings data (used for the file browsers) not updating when reverting to default.
		private void OnSettingsReverted(object settingsData, EventArgs e)
		{
			ListView listView = (ListView)mainWindow.FindName("SettingsDataGrid");
			if(listView != null)
			{
				listView.GetBindingExpression(ListView.ItemsSourceProperty).UpdateTarget();
			}
		}

		#region Log
		private int logIndex = 0;

		public void AddLogMessage(string LogMessage, LogType logType)
		{
			AddLogMessageAsync(LogMessage, logType);
		}

		public async void AddLogMessageAsync(string LogMessage, LogType logType)
		{
			await Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, (Action)(() =>
			{
				var log = new LogData()
				{
					Index = logIndex++,
					DateTime = DateTime.Now,
					Message = LogMessage,
					MessageType = logType
				};
				log.FormatOutput();

				mainWindow.LogWindow.Data.Add(log);
			}));
		}

		public MenuData LogMenuData { get; set; }

		#endregion

		public void OnAppLoaded()
		{
			if(CurrentModule != null)
			{
				if (CurrentModule.ModuleData.ModuleSettings.FirstTimeSetup)
				{
					Data.LockScreenVisibility = Visibility.Visible;
					CurrentModule.OpenSetup(OnSetupComplete);
				}
			}
		}

		public AppController(MainWindow MainAppWindow)
		{
			_instance = this;
			Log.AllCallback = AddLogMessage;

			Data = new MainAppData();
			ProjectControllers = new Dictionary<string, IProjectController>();

			mainWindow = MainAppWindow;

			LoadAppSettings();

			Data.MenuBarData.File.Register("Base",
				new MenuData(MenuID.CreateTemplate)
				{
					Header = "Create Template...",
					ClickCommand = new ActionCommand(MenuAction_AddNewTemplate)
				},
				new MenuData(MenuID.SelectModule)
				{
					Header = "Select Module...",
					ClickCommand = new ActionCommand(MenuAction_OpenModuleSelectScreen)
				}
			);

			LogMenuData = new MenuData(MenuID.OpenLog)
			{
				Header = "Open Log Window",
				ClickCommand = new ActionCommand(MenuAction_ToggleLogWindow),
				ShortcutKey = Key.F8
			};

			//LogMenuData.SetHeaderBinding(mainWindow.LogWindow.Data, "LogVisibleText");

			Data.MenuBarData.Options.Register("Base",
				LogMenuData,
				new MenuData(MenuID.SaveLog)
				{
					Header = "Save Log...",
					ClickCommand = new ActionCommand(MenuAction_SaveLog)
				}
			);

			Data.MenuBarData.Help.Register("Base",
				new MenuData(MenuID.IssuesLink)
				{
					Header = "Report Bug / Give Feedback (Github)...",
					ClickCommand = new ActionCommand(MenuAction_OpenIssuesLink)
				},
				new MenuData(MenuID.RepoLink)
				{
					Header = "Source Code (Github)...",
					ClickCommand = new ActionCommand(MenuAction_OpenRepoLink)
				},
				new MenuData(MenuID.About)
				{
					Header = "About Source Control Generator",
					ClickCommand = new ActionCommand(MenuAction_OpenAbout),
					ShortcutKey = Key.F1
				}
			);

			Data.MenuBarData.RaisePropertyChanged(String.Empty);

			RegisterMenuShortcuts();
		}
	}
}
