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

namespace SCG.Modules.DOS2DE.Windows
{
	/// <summary>
	/// Interaction logic for LocaleEditorWindow.xaml
	/// </summary>
	public partial class LocaleEditorWindow : Window
	{
		public static LocaleEditorWindow instance { get; private set; }
		/*
		public LocaleViewData LocaleData
		{
			get { return (LocaleViewData)GetValue(LocaleDataProperty); }
			set { SetValue(LocaleDataProperty, value); }
		}

		// Using a DependencyProperty as the backing store for KeywordName.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty LocaleDataProperty =
			DependencyProperty.Register("LocaleData", typeof(string), typeof(LocaleViewData), new PropertyMetadata(""));
		*/
		public LocaleViewModel LocaleData { get; set; }

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
		}

		public void ExpandContent(object obj)
		{
			if(obj is LocaleKeyEntry entry)
			{
				LocaleData.SelectedEntry = entry;
				LocaleData.SelectedEntry.Notify("EntryContent");
				ContentWindow.DataContext = LocaleData.SelectedEntry;
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
				OptionsWindow.LoadData(LocaleData.Settings);
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
			LocaleData = data;
			LocaleEditorCommands.LoadSettings(ModuleData, LocaleData);
			LocaleData.UpdateCombinedGroup(true);
			DataContext = LocaleData;
			//currentdata.Groups = new System.Collections.ObjectModel.ObservableCollection<DOS2DELocalizationGroup>(data.Groups);
			//currentdata.UpdateCombinedGroup(true);
			LocaleData.MenuData.RegisterShortcuts(this);
			LocaleData.ModuleData = ModuleData;

			ExportWindow.LocaleData = LocaleData;
			LocaleData.ExpandContentCommand = new ParameterCommand(ExpandContent);
		}

		public void SaveSettings()
		{
			LocaleEditorCommands.SaveSettings(ModuleData, LocaleData);
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
			var backupSuccess = LocaleEditorCommands.BackupDataFiles(LocaleData, ModuleData.Settings.BackupRootDirectory);
			if (backupSuccess.Result == true)
			{
				var successes = LocaleEditorCommands.SaveDataFiles(LocaleData);
				Log.Here().Important($"Saved {successes} localization files.");
			}
		}

		private void LocaleWindow_Closing(object sender, CancelEventArgs e)
		{
			LocaleEditorCommands.SaveSettings(ModuleData, LocaleData);
			LocaleData.MenuData.UnregisterShortcuts(this);
		}

		private bool NameExistsInData(string name)
		{
			return LocaleData.SelectedGroup.DataFiles.Any(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
		}

		private string GetNewFileName(string rootPath, string baseName, string extension = ".lsb")
		{
			var checkPath = Path.Combine(rootPath, baseName, extension);

			var originalBase = baseName;
			int checks = 1;
			while (File.Exists(checkPath) || NameExistsInData(baseName + extension))
			{
				baseName = originalBase + checks;
				checkPath = Path.Combine(rootPath, baseName, extension);
				checks++;
			}

			return baseName + extension;
		}

		private void AddFileButton_Click(object sender, RoutedEventArgs e)
		{
			if(LocaleData.SelectedGroup != null)
			{
				var sourceRoot = "";
				if (LocaleData.SelectedGroup.DataFiles.First() is LocaleFileData keyFileData)
				{
					sourceRoot = Path.GetDirectoryName(keyFileData.SourcePath) + @"\";
				}
				else
				{
					if(LocaleData.SelectedGroup == LocaleData.PublicGroup)
					{
						sourceRoot = Path.Combine(ModuleData.Settings.DOS2DEDataDirectory, "Public");
					}
					else if(LocaleData.SelectedGroup == LocaleData.ModsGroup)
					{
						sourceRoot = Path.Combine(ModuleData.Settings.DOS2DEDataDirectory, "Mods");
					}
				}

				string newFileName = GetNewFileName(sourceRoot, "NewFile");

				FileCommands.Save.OpenDialog(this, "Create Localization File...", sourceRoot, (string savePath) => {
					var fileData = LocaleEditorCommands.CreateFileData(savePath, Path.GetFileName(savePath));
					LocaleData.SelectedGroup.DataFiles.Add(fileData);
					LocaleData.SelectedGroup.UpdateCombinedData();
					LocaleData.SelectedGroup.SelectedFileIndex = LocaleData.SelectedGroup.Tabs.Count - 1;
				}, newFileName, "Larian Localization File (*.lsb)|*.lsb");
			}
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
			LocaleData.UpdateAnySelected(selected);
		}
	}
}
