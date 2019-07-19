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
using SCG.Extensions;

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
				ViewModel.SelectedEntry.RaisePropertyChanged("EntryContent");
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

			this.OneWayBind(this.ViewModel, vm => vm.SaveCurrentCommand, view => view.SaveButton.Command);
			this.OneWayBind(this.ViewModel, vm => vm.SaveAllCommand, view => view.SaveAllButton.Command);
			this.OneWayBind(this.ViewModel, vm => vm.ImportFileCommand, view => view.ImportFileButton.Command);

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
			if (selected) ViewModel.SelectedEntry = keyEntry;
		}

		private void EntryDataGrid_RowFocused(object sender, RoutedEventArgs e)
		{
			Log.Here().Activity($"EntryDataGrid_RowFocused: {sender} | {e.Source}");
			if(sender is DataGridRow row)
			{
				if(row.DataContext is LocaleKeyEntry localeKeyEntry)
				{
					ViewModel.SelectedEntry = localeKeyEntry;
				}
			}
		}

		private TextBox lastFocusedContentBox;
		private Xceed.Wpf.Toolkit.RichTextBox lastFocusedRichTextBox;

		private void EntryContent_GotFocus(object sender, RoutedEventArgs e)
		{
			if (sender is TextBox textBox)
			{
				lastFocusedContentBox = textBox;
				//ViewModel.SelectedEntry = (LocaleKeyEntry)textBox.Tag;
				Log.Here().Activity($"Tag: {textBox.Tag}");
			}
			else if (sender is Xceed.Wpf.Toolkit.RichTextBox rtb)
			{
				lastFocusedRichTextBox = rtb;
			}

			ViewModel.ContentFocused = true;
		}

		private void EntryContent_LostFocus(object sender, RoutedEventArgs e)
		{
			if (sender == lastFocusedContentBox)
			{
				lastFocusedContentBox = null;
			}

			if (sender == lastFocusedRichTextBox)
			{
				lastFocusedRichTextBox = null;
			}

			ViewModel.ContentFocused = false;
			ViewModel.ContentSelected = false;
		}

		private void EntryContent_SelectionChanged(object sender, RoutedEventArgs e)
		{
			if (sender is Xceed.Wpf.Toolkit.RichTextBox richTextBox)
			{
				ViewModel.ContentSelected = richTextBox.Selection?.Text != string.Empty;
			}

			if (sender is TextBox textBox)
			{
				ViewModel.ContentSelected = textBox.SelectedText != string.Empty;
			}
		}

		public void AddFontTag()
		{
			string color = "#FFFFFF";
			if (colorPicker != null && colorPicker.SelectedColor.HasValue)
			{
				color = colorPicker.SelectedColor.Value.ToHexString();
			}

			if (lastFocusedRichTextBox != null)
			{
				//Log.Here().Activity($"Content box selected: {lastFocusedRichTextBox.Selection.Text}");

				if (lastFocusedRichTextBox.Selection.Text != string.Empty)
				{
					int start = lastFocusedRichTextBox.Text.IndexOf(lastFocusedRichTextBox.Selection.Text);

					string text = lastFocusedRichTextBox.Text;
					string fontStartText = $"<font color='{color}'>";
					text = text.Insert(start, fontStartText);

					int end = start + fontStartText.Length + lastFocusedRichTextBox.Selection.Text.Length;
					text = text.Insert(end, @"</font>");

					lastFocusedRichTextBox.Text = text;

					//Log.Here().Activity($"Content box text set to: {text} | Start {start} End {end}");
				}
			}

			if (lastFocusedContentBox != null)
			{
				if (lastFocusedContentBox.SelectedText != string.Empty)
				{
					int start = lastFocusedContentBox.Text.IndexOf(lastFocusedContentBox.SelectedText);

					string text = lastFocusedContentBox.Text;
					string fontStartText = $"<font color='{color}'>";
					text = text.Insert(start, fontStartText);

					int end = start + fontStartText.Length + lastFocusedContentBox.SelectedText.Length;
					text = text.Insert(end, @"</font>");

					lastFocusedContentBox.Text = text;
				}
			}
		}

		Xceed.Wpf.Toolkit.ColorPicker colorPicker;

		private void FontColorPicker_Loaded(object sender, RoutedEventArgs e)
		{
			if(sender is Xceed.Wpf.Toolkit.ColorPicker cp)
			{
				colorPicker = cp;

				colorPicker.Focusable = false;

				var children = colorPicker.FindVisualChildren<Control>();
				foreach (var child in children)
				{
					child.Focusable = false;
				}
			}
		}
	}
}
