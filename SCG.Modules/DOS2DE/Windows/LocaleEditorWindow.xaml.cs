using SCG.Modules.DOS2DE.Data.View;
using SCG.Modules.DOS2DE.Data.View.Locale;
using SCG.Modules.DOS2DE.Utilities;
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
using Alphaleonis.Win32.Filesystem;
using SCG.Windows;
using DataGridExtensions;
using SCG.Data.View;
using SCG.Core;
using System.ComponentModel;
using SCG.Commands;
using ReactiveUI;

namespace SCG.Modules.DOS2DE.Windows
{
	/// <summary>
	/// Interaction logic for LocaleEditorWindow.xaml
	/// </summary>
	public partial class LocaleEditorWindow : ReactiveWindow<LocaleViewModel>
	{
		public static LocaleEditorWindow instance { get; private set; }
		/*
		public LocaleViewData ViewModel
		{
			get { return (LocaleViewData)GetValue(LocaleDataProperty); }
			set { SetValue(LocaleDataProperty, value); }
		}

		// Using a DependencyProperty as the backing store for KeywordName.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty LocaleDataProperty =
			DependencyProperty.Register("ViewModel", typeof(string), typeof(LocaleViewData), new PropertyMetadata(""));
		*/
		//public LocaleViewModel ViewModel { get; set; }

		public LocaleExportWindow ExportWindow { get; set; }
		public LocaleOptionsWindow OptionsWindow { get; set; }
		public LocaleContentWindow ContentWindow { get; set; }

		private DOS2DEModuleData ModuleData { get; set; }

		public LocaleEditorWindow(DOS2DEModuleData data)
		{
			InitializeComponent();

			ModuleData = data;

			ExportWindow = new LocaleExportWindow();
			ExportWindow.Hide();

			OptionsWindow = new LocaleOptionsWindow();
			OptionsWindow.Hide();

			ContentWindow = new LocaleContentWindow();
			ContentWindow.Hide();

			instance = this;

			this.WhenActivated((disposable) =>
			{
				

			});
		}

		public void ExpandContent(object obj)
		{
			if(obj is LocaleKeyEntry entry)
			{
				ViewModel.SelectedEntry = entry;
				ViewModel.SelectedEntry.Notify("EntryContent");
				ContentWindow.DataContext = ViewModel.SelectedEntry;
				if(!ContentWindow.IsVisible)
				{
					ContentWindow.Show();
					ContentWindow.Owner = this;
				}
			}
			else
			{
				ContentWindow.Hide();
			}
		}

		public void TogglePreferencesWindow()
		{
			if(!OptionsWindow.IsVisible)
			{
				OptionsWindow.LoadData(ViewModel.Settings);
				OptionsWindow.Show();
				OptionsWindow.Owner = this;
			}
			else
			{
				OptionsWindow.Hide();
			}
		}

		public void LoadData(LocaleViewModel data)
		{
			ViewModel = data;

			ViewModel.OnViewLoaded(this, ModuleData);

			ViewModel.ExpandContentCommand = new ParameterCommand(ExpandContent);

			this.OneWayBind(this.ViewModel, vm => vm.SaveCurrentCommand, view => view.SaveButton);
			this.OneWayBind(this.ViewModel, vm => vm.SaveAllCommand, view => view.SaveAllButton);
			this.OneWayBind(this.ViewModel, vm => vm.ImportFileCommand, view => view.ImportFileButton);

			DataContext = ViewModel;
			ExportWindow.LocaleData = ViewModel;
		}

		public void SaveSettings()
		{
			LocaleEditorCommands.SaveSettings(ModuleData, ViewModel);
		}

		private void Entries_SelectAll(object sender, RoutedEventArgs e)
		{
			
		}

		private void Entries_SelectNone(object sender, RoutedEventArgs e)
		{

		}

		private void DataGrid_Loaded(object sender, RoutedEventArgs e)
		{
			if(sender is DataGrid grid)
			{
				foreach (var column in grid.Columns)
				{
					column.MinWidth = column.ActualWidth;
					column.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
				}
			}
		}

		private void SaveAllButton_Click(object sender, RoutedEventArgs e)
		{
			var backupSuccess = LocaleEditorCommands.BackupDataFiles(ViewModel, ModuleData.Settings.BackupRootDirectory);
			if (backupSuccess.Result == true)
			{
				var successes = LocaleEditorCommands.SaveDataFiles(ViewModel);
				Log.Here().Important($"Saved {successes} localization files.");
			}
		}

		private void LocaleWindow_Closing(object sender, CancelEventArgs e)
		{
			LocaleEditorCommands.SaveSettings(ModuleData, ViewModel);
			ViewModel.MenuData.UnregisterShortcuts(this);
		}

		/// <summary>
		/// Single-click selection
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void LocaleEntryDataGrid_GotFocus(object sender, RoutedEventArgs e)
		{
			if(sender is DataGrid dg && e.OriginalSource is DataGridCell cell)
			{
				if (cell.Column is DataGridCheckBoxColumn column)
				{
					dg.BeginEdit();
					CheckBox chkBox = cell.Content as CheckBox;
					if (chkBox != null)
					{
						chkBox.IsChecked = !chkBox.IsChecked;
					}
					e.Handled = true;
				}
			}
		}

		public void KeyEntrySelected(LocaleKeyEntry keyEntry, bool selected)
		{
			ViewModel.UpdateAnySelected(selected);
		}
	}
}
