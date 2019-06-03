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
	/// Interaction logic for LocalizationEditorWindow.xaml
	/// </summary>
	public partial class LocalizationEditorWindow : Window
	{
		public static LocalizationEditorWindow instance { get; private set; }
		/*
		public DOS2DELocalizationViewData LocaleData
		{
			get { return (DOS2DELocalizationViewData)GetValue(LocaleDataProperty); }
			set { SetValue(LocaleDataProperty, value); }
		}

		// Using a DependencyProperty as the backing store for KeywordName.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty LocaleDataProperty =
			DependencyProperty.Register("LocaleData", typeof(string), typeof(DOS2DELocalizationViewData), new PropertyMetadata(""));
		*/
		public DOS2DELocalizationViewData LocaleData { get; set; }

		public LocaleExportWindow ExportWindow { get; set; }

		private DOS2DEModuleData moduleData;

		public LocalizationEditorWindow(DOS2DEModuleData data)
		{
			InitializeComponent();

			moduleData = data;

			ExportWindow = new LocaleExportWindow();
			ExportWindow.Hide();

			instance = this;
		}

		public void LoadData(DOS2DELocalizationViewData data)
		{
			LocaleData = data;
			LocaleData.UpdateCombinedGroup(true);
			DataContext = LocaleData;
			//currentdata.Groups = new System.Collections.ObjectModel.ObservableCollection<DOS2DELocalizationGroup>(data.Groups);
			//currentdata.UpdateCombinedGroup(true);
			LocaleData.MenuData.RegisterShortcuts(this);
			LocaleData.ModuleData = moduleData;
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
				outputTextbox.Text = DOS2DELocalizationEditor.ExportDataAsXML(LocaleData, LocaleData.ExportSource, LocaleData.ExportKeys);
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
			var backupSuccess = DOS2DELocalizationEditor.BackupDataFiles(LocaleData, moduleData.Settings.BackupRootDirectory);
			if (backupSuccess.Result == true)
			{
				var successes = DOS2DELocalizationEditor.SaveDataFiles(LocaleData);
				Log.Here().Important($"Saved {successes} localization files.");
			}
		}

		private void LocaleWindow_Closing(object sender, CancelEventArgs e)
		{
			LocaleData.MenuData.UnregisterShortcuts(this);
		}

		private void AddFileButton_Click(object sender, RoutedEventArgs e)
		{
			if(LocaleData.SelectedGroup != null)
			{
				var sourceRoot = "";
				if (LocaleData.SelectedGroup.DataFiles.First() is DOS2DEStringKeyFileData keyFileData)
				{
					sourceRoot = Path.GetDirectoryName(keyFileData.SourcePath) + @"\";
				}
				else
				{
					if(LocaleData.SelectedGroup == LocaleData.PublicGroup)
					{
						sourceRoot = Path.Combine(moduleData.Settings.DOS2DEDataDirectory, "Public");
					}
					else if(LocaleData.SelectedGroup == LocaleData.ModsGroup)
					{
						sourceRoot = Path.Combine(moduleData.Settings.DOS2DEDataDirectory, "Mods");
					}
				}

				FileCommands.Save.OpenDialog(this, "Create Localization File...", sourceRoot, (string savePath) => {
					DOS2DELocalizationEditor.AddFileData(LocaleData.SelectedGroup, savePath, Path.GetFileName(savePath));
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

		public void KeyEntrySelected(DOS2DEKeyEntry keyEntry, bool selected)
		{
			LocaleData.UpdateAnySelected(selected);
		}
	}
}
