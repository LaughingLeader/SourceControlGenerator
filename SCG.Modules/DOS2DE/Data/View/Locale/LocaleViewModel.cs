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
using SCG.Modules.DOS2DE.Data;
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
using System.Reactive.Disposables;

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

		public ILocaleFileData SelectedItem
		{
			get => SelectedGroup?.SelectedFile;
		}

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

		private bool canSave = false;

		public bool CanSave
		{
			get => canSave;
			set
			{
				this.RaiseAndSetIfChanged(ref canSave, value);
			}
		}

		private bool canAddFile = false;

		public bool CanAddFile
		{
			get => canAddFile;
			set
			{
				this.RaiseAndSetIfChanged(ref canAddFile, value);
			}
		}

		private bool canAddKeys = false;

		public bool CanAddKeys
		{
			get => canAddKeys;
			set
			{
				this.RaiseAndSetIfChanged(ref canAddKeys, value);
			}
		}

		private bool anySelected = false;

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
			Log.Here().Activity($"AnySelected: {AnySelected}");
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
						entry.Parent.ChangesUnsaved = true;
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

		public async Task SaveAll()
		{
			Log.Here().Activity("Saving all files.");
			var backupSuccess = await LocaleEditorCommands.BackupDataFiles(this, ModuleData.Settings.BackupRootDirectory);
			if (backupSuccess == true)
			{
				var successes = await LocaleEditorCommands.SaveDataFiles(this);
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

		public async Task SaveCurrent()
		{
			if(SelectedGroup != null)
			{
				if (SelectedGroup.SelectedFile != null && SelectedGroup.SelectedFile is LocaleNodeFileData keyFileData)
				{
					var backupSuccess = await LocaleEditorCommands.BackupDataFile(keyFileData.SourcePath, ModuleData.Settings.BackupRootDirectory);
					if (backupSuccess == true)
					{
						int result = await LocaleEditorCommands.SaveDataFile(keyFileData);
						if (result > 0)
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
					else
					{
						OutputText = $"Problem occured when backing up files. Check the log.";
						OutputType = LogType.Error;
					}
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
					var fileData = LocaleEditorCommands.CreateFileData(SelectedGroup, savePath, Path.GetFileName(savePath));
					SelectedGroup.DataFiles.Add(fileData);
					SelectedGroup.UpdateCombinedData();
					SelectedGroup.SelectedFileIndex = SelectedGroup.Tabs.Count - 1;
				}, newFileName, "Larian Localization File (*.lsb)|*.lsb");
			}
		}

		public void ImportFilesAsFileData(IEnumerable<string> files)
		{
			if(files.Count() > 0)
			{
				var currentGroup = Groups.Where(g => g == SelectedGroup).First();
				var newFileDataList = LocaleEditorCommands.ImportFilesAsData(files, SelectedGroup);

				CreateSnapshot(() => {
					var list = currentGroup.DataFiles.ToList();
					foreach (var entry in newFileDataList)
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

				Settings.LastFileImportPath = Path.GetDirectoryName(files.FirstOrDefault());
				this.RaisePropertyChanged("CurrentImportPath");
				this.RaisePropertyChanged("CurrentFileImportPath");

				view.SaveSettings();

				ChangesUnsaved = true;
			}
		}

		public void ImportFilesAsKeys(IEnumerable<string> files)
		{
			if(SelectedItem != null && files.Count() > 0)
			{
				Log.Here().Activity("Importing keys from files...");

				var currentItem = SelectedItem;
				var newKeys = LocaleEditorCommands.ImportFilesAsEntries(files, SelectedItem);

				CreateSnapshot(() => {
					foreach (var k in newKeys)
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

				Settings.LastEntryImportPath = Path.GetDirectoryName(files.FirstOrDefault());
				this.RaisePropertyChanged("CurrentImportPath");
				this.RaisePropertyChanged("CurrentEntryImportPath");
				view.SaveSettings();

				ChangesUnsaved = true;
			}
			else
			{
				Log.Here().Error($"SelectedItem == {SelectedItem}");
			}
		}

		//private MenuData SaveCurrentMenuData { get; set; }
		//private MenuData SelectAllMenuData { get; set; }
		//private MenuData SelectNoneMenuData { get; set; }
		//private MenuData GenerateHandlesMenuData { get; set; }

		public ICommand AddFileCommand { get; private set; }
		public ICommand AddFileToGroupCommand { get; private set; }
		public ICommand ConfirmFileAddToGroupCommand { get; private set; }
		public ICommand CancelFileAddToGroupCommand { get; private set; }
		public ICommand CloseFileCommand { get; private set; }
		public ICommand ImportFileCommand { get; internal set; }
		public ICommand ImportKeysCommand { get; internal set; }
		public ICommand ExportXMLCommand { get; private set; }

		public ICommand SaveAllCommand { get; private set; }
		public ICommand SaveCurrentCommand { get; private set; }
		public ICommand GenerateHandlesCommand { get; private set; }

		public ICommand AddNewKeyCommand { get; private set; }
		public ICommand DeleteKeysCommand { get; private set; }

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
		public ICommand SelectNoneCommand { get; private set; }

		public IObservable<bool> CanExecutePopoutContentCommand { get; private set; }
		public IObservable<bool> GlobalCommandEnabled { get; private set; }
		public IObservable<bool> CanImportFilesObservable { get; private set; }
		public IObservable<bool> CanImportKeysObservable { get; private set; }
		public IObservable<bool> AnySelectedObservable { get; private set; }

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
					LocaleCustomKeyEntry localeEntry = new LocaleCustomKeyEntry(SelectedGroup.SelectedFile)
					{
						Handle = Guid.NewGuid().ToString().Replace('-', 'g').Insert(0, "h"),
						Key = "NewKey" + customFileData.Entries.Count + 1,
						Content = ""
					};
					localeEntry.SetHistoryFromObject(this);
					AddWithHistory(customFileData.Entries, localeEntry);
					SelectedGroup.UpdateCombinedData();
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
					var deleteKeys = SelectedGroup.SelectedFile.Entries.Where(e => e.Selected).ToList();
					Log.Here().Activity($"Deleting {deleteKeys.Count} keys.");

					if(SelectedGroup.SelectedFile == SelectedGroup.CombinedEntries)
					{
						var selectedFiles = SelectedGroup.DataFiles.Where(f => f.Entries.Any(x => x.Selected)).ToList();

						List<LocaleEntryHistory> lastState = new List<LocaleEntryHistory>();
						
						foreach(var f in selectedFiles)
						{
							foreach(var e in f.Entries.Where(x => x.Selected))
							{
								lastState.Add(new LocaleEntryHistory
								{
									ParentFile = f,
									Entry = e,
									Index = f.Entries.IndexOf(e)
								});
							}
						}

						void undo()
						{
							foreach(var x in lastState)
							{
								var fileEntry = SelectedGroup.DataFiles.FirstOrDefault(f => f.SourcePath == x.ParentFile.SourcePath);
								if(fileEntry != null)
								{
									fileEntry.Entries.Insert(x.Index, x.Entry);
								}
							}
							SelectedGroup.UpdateCombinedData();
						}

						void redo()
						{
							for(var i = 0; i < selectedFiles.Count; i++)
							{
								var f = selectedFiles[i];
								f.Entries.RemoveAll(x => x.Selected);
							}
							SelectedGroup.UpdateCombinedData();
						}
						this.CreateSnapshot(undo, redo);
						redo();
					}
					else
					{
						var selectedFile = SelectedGroup.SelectedFile;

						var last = selectedFile.Entries.ToList();
						var deleteEntries = selectedFile.Entries.Where(x => x.Selected);
						void undo()
						{
							selectedFile.Entries = new ObservableCollectionExtended<ILocaleKeyEntry>(last);
							SelectedGroup.UpdateCombinedData();
						}
						void redo()
						{
							selectedFile.Entries.RemoveAll(x => deleteEntries.Contains(x));
							SelectedGroup.UpdateCombinedData();
							//Log.Here().Activity($"Entries {selectedFile.Entries.Count} | {next.Count()}.");
						}
						this.CreateSnapshot(undo, redo);
						redo();
					}
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
			ExportText = LocaleEditorCommands.ExportDataAsXML(this);

			if (view != null && view.ExportWindow != null)
			{
				if (!view.ExportWindow.IsVisible)
				{
					view.ExportWindow.Show();
					view.ExportWindow.Owner = view;
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
				LocaleCustomFileData data = new LocaleCustomFileData(newFileTabTargetGroup, NewFileTabName) { Project = targetProject };
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
				FileCommands.OpenConfirmationDialog(view, "Confirm Tab Removal", $"Remove {fileData.Name}?", "Unsaved changes will be lost.", new Action<bool>((b) =>
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

		public void UpdateAnySelected(bool selected)
		{
			if (selected)
			{
				AnySelected = true;
			}
			else
			{
				AnySelected = SelectedGroup?.DataFiles.Any(t => t.Entries.Any(e => e.Selected == true)) == true;
			}
		}

		public void KeyEntrySelected(ILocaleKeyEntry keyEntry, bool selected)
		{
			UpdateAnySelected(selected);
			if (selected)
			{
				if (SelectedEntry == null)
				{
					SelectedEntry = keyEntry;
				}
				else
				{
					if (!SelectedEntry.Selected)
					{
						SelectedEntry = keyEntry;
					}
				}
			}
			else if (!AnySelected)
			{
				SelectedEntry = null;
			}
		}

		public void OnViewLoaded(LocaleEditorWindow v, DOS2DEModuleData moduleData, CompositeDisposable disposables)
		{
			view = v;
			ModuleData = moduleData;

			Log.Here().Activity("View loaded?");

			ImportFileCommand = ReactiveCommand.Create(() =>
			{
				Log.Here().Activity("Opening file browser");
				FileCommands.Load.OpenMultiFileDialog(view, DOS2DETooltips.Button_Locale_ImportFile, 
					CurrentEntryImportPath, ImportFilesAsFileData, "", null, DOS2DEFileFilters.AllLocaleFilesList.ToArray());
			}, CanImportFilesObservable).DisposeWith(disposables);

			ImportKeysCommand = ReactiveCommand.Create(() =>
			{
				FileCommands.Load.OpenMultiFileDialog(view, DOS2DETooltips.Button_Locale_ImportKeys,
					CurrentEntryImportPath, ImportFilesAsKeys, "", null, DOS2DEFileFilters.AllLocaleFilesList.ToArray());
			}, CanImportKeysObservable).DisposeWith(disposables);

			OpenPreferencesCommand = ReactiveCommand.Create(() => { view.TogglePreferencesWindow(); }, GlobalCommandEnabled).DisposeWith(disposables);

			DeleteKeysCommand = ReactiveCommand.Create(() =>
			{
				Log.Here().Activity("Deleting keys?");
				FileCommands.OpenConfirmationDialog(view, "Delete Keys", "Delete selected keys?", "Changes will be lost.", DeleteSelectedKeys);
			}, AnySelectedObservable).DisposeWith(disposables);

			//var selectedGroupObservable = this.WhenAny(vm => vm.SelectedGroup, x => x.Value);

			//var selectedFileObservable = this.WhenAny(vm => vm.SelectedGroup.SelectedFile, x => x.Value);
			//selectedFileObservable.Subscribe();
			//selectedFileObservable.ToProperty(this, vm => vm.SelectedItem, _selectedItem).DisposeWith(disposables);

			this.WhenAny(vm => vm.SelectedEntry.EntryContent, vm => vm.Value).Subscribe((o) => {
				this.RaisePropertyChanged("SelectedEntryContent");
				this.RaisePropertyChanged("SelectedEntryHtmlContent");
			}).DisposeWith(disposables);

			void clearSelectedEntry()
			{
				if (SelectedEntry != null)
				{
					ContentSelected = false;
					ContentFocused = false;
					SelectedEntry = null;
				}
			}

			this.WhenAnyValue(vm => vm.SelectedItem, vm => vm.SelectedGroup).Subscribe((o) => clearSelectedEntry()).DisposeWith(disposables);

			CanExecutePopoutContentCommand = this.WhenAny(vm => vm.SelectedEntry, e => e.Value != null);

			LocaleEditorCommands.LoadSettings(ModuleData, this);

			UpdateCombinedGroup(true);

			ExportXMLCommand = ReactiveCommand.Create(OpenExportWindow, AnySelectedObservable).DisposeWith(disposables);
			AddFileToGroupCommand = ReactiveCommand.Create<CustomLocaleTabGroup>(AddCustomFileToGroup, CanImportFilesObservable).DisposeWith(disposables);

			var canConfirmAddFile = this.WhenAny(vm => vm.NewFileTabName, e => !String.IsNullOrWhiteSpace(e.Value));
			ConfirmFileAddToGroupCommand = ReactiveCommand.Create(ConfirmCustomFileAddToGroup, canConfirmAddFile).DisposeWith(disposables);
			CancelFileAddToGroupCommand = ReactiveCommand.Create(() =>
			{
				if (IsAddingNewFileTab)
				{
					IsAddingNewFileTab = false;
					NewFileTabName = "";
					newFileTabTargetGroup = null;
				}
			}).DisposeWith(disposables);

			CloseFileCommand = ReactiveCommand.Create<ILocaleFileData>(CloseFileInGroup, GlobalCommandEnabled).DisposeWith(disposables);
			ToggleRenameFileTabCommand = ReactiveCommand.Create<ILocaleFileData>((ILocaleFileData fileData) => {
				fileData.IsRenaming = !fileData.IsRenaming;
			}, GlobalCommandEnabled).DisposeWith(disposables);

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

			CancelRenamingFileTabCommand = ReactiveCommand.CreateFromTask<ILocaleFileData>(cancelRenamingFileTab).DisposeWith(disposables);

			SaveAllCommand = ReactiveCommand.CreateFromTask(SaveAll, GlobalCommandEnabled).DisposeWith(disposables);
			SaveCurrentCommand = ReactiveCommand.CreateFromTask(SaveCurrent, GlobalCommandEnabled).DisposeWith(disposables);
			GenerateHandlesCommand = ReactiveCommand.Create(GenerateHandles, AnySelectedObservable).DisposeWith(disposables);
			AddNewKeyCommand = ReactiveCommand.Create(AddNewKey, CanImportKeysObservable).DisposeWith(disposables);

			AddFontTagCommand = ReactiveCommand.Create(AddFontTag, AnySelectedObservable).DisposeWith(disposables);

			ToggleContentLightModeCommand = ReactiveCommand.Create(() => ContentLightMode = !ContentLightMode, GlobalCommandEnabled).DisposeWith(disposables);
			ChangeContentFontSizeCommand = ReactiveCommand.Create<string>((fontSizeStr) => {
				this.RaisePropertyChanging("ContentFontSize");
				if (int.TryParse(fontSizeStr, out contentFontSize))
				{
					this.RaisePropertyChanged("ContentFontSize");
				}
			}, GlobalCommandEnabled).DisposeWith(disposables);

			IObservable<bool> canSelectNone = this.WhenAnyValue(vm => vm.SelectedText, (text) => text != String.Empty);

			SelectNoneCommand = ReactiveCommand.Create<object>((targetObject) =>
			{
				if (targetObject is Xceed.Wpf.Toolkit.RichTextBox rtb)
				{
					rtb.Selection.Select(rtb.CaretPosition.DocumentStart, rtb.CaretPosition.DocumentStart);
				}
				else if (targetObject is System.Windows.Controls.TextBox tb)
				{
					tb.Select(tb.CaretIndex, 0);
				}
			}, canSelectNone).DisposeWith(disposables);

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
			}, GlobalCommandEnabled).DisposeWith(disposables);

			var SaveCurrentMenuData = new MenuData("SaveCurrent", "Save", SaveCurrentCommand, Key.S, ModifierKeys.Control);

			MenuData.File.Add(SaveCurrentMenuData);
			MenuData.File.Add(new MenuData("File.SaveAll", "Save All", SaveAllCommand, Key.S, ModifierKeys.Control | ModifierKeys.Shift));

			MenuData.File.Add(new MenuData("File.ImportFile", "Import File", ImportFileCommand));
			MenuData.File.Add(new MenuData("File.ImportKeys", "Import File as Keys", ImportKeysCommand));
			MenuData.File.Add(new MenuData("File.ExportSelected", DOS2DETooltips.Button_Locale_ExportToXML, ExportXMLCommand, Key.E, ModifierKeys.Control | ModifierKeys.Shift));

			//MenuData.File.Add(CreateMenuDataWithLink(() => CanAddFile, "CanAddFile", "File.ImportFile", "Import File", ImportFileCommand));
			//MenuData.File.Add(CreateMenuDataWithLink(() => CanAddKeys, "CanAddKeys", "File.ImportKeys", "Import File as Keys", ImportKeysCommand));

			//MenuData.File.Add(CreateMenuDataWithLink(() => AnySelected, "AnySelected", "File.ExportSelected", DOS2DETooltips.Button_Locale_ExportToXML, ExportXMLCommand, Key.E, ModifierKeys.Control | ModifierKeys.Shift));

			UndoMenuData = new MenuData("Edit.Undo", "Undo", UndoCommand, Key.Z, ModifierKeys.Control);
			RedoMenuData = new MenuData("Edit.Redo", "Redo", RedoCommand,
					new MenuShortcutInputBinding(Key.Z, ModifierKeys.Control | ModifierKeys.Shift),
					new MenuShortcutInputBinding(Key.Y, ModifierKeys.Control)
				);
			MenuData.Edit.Add(UndoMenuData);
			MenuData.Edit.Add(RedoMenuData);

			var SelectAllEntriesCommand = ReactiveCommand.Create(() => { SelectedItem?.SelectAll(); });
			var DeselectAllEntriesCommand = ReactiveCommand.Create(() => { SelectedItem?.SelectNone(); });

			MenuData.Edit.Add(new MenuData("Edit.SelectAll", "Select All", SelectAllEntriesCommand, Key.A, ModifierKeys.Control));
			MenuData.Edit.Add(new MenuData("Edit.SelectNone", "Select None", DeselectAllEntriesCommand, Key.D, ModifierKeys.Control));
			MenuData.Edit.Add(new MenuData("Edit.GenerateHandles", "Generate Handles for Selected", GenerateHandlesCommand, Key.G, ModifierKeys.Control | ModifierKeys.Shift));
			MenuData.Edit.Add(new MenuData("Edit.AddKey", "Add Key", AddNewKeyCommand));
			MenuData.Edit.Add(new MenuData("Edit.DeleteSelectedKeys", "Delete Selected Keys", DeleteKeysCommand));

			MenuData.Settings.Add(new MenuData("Settings.Preferences", "Preferences", OpenPreferencesCommand));

			MenuData larianWikiMenu = new MenuData("Help.Links.LarianWiki", "Larian Wiki");
			larianWikiMenu.Add(new MenuData("Help.Links.LarianWiki.KeyEditor", "Translated String Key Editor",
				ReactiveCommand.Create(() => { Helpers.Web.OpenUri(@"https://docs.larian.game/Translated_string_key_editor"); })));
			larianWikiMenu.Add(new MenuData("Help.Links.LarianWiki.LocalizationGuide", "Modding: Localization",
				ReactiveCommand.Create(() => { Helpers.Web.OpenUri(@"https://docs.larian.game/Modding:_Localization"); })));

			MenuData.Help.Add(larianWikiMenu);

			MenuData.RegisterShortcuts(view.InputBindings);

			//The window starts with the All/All selected.
			CanSave = AnySelected = CanAddFile = CanAddKeys = false;
		}

		public MenuData UndoMenuData { get; private set; }
		public MenuData RedoMenuData { get; private set; }

		public LocaleViewModel() : base()
		{
			this.PropertyChanged += LocaleViewModel_PropertyChanged;
			MenuData = new LocaleMenuData();

			CombinedGroup = new LocaleTabGroup(this, "All");
			ModsGroup = new LocaleTabGroup(this, "Locale (Mods)");
			PublicGroup = new LocaleTabGroup(this, "Locale (Public)");
			DialogGroup = new LocaleTabGroup(this, "Dialog");
			CustomGroup = new CustomLocaleTabGroup(this, "Custom");

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
			AnySelectedObservable = this.WhenAnyValue(vm => vm.AnySelected);
			CanImportFilesObservable = this.WhenAnyValue(vm => vm.CanAddFile);
			CanImportKeysObservable = this.WhenAnyValue(vm => vm.CanAddKeys);
		}
	}

	
}
