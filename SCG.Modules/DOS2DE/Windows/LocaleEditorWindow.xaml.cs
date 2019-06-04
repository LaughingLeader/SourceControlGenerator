using SCG.Modules.DOS2DE.Data.View;
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
		public LocaleViewData LocaleData { get; set; }

		public LocaleExportWindow ExportWindow { get; set; }

		private DOS2DEModuleData ModuleData { get; set; }

		public LocaleEditorWindow(DOS2DEModuleData data)
		{
			InitializeComponent();

			ModuleData = data;

			ExportWindow = new LocaleExportWindow();
			ExportWindow.Hide();

			instance = this;
		}

		public void LoadData(LocaleViewData data)
		{
			LocaleData = data;
			LocaleData.UpdateCombinedGroup(true);
			DataContext = LocaleData;
			//currentdata.Groups = new System.Collections.ObjectModel.ObservableCollection<DOS2DELocalizationGroup>(data.Groups);
			//currentdata.UpdateCombinedGroup(true);
			LocaleData.MenuData.RegisterShortcuts(this);
			LocaleData.ModuleData = ModuleData;

			LocaleEditorCommands.LoadSettings(ModuleData, LocaleData);
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

		private void ExportButton_Click(object sender, RoutedEventArgs e)
		{
			if (ExportWindow.FindName("OutputTextbox") is TextBox outputTextbox)
			{
				Log.Here().Activity("Exporting data to xml format.");
				outputTextbox.Text = "";
				outputTextbox.Text = LocaleEditorCommands.ExportDataAsXML(LocaleData, LocaleData.ExportSource, LocaleData.ExportKeys);
			}
			else
			{
				Log.Here().Activity("Failed to find textbox.");
			}

			if (!ExportWindow.IsVisible)
			{
				ExportWindow.Show();
				ExportWindow.Owner = this;
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

				FileCommands.Save.OpenDialog(this, "Create Localization File...", sourceRoot, (string savePath) => {
					var fileData = LocaleEditorCommands.CreateFileData(savePath, Path.GetFileName(savePath));
					LocaleData.SelectedGroup.DataFiles.Add(fileData);
					LocaleData.SelectedGroup.UpdateCombinedData();
					LocaleData.SelectedGroup.SelectedFileIndex = LocaleData.SelectedGroup.Tabs.Count - 1;
				}, "NewFile.lsb", "Larian Localization File (*.lsb)|*.lsb");
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
