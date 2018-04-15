﻿using System;
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
using LL.SCG.Core;
using LL.SCG.Data;
using LL.SCG.Data.View;
using LL.SCG.Interfaces;
using LL.SCG.Windows;
using LL.SCG.Controls.Behavior;
using LL.SCG.DOS2.Core;
using System.ComponentModel;

namespace LL.SCG.DOS2.Controls
{
	/// <summary>
	/// Interaction logic for ProjectViewControl.xaml
	/// </summary>
	public partial class ProjectViewControl : UserControl
	{
		private DOS2ProjectController Controller { get; set; }
		private MainWindow mainWindow;

		private bool gridSplitterMoving = false;

		public ProjectViewControl(MainWindow mainAppWindow, DOS2ProjectController controller)
		{
			InitializeComponent();

			Controller = controller;
			mainWindow = mainAppWindow;

			DataContext = Controller.Data;

			var gridSplitter = (GridSplitter)this.FindName("ProjectsDataGridSplitter");
			if (gridSplitter != null)
			{
				gridSplitter.DragStarted += (s, e) => { gridSplitterMoving = true; };
				gridSplitter.DragCompleted += (s, e) => { gridSplitterMoving = false; };
			}
		}

		private void AvailableProjectsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ListBox list = (ListBox)this.FindName("AvailableProjectsList");
			if (list != null)
			{
				var selectedCount = list.SelectedItems.Count;
				if (selectedCount > 1)
				{
					Controller.Data.ManageButtonsText = "Manage Selected Projects";
				}
				else if (selectedCount == 1)
				{
					Controller.Data.ManageButtonsText = "Manage Selected Project";
				}
				else
				{
					if (Controller.Data.NewProjects.Count > 0)
					{
						Controller.Data.ManageButtonsText = "Select a Project";
					}
					else
					{
						Controller.Data.ManageButtonsText = "No New Projects Found";
					}
				}

			}
		}

		private void AddSelectedProjectsButton_Click(object sender, RoutedEventArgs e)
		{
			Console.WriteLine("Adding projects...");
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
				Log.Here().Activity("No projects selected!");
			}
		}

		private void AvailableProjectsList_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
		{
			/*
			ListBox list = (ListBox)this.FindName("AvailableProjectsList");
			if (list != null && list.SelectedItems.Count > 0)
			{
				foreach(ListBoxItem item in list.ItemContainerGenerator.Items.Cast<ListBoxItem>().ToList())
				{
					if(item.IsMouseOver)
					{
						
					}
				}
			}
			else
			{
				Log.Here().Activity("No projects selected!");
			}
			*/
		}

		private bool availableProjectsVisible = true;

		private void Btn_AvailableProjects_Click(object sender, RoutedEventArgs e)
		{
			var grid = (Grid)this.FindName("AvailableProjectsView");
			if (grid != null)
			{
				var viewRow = grid.ColumnDefinitions.ElementAtOrDefault(1);

				if (availableProjectsVisible)
				{
					viewRow.Width = new GridLength(0);
				}
				else
				{
					viewRow.Width = new GridLength(1, GridUnitType.Star);
				}

				availableProjectsVisible = !availableProjectsVisible;

				if (availableProjectsVisible)
					Controller.Data.AvailableProjectsToggleText = "Hide Available Projects";
				else
					Controller.Data.AvailableProjectsToggleText = "Show Available Projects";
			}
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
							//data.Selected = true;
							/*
							if (!canArchive)
							{
								string gitProjectDirectory = System.IO.Path.Combine(Controller.Data.AppSettings.GitRootDirectory, data.Name);
								if (Directory.Exists(gitProjectDirectory)) canArchive = true;
							}
							*/
						}
					}
				}
				//Log.Here().Activity("Selected projects: {0}", managedGrid.SelectedItems.Count);
				DeselectSelectedRows();
			}

			Controller.Data.ProjectSelected = projectSelected;
		}

		private async void DeselectSelectedRows()
		{
			Task.Delay(200);

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
		}

		private void BackupButton_Click(object sender, RoutedEventArgs e)
		{
			Controller.BackupSelectedProjects();
		}

		private void BackupSelectedButton_Click(object sender, RoutedEventArgs e)
		{
			if (String.IsNullOrWhiteSpace(Controller.Data.Settings.LastBackupPath))
			{
				Controller.Data.Settings.LastBackupPath = Controller.Data.Settings.BackupRootDirectory;
			}

			FileCommands.Load.OpenFolderDialog(mainWindow, "Select Archive Export Location", Controller.Data.Settings.LastBackupPath, (path) =>
			{
				Controller.Data.Settings.LastBackupPath = path;
				Controller.BackupSelectedProjects(path);
			}, false);
		}

		private void GitGenerationButton_Click(object sender, RoutedEventArgs e)
		{
			var selectedProjects = Controller.Data.ManagedProjects.Where(p => p.Selected).ToList<IProjectData>();
			mainWindow.OpenGitGenerationWindow(Controller.Data.GitGenerationSettings, selectedProjects, Controller.StartGitGenerationAsync);
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
			if(sender is Button btn)
			{
				btn.ContextMenu.PlacementTarget = btn;
			}
		}

		private void ManagedProjectsDataGrid_Loaded(object sender, RoutedEventArgs e)
		{
			if(sender is DataGrid grid)
			{
				double availableSpace = this.ActualWidth;
				double starSpace = 0.0;
				double starFactor = 0.0;

				foreach (DataGridColumn column in grid.Columns.AsParallel())
				{
					if(column.Header is string headerName)
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
			DataGridRow row = sender as DataGridRow;
			if(row.DataContext is ModProjectData data)
			{
				data.Selected = !data.Selected;
			}
		}
	}
}