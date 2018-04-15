using System;
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
using Ookii.Dialogs.Wpf;
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

			logWindow = new LogWindow();
			logWindow.Hide();

			Controller = new AppController(this);
			DataContext = Controller.Data;

			LoadModules();
			Controller.SetModule();
			LoadProjectModuleView(Controller.CurrentModule);
		}

		public void LoadModules()
		{
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
					try
					{
						Loader.Call(AppDomain.CurrentDomain, module.FullName, "LL.SCG.AddonModule", "Init");
					}
					catch(Exception ex)
					{
						Log.Here().Error("Error loading module file {0}: {1}", module.Name, ex.ToString());
					}
				}
			}
		}

		public void LoadProjectModuleView(IProjectController projectController)
		{
			var view = projectController.GetProjectView(this);
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


				if (Data != null) Log.Here().Important("Test: {0}", Data.KeyList.Count);

				DataContext = null;
				DataContext = Controller.Data;
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

		public string LogVisibleText
		{
			get => LogWindowShown ? "Close Log Window" : "Open Log Window";
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

		private void LogWindowToggle_Click(object sender, RoutedEventArgs e)
		{
			if (LogWindowShown)
			{
				logWindow.Hide();
			}
			else
			{
				logWindow.Show();
			}

			RaisePropertyChanged("LogVisibleText");
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

		private void Button_NewTemplate_Click(object sender, RoutedEventArgs e)
		{
			if(!Data.AddTemplateControlVisible)
			{
				TabControl mainTabs = (TabControl)FindName("MainTabsControl");

				Data.CreateNewTemplateData();

				Dispatcher.BeginInvoke((Action)(() =>
				{
					Data.AddTemplateControlVisible = true;
					if (mainTabs != null) mainTabs.SelectedIndex = 1;
				}));
			}
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
	}
}
