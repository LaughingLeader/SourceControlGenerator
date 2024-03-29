﻿using SCG.Modules.DOS2DE.Data.View;
using SCG.Modules.DOS2DE.Data.View.Locale;
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
using System.Reactive.Concurrency;
using SCG.Extensions;
using System.Reactive.Disposables;
using SCG.Controls;
using SCG.Modules.DOS2DE.Core;
using TheArtOfDev.HtmlRenderer.WPF;
using DynamicData.Binding;
using SCG.Modules.DOS2DE.LocalizationEditor.Models;
using SCG.Modules.DOS2DE.LocalizationEditor.ViewModels;
using SCG.Modules.DOS2DE.LocalizationEditor.Utilities;

namespace SCG.Modules.DOS2DE.LocalizationEditor.Views
{
	/// <summary>
	/// Interaction logic for LocaleEditorWindow.xaml
	/// </summary>
	public partial class LocaleEditorWindow : ClipboardMonitorWindow, IViewFor<LocaleViewModel>, IActivatableView
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

		private object KeyActionsGrid;

		public LocaleViewModel ViewModel { get; set; }
		object IViewFor.ViewModel
		{
			get => ViewModel;
			set => ViewModel = value as LocaleViewModel;
		}

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

				KeyActionsGrid = this.FindResource("KeyActionsGrid");

				ViewModel.OnViewLoaded(this, ModuleData, disposables);

				this.ClipboardUpdateCommand = ViewModel.OnClipboardChangedCommand;

				ViewModel.PopoutContentCommand = ReactiveCommand.Create(() => PopoutContentWindow(ViewModel.SelectedEntry), ViewModel.CanExecutePopoutContentCommand).DisposeWith(d);

				//this.OneWayBind(this.ViewModel, vm => vm.SelectedEntryHtmlContent, view => view.EntryContentPreviewHtmlPanel.Text).DisposeWith(d);

				//this.OneWayBind(this.ViewModel, vm => vm.RemoveSelectedMissingEntriesCommand, view => view.ConfirmRemovedEntriesButton.Command).DisposeWith(d);
				//this.OneWayBind(this.ViewModel, vm => vm.CloseMissingEntriesCommand, view => view.CancelRemovedEntriesButton.Command).DisposeWith(d);
				//this.OneWayBind(this.ViewModel, vm => vm.CopySimpleMissingEntriesCommand, view => view.CopySimpleMissingEntriesButton.Command).DisposeWith(d);
				//this.OneWayBind(this.ViewModel, vm => vm.CopyAllMissingEntriesCommand, view => view.CopyAllDataMissingEntriesButton.Command).DisposeWith(d);

				//this.OneWayBind(this.ViewModel, vm => vm.MissingEntries, view => view.RemovedEntriesListView.ItemsSource).DisposeWith(d);
				//this.OneWayBind(this.ViewModel, vm => vm.MissingEntriesViewVisible, view => view.RemovedEntriesGrid.Visibility).DisposeWith(d);

				//this.OneWayBind(this.ViewModel, vm => vm.SaveCurrentCommand, view => view.SaveButton.Command).DisposeWith(d);
				//this.OneWayBind(this.ViewModel, vm => vm.SaveAllCommand, view => view.SaveAllButton.Command).DisposeWith(d);
				//this.OneWayBind(this.ViewModel, vm => vm.AddFileCommand, view => view.AddFileButton.Command).DisposeWith(d);
				//this.OneWayBind(this.ViewModel, vm => vm.ImportFileCommand, view => view.ImportFileButton.Command).DisposeWith(d);

				CreateButtonBinding("RemoveSelectedMissingEntriesCommand", ConfirmRemovedEntriesButton);
				CreateButtonBinding("CloseMissingEntriesCommand", CancelRemovedEntriesButton);
				CreateButtonBinding("CopySimpleMissingEntriesCommand", CopySimpleMissingEntriesButton);
				CreateButtonBinding("CopyAllMissingEntriesCommand", CopyAllDataMissingEntriesButton);

				CreateBinding("MissingEntries", RemovedEntriesListView, ListView.ItemsSourceProperty);
				CreateBinding("MissingEntriesViewVisible", RemovedEntriesGrid, Grid.VisibilityProperty);

				CreateButtonBinding("SaveCurrentCommand", SaveButton);
				CreateButtonBinding("SaveAllCommand", SaveAllButton);
				CreateButtonBinding("AddFileCommand", AddFileButton);
				CreateButtonBinding("ImportFileCommand", ImportFileButton);

				var res = this.TryFindResource("EntryContentPreview");
				if (res != null && res is HtmlPanel entryContentPreviewHtmlPanel)
				{
					CreateBinding("SelectedEntryHtmlContent", entryContentPreviewHtmlPanel, HtmlPanel.TextProperty);
				}

				Log.Here().Important("Activated LocaleEditorWindow");

				LoadData();
			});
		}

		private void CreateBinding(string vmProperty, FrameworkElement element, DependencyProperty prop)
		{
			Binding binding = new Binding(vmProperty);
			binding.Source = ViewModel;
			binding.Mode = BindingMode.OneWay;
			element.SetBinding(prop, binding);
		}

		private void CreateButtonBinding(string vmProperty, Button button)
		{
			Binding binding = new Binding(vmProperty);
			binding.Source = ViewModel;
			binding.Mode = BindingMode.OneWay;
			button.SetBinding(Button.CommandProperty, binding);
		}

		private void LocaleEditorWindow_KeyDown(object sender, KeyEventArgs e)
		{
			if (ViewModel.IsAddingNewFileTab)
			{

			}
		}

		public void LoadData()
		{
			ExportWindow.ResetBindings();
			ViewModel.Reload();
		}

		private void Init()
		{
			InitializeComponent();

			ViewModel = new LocaleViewModel();
			DataContext = ViewModel;

			ExportWindow = new LocaleExportWindow();
			ExportWindow.ViewModel = ViewModel;
			ExportWindow.Hide();

			OptionsWindow = new LocaleOptionsWindow();
			OptionsWindow.Hide();

			ContentWindow = new LocaleContentWindow();
			ContentWindow.Hide();
		}

		public void PopoutContentWindow(ILocaleKeyEntry entry)
		{
			if (ContentWindow.ViewModel == null)
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
			if (!OptionsWindow.IsVisible)
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

		private struct ViewObservableProperty
		{
			public object View { get; set; }
			public ObservableAsPropertyHelper<string> PropertyHelper { get; set; }
		}

		private List<ViewObservableProperty> selectedTextObservables = new List<ViewObservableProperty>();

		private void LocaleEntryDataGrid_Loaded(object sender, RoutedEventArgs e)
		{
			if (sender is DataGrid dg)
			{
				dg.InputBindings.Clear();
			}
		}

		private void LocaleEntryDataGrid_Unloaded(object sender, RoutedEventArgs e)
		{

		}

		public void LocaleEntryDataGrid_BuildIndexes()
		{
			if (this.TryFindName<DataGrid>("LocaleEntryDataGrid", out var dg))
			{
				RxApp.TaskpoolScheduler.Schedule(() =>
				{
					int i = 0;
					foreach (var entry in dg.Items)
					{
						if (entry is ILocaleKeyEntry keyEntry)
						{
							i++;
							keyEntry.Index = i;
						}
					}
				});
			}
		}

		private void OnLocaleTextboxGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			if (sender is TextBox tb)
			{
				ViewModel.CurrentTextBox = tb;
				e.Handled = true;
			}
		}

		private void OnLocaleTextboxLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			ViewModel.CurrentTextBox = null;
			e.Handled = true;
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
			if (sender is DataGrid grid)
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
			if (sender is DataGridRow row)
			{
				if (row.DataContext is LocaleNodeKeyEntry localeKeyEntry)
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

			if (lastFocusedContentBox == null && lastFocusedRichTextBox == null)
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
				if (richTextBox.Selection != null)
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
			if (sender is Xceed.Wpf.Toolkit.ColorPicker cp)
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
			if (sender is TabItem tb)
			{
				if (tb.DataContext is ILocaleFileData fileData)
				{
					fileData.IsRenaming = false;

					e.Handled = true;
				}
			}
		}

		private TabControl fileTabControl;

		public void FocusSelectedTab()
		{
			if (fileTabControl != null)
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

		private void LocaleGroupTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (sender is TabControl tc)
			{
				RxApp.MainThreadScheduler.Schedule(TimeSpan.FromMilliseconds(10), _ =>
				{
					if (tc != null && tc.HasItems)
					{
						var fileTabControl = tc.FindVisualChildren<TabControl>().FirstOrDefault();
						if (fileTabControl != null && fileTabControl.Items.Count > 19)
						{
							var tab = fileTabControl.FindVisualChildren<TabItem>().FirstOrDefault(x => x.DataContext == fileTabControl.SelectedItem);
							//var tab = fileTabControl.SelectedValue as TabItem;
							if (tab != null)
							{
								tab.BringIntoView();
							}
						}
					}
				});
			}
		}

		private void LocaleFileTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (sender is TabControl tc)
			{
				fileTabControl = tc;
			}
		}

		private void RemovedEntriesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ViewModel.MissingKeyEntrySelected = ViewModel.MissingEntries.Any(x => x.Selected == true);
		}

		//RemovedEntriesListViewCheckboxHeader
		private void RemovedEntriesListViewCheckboxHeader_Checked(object sender, RoutedEventArgs e)
		{
			if (ViewModel.MissingEntries != null)
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

					switch (header)
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

					if (sortHeader != "") RemovedEntriesListView_Sort(sortHeader, direction, sender);

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

		private void EntryHandleTextBox_GotFocus(object sender, RoutedEventArgs e)
		{
			if (sender is TextBox tb)
			{
				if (tb.Text == LocaleEditorCommands.UnsetHandle || String.IsNullOrWhiteSpace(tb.Text))
				{
					//Delay so the selector can enter the textbox
					RxApp.MainThreadScheduler.Schedule(TimeSpan.FromMilliseconds(50), _ =>
					{
						tb.SelectAll();
					});
				}
			}
		}

		private void LocaleEntryGridContentPresenter_Loaded(object sender, RoutedEventArgs e)
		{
			if (sender is ContentPresenter cp)
			{
				if (cp.Content != KeyActionsGrid)
				{
					cp.Content = KeyActionsGrid;
				}
			}
		}

		public void SetContentToKeyActionsGrid()
		{
			ContentPresenter cp = null;
			if (this.TryFindName("LocaleEntryGridContentPresenter", out cp))
			{
				cp.Content = KeyActionsGrid;
			}
		}

		private void LocaleEntryGridContentPresenter_Unloaded(object sender, RoutedEventArgs e)
		{
			if (sender is ContentPresenter cp)
			{
				cp.Content = null;
			}
		}

		private void LocaleEntryDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			//if (sender is DataGrid dg)
			//{

			//	foreach (var item in dg.SelectedItems)
			//	{
			//		if (item is ILocaleKeyEntry data)
			//		{
			//			data.Selected = true;
			//		}
			//	}
			//}

			foreach (var item in e.AddedItems)
			{
				if (item is ILocaleKeyEntry data)
				{
					data.Selected = true;
				}
			}

			foreach (var item in e.RemovedItems)
			{
				if (item is ILocaleKeyEntry data)
				{
					data.Selected = false;
				}
			}

			var lastEntry = e.AddedItems.OfType<ILocaleKeyEntry>().LastOrDefault();
			if (lastEntry != null)
			{
				ViewModel.SelectedEntry = lastEntry;
			}
		}

		private void FileTabRenamingTextBox_Loaded(object sender, RoutedEventArgs e)
		{
			if (sender is UnfocusableTextBox tb)
			{
				tb.Focus();
				tb.SelectAll();
			}
		}
	}
}
