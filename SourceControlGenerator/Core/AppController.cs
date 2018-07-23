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
using SCG.FileGen;
using SCG.Windows;
using SCG.Interfaces;
using SCG.Data.App;
using System.Windows;
using System.Windows.Controls;
using SCG.Commands;
using System.Globalization;
using System.Windows.Input;
using SCG.Util;
using System.Windows.Threading;
using SCG.Controls;

namespace SCG.Core
{
   public class AppController : PropertyChangedBase
	{
		private static AppController _instance;

		public static AppController Main => _instance;

		private MainWindow mainWindow;

		public MainAppData Data { get; set; }

		public Dictionary<string, IProjectController> ProjectControllers { get; set; }

		private bool gitDetected = false;

		public bool GitDetected
		{
			get { return gitDetected; }
			set
			{
				gitDetected = value;
				RaisePropertyChanged("GitDetected");
			}
		}

		public ICommand OpenGitWebsiteCommand { get; private set; }
		public ICommand SetSetupFoldersToRelativeCommand { get; private set; }
		public ICommand SetSetupFoldersToMyDocumentsCommand { get; private set; }
		public ICommand OpenProjectReadmeInMarkdownConverterCommand { get; private set; }

		#region Modules
		public void InitModules()
		{
			var totalLoaded = StartLoadingModules().GetAwaiter().GetResult();

			Log.Here().Important($"Loaded {totalLoaded} project modules.");

			if (!String.IsNullOrWhiteSpace(Data.AppSettings.LastModule) && SetModule(Data.AppSettings.LastModule))
			{
				Data.ModuleSelectionVisibility = Visibility.Collapsed;
			}
			else
			{
				Data.ModuleSelectionVisibility = Visibility.Visible;
			}
		}

		private Task<int> StartLoadingModules()
		{
			return LoadModules();
		}

		private async Task<int> LoadModules()
		{
			int totalModulesLoaded = 0;

			DirectoryInfo modulesFolder = new DirectoryInfo("Modules");
			modulesFolder.Create();
			var modules = modulesFolder.GetFiles("*.dll", SearchOption.AllDirectories);

			if (modules.Length > 0)
			{
				var tasks = new List<Task<bool>>();
				for (var i = 0; i < modules.Length; i++)
				{
					var module = modules[i];
					tasks.Add(LoadModule(module.FullName, module.Name));
					//Assembly.LoadFrom(module.FullName);
				}

				foreach (var task in await Task.WhenAll(tasks))
				{
					if (task == true)
					{
						totalModulesLoaded += 1;
					}
				}

				if (totalModulesLoaded <= 0) Log.Here().Important("No modules were loaded.");
			}

			return totalModulesLoaded;
		}

		private async Task<bool> LoadModule(string fileName, string Name = "")
		{
			try
			{
				Log.Here().Important($"Attempting to load module {Name}.");
				AssemblyLoader.Call(AppDomain.CurrentDomain, fileName, "SCG.Module", "Init");
				return await Task.FromResult(true);
			}
			catch (Exception ex)
			{
				Log.Here().Error("Error loading module file {0}: {1}", fileName, ex.ToString());
			}
			return await Task.FromResult(false);
		}

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
			try
			{
				Log.Here().Activity("Module set to [{0}]. Starting...", projectController.ModuleData.ModuleName);

				CurrentModule = projectController;
				CurrentModule.Initialize(Data);
				FileCommands.Load.LoadAll(CurrentModule.ModuleData);
				CurrentModule.Start();

				Data.CurrentModuleData = CurrentModule.ModuleData;
				Data.ModuleIsLoaded = true;

				if (Data.AppSettings.LastModule != CurrentModule.ModuleData.ModuleName)
				{
					Data.AppSettings.LastModule = CurrentModule.ModuleData.ModuleName;
					SaveAppSettings();
				}

				OnModuleSet?.Invoke(this, EventArgs.Empty);

				RegisterMenuShortcuts();

				if (CurrentModule.ModuleData != null)
				{
					CurrentModule.ModuleData.OnSettingsReverted += OnSettingsReverted;

					if (mainWindow.IsLoaded)
					{
						OnAppLoaded();
					}
				}

				Data.MergeKeyLists();

				Data.WindowTitle = "Source Control Generator - " + CurrentModule.ModuleData.ModuleName;

				return true;
			}
			catch(Exception ex)
			{
				Log.Here().Error($"Error loading module: {ex.ToString()}");
				return false;
			}
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

			Data.WindowTitle = "Source Control Generator";
		}
		#endregion
		#region Progress

		public void StartProgress(string Title, Action StartAction, string StartMessage = "", int StartValue = 0, Action OnCompleted = null)
		{
			if(!Data.ProgressActive)
			{
				Data.ProgressTitle = Title;
				Data.ProgressMessage = StartMessage;
				Data.ProgressValue = StartValue;
				Data.ProgressLog = "";
				Data.ProgressVisiblity = System.Windows.Visibility.Visible;
				//mainWindow.IsEnabled = false;

				mainWindow.Dispatcher.Invoke(new Action(() =>
				{
					StartAction();
				}), DispatcherPriority.ApplicationIdle);

				Data.ProgressActive = true;
			}
		}

		public void UpdateProgress(int Value = 1, string Message = null)
		{
			mainWindow.Dispatcher.Invoke(new Action(() =>
			{
				if (Message != null) Data.ProgressLog += Environment.NewLine + Message;
				Data.ProgressValue += Value;

				//Log.Here().Activity($"Updated progress to {Value}.");
			}), DispatcherPriority.Background);
		}

		public void SetProgress(int Value = 1, string Message = null)
		{
			mainWindow.Dispatcher.Invoke(new Action(() =>
			{
				//Log.Here().Activity($"Setting progress to {Value}.");
				if (Message != null) Data.ProgressMessage = Message;
				Data.ProgressValue = Value;
			}), DispatcherPriority.ApplicationIdle);
		}

		public void UpdateProgressMessage(string Message)
		{
			mainWindow.Dispatcher.Invoke(new Action(() =>
			{
				Data.ProgressMessage = Message;
			}), DispatcherPriority.ApplicationIdle);
		}

		public void UpdateProgressLog(string Message)
		{
			mainWindow.Dispatcher.Invoke(new Action(() =>
			{
				Data.ProgressLog += Environment.NewLine + Message;
			}), DispatcherPriority.ApplicationIdle);
		}

		public void UpdateProgressTitle(string Title)
		{
			if(Data.ProgressTitle != Title)
			{
				mainWindow.Dispatcher.Invoke(new Action(() =>
				{
					Data.ProgressTitle = Title;
				}), DispatcherPriority.ContextIdle);
			}
		}

		public async void FinishProgress()
		{
			Data.ProgressValue = Data.ProgressValueMax;
			await Task.Delay(500);
			await mainWindow.Dispatcher.BeginInvoke(new Action(() =>
			{
				Data.ProgressValue = Data.ProgressValueMax;
				OnProgressComplete();
			}), DispatcherPriority.Background);
		}

		private void OnProgressComplete()
		{
			HideProgressBar();
			Data.ProgressActive = false;
		}

		private void HideProgressBar()
		{
			mainWindow.IsEnabled = true;
			Data.ProgressVisiblity = System.Windows.Visibility.Collapsed;
		}
		#endregion

		public void LoadAppSettings()
		{
			if (File.Exists(DefaultPaths.MainAppSettingsFile))
			{
				try
				{
					Log.Here().Activity($"Loading main app settings from {DefaultPaths.MainAppSettingsFile}");
					Data.AppSettings = JsonConvert.DeserializeObject<AppSettingsData>(File.ReadAllText(DefaultPaths.MainAppSettingsFile));
				}
				catch(Exception ex)
				{
					Log.Here().Error($"Error loading main app settings from {DefaultPaths.MainAppSettingsFile}: {ex.ToString()}");
				}
			}
			else
			{
				Log.Here().Warning($"Main app settings file at {DefaultPaths.MainAppSettingsFile} not found. Creating new file.");
				Data.AppSettings = new AppSettingsData();
			}
		}

		public void SaveAppSettings()
		{
			try
			{
				Log.Here().Activity($"Saving main app settings to {DefaultPaths.MainAppSettingsFile}");
				var json = JsonConvert.SerializeObject(Data.AppSettings);
				if (FileCommands.WriteToFile(DefaultPaths.MainAppSettingsFile, json))
				{
					Log.Here().Activity($"Main app settings saved.");
				}
				else
				{
					Log.Here().Error($"Error saving main app settings to {DefaultPaths.MainAppSettingsFile}.");
				}
			}
			catch (Exception ex)
			{
				Log.Here().Error($"Error loading main app settings from {DefaultPaths.MainAppSettingsFile}: {ex.ToString()}");
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
			if (Data.ProgressActive) return;

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
			if (Data.ProgressActive) return;

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
				//mainWindow.LogWindow.Owner = mainWindow;
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

		public void MenuAction_ToggleMarkdownWindow()
		{
			if(!mainWindow.MarkdownConverterWindow.IsVisible)
			{
				mainWindow.MarkdownConverterWindow.Show();
			}
			else
			{
				mainWindow.MarkdownConverterWindow.Hide();
			}
		}

		public void MenuAction_OpenAbout()
		{
			if (Data.ProgressActive) return;

			if (!mainWindow.AboutWindow.IsVisible)
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
			Helpers.Web.OpenUri(DefaultPaths.IssuesLink);
		}

		public void MenuAction_OpenReleasesLink()
		{
			Helpers.Web.OpenUri(DefaultPaths.ReleasesLink);
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
			ListView listView = (ListView)mainWindow.FindName("ModuleSettingsListView");
			if(listView != null)
			{
				listView.GetBindingExpression(ListView.ItemsSourceProperty).UpdateTarget();
			}

			listView = (ListView)mainWindow.FindName("MainSettingsListView");
			if (listView != null)
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
			await mainWindow.Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)(() =>
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

		public void MakeSettingsFoldersToPortable()
		{
			DefaultPaths.RootFolder = DefaultPaths.DefaultPortableRootFolder;
			if(CurrentModule != null && CurrentModule.ModuleData != null && CurrentModule.ModuleData.ModuleSettings != null)
			{
				CurrentModule.ModuleData.ModuleSettings.SetToDefault(CurrentModule.ModuleData);
			}
		}

		public void SwitchSettingsFoldersToMyDocuments()
		{
			var myDocumentsRoot = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			if(Directory.Exists(myDocumentsRoot))
			{
				DefaultPaths.RootFolder = Path.Combine(myDocumentsRoot, DefaultPaths.DefaultMyDocumentsRootFolder);
				if (CurrentModule != null && CurrentModule.ModuleData != null && CurrentModule.ModuleData.ModuleSettings != null)
				{
					CurrentModule.ModuleData.ModuleSettings.SetToDefault(CurrentModule.ModuleData);
				}
			}
			
		}

		public void OpenProjectReadmeInMarkdownConverter(object obj)
		{
			Log.Here().Activity("Opening readme...");
			if (obj is IProjectData projectData)
			{
				if (CurrentModule != null && CurrentModule.ModuleData != null && CurrentModule.ModuleData.ModuleSettings != null)
				{
					var projectGitFolder = Path.Combine(CurrentModule.ModuleData.ModuleSettings.GitRootDirectory, projectData.ProjectName);
					if (Directory.Exists(projectGitFolder))
					{
						var readmeFilePath = Path.Combine(projectGitFolder, "README.md");
						if (File.Exists(readmeFilePath))
						{
							var converterData = mainWindow.MarkdownConverterWindow.ViewData;
							converterData.SetBatchExportRoot(Path.Combine(DefaultPaths.ModuleExportFolder(CurrentModule.ModuleData), projectData.ProjectName));
							converterData.LoadInputFile(readmeFilePath);
							converterData.Mode = MarkdownConverterMode.Batch;
							MenuAction_ToggleMarkdownWindow();
						}
						else
						{
							Log.Here().Error("Cannot find project file Readme.md.");
						}
					}
					else
					{
						Log.Here().Error("Cannot find project git folder.");
					}
				}
				else
				{
					Log.Here().Error("Current module data is null!");
				}
			}
		}

		public void SaveKeywords()
		{
			if (Data.CurrentModuleData != null && Data.CurrentModuleData.ModuleSettings != null)
			{
				if (FileCommands.Save.SaveUserKeywords(Data.CurrentModuleData))
				{
					MainWindow.FooterLog("Saved user keywords to {0}", Data.CurrentModuleData.ModuleSettings.UserKeywordsFile);
				}
				else
				{
					MainWindow.FooterLog("Error saving Keywords to {0}", Data.CurrentModuleData.ModuleSettings.UserKeywordsFile);
				}
			}
		}

		public void SaveKeywordsAs()
		{
			if (Data.CurrentModuleData != null && Data.CurrentModuleData.ModuleSettings != null)
			{
				string json = JsonConvert.SerializeObject(Data.CurrentModuleData.UserKeywords, Newtonsoft.Json.Formatting.Indented);

				var filename = Path.GetFileName(Data.CurrentModuleData.ModuleSettings.UserKeywordsFile);

				FileCommands.Save.OpenDialogAndSave(mainWindow, "Save Keywords As...", Data.CurrentModuleData.ModuleSettings.UserKeywordsFile, json, OnKeywordsSaveAs, filename, "", CommonFileFilters.Json);
			}
		}

		private void OnKeywordsSaveAs(bool success, string path)
		{
			if (success)
			{
				if (FileCommands.PathIsRelative(path))
				{
					path = Common.Functions.GetRelativePath.RelativePathGetter.Relative(Directory.GetCurrentDirectory(), path);
				}
				Data.CurrentModuleData.ModuleSettings.UserKeywordsFile = path;
				MainWindow.FooterLog("Saved Keywords to {0}", path);
			}
			else
			{
				MainWindow.FooterLog("Error saving Keywords to {0}", path);
			}
		}

		public void OnAppLoaded()
		{
			if(CurrentModule != null && CurrentModule.ModuleData != null)
			{
				if (CurrentModule.ModuleData.ModuleSettings.FirstTimeSetup)
				{
					Data.LockScreenVisibility = Visibility.Visible;
					CurrentModule.OpenSetup(OnSetupComplete);
				}

				mainWindow.MarkdownConverterWindow.SetData(FileCommands.Load.LoadModuleMarkdownConverterSettings(CurrentModule.ModuleData));
			}
		}

		private void InitDefaultMenus()
		{
			/*
			Data.MenuBarData.File.Register("Base",
				Disabled for now (WIP)
				new MenuData(MenuID.CreateTemplate)
				{
					Header = "Create Template...",
					ClickCommand = new ActionCommand(MenuAction_AddNewTemplate)
				},
			);
			*/

			LogMenuData = new MenuData(MenuID.OpenLog)
			{
				Header = "Open Log Window",
				ClickCommand = new ActionCommand(MenuAction_ToggleLogWindow),
				ShortcutKey = Key.F8
			};

			//LogMenuData.SetHeaderBinding(mainWindow.LogWindow.Data, "LogVisibleText");

			Data.MenuBarData.Options.Register("Base",
				new MenuData(MenuID.SelectModule)
				{
					Header = "Select Module...",
					ClickCommand = new ActionCommand(MenuAction_OpenModuleSelectScreen)
				},
				LogMenuData,
				new MenuData(MenuID.SaveLog)
				{
					Header = "Save Log...",
					ClickCommand = new ActionCommand(MenuAction_SaveLog)
				}
			);

			Data.MenuBarData.Tools.Register("Base",
				new MenuData(MenuID.Markdown)
				{
					Header = "Open Markdown Converter",
					ClickCommand = new ActionCommand(MenuAction_ToggleMarkdownWindow),
					ShortcutKey = Key.F3
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
					Header = "Releases (Github)...",
					ClickCommand = new ActionCommand(MenuAction_OpenReleasesLink)
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

		public AppController(MainWindow MainAppWindow)
		{
			_instance = this;
			RaisePropertyChanged("Main");
			Log.AllCallback = AddLogMessage;

			Data = new MainAppData();

			Data.SaveKeywordsCommand = new ActionCommand(SaveKeywords);
			Data.SaveKeywordsAsCommand = new ActionCommand(SaveKeywordsAs);

			ProjectControllers = new Dictionary<string, IProjectController>();

			OpenGitWebsiteCommand = new ActionCommand(() => { Helpers.Web.OpenUri("https://git-scm.com/downloads"); });

			SetSetupFoldersToMyDocumentsCommand = new ActionCommand(SwitchSettingsFoldersToMyDocuments);
			SetSetupFoldersToRelativeCommand = new ActionCommand(MakeSettingsFoldersToPortable);

			OpenProjectReadmeInMarkdownConverterCommand = new ParameterCommand(OpenProjectReadmeInMarkdownConverter);

			mainWindow = MainAppWindow;

			Data.Portable = File.Exists(DefaultPaths.PortableSettingsFile);

			if (Data.Portable)
			{
				DefaultPaths.RootFolder = DefaultPaths.DefaultPortableRootFolder;
			}
			else
			{
				var myDocumentsRoot = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
				if (Directory.Exists(myDocumentsRoot))
				{
					DefaultPaths.RootFolder = Path.Combine(myDocumentsRoot, DefaultPaths.DefaultMyDocumentsRootFolder);
				}
			}

			LoadAppSettings();
			InitDefaultMenus();

			if(String.IsNullOrWhiteSpace(Data.AppSettings.GitInstallPath))
			{
				var gitPath = Helpers.Registry.GetRegistryKeyValue("InstallPath", "GitForWindows", "SOFTWARE");
				if(!String.IsNullOrEmpty(gitPath))
				{
					Data.AppSettings.GitInstallPath = gitPath;
					RaisePropertyChanged("GitInstallPath");
					GitDetected = true;
					Log.Here().Important($"Git install location found at {gitPath}.");
				}
				else
				{
					Log.Here().Error($"Git install location not found.");
				}
			}
		}
	}
}
