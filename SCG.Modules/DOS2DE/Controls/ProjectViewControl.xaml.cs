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

using SCG.Core;
using SCG.Data;
using SCG.Data.View;
using SCG.Interfaces;
using SCG.Windows;
using SCG.Controls.Behavior;
using SCG.Modules.DOS2DE.Core;
using System.ComponentModel;
using SCG.Modules.DOS2DE.Windows;
using SCG.Modules.DOS2DE.Data.View;
using SCG.Commands;
using Alphaleonis.Win32.Filesystem;
using System.Windows.Threading;
using SCG.Util;
using SCG.Collections;

namespace SCG.Modules.DOS2DE.Controls
{
	/// <summary>
	/// Interaction logic for ProjectViewControl.xaml
	/// </summary>
	public partial class ProjectViewControl : UserControl
	{
		public DOS2DEProjectController Controller { get; private set; }
		public MainWindow MainWindow { get; private set; }
		public EditVersionWindow EditVersionWindow { get; private set; }

		//private bool gridSplitterMoving = false;

		public static DOS2DEModuleData TestData { get; private set; } = createTestData();

		private static DOS2DEModuleData createTestData()
		{
			DOS2DEModuleData testData = new DOS2DEModuleData();

			testData.ModProjects = createTestProjects();
			
			foreach(var project in testData.ModProjects)
			{
				testData.ManagedProjects.Add(project);
			}

			return testData;
		}

		private static ObservableImmutableList<ModProjectData> createTestProjects()
		{
			ObservableImmutableList<ModProjectData> projects = new ObservableImmutableList<ModProjectData>();

			var dataDirectory = @"G:\Divinity Original Sin 2\DefEd\Data";

			if (Directory.Exists(dataDirectory))
			{
				string projectsPath = Path.Combine(dataDirectory, "Projects");
				string modsPath = Path.Combine(dataDirectory, "Mods");

				if (Directory.Exists(modsPath))
				{
					DirectoryInfo modsRoot = new DirectoryInfo(modsPath);
					var modFolders = modsRoot.GetDirectories().Where(s => !DOS2DECommands.IgnoredFolders.Contains(s.Name));

					if (modFolders != null)
					{
						foreach (DirectoryInfo modFolderInfo in modFolders)
						{
							var modFolderName = modFolderInfo.Name;
							Log.Here().Activity("Checking project mod folder: {0}", modFolderName);

							var metaFile = modFolderInfo.GetFiles("meta.lsx").FirstOrDefault();
							if (metaFile != null)
							{
								ModProjectData modProjectData = new ModProjectData(metaFile, projectsPath);
								projects.DoOperation(data => data.Add(modProjectData));
							}
						}
					}
				}
			}

			return projects;
		}

		private static ProjectViewControl _instance { get; set; }

		public ProjectViewControl(MainWindow mainAppWindow, DOS2DEProjectController controller)
		{
			InitializeComponent();

			_instance = this;

			Controller = controller;
			MainWindow = mainAppWindow;

			EditVersionWindow = new EditVersionWindow();
			EditVersionWindow.Hide();

			DataContext = Controller.Data;

			ToggleAvailableProjectsView(Controller.Data.NewProjectsAvailable);

			/*
			var gridSplitter = (GridSplitter)this.FindName("ProjectsDataGridSplitter");
			if (gridSplitter != null)
			{
				gridSplitter.DragStarted += (s, e) => { gridSplitterMoving = true; };
				gridSplitter.DragCompleted += (s, e) => { gridSplitterMoving = false; };
			}
			*/
		}

		public static void EditProjectVersion(ModProjectData projectData)
		{
			_instance.EditVersionWindow.LoadData(projectData);
			_instance.EditVersionWindow.Owner = _instance.MainWindow;
			_instance.EditVersionWindow.Show();
		}

		private void ProjectViewControlMain_Loaded(object sender, RoutedEventArgs e)
		{
			
		}

		private void AvailableProjectsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ListBox list = (ListBox)this.FindName("AvailableProjectsList");
			if (list != null)
			{
				Controller.Data.UpdateManageButtonsText(list.SelectedItems.Count);
			}
		}

		private void AddSelectedProjectsButton_Click(object sender, RoutedEventArgs e)
		{
			ListBox list = (ListBox)this.FindName("AvailableProjectsList");
			if (list != null && list.SelectedItems.Count > 0)
			{
				List<AvailableProjectViewData> selectedItems = list.SelectedItems.Cast<AvailableProjectViewData>().ToList();
				if (selectedItems != null)
				{
					Controller.AddProjects(selectedItems);
				}
				else
				{
					Log.Here().Error("Error casting selected items to AvailableProjectViewData");
				}
			}
			else
			{
				//Log.Here().Activity("No projects selected!");
			}
		}

		private void AvailableProjectsList_TargetUpdated(object sender, DataTransferEventArgs e)
		{
			if(sender is ListBox listBox)
			{
				if(listBox.Items.Count > 0)
				{
					listBox.ScrollIntoView(listBox.Items[0]);
				}
				else
				{
					ToggleAvailableProjectsView(false);
					Controller.Data.NewProjectsAvailable = false;
				}
			}
		}

		private void Btn_AvailableProjects_Click(object sender, RoutedEventArgs e)
		{
			ToggleAvailableProjectsView();
		}

		public void ToggleAvailableProjectsView(bool? NextValue = null)
		{
			var grid = (Grid)this.FindName("AvailableProjectsView");
			if (grid != null)
			{
				bool nextValue = Controller.Data.AvailableProjectsVisible;
				if (NextValue != null)
				{
					nextValue = NextValue.Value;
				}
				else
				{
					nextValue = !Controller.Data.AvailableProjectsVisible;
				}

				var viewRow = grid.ColumnDefinitions.ElementAtOrDefault(1);

				if (!nextValue)
				{
					viewRow.Width = new GridLength(0);
				}
				else
				{
					viewRow.Width = new GridLength(1, GridUnitType.Star);
				}

				Controller.Data.AvailableProjectsVisible = nextValue;
			}
		}

		private void ManagedProjectsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			bool canGitGenerate = false;
			bool projectSelected = false;

			DataGrid managedGrid = (DataGrid)this.FindName("ManagedProjectsDataGrid");

			if (managedGrid != null)
			{
				foreach (var item in managedGrid.SelectedItems)
				{
					if (item is ModProjectData data)
					{
						if (data.Selected)
						{
							if (!data.GitGenerated)
							{
								canGitGenerate = true;
							}

							projectSelected = true;
						}
					}
				}
			}

			Controller.Data.ProjectSelected = projectSelected;
			Controller.Data.CanGenerateGit = canGitGenerate && AppController.Main.GitDetected;
			Controller.Data.CanCreatePackages = projectSelected;
			Controller.SelectionChanged();
		}

		private async void DeselectSelectedRows()
		{
			await Task.Delay(200);

			DataGrid managedGrid = (DataGrid)this.FindName("ManagedProjectsDataGrid");

			foreach (var row in managedGrid.ItemsSource)
			{
				if (!managedGrid.SelectedItems.Contains(row))
				{
					if (row is ModProjectData data)
					{
						//data.Selected = false;
					}
				}
			}
		}

		private void ManagedProjects_SelectAll(object sender, RoutedEventArgs e)
		{
			//foreach (var project in Controller.DOS2Data.DetectedProjects)
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

			Controller.Data.ProjectSelected = true;
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

					if (managedGrid.SelectedItems.Contains(row)) managedGrid.SelectedItems.Remove(row);
				}
			}

			Controller.Data.ProjectSelected = false;
		}

		private void BackupButton_Click(object sender, RoutedEventArgs e)
		{
			Controller.BackupSelectedProjects();
		}

		private void BackupSelectedButton_Click(object sender, RoutedEventArgs e)
		{
			Controller.BackupSelectedProjectsTo();
		}

		private void PackageCreateButton_Click(object sender, RoutedEventArgs e)
		{
			Controller.PackageSelectedProjects();
		}

		private void GitGenerationButton_Click(object sender, RoutedEventArgs e)
		{
			Controller.OpenGitGeneratorWindow();
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

		private void SetDataFolderContextMenuTarget(object sender, RoutedEventArgs e)
		{
			if (sender is Button btn && btn.ContextMenu != null && btn.ContextMenu.DataContext == null)
			{
				btn.ContextMenu.PlacementTarget = btn;
				btn.ContextMenu.DataContext = btn.DataContext;
			}
		}

		private void ManagedProjectsDataGrid_Loaded(object sender, RoutedEventArgs e)
		{
			if (sender is DataGrid grid)
			{
				double availableSpace = this.ActualWidth;

				foreach(var column in grid.Columns)
				{
					if (column.Header is string headerName)
					{
						if (headerName != "Description" && column.Visibility == Visibility.Visible && column.GetType() == typeof(DataGridTextColumn))
						{
							column.Width = DataGridLength.Auto;
						}
					}
				}
			}
		}

		private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			//DataGridRow row = sender as DataGridRow;
			//if (row.DataContext is ModProjectData data)
			//{
			//	data.Selected = !data.Selected;
			//}
		}

		private int shiftKeyDown = 0;

		private int ShiftKeyDown
		{
			get => shiftKeyDown;
			set
			{
				shiftKeyDown = value;
				if (shiftKeyDown < 0) shiftKeyDown = 0;
			}
		}


		private void ProjectViewControlMain_KeyDown(object sender, KeyEventArgs e)
		{
			if(e.Key == Key.LeftShift || e.Key == Key.RightShift)
			{
				ShiftKeyDown += 1;
			}
		}

		private void ProjectViewControlMain_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
			{
				ShiftKeyDown -= 1;
			}
		}

		private void Btn_ModProjects_Refresh_Click(object sender, RoutedEventArgs e)
		{
			//Controller.RefreshModProjects();
			Controller.RefreshAllProjects();
		}

		private void Btn_AvailableProjects_Refresh_Click(object sender, RoutedEventArgs e)
		{
			Controller.RefreshAllProjects();
		}
	}
}
