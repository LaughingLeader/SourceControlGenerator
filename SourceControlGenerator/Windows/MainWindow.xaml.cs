using System;
using System.Collections.Generic;
using System.ComponentModel;
using Alphaleonis.Win32.Filesystem;
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
using SCG.Core;
using SCG.Commands;
using SCG.Data;
using SCG.Data.View;
using SCG.Util;
using SCG.Windows;
using SCG.Interfaces;
using Newtonsoft.Json;
using SCG.Modules;
using SCG.FileGen;
using SCG.Controls;
using System.Windows.Threading;

namespace SCG.Windows
{
	public interface IToolWindow
	{
		void Init(AppController appController);
	}
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : ClipboardMonitorWindow
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

		private MarkdownConverterWindow markdownConverterWindow;

		public MarkdownConverterWindow MarkdownConverterWindow
		{
			get { return markdownConverterWindow; }
			set { markdownConverterWindow = value; }
		}

		private TextGenerator textGeneratorWindow;

		public TextGenerator TextGeneratorWindow
		{
			get { return textGeneratorWindow; }
			set { textGeneratorWindow = value; }
		}

		private GitGenerationWindow gitGenerationWindow;

		public GitGenerationWindow GitGenerationWindow
		{
			get { return gitGenerationWindow; }
			set { gitGenerationWindow = value; }
		}

		public DebugWindow DebugWindow { get; set; }

		public List<Window> SubWindows { get; set; }

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

			Start();

			LogWindow = new LogWindow(this);
			AboutWindow = new AboutWindow(this);
			MarkdownConverterWindow = new MarkdownConverterWindow();
			TextGeneratorWindow = new TextGenerator();
			GitGenerationWindow = new GitGenerationWindow();

			SubWindows = new List<Window>()
			{
				LogWindow,
				AboutWindow,
				MarkdownConverterWindow,
				TextGeneratorWindow,
				GitGenerationWindow
			};

			foreach(var window in SubWindows)
			{
				window.Hide();
			}

			Controller = new AppController(this);
			DataContext = Controller.Data;

			Controller.OnModuleSet += LoadProjectModuleView;

			Controller.InitModules();
		}

		protected override void OnClipboardUpdate()
		{
			base.OnClipboardUpdate();

			Controller.Data.ClipboardPopulated = Clipboard.ContainsText();
		}

		private void MainAppWindow_Loaded(object sender, RoutedEventArgs e)
		{
			//Super long tooltip durations
			ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(Int32.MaxValue));
			Controller.OnAppLoaded();

			foreach (var window in SubWindows)
			{
				if(window is IToolWindow toolWindow)
				{
					toolWindow.Init(Controller);
				}

				//window.Owner = this;
				window.Topmost = false;
			}
		}

		public void LoadProjectModuleView(object sender, EventArgs e)
		{
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
				_instance.Dispatcher.BeginInvoke((Action)(() => {
					Message = String.Format(Message, Vars);
					_instance.FooterOutputText = Message;
					_instance.FooterOutputType = LogType.Important;
					_instance.FooterOutputDate = DateTime.Now.ToShortTimeString();
					Log.AllCallback?.Invoke(Message, LogType.Important);
				}),
				DispatcherPriority.Background);
			}
		}

		public static void FooterError(string Message, params object[] Vars)
		{
			if (_instance != null)
			{
				_instance.Dispatcher.BeginInvoke((Action)(() => {
					Message = String.Format(Message, Vars);
					_instance.FooterOutputText = Message;
					_instance.FooterOutputType = LogType.Error;
					_instance.FooterOutputDate = DateTime.Now.ToShortTimeString();
					Log.AllCallback?.Invoke(Message, LogType.Error);
				}),
				DispatcherPriority.Background);
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

		private void OpenKeywordsButton_Click(object sender, RoutedEventArgs e)
		{
			FileCommands.Load.OpenFileDialog(this, "Open Keywords", Data.ModuleSettings.UserKeywordsFile, (path) => { FileCommands.Load.LoadUserKeywords(Data, path); }, CommonFileFilters.Json);
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
			FileCommands.OpenConfirmationDialog(this, "Reset Keyword List?", "Reset Keyword values to default?", "Changes will be lost.", (bool confirmed) =>
			{
				if(confirmed) Data.UserKeywords.ResetToDefault();
			});
		}

		private Action onGitGenerationConfirmed;

		public void OpenGitGenerationWindow(GitGenerationSettings settings, List<IProjectData> selectedProjects, Action onConfirm)
		{
			if(!GitGenerationWindow.IsVisible)
			{
				onGitGenerationConfirmed = onConfirm;

				if (gitGenerationWindow == null)
				{
					gitGenerationWindow = new GitGenerationWindow();
				}

				if (selectedProjects != null && selectedProjects.Count > 0)
				{
					Controller.Data.LockScreenVisibility = Visibility.Visible;
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
		}

		public void StartGitGeneration(GitGenerationSettings settings)
		{
			Controller.Data.LockScreenVisibility = Visibility.Collapsed;
			onGitGenerationConfirmed?.Invoke();
		}

		public void OnGitWindowCanceled()
		{
			Controller.Data.LockScreenVisibility = Visibility.Collapsed;
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
			
		}

		private void Tab_ResetFocus(object sender, EventArgs e)
		{
			if (sender is TabControl || sender is TabItem)
			{
				Dispatcher.BeginInvoke((Action)(() =>
				{
					IInputElement focusedControl = FocusManager.GetFocusedElement(this);
					if (focusedControl is TextBox textBox)
					{
						// Move to a parent that can take focus
						FrameworkElement parent = (FrameworkElement)textBox.Parent;
						while (parent != null && parent is IInputElement && !((IInputElement)parent).Focusable)
						{
							parent = (FrameworkElement)parent.Parent;
						}

						DependencyObject scope = FocusManager.GetFocusScope(textBox);
						FocusManager.SetFocusedElement(scope, parent as IInputElement);
					}
				}), DispatcherPriority.Background);
			}
		}
	}
}
