﻿using SCG.Modules.DOS2DE.Data.View;
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
using SCG.Data.View;
using SCG.Core;
using System.ComponentModel;
using SCG.Commands;
using ReactiveUI;
using SCG.Extensions;
using System.Reactive.Disposables;
using SCG.Controls;
using SCG.Modules.DOS2DE.Core;
using TheArtOfDev.HtmlRenderer.WPF;
using DynamicData.Binding;

namespace SCG.Modules.DOS2DE.Windows
{
	public class LocaleEditorDebugViewModel : ReactiveObject
	{
		private string test = "Count";

		public string Test
		{
			get => test;
			set { this.RaiseAndSetIfChanged(ref test, value); }
		}


		private List<ILocaleKeyEntry> getTestRemovedEntries()
		{
			var list = new List<ILocaleKeyEntry>();
			var parent = new LocaleCustomFileData(null, "Test.lsb");
			for (var i = 0; i < 20; i++)
			{
				list.Add(new LocaleCustomKeyEntry(parent)
				{
					Key = "TestKey" + i,
					Content = "TestContentBlahblahblahblahblahblahblahblahblah",
					Handle = "NoHandle"
				});
			}
			return list;
		}
		public ObservableCollectionExtended<ILocaleKeyEntry> MissingEntries { get; set; }

		public LocaleEditorDebugViewModel()
		{
			MissingEntries = new ObservableCollectionExtended<ILocaleKeyEntry>(getTestRemovedEntries());

			Test = "Count:" + MissingEntries.Count;
		}
	}
	/// <summary>
	/// Interaction logic for LocaleEditorWindow.xaml
	/// </summary>
	public partial class LocaleEditorWindow : ReactiveWindow<LocaleViewModel>
	{
		
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

		public LocaleEditorWindow()
		{
			Init();
		}

		private CompositeDisposable disposables;

		public LocaleEditorWindow(DOS2DEModuleData data)
		{
			ModuleData = data;

			Init();

			this.WhenActivated((d) =>
			{
				KeyDown += LocaleEditorWindow_KeyDown;

				disposables = d;

				ViewModel.OnViewLoaded(this, ModuleData, disposables);

				this.OneWayBind(this.ViewModel, vm => vm.RemoveSelectedMissingEntriesCommand, view => view.ConfirmRemovedEntriesButton.Command).DisposeWith(d);
				this.OneWayBind(this.ViewModel, vm => vm.CloseMissingEntriesCommand, view => view.CancelRemovedEntriesButton.Command).DisposeWith(d);
				this.OneWayBind(this.ViewModel, vm => vm.CopySimpleMissingEntriesCommand, view => view.CopySimpleMissingEntriesButton.Command).DisposeWith(d);
				this.OneWayBind(this.ViewModel, vm => vm.CopyAllMissingEntriesCommand, view => view.CopyAllDataMissingEntriesButton.Command).DisposeWith(d);
				this.OneWayBind(this.ViewModel, vm => vm.MissingEntries, view => view.RemovedEntriesListView.ItemsSource).DisposeWith(d);
				this.OneWayBind(this.ViewModel, vm => vm.MissingEntriesViewVisible, view => view.RemovedEntriesGrid.Visibility).DisposeWith(d);

				ViewModel.PopoutContentCommand = ReactiveCommand.Create(() => PopoutContentWindow(ViewModel.SelectedEntry), ViewModel.CanExecutePopoutContentCommand).DisposeWith(d);

				entryContentPreviewHtmlPanel = (HtmlPanel)this.TryFindResource("EntryContentPreview");

				this.OneWayBind(this.ViewModel, vm => vm.SaveCurrentCommand, view => view.SaveButton.Command).DisposeWith(d);
				this.OneWayBind(this.ViewModel, vm => vm.SaveAllCommand, view => view.SaveAllButton.Command).DisposeWith(d);
				this.OneWayBind(this.ViewModel, vm => vm.AddFileCommand, view => view.AddFileButton.Command).DisposeWith(d);
				this.OneWayBind(this.ViewModel, vm => vm.ImportFileCommand, view => view.ImportFileButton.Command).DisposeWith(d);

				this.OneWayBind(this.ViewModel, vm => vm.SelectedEntryHtmlContent, view => view.EntryContentPreviewHtmlPanel.Text).DisposeWith(d);
			});
		}

		private void LocaleEditorWindow_KeyDown(object sender, KeyEventArgs e)
		{
			if(ViewModel.IsAddingNewFileTab)
			{
				
			}
		}

		private void Init()
		{
			InitializeComponent();

			ExportWindow = new LocaleExportWindow();
			ExportWindow.Hide();

			OptionsWindow = new LocaleOptionsWindow();
			OptionsWindow.Hide();

			ContentWindow = new LocaleContentWindow();
			ContentWindow.Hide();
		}

		public void PopoutContentWindow(ILocaleKeyEntry entry)
		{
			if(ContentWindow.ViewModel == null)
			{
				ContentWindow.ViewModel = new LocaleContentWindowViewModel()
				{
					ContentFontSize = ViewModel.ContentFontSize,
					ContentLightMode = ViewModel.ContentLightMode,
					Entry = entry,
					SelectedColor = ViewModel.SelectedColor
				};
			}
			if (!ContentWindow.IsVisible)
			{
				ContentWindow.Activate();
				ContentWindow.Show();
				ContentWindow.Owner = this;
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

		private bool TryFindName<T>(string name, out T target) where T : FrameworkElement
		{
			var element = this.FindName(name);
			target = (T)element;
			return element != null;
		}

		private bool HasNamedElement(string name)
		{
			var element = this.FindName(name);
			return element != null;
		}

		public T GetElement<T>(string name)
		{
			var element = this.FindName(name);
			return (T)element;
		}

		private HtmlPanel entryContentPreviewHtmlPanel;

		public HtmlPanel EntryContentPreviewHtmlPanel => entryContentPreviewHtmlPanel;

		public void LoadData(LocaleViewModel data)
		{
			ViewModel = data;

			DataContext = ViewModel;
			ExportWindow.ViewModel = ViewModel;
		}

		private struct ViewObservableProperty
		{
			public object View { get; set; }
			public ObservableAsPropertyHelper<string> PropertyHelper { get; set; }
		}

		private List<ViewObservableProperty> selectedTextObservables = new List<ViewObservableProperty>();

		private void EntryContentRichTextBox_Loaded(object sender, RoutedEventArgs e)
		{
			if(sender is RichTextBox richTextBox)
			{
				/* Undo/Redo is disable in via xaml for this box, as otherwise the TextFormatter gets tracked and
				 * outputs the internal undo commands to the history of the key entry. 
				 * Here we're binding the global Undo/Redo as input keys in the RichTextBox.
				*/

				ViewModel.UndoMenuData.RegisterInputBinding(richTextBox.InputBindings);
				ViewModel.RedoMenuData.RegisterInputBinding(richTextBox.InputBindings);
			}
			
			//if (!selectedTextObservables.Any(o => o.View == sender) && sender is RichTextBox richTextBox)
			//{
			//	selectedTextObservables.Add(new ViewObservableProperty {
			//		View = richTextBox,
			//		PropertyHelper = richTextBox.WhenAnyValue(v => v.Selection.Text).ToProperty(ViewModel, "SelectedText")
			//	});
			//	//Log.Here().Activity("Added new RichTextBox ObservableAsPropertyHelper");
			//}
		}

		private void EntryContentRichTextBox_Unloaded(object sender, RoutedEventArgs e)
		{
			if (sender is RichTextBox richTextBox)
			{
				ViewModel.UndoMenuData.UnregisterInputBinding(richTextBox.InputBindings);
				ViewModel.RedoMenuData.UnregisterInputBinding(richTextBox.InputBindings);
			}
			//var vo = selectedTextObservables.FirstOrDefault(x => x.View == sender);
			//if(vo.PropertyHelper != null)
			//{
			//	vo.PropertyHelper.Dispose();
			//	selectedTextObservables.Remove(vo);
			//	//Log.Here().Activity("Removed RichTextBox ObservableAsPropertyHelper");
			//}
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
			ViewModel.MenuData.UnregisterShortcuts(this.InputBindings);
		}

		private void EntryDataGrid_RowFocused(object sender, RoutedEventArgs e)
		{
			//Log.Here().Activity($"EntryDataGrid_RowFocused: {sender} | {e.Source}");
			if(sender is DataGridRow row)
			{
				if(row.DataContext is LocaleNodeKeyEntry localeKeyEntry)
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

			if(lastFocusedContentBox == null && lastFocusedRichTextBox == null)
			{
				ViewModel.ContentFocused = false;
				ViewModel.ContentSelected = false;
			}
		}

		private void EntryContent_SelectionChanged(object sender, RoutedEventArgs e)
		{
			if (sender is Xceed.Wpf.Toolkit.RichTextBox richTextBox)
			{
				ViewModel.ContentSelected = richTextBox.Selection?.Text != string.Empty;
				if(richTextBox.Selection != null)
				{
					ViewModel.SelectedText = richTextBox.Selection.Text;
				}
			}

			if (sender is TextBox textBox)
			{
				ViewModel.ContentSelected = textBox.SelectedText != string.Empty;
				ViewModel.SelectedText = textBox.SelectedText;
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

		private void TextBox_KeyDown_MoveFocus(object sender, KeyEventArgs e)
		{
			if (sender is TextBox tb)
			{
				if (tb.DataContext is ILocaleFileData fileData)
				{
					if (e.Key == Key.Enter || e.Key == Key.Return)
					{
						ViewModel.ConfirmRenaming(fileData);
					}
					else if (e.Key == Key.Escape)
					{
						ViewModel.CancelRenaming(fileData);
					}
				}
				else
				{
					if (e.Key == Key.Enter || e.Key == Key.Return)
					{
						tb.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
					}
				}
			}
		}

		public void FocusGroupTabItem()
		{

		}

		private void FileDataTab_LostFocus(object sender, RoutedEventArgs e)
		{
			if(sender is TabItem tb)
			{
				if(tb.DataContext is ILocaleFileData fileData)
				{
					fileData.IsRenaming = false;
					e.Handled = true;
				}
			}
		}

		private TabControl fileTabControl;

		public void FocusSelectedTab()
		{
			if(fileTabControl != null)
			{
				Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(async () =>
				{
					await Task.Delay(50);

					var tabs = fileTabControl.FindVisualChildren<TabItem>();
					var tab = tabs.FirstOrDefault(x => x.DataContext == ViewModel.SelectedFile);
					if (tab != null)
					{
						tab.BringIntoView();
						tab.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
						foreach (var dg in tab.FindVisualChildren<DataGrid>())
						{
							if (dg.Name == "LocaleEntryDataGrid")
							{
								var tb = dg.FindVisualChildren<TextBox>().FirstOrDefault();
								if (tb != null)
								{
									tb.Focus();
									tb.SelectAll();
								}
								else
								{
									Log.Here().Activity("Couldn't find textbox");
								}
							}
						}
						
					}
				}));
			}
		}

		private void LocaleFileTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (sender is TabControl tc)
			{
				fileTabControl = tc;
			}
		}

		//RemovedEntriesListViewCheckboxHeader
		private void RemovedEntriesListViewCheckboxHeader_Checked(object sender, RoutedEventArgs e)
		{
			if(ViewModel.MissingEntries != null)
			{
				foreach (var entry in ViewModel.MissingEntries)
				{
					entry.Selected = (bool)((CheckBox)sender).IsChecked;
				}
			}
		}

		GridViewColumnHeader _lastHeaderClicked = null;
		ListSortDirection _lastDirection = ListSortDirection.Ascending;

		private void RemovedEntriesListView_Click(object sender, RoutedEventArgs e)
		{
			GridViewColumnHeader headerClicked = e.OriginalSource as GridViewColumnHeader;
			ListSortDirection direction;

			if (headerClicked != null)
			{
				if (headerClicked.Role != GridViewColumnHeaderRole.Padding)
				{
					if (headerClicked != _lastHeaderClicked)
					{
						direction = ListSortDirection.Ascending;
					}
					else
					{
						if (_lastDirection == ListSortDirection.Ascending)
						{
							direction = ListSortDirection.Descending;
						}
						else
						{
							direction = ListSortDirection.Ascending;
						}
					}

					string header = "";

					if (headerClicked.Column.Header is TextBlock textBlock)
					{
						header = textBlock.Text;
					}
					else if (headerClicked.Column.Header is string gridHeader)
					{
						header = gridHeader;
					}

					string sortHeader = header;

					switch(header)
					{
						case "File":
							sortHeader = "Parent.Name";
							break;
						case "Key":
							sortHeader = "EntryKey";
							break;
						case "Content":
							sortHeader = "EntryContent";
							break;
					}

					if(sortHeader != "") RemovedEntriesListView_Sort(sortHeader, direction, sender);

					_lastHeaderClicked = headerClicked;
					_lastDirection = direction;
				}
			}
		}

		private void RemovedEntriesListView_Sort(string sortBy, ListSortDirection direction, object sender)
		{
			ListView lv = sender as ListView;
			ICollectionView dataView =
			  CollectionViewSource.GetDefaultView(lv.ItemsSource);

			dataView.SortDescriptions.Clear();
			SortDescription sd = new SortDescription(sortBy, direction);
			dataView.SortDescriptions.Add(sd);
			dataView.Refresh();
		}

		private void FileDataEntryTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if(e.Key == Key.Enter)
			{
				Keyboard.ClearFocus();
				e.Handled = true;
			}
		}

		private void LostKeyboardFocus_UpdateSource(object sender, KeyboardFocusChangedEventArgs e)
		{
			if (sender is TextBox tb)
			{
				BindingExpression be = tb.GetBindingExpression(TextBox.TextProperty);
				be.UpdateSource();
				Keyboard.ClearFocus();
			}
		}

		public void ResizeEntryKeyColumn()
		{
			foreach (var dg in this.MainTabControl.FindVisualChildren<DataGrid>())
			{
				if (dg.Name == "LocaleEntryDataGrid")
				{
					DataGridColumn col = dg.Columns.Where(x => x.Header.Equals("Key")).FirstOrDefault();
					if (col != null)
					{
						col.Width = 0;
						dg.UpdateLayout();
						col.Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
					}
				}
			}
		}
	}
}
