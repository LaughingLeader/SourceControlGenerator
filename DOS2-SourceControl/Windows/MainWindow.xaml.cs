using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using LL.DOS2.SourceControl.Core;
using LL.DOS2.SourceControl.Core.Commands;
using LL.DOS2.SourceControl.Data;
using LL.DOS2.SourceControl.Data.View;
using LL.DOS2.SourceControl.Util;
using LL.DOS2.SourceControl.Windows;

namespace LL.DOS2.SourceControl.Windows
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		private SettingsController _settingsController;

		public SettingsController SettingsController => _settingsController;

		private LogWindow logWindow;

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

		public static void FooterLog(LogType logType, string Message, params object[] Vars)
		{
			if (_instance != null)
			{
				Message = String.Format(Message, Vars);
				_instance.FooterOutputText = Message;
				_instance.FooterOutputType = logType;
				_instance.FooterOutputDate = DateTime.Now.ToShortTimeString();
				Log.AllCallback?.Invoke(Message, logType);
			}
		}

		//public LoadKeywordsCommand LoadKeywords { get; set; }

		private FileCommand loadKeywords;

		public FileCommand LoadKeywords
		{
			get { return loadKeywords; }
			set
			{
				loadKeywords = value;
				RaisePropertyChanged("LoadKeywords");
			}
		}


		public MainWindow()
		{
			InitializeComponent();

			_instance = this;
			_settingsController = new SettingsController(this);

			this.DataContext = SettingsController.Data;

			CollectionViewSource managedProjectsViewSource;
			managedProjectsViewSource = (CollectionViewSource)(FindResource("ManagedProjectsViewSource"));
			managedProjectsViewSource.Source = SettingsController.Data.ManagedProjects;

			LoadKeywords = new FileCommand(FileCommands.Load.LoadUserKeywords);

			logWindow = new LogWindow();
			logWindow.Hide();
		}

		private void HandleColumnHeaderSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
		{
			if (sizeChangedEventArgs.NewSize.Width <= 60)
			{
				sizeChangedEventArgs.Handled = true;
				//((GridViewColumnHeader)sender).Column.Width = 60;
			}
		}

		private void AvailableProjectsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ListBox list = (ListBox)this.FindName("AvailableProjectsList");
			if(list != null)
			{
				var selectedCount = list.SelectedItems.Count;
				if (selectedCount > 1)
				{
					SettingsController.Data.ManageButtonsText = "Manage Selected Projects";
				}
				else if (selectedCount == 1)
				{
					SettingsController.Data.ManageButtonsText = "Manage Selected Project";
				}
				else
				{
					SettingsController.Data.ManageButtonsText = "Select a Project";
				}

			}
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			//Super long tooltip durations
			ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(Int32.MaxValue));
		}

		private void AddSelectedProjectsButton_Click(object sender, RoutedEventArgs e)
		{
			ListBox list = (ListBox)this.FindName("AvailableProjectsList");
			if(list != null && list.SelectedItems.Count > 0)
			{
				List<AvailableProjectViewData> selectedItems = list.SelectedItems.Cast<AvailableProjectViewData>().ToList();
				if(selectedItems != null)
				{
					SettingsController.AddProjectsToManaged(selectedItems);
				}
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
			if (tab.IsSelected)
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
			if(FileCommands.WriteToFile(this.SettingsController.Data.AppSettings.KeywordsFile, this.SettingsController.Data.KeywordListText))
			{
				FooterLog(LogType.Important, "Saved Keywords to {0}", this.SettingsController.Data.AppSettings.KeywordsFile);
			}
			else
			{
				FooterLog(LogType.Important, "Error saving Keywords to {0}", this.SettingsController.Data.AppSettings.KeywordsFile);
			}
		}

		private void SaveAsKeywordsButton_Click(object sender, RoutedEventArgs e)
		{
			FileCommands.Save.OpenDialog(this, "Save Keywords", this.SettingsController.Data.AppSettings.KeywordsFile, this.SettingsController.Data.KeywordListText);
		}

		private void OpenKeywordsButton_Click(object sender, RoutedEventArgs e)
		{
			FileCommands.Load.OpenDialog(this, "Open Keywords", this.SettingsController.Data.AppSettings.KeywordsFile, FileCommands.Load.LoadUserKeywords);
		}

		private void KeywordsList_Add_Click(object sender, RoutedEventArgs e)
		{
			SettingsController.Data.KeywordList.Add(new Data.KeywordData());
		}

		private void KeywordsList_Remove_Click(object sender, RoutedEventArgs e)
		{
			SettingsController.Data.KeywordList.Remove(SettingsController.Data.KeywordList.Last());
		}

		private void KeywordsList_Default_Click(object sender, RoutedEventArgs e)
		{
			FileCommands.OpenConfirmationDialog(this, "Reset Keyword List", "Reset Keyword values to default?", "Confirm or Cancel", () =>
			{
				SettingsController.Data.KeywordList.Clear();
				SettingsController.Data.KeywordList.Add(new KeywordData());
				SettingsController.Data.KeywordList.Add(new KeywordData());
				SettingsController.Data.KeywordList.Add(new KeywordData());
				SettingsController.Data.KeywordList.Add(new KeywordData());
			});
		}

		private void ManagedProjectsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			bool projectSelected = false;
			DataGrid managedGrid = (DataGrid)this.FindName("ManagedProjectsDataGrid");
			if(managedGrid != null)
			{
				if (managedGrid.SelectedItems.Count > 0) projectSelected = true;
				Log.Here().Activity("Selected projects: {0}", managedGrid.SelectedItems.Count);
			}

			SettingsController.Data.ProjectSelected = projectSelected;
		}
	}
}
