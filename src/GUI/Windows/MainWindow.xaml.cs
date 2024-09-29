using AdonisUI;

using AutoUpdaterDotNET;

using SCG.Core;
using SCG.Data;
using SCG.Data.View;
using SCG.FileGen;
using SCG.Interfaces;

using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SCG.Windows;

public interface IToolWindow
{
	void Init(AppController appController);
}
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : ClipboardMonitorWindow, IViewFor<MainAppData>
{
	public MainAppData ViewModel { get; set; }
	object IViewFor.ViewModel { get; set; }

	private static MainWindow _instance;

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

	public DebugWindow? DebugWindow { get; set; }

	public List<Window> SubWindows { get; set; }

	private UserControl lastModuleView;

	public AppController Controller { get; private set; }

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

		TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;

		SubWindows =
		[
			LogWindow,
			AboutWindow,
			MarkdownConverterWindow,
			TextGeneratorWindow,
			GitGenerationWindow,
		];

		foreach (var window in SubWindows)
		{
			window.Hide();
		}

		Controller = new AppController(this);
		this.WhenAnyValue(v => v.Controller.Data).Subscribe((vm) =>
		{
			ViewModel = vm;
			DataContext = ViewModel;
		});

		Controller.LoadAppSettings();
		Controller.InitModules();

		if (String.IsNullOrWhiteSpace(ViewModel.AppSettings.GitInstallPath))
		{
			var gitPath = GitGenerator.GetGitInstallPath();
			if (!String.IsNullOrEmpty(gitPath))
			{
				ViewModel.AppSettings.GitInstallPath = gitPath;
				Log.Here().Important($"Git install location found at {gitPath}.");
			}
			else
			{
				Log.Here().Error($"Git install location not found.");
			}
		}

		AutoUpdater.ApplicationExitEvent += AutoUpdater_ApplicationExitEvent;
		AutoUpdater.HttpUserAgent = "SourceControlGeneratorUser";

		this.WhenActivated(d =>
		{
			this.OneWayBind(ViewModel, vm => vm.ProgressValueTaskBar, v => v.TaskbarItemInfo.ProgressValue);

			Controller.CheckForUpdates();
		});

		this.Loaded += MainWindow_Loaded;
	}

	public void BuildMenu()
	{
		TopMenu.Items.Clear();
		foreach (var menuEntry in ViewModel.MenuBarData.Menus)
		{
			if(menuEntry.Children.Count > 0)
			{
				AddMenuItem(menuEntry, TopMenu.Items);
			}
		}
	}

	private void AddMenuItem(IMenuData entry, ItemCollection target)
	{
		if (entry is MenuData menuEntry)
		{
			var menuItem = new MenuItem()
			{
				Command = menuEntry.Command,
				Tag = menuEntry,
				DataContext = menuEntry
			};
			if (menuEntry.Header.StartsWith("_"))
			{
				menuItem.Header = new AccessText() { Text = menuEntry.Header };
			}
			else
			{
				menuItem.Header = new TextBlock() { Text = menuEntry.Header };
			}
			target.Add(menuItem);
			foreach (var child in menuEntry.Children)
			{
				AddMenuItem(child, menuItem.Items);
			}
		}
		else if (entry is SeparatorData)
		{
			target.Add(new Separator() { Tag = entry });
		}
	}

	public static readonly Uri LightTheme = new("pack://application:,,,/AdonisUI;component/ColorSchemes/Light.xaml");
	public static readonly Uri DarkTheme = new("pack://application:,,,/AdonisUI;component/ColorSchemes/Dark.xaml");

	public void SetTheme(bool darkMode)
	{
		ResourceLocator.SetColorScheme(Resources, darkMode ? DarkTheme : LightTheme);
		foreach(var window in SubWindows)
		{
			ResourceLocator.SetColorScheme(window.Resources, darkMode ? DarkTheme : LightTheme);
		}
	}

	private void MainWindow_Loaded(object sender, RoutedEventArgs e)
	{
		if (!String.IsNullOrWhiteSpace(Controller.Data.AppSettings.LastModule) && Controller.SetModule(Controller.Data.AppSettings.LastModule))
		{
			Controller.Data.ModuleSelectionVisibility = Visibility.Collapsed;
		}
		else
		{
			Controller.Data.ModuleSelectionVisibility = Visibility.Visible;
		}
		SetTheme(true);
	}

	private void AutoUpdater_ApplicationExitEvent()
	{
		this.Controller.Data.AppSettings.LastUpdateCheck = DateTimeOffset.Now.ToUnixTimeSeconds();
		this.Controller.SaveAppSettings();
		App.Current.Shutdown();
	}

	public void OnModuleSet(IProjectController module)
	{
		if (module != null)
		{
			var view = module.GetProjectView(this);
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
		else
		{
			var viewGrid = (Grid)FindName("ProjectsViewGrid");
			if (viewGrid != null && lastModuleView != null)
			{
				viewGrid.Children.Remove(lastModuleView);
				lastModuleView = null;
			}

			Controller.Data.ModuleSelectionVisibility = Visibility.Visible;
		}

		BuildMenu();
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
			if (window is IToolWindow toolWindow)
			{
				toolWindow.Init(Controller);
			}

			//window.Owner = this;
			window.Topmost = false;
		}
	}

	public bool LogWindowShown
	{
		get
		{
			if (logWindow != null) return logWindow.IsVisible;
			return false;
		}
	}

	public static void FooterLog(string Message, params object[] Vars)
	{
		RxApp.MainThreadScheduler.Schedule(_ =>
		{
			Message = String.Format(Message, Vars);
			_instance.Controller.SetFooter(Message, LogType.Important);
			Log.AllCallback?.Invoke(Message, LogType.Important);
		});
	}

	public static void FooterError(string Message, params object[] Vars)
	{
		RxApp.MainThreadScheduler.Schedule(_ =>
		{
			Message = String.Format(Message, Vars);
			_instance.Controller.SetFooter(Message, LogType.Error);
			Log.AllCallback?.Invoke(Message, LogType.Error);
		});
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
		var tab = (TabItem)this.FindName("Tab_Settings");
		if (tab != null && tab.IsSelected)
		{
			tab.Focus();
		}
	}

	private void OpenKeywordsButton_Click(object sender, RoutedEventArgs e)
	{
		FileCommands.Load.OpenFileDialog(this, "Open Keywords", Data.ModuleSettings.UserKeywordsFile, (path) =>
		{
			FileCommands.Load.LoadUserKeywords(Data, path);
		}, "", null, CommonFileFilters.Json);
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
			if (confirmed) Data.UserKeywords.ResetToDefault();
		});
	}

	private Action onGitGenerationConfirmed;

	public void OpenGitGenerationWindow(GitGenerationSettings settings, List<IProjectData> selectedProjects, Action onConfirm)
	{
		if (!GitGenerationWindow.IsVisible)
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
			else if (list.IsFocused)
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
			RxApp.MainThreadScheduler.Schedule(() =>
			{
				var focusedControl = FocusManager.GetFocusedElement(this);
				if (focusedControl is TextBox textBox)
				{
					// Move to a parent that can take focus
					var parent = (FrameworkElement)textBox.Parent;
					while (parent != null && parent is IInputElement && !((IInputElement)parent).Focusable)
					{
						parent = (FrameworkElement)parent.Parent;
					}

					var scope = FocusManager.GetFocusScope(textBox);
					FocusManager.SetFocusedElement(scope, parent as IInputElement);
				}
			});
		}
	}
}
