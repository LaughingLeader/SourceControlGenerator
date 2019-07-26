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
using ReactiveUI;
using SCG.Modules.DOS2DE.Data.View.Locale;
using SCG.Modules.DOS2DE.Utilities;
using System.Reactive;
using System.Reactive.Linq;
using DynamicData;
using System.Windows.Media.Animation;
using System.Reactive.Disposables;

namespace SCG.Modules.DOS2DE.Views
{
	/// <summary>
	/// Needed so the XAML designer can render ReactiveUserControl.
	/// </summary>
	public class DOS2DEProjectsViewBase : ReactiveUserControl<DOS2DEModuleData>
	{
		
	}

	/// <summary>
	/// Interaction logic for ProjectViewControl.xaml
	/// </summary>
	public partial class DOS2DEProjectsView : DOS2DEProjectsViewBase
	{
		public DOS2DEProjectController Controller { get; private set; }
		public MainWindow MainWindow { get; private set; }
		public EditVersionWindow EditVersionWindow { get; private set; }

		public LocaleEditorWindow LocaleEditorWindow { get; private set; }

		private static DOS2DEProjectsView _instance { get; set; }

		public DOS2DEProjectsView()
		{
			InitializeComponent();

			_instance = this;

			if (DesignerProperties.GetIsInDesignMode(this))
			{
				ViewModel = new DOS2DEModuleData();
				ViewModel.AvailableProjectsVisible = true;
				ViewModel.LoadPanelVisibility = Visibility.Collapsed;
			}

			Init();
		}

		public DOS2DEProjectsView(MainWindow mainAppWindow, DOS2DEProjectController controller)
		{
			InitializeComponent();

			_instance = this;

			Init();

			Controller = controller;
			MainWindow = mainAppWindow;

			this.ViewModel = Controller.Data;
			DataContext = this.ViewModel;

			//ViewModel.LoadPanelVisibility = Visibility.Visible;
			ToggleAvailableProjectsView(Controller.Data.NewProjectsAvailable);
		}

		public void Init()
		{
			EditVersionWindow = new EditVersionWindow();
			EditVersionWindow.Hide();

			LoadingProjectsPanel.Opacity = 1d;

			this.WhenActivated((d) =>
			{
				this.OneWayBind(ViewModel, vm => vm.ManagedProjects, v => v.ManagedProjectsDataGrid.ItemsSource).DisposeWith(d);
				this.OneWayBind(ViewModel, vm => vm.UnmanagedProjects, v => v.AvailableProjectsList.ItemsSource).DisposeWith(d);
			});
			
		}


		private void InitLocalizationEditor()
		{
			LocaleEditorWindow = new LocaleEditorWindow(this.ViewModel);
			LocaleEditorWindow.Closing += LocalizationEditorWindow_Closing;
		}

		public void ToggleLocalizationEditor()
		{
			if (LocaleEditorWindow == null)
			{
				InitLocalizationEditor();
			}

			if (!LocaleEditorWindow.IsVisible)
			{
				OpenLocalizationEditorAsync();
			}
			else
			{
				LocaleEditorWindow.Close();
			}
		}

		public static void ToggleDOS2DELocalizationEditor()
		{
			_instance?.ToggleLocalizationEditor();
		}

		public void OpenLocalizationEditorWithData(LocaleViewModel data)
		{
			if (LocaleEditorWindow == null) InitLocalizationEditor();
			LocaleEditorWindow.LoadData(data);
			if (!LocaleEditorWindow.IsVisible)
			{
				LocaleEditorWindow.Show();
			}
		}

		public static async Task<Unit> OpenLocalizationEditorForProject(ModProjectData modData)
		{
			var data = await LocaleEditorCommands.LoadLocalizationDataAsync(_instance.ViewModel, modData).ConfigureAwait(false);
			_instance.MainWindow.Dispatcher.Invoke(new Action(() => {
				_instance.OpenLocalizationEditorWithData(data);
			}), DispatcherPriority.Normal);
			return Unit.Default;
		}

		private async void OpenLocalizationEditorAsync()
		{
			var data = await LocaleEditorCommands.LoadLocalizationDataAsync(ViewModel,
				ViewModel.ManagedProjects.Where(p => p.Selected)).ConfigureAwait(false);
			_instance.MainWindow.Dispatcher.Invoke(new Action(() => OpenLocalizationEditorWithData(data)), DispatcherPriority.Normal);
		}

		private void LocalizationEditorWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			LocaleEditorWindow = null;
		}

		public static void EditProjectVersion(ModProjectData projectData)
		{
			_instance.EditVersionWindow.LoadData(projectData);
			_instance.EditVersionWindow.Owner = _instance.MainWindow;
			_instance.EditVersionWindow.Show();
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
				var selectedItems = list.SelectedItems.Cast<ModProjectData>();
				if (selectedItems != null)
				{
					Controller.AddProjects(selectedItems.Select(m => m.UUID));
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
					ToggleAvailableProjectsView(true);
					//Controller.Data.NewProjectsAvailable = false;
				}
			}
		}

		private void Btn_AvailableProjects_Click(object sender, RoutedEventArgs e)
		{
			ToggleAvailableProjectsView();
		}

		public void FadeLoadingPanel(bool fadeOut = true)
		{
			ViewModel.LoadPanelVisibility = Visibility.Visible;

			Storyboard storyboard = new Storyboard();
			TimeSpan duration = TimeSpan.FromMilliseconds(500);
			DoubleAnimation animation = new DoubleAnimation();

			if (fadeOut)
			{
				LoadingProjectsPanel.Opacity = 1d;
				animation.From = 1.0;
				animation.To = 0.0;
				animation.Completed += (s, e) => ViewModel.LoadPanelVisibility = Visibility.Collapsed;
			}
			else
			{
				LoadingProjectsPanel.Opacity = 0d;
				animation.From = 0.0;
				animation.To = 1.0;
				animation.Completed += (s, e) => ViewModel.LoadPanelVisibility = Visibility.Visible;
			}
			animation.Duration = new Duration(duration);

			Storyboard.SetTargetName(animation, LoadingProjectsPanel.Name);
			Storyboard.SetTargetProperty(animation, new PropertyPath(OpacityProperty));
			// Add the animation to the storyboard
			storyboard.Children.Add(animation);

			// Begin the storyboard
			storyboard.Begin(this);

			Dispatcher.Invoke(() =>
			{
				
			}, DispatcherPriority.ApplicationIdle);
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
							//Log.Here().Activity($"Project GitGenerated: {data.GitGenerated} | Git Detected: {AppController.Main.Data.GitDetected}");

							projectSelected = true;
						}
					}
				}
			}

			Controller.Data.ProjectSelected = projectSelected;
			Controller.Data.CanGenerateGit = canGitGenerate && AppController.Main.Data.GitDetected;
			Controller.Data.CanCreatePackages = projectSelected;
			Controller.SelectionChanged();
		}

		private async void DeselectSelectedRows()
		{
			await Task.Delay(200).ConfigureAwait(false);

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


		private void ProjectView_KeyDown(object sender, KeyEventArgs e)
		{
			if(e.Key == Key.LeftShift || e.Key == Key.RightShift)
			{
				ShiftKeyDown += 1;
			}
		}

		private void ProjectView_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
			{
				ShiftKeyDown -= 1;
			}
		}

		#region Design Data

		private void CreateTestData()
		{
			var newMods = new List<ModProjectData>();
			for (var i = 0; i < 4; i++)
			{
				var mod = new ModProjectData();
				mod.ModuleInfo = new SCG.Data.Xml.ModuleInfo()
				{
					Name = "Test" + i,
					Author = "LaughingLeader"
				};
				mod.ProjectInfo = new SCG.Data.Xml.ProjectInfo();
				mod.IsManaged = true;
				//mod.ThumbnailPath = @"G:\Divinity Original Sin 2\DefEd\Data\Projects\LeaderLib\thumbnail.png";
				mod.ThumbnailExists = false;

				newMods.Add(mod);
			}

			for (var i = 0; i < 4; i++)
			{
				var mod = new ModProjectData();
				mod.ModuleInfo = new SCG.Data.Xml.ModuleInfo()
				{
					Name = "TestNew" + i,
					Author = "Test",
				};
				mod.ProjectInfo = new SCG.Data.Xml.ProjectInfo();
				mod.IsManaged = false;

				newMods.Add(mod);
			}

			ViewModel.ModProjects.AddRange(newMods);

			//var mods = CreateTestProjects();

			//foreach (var m in mods)
			//{
			//	ModProjectsSource.Add(m);
			//	m.IsManaged = true;
			//}

		}

		private List<ModProjectData> CreateTestProjects()
		{
			List<ModProjectData> projects = new List<ModProjectData>();

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
								ModProjectData modProjectData = new ModProjectData();
								modProjectData.LoadAllData(metaFile.FullName, projectsPath);
								projects.Add(modProjectData);
							}
						}
					}
				}
			}

			return projects;
		}
		#endregion
	}
}
