using System;
using System.Collections.Generic;
using System.Linq;
using SCG.Data;
using System.Collections.ObjectModel;
using System.Windows.Input;
using SCG.Commands;
using SCG.Modules.DOS2DE.Utilities;
using System.Windows;
using SCG.Data.View;
using LSLib.LS;
using SCG.Modules.DOS2DE.Windows;
using SCG.Modules.DOS2DE.Core;
using SCG.Modules.DOS2DE.Data.App;
using Alphaleonis.Win32;
using Alphaleonis.Win32.Filesystem;
using SCG.Core;
using System.ComponentModel;
using SCG.Extensions;
using DynamicData.Binding;
using ReactiveUI;
using System.Reactive;
using System.Windows.Media;
using SCG.Windows;
using System.Threading.Tasks;

namespace SCG.Modules.DOS2DE.Data.View.Locale
{
	public class LocaleViewModel : HistoryBaseViewModel
	{
		private Dictionary<string, List<Action>> MenuEnabledLinks = new Dictionary<string, List<Action>>();

		private LocaleEditorWindow view;

		private void LocaleViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName != null)
			{
				if (MenuEnabledLinks.ContainsKey(e.PropertyName))
				{
					foreach (var setEnabledAction in MenuEnabledLinks[e.PropertyName])
					{
						setEnabledAction.Invoke();
					}
				}
			}
		}

		public LocaleEditorSettingsData Settings { get; set; }

		public DOS2DEModuleData ModuleData { get; set; }

		public List<ModProjectData> LinkedProjects { get; set; } = new List<ModProjectData>();

		private LocaleMenuData menuData;

		public LocaleMenuData MenuData
		{
			get => menuData;
			set
			{
				this.RaiseAndSetIfChanged(ref menuData, value);
			}
		}

		private bool changesUnsaved = false;

		public bool ChangesUnsaved
		{
			get => changesUnsaved;
			set
			{
				if (this.RaiseAndSetIfChanged(ref changesUnsaved, value) == true)
				{
					WindowTitle = "*Localization Editor";
				}
				else
				{
					WindowTitle = "Localization Editor";
				}
			}
		}

		private string windowTitle = "Localization Editor";

		public string WindowTitle
		{
			get => windowTitle;
			private set
			{
				this.RaiseAndSetIfChanged(ref windowTitle, value);
			}
		}

		public ObservableCollectionExtended<LocaleTabGroup> Groups { get; set; }

		private int selectedGroupIndex = -1;

		public int SelectedGroupIndex
		{
			get => selectedGroupIndex;
			set
			{
				bool updateCanSave = selectedGroupIndex != value;
				
				this.RaiseAndSetIfChanged(ref selectedGroupIndex, value);
				this.RaisePropertyChanged("SelectedGroup");
				this.RaisePropertyChanged("SelectedItem");
				this.RaisePropertyChanged("CurrentImportPath");

				if (updateCanSave)
				{
					SelectedFileChanged(Groups[selectedGroupIndex], Groups[selectedGroupIndex].SelectedFile);
				}
			}
		}

		private bool isAddingNewFileTab = false;

		public bool IsAddingNewFileTab
		{
			get => isAddingNewFileTab;
			set { this.RaiseAndSetIfChanged(ref isAddingNewFileTab, value); }
		}

		private string newFileTabName;

		public string NewFileTabName
		{
			get => newFileTabName;
			set { this.RaiseAndSetIfChanged(ref newFileTabName, value); }
		}

		private int newFileTargetProjectIndex = 0;

		public int NewFileTargetProjectIndex
		{
			get => newFileTargetProjectIndex;
			set { this.RaiseAndSetIfChanged(ref newFileTargetProjectIndex, value); }
		}

		private LocaleTabGroup newFileTabTargetGroup;

		private LocaleTabGroup modsGroup;

		public LocaleTabGroup ModsGroup
		{
			get => modsGroup;
			set
			{
				this.RaiseAndSetIfChanged(ref modsGroup, value);
			}
		}

		private LocaleTabGroup dialogGroup;

		public LocaleTabGroup DialogGroup
		{
			get => dialogGroup;
			set
			{
				this.RaiseAndSetIfChanged(ref dialogGroup, value);
			}
		}

		private LocaleTabGroup publicGroup;

		public LocaleTabGroup PublicGroup
		{
			get => publicGroup;
			set
			{
				this.RaiseAndSetIfChanged(ref publicGroup, value);
			}
		}
		private CustomLocaleTabGroup customGroup;

		public CustomLocaleTabGroup CustomGroup
		{
			get => customGroup;
			set
			{
				this.RaiseAndSetIfChanged(ref customGroup, value);
			}
		}

		private List<LocaleTabGroup> GetCoreGroups()
		{
			return new List<LocaleTabGroup>() { PublicGroup, ModsGroup, DialogGroup, CustomGroup };
		}

		private LocaleTabGroup combinedGroup;

		public LocaleTabGroup CombinedGroup
		{
			get => combinedGroup;
			private set
			{
				this.RaiseAndSetIfChanged(ref combinedGroup, value);
			}
		}

		public LocaleTabGroup SelectedGroup
		{
			get
			{
				return SelectedGroupIndex > -1 ? Groups[SelectedGroupIndex] : null;
			}
		}

		private ILocaleKeyEntry selectedEntry;

		public ILocaleKeyEntry SelectedEntry
		{
			get => selectedEntry;
			set
			{
				this.RaiseAndSetIfChanged(ref selectedEntry, value);
				this.RaisePropertyChanged("SelectedEntryContent");
				this.RaisePropertyChanged("SelectedEntryHtmlContent");
			}
		}

		private ILocaleFileData _selectedItem = null;

		public ILocaleFileData SelectedItem => _selectedItem;

		public string SelectedEntryContent
		{
			get
			{
				return SelectedEntry?.EntryContent;
			}
			set
			{
				if(SelectedEntry != null)
				{
					SelectedEntry.EntryContent = value;
				}
			}
		}

		public string SelectedEntryHtmlContent
		{
			get
			{
				return $"<body>{SelectedEntry?.EntryContent}</body>";
			}
		}

		private bool canSave;

		public bool CanSave
		{
			get => canSave;
			set
			{
				this.RaiseAndSetIfChanged(ref canSave, value);
				SaveCurrentMenuData.IsEnabled = canSave;
			}
		}

		private bool canAddFile;

		public bool CanAddFile
		{
			get => canAddFile;
			set
			{
				this.RaiseAndSetIfChanged(ref canAddFile, value);
			}
		}

		private bool canAddKeys;

		public bool CanAddKeys
		{
			get => canAddKeys;
			set
			{
				this.RaiseAndSetIfChanged(ref canAddKeys, value);
			}
		}

		private bool anySelected;

		public bool AnySelected
		{
			get => anySelected;
			set
			{
				this.RaiseAndSetIfChanged(ref anySelected, value);
			}
		}

		private bool contentSelected = false;

		public bool ContentSelected
		{
			get => contentSelected;
			set
			{
				this.RaiseAndSetIfChanged(ref contentSelected, value);
			}
		}

		private bool contentFocused = false;

		public bool ContentFocused
		{
			get => contentFocused;
			set
			{
				this.RaiseAndSetIfChanged(ref contentFocused, value);
			}
		}

		private bool contentPreviewModeEnabled = false;

		public bool ContentPreviewModeEnabled
		{
			get => contentPreviewModeEnabled;
			set
			{
				this.RaiseAndSetIfChanged(ref contentPreviewModeEnabled, value);
				this.RaisePropertyChanged("CanEditContentPreview");
			}
		}

		public bool CanEditContentPreview => !ContentPreviewModeEnabled;

		private bool contentLightMode = true;

		public bool ContentLightMode
		{
			get => contentLightMode;
			set
			{
				this.RaiseAndSetIfChanged(ref contentLightMode, value);
			}
		}

		private int contentFontSize = 12;

		public int ContentFontSize
		{
			get => contentFontSize;
			set
			{
				this.RaiseAndSetIfChanged(ref contentFontSize, value);
			}
		}

		private System.Windows.Media.Color? selectedColor;

		public System.Windows.Media.Color? SelectedColor
		{
			get => selectedColor;
			set
			{
				this.RaiseAndSetIfChanged(ref selectedColor, value);
			}
		}

		private string selectedText = "";

		public string SelectedText
		{
			get => selectedText;
			set
			{
				this.RaiseAndSetIfChanged(ref selectedText, value);
			}
		}

		public void UpdateAnySelected(bool recentSelection = false)
		{
			if(recentSelection)
			{
				AnySelected = true;
			}
			else
			{
				AnySelected = SelectedGroup?.Tabs.Any(t => t.Entries.Any(e => e.Selected)) == true;
			}
		}

		private void SelectedFileChanged(LocaleTabGroup group, ILocaleFileData keyFileData)
		{
			if (keyFileData != null)
			{
				CanSave = (group.CombinedEntries != keyFileData) || group.DataFiles.Count == 1;
				//Log.Here().Activity($"Selected file changed to {group.Name} | {keyFileData.Name}");
			}
			else
			{
				CanSave = false;
			}

			CanAddFile = group != CombinedGroup && group != DialogGroup;
			CanAddKeys = SelectedGroup != null && SelectedGroup.SelectedFile != null && !SelectedGroup.SelectedFile.Locked;

			//if (group == DialogGroup)
			//{
			//	DOS2DETooltips.Button_Locale_ImportDisabled = "Import Disabled for Dialog";
			//	DOS2DETooltips.TooltipChanged("Button_Locale_ImportDisabled");
			//}
			//else
			//{
			//	DOS2DETooltips.Button_Locale_ImportDisabled = "Import Disabled";
			//	DOS2DETooltips.TooltipChanged("Button_Locale_ImportDisabled");
			//}
			//Log.Here().Activity($"Can add file: {CanAddFile}");
		}

		private string outputDate;

		public string OutputDate
		{
			get => outputDate;
			set
			{
				this.RaiseAndSetIfChanged(ref outputDate, value);
			}
		}

		private string outputText;

		public string OutputText
		{
			get => outputText;
			set
			{
				this.RaiseAndSetIfChanged(ref outputText, value);
			}
		}

		private LogType outputType;

		public LogType OutputType
		{
			get => outputType;
			set
			{
				this.RaiseAndSetIfChanged(ref outputType, value);
			}
		}

		private bool MultipleGroupsEntriesFilled()
		{
			int total = 0;
			if (ModsGroup?.DataFiles?.Count > 0) total += 1;
			if (PublicGroup?.DataFiles?.Count > 0) total += 1;
			if (DialogGroup?.DataFiles?.Count > 0) total += 1;
			return total > 1;
		}

		public int TotalFiles
		{
			get
			{
				var count = CombinedGroup?.CombinedEntries.Entries?.Count;
				return count != null ? count.Value : 0;
			}
		}

		public string CurrentImportPath
		{
			get
			{
				if (SelectedItem != null && SelectedItem is LocaleNodeFileData data)
				{
					return Path.GetDirectoryName(data.SourcePath);
				}
				else if (SelectedGroup != null)
				{
					return SelectedGroup.SourceDirectories.FirstOrDefault();
				}
				return Directory.GetCurrentDirectory();
			}
		}

		public string CurrentFileImportPath
		{
			get
			{
				if (Settings != null && Directory.Exists(Settings.LastFileImportPath))
				{
					return Settings.LastFileImportPath;
				}
				else
				{
					Log.Here().Activity($"Setting: {Settings} | {Settings?.LastFileImportPath}");
				}
				return CurrentImportPath;
			}
		}

		public string CurrentEntryImportPath
		{
			get
			{
				if (Settings != null && Directory.Exists(Settings.LastEntryImportPath))
				{
					return Settings.LastEntryImportPath;
				}
				return CurrentImportPath;
			}
		}


		public void GenerateHandles()
		{
			if (SelectedGroup != null && SelectedGroup.SelectedFile != null)
			{
				Log.Here().Activity("Generating handles...");

				List<LocaleHandleHistory> lastHandles = new List<LocaleHandleHistory>();
				List<LocaleHandleHistory> newHandles = new List<LocaleHandleHistory>();

				foreach (var entry in SelectedGroup.SelectedFile.Entries.Where(e => e.Selected))
				{
					if (entry.Handle.Equals("ls::TranslatedStringRepository::s_HandleUnknown", StringComparison.OrdinalIgnoreCase))
					{
						lastHandles.Add(new LocaleHandleHistory(entry, entry.Handle));
						entry.Handle = LocaleEditorCommands.CreateHandle();
						newHandles.Add(new LocaleHandleHistory(entry, entry.Handle));
						Log.Here().Activity($"[{entry.Key}] New handle generated. [{entry.Handle}]");
					}
				}

				CreateSnapshot(() => {
					foreach(var e in lastHandles)
					{
						e.Key.Handle = e.Handle;
					}
				}, () => {
					foreach (var e in newHandles)
					{
						e.Key.Handle = e.Handle;
					}
				});
			}
			else
			{
				Log.Here().Activity("No selected file found. Skipping handle generation.");
			}
		}

		public void UpdateCombinedGroup(bool updateCombinedEntries = false)
		{
			CombinedGroup.DataFiles = new ObservableCollectionExtended<ILocaleFileData>();
			CombinedGroup.DataFiles.AddRange(ModsGroup.DataFiles);
			CombinedGroup.DataFiles.AddRange(PublicGroup.DataFiles);
			CombinedGroup.DataFiles.AddRange(DialogGroup.DataFiles);
			CombinedGroup.DataFiles.AddRange(CustomGroup.DataFiles);
			CombinedGroup.Visibility = MultipleGroupsEntriesFilled();

			if (!CombinedGroup.Visibility)
			{
				if (PublicGroup.Visibility)
				{
					SelectedGroupIndex = 1;
				}
				else if (ModsGroup.Visibility)
				{
					SelectedGroupIndex = 2;
				}
				else if (DialogGroup.Visibility)
				{
					SelectedGroupIndex = 3;
				}
				else
				{
					SelectedGroupIndex = 0;
				}
			}
			else
			{
				SelectedGroupIndex = 0;
			}

			this.RaisePropertyChanged("CombinedGroup");
			this.RaisePropertyChanged("Groups");

			//Log.Here().Activity($"Setting selected group index to '{SelectedGroupIndex}' {CombinedGroup.Visibility}.");

			if (updateCombinedEntries)
			{
				foreach (var g in Groups)
				{
					g.UpdateCombinedData();
				}
			}
		}

		public void SaveAll()
		{
			Log.Here().Activity("Saving all files.");
			var backupSuccess = LocaleEditorCommands.BackupDataFiles(this, ModuleData.Settings.BackupRootDirectory);
			if (backupSuccess.Result == true)
			{
				var successes = LocaleEditorCommands.SaveDataFiles(this);
				Log.Here().Important($"Saved {successes} localization files.");
				OutputText = $"Saved {successes}/{TotalFiles} files.";
				OutputType = LogType.Important;
			}
			else
			{
				OutputText = $"Problem occured when backing up files. Check the log.";
				OutputType = LogType.Error;
			}
			OutputDate = DateTime.Now.ToShortTimeString();
		}

		public void SaveCurrent()
		{
			if(SelectedGroup != null)
			{
				if (SelectedGroup.SelectedFile != null && SelectedGroup.SelectedFile is LocaleNodeFileData keyFileData)
				{
					var result = LocaleEditorCommands.SaveDataFile(keyFileData);
					if(result > 0)
					{
						OutputText = $"Saved '{keyFileData.SourcePath}'";
						OutputType = LogType.Important;
					}
					else
					{
						OutputText = $"Error saving '{keyFileData.SourcePath}'. Check the log.";
						OutputType = LogType.Error;
					}
					OutputDate = DateTime.Now.ToShortTimeString();

					keyFileData.ChangesUnsaved = false;

					ChangesUnsaved = false;
				}
			}
		}

		private bool NameExistsInData(string name)
		{
			return SelectedGroup?.DataFiles.Any(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) == true;
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

		public void AddFileToSelectedGroup()
		{
			if (SelectedGroup != null)
			{
				var sourceRoot = "";
				if (SelectedGroup.DataFiles.First() is LocaleNodeFileData keyFileData)
				{
					sourceRoot = Path.GetDirectoryName(keyFileData.SourcePath) + @"\";
				}
				else
				{
					if (SelectedGroup == PublicGroup)
					{
						sourceRoot = Path.Combine(ModuleData.Settings.DOS2DEDataDirectory, "Public");
					}
					else if (SelectedGroup == ModsGroup)
					{
						sourceRoot = Path.Combine(ModuleData.Settings.DOS2DEDataDirectory, "Mods");
					}
				}

				string newFileName = GetNewFileName(sourceRoot, "NewFile");

				FileCommands.Save.OpenDialog(this.view, "Create Localization File...", sourceRoot, (string savePath) => {
					var fileData = LocaleEditorCommands.CreateFileData(savePath, Path.GetFileName(savePath));
					SelectedGroup.DataFiles.Add(fileData);
					SelectedGroup.UpdateCombinedData();
					SelectedGroup.SelectedFileIndex = SelectedGroup.Tabs.Count - 1;
				}, newFileName, "Larian Localization File (*.lsb)|*.lsb");
			}
		}

		public void ImportFileAsFileData(IEnumerable<string> files)
		{
			var currentGroup = Groups.Where(g => g == SelectedGroup).First();
			var newFileDataList = LocaleEditorCommands.ImportFilesAsData(files, SelectedGroup);

			CreateSnapshot(() => {
				var list = currentGroup.DataFiles.ToList();
				foreach(var entry in newFileDataList)
				{
					if (list.Contains(entry)) list.Remove(entry);
				}
				currentGroup.DataFiles = new ObservableCollectionExtended<ILocaleFileData>(list);
			}, () => {
				currentGroup.DataFiles.AddRange(newFileDataList);
			});

			SelectedGroup.DataFiles.AddRange(newFileDataList);

			SelectedGroup.ChangesUnsaved = true;
			SelectedGroup.UpdateCombinedData();
			SelectedGroup.SelectLast();

			if(files.Count() > 0)
			{
				Settings.LastFileImportPath = Path.GetDirectoryName(files.FirstOrDefault());
				this.RaisePropertyChanged("CurrentImportPath");
				this.RaisePropertyChanged("CurrentFileImportPath");
			}
			LocaleEditorWindow.instance.SaveSettings();

			ChangesUnsaved = true;
		}

		public void ImportFileAsKeys(IEnumerable<string> files)
		{
			Log.Here().Activity("Importing keys from files...");

			var currentItem = SelectedItem;
			var newKeys = LocaleEditorCommands.ImportFilesAsEntries(files, SelectedItem as LocaleNodeFileData);

			CreateSnapshot(() => {
				foreach(var k in newKeys)
				{
					currentItem.Entries.Remove(k);
				}
			}, () => {
				foreach (var k in newKeys)
				{
					currentItem.Entries.Add(k);
				}
			});

			foreach (var k in newKeys)
			{
				SelectedItem.Entries.Add(k);
			}

			SelectedItem.ChangesUnsaved = true;
			SelectedGroup.UpdateCombinedData();

			if (files.Count() > 0)
			{
				Settings.LastEntryImportPath = Path.GetDirectoryName(files.FirstOrDefault());
				this.RaisePropertyChanged("CurrentImportPath");
				this.RaisePropertyChanged("CurrentEntryImportPath");
			}
			LocaleEditorWindow.instance.SaveSettings();

			ChangesUnsaved = true;
		}

		private MenuData SaveCurrentMenuData { get; set; }
		//private MenuData SelectAllMenuData { get; set; }
		//private MenuData SelectNoneMenuData { get; set; }
		//private MenuData GenerateHandlesMenuData { get; set; }

		public ICommand AddFileCommand { get; private set; }
		public ICommand AddFileToGroupCommand { get; private set; }
		public ICommand ConfirmFileAddToGroupCommand { get; private set; }
		public ICommand CancelFileAddToGroupCommand { get; private set; }
		public ICommand CloseFileCommand { get; private set; }
		public OpenFileBrowserCommand ImportFileCommand { get; private set; }
		public OpenFileBrowserCommand ImportKeysCommand { get; private set; }
		public ICommand ExportXMLCommand { get; private set; }

		public ICommand SaveAllCommand { get; private set; }
		public ICommand SaveCurrentCommand { get; private set; }
		public ICommand GenerateHandlesCommand { get; private set; }

		public ICommand AddNewKeyCommand { get; private set; }
		public TaskCommand DeleteKeysCommand { get; private set; }

		public ICommand OpenPreferencesCommand { get; private set; }

		//public ICommand ExpandContentCommand { get; set; }

		public ICommand AddFontTagCommand { get; private set; }

		// Content Context Menu
		public ICommand ToggleContentLightModeCommand { get; private set; }
		public ICommand ChangeContentFontSizeCommand { get; private set; }
		public ICommand PopoutContentCommand { get; set; }
		public ICommand CopyToClipboardCommand { get; private set; }
		public ICommand ToggleRenameFileTabCommand { get; private set; }
		public ICommand CancelRenamingFileTabCommand { get; private set; }

		public IObservable<bool> CanExecutePopoutContentCommand { get; private set; }
		public IObservable<bool> GlobalCommandEnabled { get; private set; }

		public void AddNewKey()
		{
			if (SelectedGroup != null && SelectedGroup.SelectedFile != null)
			{
				if (SelectedGroup.SelectedFile is LocaleNodeFileData fileData && fileData.Format == LSLib.LS.Enums.ResourceFormat.LSB)
				{
					LocaleNodeKeyEntry localeEntry = LocaleEditorCommands.CreateNewLocaleEntry(fileData);
					localeEntry.SetHistoryFromObject(this);
					AddWithHistory(fileData.Entries, localeEntry);
					SelectedGroup.UpdateCombinedData();
				}
				else if(SelectedGroup.SelectedFile is LocaleCustomFileData customFileData)
				{
					LocaleCustomKeyEntry localeEntry = new LocaleCustomKeyEntry
					{
						Handle = Guid.NewGuid().ToString().Replace('-', 'g').Insert(0, "h"),
						Key = "NewKey" + customFileData.Entries.Count + 1,
						Content = ""
					};
					localeEntry.SetHistoryFromObject(this);
					AddWithHistory(customFileData.Entries, localeEntry);
					SelectedGroup.UpdateCombinedData();
					customFileData.Entries.Add(localeEntry);
				}
			}
			else
			{
				Log.Here().Activity("No selected file found. Skipping key generation.");
			}
		}

		public void DeleteSelectedKeys(bool confirm)
		{
			if (confirm)
			{
				if (SelectedGroup != null && SelectedGroup.SelectedFile != null)
				{
					foreach (var entry in SelectedGroup.SelectedFile.Entries.Where(e => e.Selected).ToList())
					{
						RemoveWithHistory(SelectedGroup.SelectedFile.Entries, entry);
					}

					SelectedGroup.UpdateCombinedData();
				}
				else
				{
					Log.Here().Activity("No selected file(s) found. Skipping delete operation.");
				}
			}
		}

		private string exportText = "";

		public string ExportText
		{
			get => exportText;
			set
			{
				this.RaiseAndSetIfChanged(ref exportText, value);
			}
		}

		public void OpenExportWindow()
		{
			ExportText = LocaleEditorCommands.ExportDataAsXML(this, Settings.ExportSource, Settings.ExportKeys);

			if (LocaleEditorWindow.instance != null && LocaleEditorWindow.instance.ExportWindow != null)
			{
				if (!LocaleEditorWindow.instance.ExportWindow.IsVisible)
				{
					LocaleEditorWindow.instance.ExportWindow.Show();
					LocaleEditorWindow.instance.ExportWindow.Owner = LocaleEditorWindow.instance;
				}
			}
		}

		public void AddCustomFileToGroup(LocaleTabGroup group)
		{
			if(!IsAddingNewFileTab)
			{
				string nextName = "Custom" + group.DataFiles.Count + 1;
				NewFileTabName = nextName;
				IsAddingNewFileTab = true;
				newFileTabTargetGroup = group;
			}
			
			//LocaleCustomFileData data = new LocaleCustomFileData(nextName);
		}

		public void ConfirmCustomFileAddToGroup()
		{
			if (IsAddingNewFileTab)
			{
				ModProjectData targetProject = LinkedProjects[NewFileTargetProjectIndex];
				LocaleCustomFileData data = new LocaleCustomFileData(NewFileTabName) { Project = targetProject };
				newFileTabTargetGroup.DataFiles.Add(data);
				newFileTabTargetGroup.UpdateCombinedData();
				IsAddingNewFileTab = false;
			}
		}

		public void CloseFileInGroup(ILocaleFileData fileData)
		{
			LocaleTabGroup target = GetCoreGroups().FirstOrDefault(g => g.DataFiles.Contains(fileData));

			if (target != null)
			{
				FileCommands.OpenConfirmationDialog(LocaleEditorWindow.instance, "Confirm Tab Removal", $"Remove {fileData.Name}?", "Unsaved changes will be lost.", new Action<bool>((b) =>
				{
					if (b)
					{
						target.DataFiles.Remove(fileData);
						target.UpdateCombinedData();
					}
				}));
			}
		}

		public void AddFontTag()
		{
			//We get/set Content here instead of EntryContent so we don't create double histories.
			if (SelectedEntry != null && SelectedText != string.Empty)
			{
				var lastSelected = SelectedEntry;
				var lastText = SelectedEntry.Content;

				void undo()
				{
					if(lastSelected != null)
					{
						lastSelected.Content = lastText;

						if(lastSelected == SelectedEntry)
						{
							this.RaisePropertyChanged("SelectedEntryContent");
							this.RaisePropertyChanged("SelectedEntryHtmlContent");
						}
					}
				}

				void redo()
				{
					string color = SelectedColor == null ? "#FFFFFF" : SelectedColor.Value.ToHexString();

					int start = SelectedEntry.Content.IndexOf(SelectedText);
					if (start <= -1) start = 0;

					string text = SelectedEntry.Content;
					string fontStartText = $"<font color='{color}'>";
					text = text.Insert(start, fontStartText);

					int end = start + fontStartText.Length + SelectedText.Length;
					if (end >= text.Length - 1)
					{
						text = text += @"</font>";
					}
					else
					{
						text = text.Insert(end, @"</font>");
					}

					SelectedEntry.Content = text;
					this.RaisePropertyChanged("SelectedEntryContent");
					this.RaisePropertyChanged("SelectedEntryHtmlContent");

					//Log.Here().Activity($"Content box text set to: {text} | Start {start} End {end} Selected {SelectedText}");
				}

				CreateSnapshot(undo, redo);
				redo();
			}
		}

		/// <summary>
		/// Creates a new MenuData whose IsEnabled property is linked to a property in this view data.
		/// When the property in this class changes, the linked menudata's IsEnabled will update accordingly.
		/// </summary>
		/// <param name="setEnabled"></param>
		/// <param name="PropertyName"></param>
		/// <param name="MenuID"></param>
		/// <param name="menuName"></param>
		/// <param name="command"></param>
		/// <param name="shortcutKey"></param>
		/// <param name="shortcutModifiers"></param>
		/// <returns></returns>
		private MenuData CreateMenuDataWithLink(Func<bool>setEnabled, string PropertyName, string MenuID, string menuName, ICommand command = null,
			Key? shortcutKey = null, ModifierKeys? shortcutModifiers = null)
		{
			var mdata = new MenuData(MenuID, menuName, command, shortcutKey, shortcutModifiers);

			if(!MenuEnabledLinks.ContainsKey(PropertyName))
			{
				MenuEnabledLinks.Add(PropertyName, new List<Action>());
			}

			MenuEnabledLinks[PropertyName].Add(() => {
				mdata.IsEnabled = setEnabled();
				//Log.Here().Activity($"Set IsEnabled for menu entry {mdata.ID} to {mdata.IsEnabled}");
			});
			return mdata;
		}

		public void OnViewLoaded(LocaleEditorWindow v, DOS2DEModuleData moduleData)
		{
			view = v;

			ModuleData = moduleData;
			LocaleEditorCommands.LoadSettings(ModuleData, this);
			MenuData.RegisterShortcuts(view.InputBindings);

			UpdateCombinedGroup(true);
		}

		public MenuData UndoMenuData { get; private set; }
		public MenuData RedoMenuData { get; private set; }

		public LocaleViewModel() : base()
		{
			this.PropertyChanged += LocaleViewModel_PropertyChanged;
			MenuData = new LocaleMenuData();

			CombinedGroup = new LocaleTabGroup("All");
			ModsGroup = new LocaleTabGroup("Locale (Mods)");
			PublicGroup = new LocaleTabGroup("Locale (Public)");
			DialogGroup = new LocaleTabGroup("Dialog");
			CustomGroup = new CustomLocaleTabGroup("Custom");

			Groups = new ObservableCollectionExtended<LocaleTabGroup>
			{
				CombinedGroup,
				ModsGroup,
				PublicGroup,
				DialogGroup,
				CustomGroup
			};

			foreach (var g in Groups)
			{
				g.SelectedFileChanged = SelectedFileChanged;
			}

			GlobalCommandEnabled = this.WhenAny(vm => vm.IsAddingNewFileTab, e => e.Value == false);

			ExportXMLCommand = ReactiveCommand.Create(OpenExportWindow, GlobalCommandEnabled);

			ImportFileCommand = new OpenFileBrowserCommand(ImportFileAsFileData)
			{
				DefaultParams = new OpenFileBrowserParams()
				{
					Title = DOS2DETooltips.Button_Locale_ImportFile,
					ParentWindow = LocaleEditorWindow.instance,
					UseFolderBrowser = false,
					Filters = DOS2DEFileFilters.AllLocaleFilesList.ToArray(),
					StartDirectory = CurrentFileImportPath
				}
			};

			GlobalCommandEnabled.BindTo(ImportFileCommand, c => c.Enabled);

			ImportKeysCommand = new OpenFileBrowserCommand(ImportFileAsKeys)
			{
				DefaultParams = new OpenFileBrowserParams()
				{
					Title = DOS2DETooltips.Button_Locale_ImportKeys,
					ParentWindow = LocaleEditorWindow.instance,
					UseFolderBrowser = false,
					Filters = DOS2DEFileFilters.AllLocaleFilesList.ToArray(),
					StartDirectory = CurrentEntryImportPath
				}
			};

			GlobalCommandEnabled.BindTo(ImportKeysCommand, c => c.Enabled);

			AddFileToGroupCommand = ReactiveCommand.Create<CustomLocaleTabGroup>(AddCustomFileToGroup);

			var canConfirmAddFile = this.WhenAny(vm => vm.NewFileTabName, e => !String.IsNullOrWhiteSpace(e.Value));
			ConfirmFileAddToGroupCommand = ReactiveCommand.Create(ConfirmCustomFileAddToGroup, canConfirmAddFile);
			CancelFileAddToGroupCommand = ReactiveCommand.Create(() =>
			{
				if(IsAddingNewFileTab)
				{
					IsAddingNewFileTab = false;
					NewFileTabName = "";
					newFileTabTargetGroup = null;
				}
			});

			CloseFileCommand = ReactiveCommand.Create<ILocaleFileData>(CloseFileInGroup, GlobalCommandEnabled);
			ToggleRenameFileTabCommand = ReactiveCommand.Create<ILocaleFileData>((ILocaleFileData fileData) => {
				fileData.IsRenaming = !fileData.IsRenaming;
			}, GlobalCommandEnabled);

			async Task<Unit> cancelRenamingFileTab(ILocaleFileData fileData)
			{
				await Task.Delay(50);
				if (fileData.IsRenaming)
				{
					fileData.RenameText = fileData.Name;
					fileData.IsRenaming = false;
				}
				return Unit.Default;
			}

			CancelRenamingFileTabCommand = ReactiveCommand.CreateFromTask<ILocaleFileData>(cancelRenamingFileTab);

			SaveAllCommand = ReactiveCommand.Create(SaveAll, GlobalCommandEnabled);
			SaveCurrentCommand = ReactiveCommand.Create(SaveCurrent, GlobalCommandEnabled);
			GenerateHandlesCommand = ReactiveCommand.Create(GenerateHandles, GlobalCommandEnabled);
			AddNewKeyCommand = ReactiveCommand.Create(AddNewKey, GlobalCommandEnabled);
			DeleteKeysCommand = new TaskCommand(DeleteSelectedKeys, LocaleEditorWindow.instance, "Delete Keys", 
				"Delete selected keys?", "Changes will be lost.");

			GlobalCommandEnabled.BindTo(DeleteKeysCommand, c => c.Enabled);

			OpenPreferencesCommand = ReactiveCommand.Create(() => { LocaleEditorWindow.instance?.TogglePreferencesWindow(); }, GlobalCommandEnabled);

			AddFontTagCommand = ReactiveCommand.Create(AddFontTag, GlobalCommandEnabled);

			ToggleContentLightModeCommand = ReactiveCommand.Create(() => ContentLightMode = !ContentLightMode, GlobalCommandEnabled);
			ChangeContentFontSizeCommand = ReactiveCommand.Create<string>((fontSizeStr) => {
				this.RaisePropertyChanging("ContentFontSize");
				if (int.TryParse(fontSizeStr, out contentFontSize))
				{
					this.RaisePropertyChanged("ContentFontSize");
				}
			}, GlobalCommandEnabled);

			CopyToClipboardCommand = ReactiveCommand.Create<string>((str) =>
			{
				if (str != String.Empty)
				{
					string current = Clipboard.GetText(TextDataFormat.Text);
					void undo()
					{
						Clipboard.SetText(current);

						AppController.Main.SetFooter($"Reverted clipboard text to '{current}'.", LogType.Important);
					};
					void redo()
					{
						Clipboard.SetText(str, TextDataFormat.Text);

						AppController.Main.SetFooter($"Copied text '{str}' to clipboard.", LogType.Activity);
					}

					CreateSnapshot(undo, redo);
					redo();
				}
			}, GlobalCommandEnabled);

			SaveCurrentMenuData = new MenuData("SaveCurrent", "Save", SaveCurrentCommand, Key.S, ModifierKeys.Control);

			MenuData.File.Add(SaveCurrentMenuData);
			MenuData.File.Add(new MenuData("File.SaveAll", "Save All", SaveAllCommand, Key.S, ModifierKeys.Control | ModifierKeys.Shift));
			
			MenuData.File.Add(CreateMenuDataWithLink(() => CanAddFile, "CanAddFile", "File.ImportFile",
				"Import File", ImportFileCommand));

			MenuData.File.Add(CreateMenuDataWithLink(() => CanAddKeys, "CanAddKeys", "File.ImportKeys",
				"Import File as Keys", ImportKeysCommand));

			MenuData.File.Add(CreateMenuDataWithLink(() => AnySelected, "AnySelected", "File.ExportSelected",
				DOS2DETooltips.Button_Locale_ExportToXML, ExportXMLCommand, Key.E, ModifierKeys.Control | ModifierKeys.Shift));

			UndoMenuData = new MenuData("Edit.Undo", "Undo", UndoCommand, Key.Z, ModifierKeys.Control);
			RedoMenuData = new MenuData("Edit.Redo", "Redo", RedoCommand,
					new MenuShortcutInputBinding(Key.Z, ModifierKeys.Control | ModifierKeys.Shift),
					new MenuShortcutInputBinding(Key.Y, ModifierKeys.Control)
				);
			MenuData.Edit.Add(UndoMenuData);
			MenuData.Edit.Add(RedoMenuData);

			MenuData.Edit.Add(CreateMenuDataWithLink(() => SelectedItem != null, "SelectedItem", "Edit.SelectAll", 
				"Select All", ReactiveCommand.Create(() => { SelectedItem?.SelectAll(); }), Key.A, ModifierKeys.Control));

			MenuData.Edit.Add(CreateMenuDataWithLink(() => SelectedItem != null, "SelectedItem", "Edit.SelectNone", 
				"Select None", ReactiveCommand.Create(() => { SelectedItem?.SelectNone(); }), Key.D, ModifierKeys.Control));

			MenuData.Edit.Add(CreateMenuDataWithLink(() => SelectedItem != null, "SelectedItem", "Edit.GenerateHandles", 
				"Generate Handles for Selected", ReactiveCommand.Create(GenerateHandles), Key.G, ModifierKeys.Control | ModifierKeys.Shift));

			MenuData.Edit.Add(CreateMenuDataWithLink(() => CanAddKeys, "CanAddKeys", "Edit.AddKey",
				"Add Key", AddNewKeyCommand));

			MenuData.Edit.Add(CreateMenuDataWithLink(() => AnySelected, "AnySelected", "Edit.DeleteKeys",
				"Delete Selected Keys", DeleteKeysCommand));

			MenuData.Settings.Add(new MenuData("Settings.Preferences", "Preferences", OpenPreferencesCommand));

			MenuData larianWikiMenu = new MenuData("Help.Links.LarianWiki", "Larian Wiki");
			larianWikiMenu.Add(new MenuData("Help.Links.LarianWiki.KeyEditor", "Translated String Key Editor",
				ReactiveCommand.Create(() => { Helpers.Web.OpenUri(@"https://docs.larian.game/Translated_string_key_editor"); })));
			larianWikiMenu.Add(new MenuData("Help.Links.LarianWiki.LocalizationGuide", "Modding: Localization",
				ReactiveCommand.Create(() => { Helpers.Web.OpenUri(@"https://docs.larian.game/Modding:_Localization"); })));

			MenuData.Help.Add(larianWikiMenu);

			CanSave = false;
			AnySelected = false;
			CanAddFile = false;
			CanAddKeys = false;

			this.WhenAny(vm => vm.SelectedGroup.SelectedFile, x => x.Value).ToProperty(this, vm => vm.SelectedItem, _selectedItem);

			this.WhenAny(vm => vm.SelectedEntry.EntryContent, vm => vm.Value).Subscribe((o) => {
				this.RaisePropertyChanged("SelectedEntryContent");
				this.RaisePropertyChanged("SelectedEntryHtmlContent");
			});

			void clearSelectedEntry()
			{
				if(SelectedEntry != null)
				{
					ContentSelected = false;
					ContentFocused = false;
					SelectedEntry = null;
				}
			}

			this.WhenAnyValue(vm => vm.SelectedItem, vm => vm.SelectedGroup).Subscribe((o) => clearSelectedEntry());

			CanExecutePopoutContentCommand = this.WhenAny(vm => vm.SelectedEntry, e => e.Value != null);
		}
	}

	
}
