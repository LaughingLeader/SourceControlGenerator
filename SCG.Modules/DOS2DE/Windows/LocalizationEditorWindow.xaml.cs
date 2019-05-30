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
using System.Windows.Shapes;
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
				outputTextbox.Text = DOS2DELocalizationEditor.ExportDataAsXML(LocaleData, LocaleData.ExportSource, LocaleData.ExportKeys);
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
	}
}
