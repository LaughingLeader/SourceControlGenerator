using System;
using System.Collections.Generic;
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
using LL.DOS2.SourceControl.Data.View;
using LL.DOS2.SourceControl.Windows;

namespace LL.DOS2.SourceControl
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private SettingsController _settingsController;

		public SettingsController SettingsController => _settingsController;

		private LogWindow logWindow;

		public bool LogWindowShown { get; set; }

		public MainWindow()
		{
			InitializeComponent();

			_settingsController = new SettingsController(this);

			this.DataContext = SettingsController.Data;

			CollectionViewSource managedProjectsViewSource;
			managedProjectsViewSource = (CollectionViewSource)(FindResource("ManagedProjectsViewSource"));
			managedProjectsViewSource.Source = SettingsController.Data.ManagedProjects;

			logWindow = new LogWindow();
			logWindow.Show();
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
	}
}
