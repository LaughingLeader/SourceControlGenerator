﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LL.SCG.Core;
using LL.SCG.Commands;
using LL.SCG.Data;
using LL.SCG.Data.View;
using LL.SCG.Util;
using LL.SCG.Windows;
using LL.SCG.Interfaces;
using Newtonsoft.Json;
using LL.SCG.Modules;
using LL.SCG.FileGen;

namespace LL.SCG.Windows
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		private LogWindow logWindow;

		public LogWindow LogWindow
		{
			get { return logWindow; }
			set { logWindow = value; }
		}

		private AboutWindow aboutWindow;

		public AboutWindow AboutWindow
		{
			get { return aboutWindow; }
			set { aboutWindow = value; }
		}


		private GitGenerationWindow gitGenerationWindow;
		private UserControl lastModuleView;

		public AppController Controller
		{
			get { return (AppController)GetValue(ControllerProperty); }
			set { SetValue(ControllerProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Controller.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ControllerProperty =
			DependencyProperty.Register("Controller", typeof(AppController), typeof(MainWindow), new PropertyMetadata(null));

		private IModuleData Data => Controller.Data.CurrentModuleData;

		public MainWindow()
		{
			InitializeComponent();

			_instance = this;

			logWindow = new LogWindow(this);
			logWindow.Hide();

			aboutWindow = new AboutWindow(this);
			aboutWindow.Hide();

			Controller = new AppController(this);
			DataContext = Controller.Data;

			logWindow.Init(Controller);


			Controller.OnModuleSet += LoadProjectModuleView;

			var totalLoaded = StartLoadingModules().GetAwaiter().GetResult();

			Log.Here().Important($"Loaded {totalLoaded} project modules.");

			if (!String.IsNullOrWhiteSpace(Controller.Data.AppSettings.LastModule) && Controller.SetModule(Controller.Data.AppSettings.LastModule))
			{
				Controller.Data.ModuleSelectionVisibility = Visibility.Collapsed;
			}
			else
			{
				Controller.Data.ModuleSelectionVisibility = Visibility.Visible;
			}
		}

		public Task<int> StartLoadingModules()
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
				for (var i = 0; i < modules.Length; i++)
				{
					var module = modules[i];
					Log.Here().Important("Module {0} found. Attempting to initialize.", module.Name);
					//Assembly.LoadFrom(module.FullName);

					var result = await LoadModule(module.FullName);
					if(result)
					{
						totalModulesLoaded += 1;
					}
				}
			}

			return totalModulesLoaded;
		}

		private async Task<bool> LoadModule(string fileName)
		{
			try
			{
				Loader.Call(AppDomain.CurrentDomain, fileName, "LL.SCG.Module", "Init");
				return true;
			}
			catch (Exception ex)
			{
				Log.Here().Error("Error loading module file {0}: {1}", fileName, ex.ToString());
			}

			return false;
		}

		public void LoadProjectModuleView(object sender, EventArgs e)
		{
			Task.Delay(10);
			var view = Controller.CurrentModule.GetProjectView(this);
			var viewGrid = (Grid)FindName("ProjectsViewGrid");

			if (view != null && viewGrid != null)
			{
				if (lastModuleView != null)
				{
					viewGrid.Children.Remove(lastModuleView);
					lastModuleView = null;
				}

				viewGrid.Children.Add(view);
				lastModuleView = view;

				Log.Here().Activity("Loaded project view for module.");

				DataContext = null;
				DataContext = Controller.Data;

				Controller.Data.ModuleSelectionVisibility = Visibility.Collapsed;
			}
		}

		public bool LogWindowShown
		{
			get
			{
				if(logWindow != null) return logWindow.IsVisible;
				return false;
			}
		}

		

		private string footerOutputDate;

		public string FooterOutputDate
		{
			get { return footerOutputDate; }
			set
			{
				footerOutputDate = value;
				RaisePropertyChanged("FooterOutputDate");
			}
		}


		private string footerOutputText;

		public string FooterOutputText
		{
			get { return footerOutputText; }
			set
			{
				footerOutputText = value;
				RaisePropertyChanged("FooterOutputText");
			}
		}

		private LogType footerOutputType;

		public LogType FooterOutputType
		{
			get { return footerOutputType; }
			set
			{
				footerOutputType = value;
				RaisePropertyChanged("FooterOutputType");
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public void RaisePropertyChanged(string propertyName)
		{
			OnPropertyChanged(propertyName);
		}

		private void OnPropertyChanged(String property)
		{
			PropertyChangedEventHandler handler = this.PropertyChanged;

			if (handler != null)
			{
				var e = new PropertyChangedEventArgs(property);
				handler(this, e);
			}
		}

		private static MainWindow _instance;

		public static void FooterLog(string Message, params object[] Vars)
		{
			if (_instance != null)
			{
				Message = String.Format(Message, Vars);
				_instance.FooterOutputText = Message;
				_instance.FooterOutputType = LogType.Important;
				_instance.FooterOutputDate = DateTime.Now.ToShortTimeString();
				Log.AllCallback?.Invoke(Message, LogType.Important);
			}
		}

		public static void FooterError(string Message, params object[] Vars)
		{
			if (_instance != null)
			{
				Message = String.Format(Message, Vars);
				_instance.FooterOutputText = Message;
				_instance.FooterOutputType = LogType.Error;
				_instance.FooterOutputDate = DateTime.Now.ToShortTimeString();
				Log.AllCallback?.Invoke(Message, LogType.Error);
			}
		}
		

		private void HandleColumnHeaderSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
		{
			if (sizeChangedEventArgs.NewSize.Width <= 60)
			{
				sizeChangedEventArgs.Handled = true;
				//((GridViewColumnHeader)sender).Column.Width = 60;
			}
		}

		

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			//Super long tooltip durations
			ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(Int32.MaxValue));
		}

		private void MainAppWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (LogWindowShown)
			{
				logWindow.Close();
			}
		}

		private void PreventInitialTextboxFocus(object sender, SelectionChangedEventArgs e)
		{
			TabItem tab = (TabItem)this.FindName("Tab_Settings");
			if (tab != null && tab.IsSelected)
			{
				tab.Focus();
			}
		}

		private void SaveKeywordsButton_Click(object sender, RoutedEventArgs e)
		{
			if (Data != null && Data.ModuleSettings != null)
			{
				if (FileCommands.Save.SaveUserKeywords(Data))
				{
					FooterLog("Saved user keywords to {0}", Data.ModuleSettings.UserKeywordsFile);
				}
				else
				{
					FooterLog("Error saving Keywords to {0}", Data.ModuleSettings.UserKeywordsFile);
				}
			}
		}

		private void OnKeywordsSaveAs(bool success, string path)
		{
			if (success)
			{
				if(FileCommands.PathIsRelative(path))
				{
					path = Common.Functions.GetRelativePath.RelativePathGetter.Relative(Directory.GetCurrentDirectory(), path);
				}
				Data.ModuleSettings.UserKeywordsFile = path;
				MainWindow.FooterLog("Saved Keywords to {0}", path);
			}
			else
			{
				MainWindow.FooterLog("Error saving Keywords to {0}", path);
			}
		}

		private void SaveAsKeywordsButton_Click(object sender, RoutedEventArgs e)
		{
			string json = JsonConvert.SerializeObject(Data.UserKeywords, Newtonsoft.Json.Formatting.Indented);
			FileCommands.Save.OpenDialogAndSave(this, "Save Keywords", Data.ModuleSettings.UserKeywordsFile, json, OnKeywordsSaveAs);
		}

		private void OpenKeywordsButton_Click(object sender, RoutedEventArgs e)
		{
			FileCommands.Load.OpenFileDialog(this, "Open Keywords", Data.ModuleSettings.UserKeywordsFile, (path) => { FileCommands.Load.LoadUserKeywords(Data, path); });
		}

		private void KeywordsList_Add_Click(object sender, RoutedEventArgs e)
		{
			Data.UserKeywords.AddKeyword();
		}

		private void KeywordsList_Remove_Click(object sender, RoutedEventArgs e)
		{
			Data.UserKeywords.RemoveLast();
		}

		private void KeywordsList_Default_Click(object sender, RoutedEventArgs e)
		{
			FileCommands.OpenConfirmationDialog(this, "Reset Keyword List?", "Reset Keyword values to default?", "Changes will be lost.", () =>
			{
				Data.UserKeywords.ResetToDefault();
			});
		}

		private Action onGitGenerationConfirmed;

		public void OpenGitGenerationWindow(GitGenerationSettings settings, List<IProjectData> selectedProjects, Action onConfirm)
		{
			onGitGenerationConfirmed = onConfirm;

			if (gitGenerationWindow == null)
			{
				gitGenerationWindow = new GitGenerationWindow();
			}

			if (selectedProjects != null && selectedProjects.Count > 0)
			{
				gitGenerationWindow.Init(this, settings, selectedProjects);
				gitGenerationWindow.Owner = this;
				gitGenerationWindow.Show();
			}
			else
			{
				if (gitGenerationWindow.IsVisible) gitGenerationWindow.Hide();
			}

			if (settings.TemplateSettings.Count <= 0)
			{
				Log.Here().Error("Template settings are empty!");
			}
		}

		public void StartGitGeneration(GitGenerationSettings settings)
		{
			onGitGenerationConfirmed?.Invoke();
		}

		public void OnGitWindowCanceled()
		{
			
		}

		private void SettingsDataGrid_Selected(object sender, RoutedEventArgs e)
		{
			
				
		}

		private void SettingsDataGrid_CurrentCellChanged(object sender, EventArgs e)
		{
			
		}

		private void SettingsDataGrid_GotFocus(object sender, RoutedEventArgs e)
		{
			
		}

		private void ModuleSelection_LoadModuleClick(object sender, RoutedEventArgs e)
		{
			Controller.SetModuleToSelected();
		}

		private void ModuleSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (sender is ListView list)
			{
				if (list.SelectedItem is ModuleSelectionData module)
				{
					Controller.SetSelectedModule(module.ModuleName);
				}
				else if(list.IsFocused)
				{
					Controller.SetSelectedModule();
				}
			}
		}

		private void ProgressScreen_Loaded(object sender, EventArgs e)
		{
			Controller.OnProgressLoaded?.Invoke();
		}
	}
}
