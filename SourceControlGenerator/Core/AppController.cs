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

		public void SetModule(string moduleName = "")
		{
			IProjectController nextModule = null;

			if (!String.IsNullOrWhiteSpace(moduleName))
			{
				if (ProjectControllers.ContainsKey(moduleName))
				{
					nextModule = ProjectControllers[moduleName];
				}
			}
			else
			{
				if(ProjectControllers.Count > 0)
				{
					nextModule = ProjectControllers.First().Value;
				}
			}

			if(nextModule != null)
			{
				CurrentModule = ProjectControllers.First().Value;
				CurrentModule.Initialize(Data);
				FileCommands.Load.LoadAll(CurrentModule.ModuleData);
				CurrentModule.Start();

				Data.CurrentModuleData = CurrentModule.ModuleData;
				Data.ModuleIsLoaded = true;

				Log.Here().Activity("Module set to {0}", CurrentModule.ModuleData.ModuleName);
			}
		}

		public static void RegisterController(string Name, IProjectController projectController)
		{
			_instance.ProjectControllers.Add(Name, projectController);
			projectController.MainAppData = _instance.Data;

			Log.Here().Important("Registered controller for module {0}.", Name);
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

		public AppController(MainWindow MainAppWindow)
		{
			_instance = this;

			Data = new MainAppData();
			ProjectControllers = new Dictionary<string, IProjectController>();

			mainWindow = MainAppWindow;
		}
	}
}
