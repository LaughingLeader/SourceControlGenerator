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
using LL.DOS2.SourceControl.Core;
using LL.DOS2.SourceControl.Commands;
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

		private bool gridSplitterMoving = false;

		public MainWindow()
		{
			InitializeComponent();

			_instance = this;
			_settingsController = new SettingsController(this);

			this.DataContext = SettingsController.Data;

			CollectionViewSource managedProjectsViewSource;
			managedProjectsViewSource = (CollectionViewSource)(FindResource("ManagedProjectsViewSource"));
			managedProjectsViewSource.Source = SettingsController.Data.ManagedProjects;

			logWindow = new LogWindow();
			logWindow.Hide();

			var gridSplitter = (GridSplitter)this.FindName("ProjectsDataGridSplitter");
			if(gridSplitter != null)
			{
				gridSplitter.DragStarted += (s, e) => { gridSplitterMoving = true; };
				gridSplitter.DragCompleted += (s, e) => { gridSplitterMoving = false; };
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
					if(SettingsController.Data.AvailableProjects.Count > 0)
					{
						SettingsController.Data.ManageButtonsText = "Select a Project";
					}
					else
					{
						SettingsController.Data.ManageButtonsText = "No New Projects Found";
					}
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
			if(FileCommands.WriteToFile(this.SettingsController.Data.AppSettings.KeywordsFile, this.SettingsController.Data.KeywordListText))
			{
				FooterLog("Saved Keywords to {0}", this.SettingsController.Data.AppSettings.KeywordsFile);
			}
			else
			{
				FooterLog("Error saving Keywords to {0}", this.SettingsController.Data.AppSettings.KeywordsFile);
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
				this.SettingsController.Data.AppSettings.KeywordsFile = path;
				MainWindow.FooterLog("Saved Keywords to {0}", path);
			}
			else
			{
				MainWindow.FooterLog("Error saving Keywords to {0}", path);
			}
		}

		private void SaveAsKeywordsButton_Click(object sender, RoutedEventArgs e)
		{
			FileCommands.Save.OpenDialogAndSave(this, "Save Keywords", this.SettingsController.Data.AppSettings.KeywordsFile, this.SettingsController.Data.KeywordListText, OnKeywordsSaveAs);
		}

		private void OpenKeywordsButton_Click(object sender, RoutedEventArgs e)
		{
			FileCommands.Load.OpenFileDialog(this, "Open Keywords", this.SettingsController.Data.AppSettings.KeywordsFile, FileCommands.Load.LoadUserKeywords);
		}

		private void KeywordsList_Add_Click(object sender, RoutedEventArgs e)
		{
			SettingsController.Data.UserKeywords.AddKeyword();
		}

		private void KeywordsList_Remove_Click(object sender, RoutedEventArgs e)
		{
			SettingsController.Data.UserKeywords.RemoveLast();
		}

		private void KeywordsList_Default_Click(object sender, RoutedEventArgs e)
		{
			FileCommands.OpenConfirmationDialog(this, "Reset Keyword List?", "Reset Keyword values to default?", "Changes will be lost.", () =>
			{
				SettingsController.Data.UserKeywords.ResetToDefault();
			});
		}

		private bool availableProjectsVisible = true;

		private void Btn_AvailableProjects_Click(object sender, RoutedEventArgs e)
		{
			var grid = (Grid)this.FindName("AvailableProjectsView");
			if(grid != null)
			{
				var viewRow = grid.ColumnDefinitions.ElementAtOrDefault(1);

				if(availableProjectsVisible)
				{
					viewRow.Width = new GridLength(0);
				}
				else
				{
					viewRow.Width = new GridLength(1, GridUnitType.Star);
				}

				availableProjectsVisible = !availableProjectsVisible;

				if (availableProjectsVisible)
					SettingsController.Data.AvailableProjectsToggleText = "Hide Available Projects";
				else
					SettingsController.Data.AvailableProjectsToggleText = "Show Available Projects";
			}
		}

		public bool HasFocus(Control aControl, bool aCheckChildren)
		{
			var oFocused = System.Windows.Input.FocusManager.GetFocusedElement(this) as DependencyObject;
			if (!aCheckChildren)
				return oFocused == aControl;
			while (oFocused != null)
			{
				if (oFocused == aControl)
					return true;
				oFocused = System.Windows.Media.VisualTreeHelper.GetParent(oFocused);
			}
			return false;
		}

		private void ManagedProjectsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			bool projectSelected = false;
			DataGrid managedGrid = (DataGrid)this.FindName("ManagedProjectsDataGrid");

			if (managedGrid != null)
			{
				if (managedGrid.SelectedItems.Count > 0)
				{
					projectSelected = true;

					foreach (var item in managedGrid.SelectedItems)
					{
						if (item is ModProjectData data)
						{
							data.Selected = true;
						}
					}
				}
				//Log.Here().Activity("Selected projects: {0}", managedGrid.SelectedItems.Count);
				foreach (var row in managedGrid.ItemsSource)
				{
					if (!managedGrid.SelectedItems.Contains(row))
					{
						if (row is ModProjectData data)
						{
							data.Selected = false;
						}
					}
				}
			}

			SettingsController.Data.ProjectSelected = projectSelected;
		}

		private void ManagedProjects_SelectAll(object sender, RoutedEventArgs e)
		{
			//foreach (var project in SettingsController.Data.ManagedProjects)
			//{
			//	project.Selected = true;
			//}

			DataGrid managedGrid = (DataGrid)this.FindName("ManagedProjectsDataGrid");

			if (managedGrid != null)
			{
				foreach (var row in managedGrid.ItemsSource)
				{
					if (row is ModProjectData data)
					{
						data.Selected = true;
					}

					managedGrid.SelectedItems.Add(row);
				}
			}
		}

		private void ManagedProjects_SelectNone(object sender, RoutedEventArgs e)
		{
			DataGrid managedGrid = (DataGrid)this.FindName("ManagedProjectsDataGrid");

			if (managedGrid != null)
			{
				foreach (var row in managedGrid.ItemsSource)
				{
					if (row is ModProjectData data)
					{
						data.Selected = false;
					}

					if(managedGrid.SelectedItems.Contains(row)) managedGrid.SelectedItems.Remove(row);
				}
			}
		}
	}
}
