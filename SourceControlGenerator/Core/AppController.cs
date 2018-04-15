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

			return true;
		}

		public void StartProgress(string Title, int StartValue = 0)
		{
			mainWindow.IsEnabled = false;
			Data.ProgressTitle = Title;
			Data.ProgressValue = StartValue;
			Data.ProgressVisiblity = System.Windows.Visibility.Visible;
		}

		public void UpdateProgress(int Value = 1, string Message = null)
		{
			Data.ProgressValue += Value;
			if(Message != null) Data.ProgressMessage = Message;
		}

		public async void UpdateProgressMessage(string Message)
		{
			await Task.Delay(200);
			Data.ProgressMessage = Message;
		}

		public async void FinishProgress()
		{
			mainWindow.IsEnabled = true;
			await Task.Delay(500);
			Data.ProgressVisiblity = System.Windows.Visibility.Collapsed;
		}

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
				Log.Here().Important($"Main app settings file at {DefaultPaths.MainAppSettings} not found. Creating new file.");
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

		public AppController(MainWindow MainAppWindow)
		{
			_instance = this;

			Data = new MainAppData();
			ProjectControllers = new Dictionary<string, IProjectController>();

			mainWindow = MainAppWindow;

			LoadAppSettings();
		}
	}
}
