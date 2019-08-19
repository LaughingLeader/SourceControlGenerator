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
using SCG.Modules.DOS2DE.Data.Savable;
using SCG.FileGen;

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

		public List<LocaleProjectLinkData> LinkedLocaleData { get; set; } = new List<LocaleProjectLinkData>();

		private LocaleMenuData menuData;

		public LocaleMenuData MenuData
		{
			get => menuData;
			set
			{
				this.RaiseAndSetIfChanged(ref menuData, value);
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

				if(Groups.Count > selectedGroupIndex)
				{
					SelectedGroup = Groups[SelectedGroupIndex];
				}

				this.RaisePropertyChanged("CurrentImportPath");

				if (updateCanSave)
				{
					SelectedFileChanged(SelectedGroup, SelectedFile);
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

		private LocaleTabGroup selectedGroup;

		public LocaleTabGroup SelectedGroup
		{
			get => selectedGroup;
			set
			{
				this.RaiseAndSetIfChanged(ref selectedGroup, value);
				if(selectedGroup != null)
				{
					SelectedFile = selectedGroup.SelectedFile;
				}
			}
		}

		private ILocaleFileData selectedFile;

		public ILocaleFileData SelectedFile
		{
			get => selectedFile;
			set
			{
				if (selectedFile != null && selectedFile != value && selectedFile.IsRenaming) selectedFile.IsRenaming = false;
				this.RaiseAndSetIfChanged(ref selectedFile, value);
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

		private bool subWindowOpen = false;

		public bool IsSubWindowOpen
		{
			get => subWindowOpen;
			set { this.RaiseAndSetIfChanged(ref subWindowOpen, value); }
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

		private bool anyEntrySelected = false;

		public bool AnyEntrySelected
		{
			get => anyEntrySelected;
			set
			{
				this.RaiseAndSetIfChanged(ref anyEntrySelected, value);
			}
		}

		private bool anyFileSelected = false;

		public bool AnyFileSelected
		{
			get => anyFileSelected;
			set
			{
				this.RaiseAndSetIfChanged(ref anyFileSelected, value);
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
			SelectedFile = keyFileData;
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
				if (SelectedFile != null && SelectedFile is LocaleNodeFileData data)
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

			LocaleEditorCommands.SaveAllLinkedData(ModuleData, LinkedLocaleData);
		}

		public async Task SaveCurrent()
		{
			if(SelectedFile != null)
			{
				if (SelectedFile is LocaleNodeFileData keyFileData)
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

					LocaleEditorCommands.SaveLinkedDataForFile(ModuleData, LinkedLocaleData, SelectedFile);
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

				IsSubWindowOpen = true;

				void writeToFile(FileDialogResult result, string path)
				{
					IsSubWindowOpen = false;

					if (result == FileDialogResult.Ok)
					{
						var fileData = LocaleEditorCommands.CreateFileData(SelectedGroup, path, Path.GetFileNameWithoutExtension(path));
						SelectedGroup.DataFiles.Add(fileData);
						SelectedGroup.UpdateCombinedData();
						SelectedGroup.SelectedFileIndex = SelectedGroup.Tabs.Count - 1;
					}
				}

				FileCommands.Save.OpenSaveDialog(view, "Create Localization File...",
					writeToFile, newFileName, sourceRoot, DOS2DEFileFilters.LarianBinaryFile);
			}
		}

		public void ImportFilesAsFileData(IEnumerable<string> files)
		{
			IsSubWindowOpen = false;

			if (files.Count() > 0)
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
				view.FocusSelectedTab();

				Settings.LastFileImportPath = Path.GetDirectoryName(files.FirstOrDefault());
				this.RaisePropertyChanged("CurrentImportPath");
				this.RaisePropertyChanged("CurrentFileImportPath");

				view.SaveSettings();

				ChangesUnsaved = true;

				
			}
		}

		public void ExportFileAsText(ILocaleFileData localeFileData)
		{
			string exportName = Path.GetFileNameWithoutExtension(localeFileData.SourcePath) + ".tsv";
			IsSubWindowOpen = true;
			void writeToFile(FileDialogResult result, string path)
			{
				IsSubWindowOpen = false;

				if (result == FileDialogResult.Ok)
				{
					string delimiter = @"\t";
					if (FileCommands.FileExtensionFound(path, ".csv"))
					{
						delimiter = ",";
					}
					string contents = "";
					var entries = localeFileData.Entries.OrderBy(x => x.Key).ToList();
					for (var i = 0; i < entries.Count(); i++)
					{
						var entry = entries[i];
						contents += entry.Key + delimiter + entry.Content;
						if (i < entries.Count - 1) contents += Environment.NewLine;
					}

					if(FileCommands.WriteToFile(path, contents))
					{
						OutputType = LogType.Important;
						OutputText = $"Saved locale file to '{path}'.";
						OutputDate = DateTime.Now.ToShortTimeString();
					}
					else
					{
						OutputType = LogType.Error;
						OutputText = $"Problem saving file '{path}'.";
						OutputDate = DateTime.Now.ToShortTimeString();
					}
				}
			}

			FileCommands.Save.OpenSaveDialog(view, "Save Locale File As...",
				writeToFile, exportName, CurrentEntryImportPath, DOS2DEFileFilters.AllLocaleFilesList.ToArray());
		}

		public void ImportFilesAsKeys(IEnumerable<string> files)
		{
			IsSubWindowOpen = false;

			if (SelectedFile != null && files.Count() > 0)
			{
				Log.Here().Activity("Importing keys from files...");

				var currentItem = SelectedFile;
				var newKeys = LocaleEditorCommands.ImportFilesAsEntries(files, SelectedFile);

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
					SelectedFile.Entries.Add(k);
				}

				SelectedFile.ChangesUnsaved = true;
				SelectedGroup.UpdateCombinedData();

				Settings.LastEntryImportPath = Path.GetDirectoryName(files.FirstOrDefault());
				this.RaisePropertyChanged("CurrentImportPath");
				this.RaisePropertyChanged("CurrentEntryImportPath");
				view.SaveSettings();

				ChangesUnsaved = true;
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
		public ICommand RefreshFileCommand { get; private set; }
		public ICommand ReloadFileLinkDataCommand { get; private set; }
		public ICommand SetFileLinkDataCommand { get; private set; }
		public ICommand RemoveFileLinkDataCommand { get; private set; }

		public ICommand OpenPreferencesCommand { get; private set; }

		//public ICommand ExpandContentCommand { get; set; }

		public ICommand AddFontTagCommand { get; private set; }

		// Content Context Menu
		public ICommand ToggleContentLightModeCommand { get; private set; }
		public ICommand ChangeContentFontSizeCommand { get; private set; }
		public ICommand PopoutContentCommand { get; set; }
		public ICommand CopyToClipboardCommand { get; private set; }
		public ICommand ToggleRenameFileTabCommand { get; private set; }
		public ICommand ConfirmRenameFileTabCommand { get; private set; }
		public ICommand CancelRenamingFileTabCommand { get; private set; }
		public ICommand SelectNoneCommand { get; private set; }
		public ICommand ExportFileAsTextualCommand { get; private set; }

		public IObservable<bool> GlobalCanActObservable { get; private set; }
		public IObservable<bool> SubWindowOpenedObservable { get; private set; }
		public IObservable<bool> CanExecutePopoutContentCommand { get; private set; }
		public IObservable<bool> FileSelectedObservable { get; private set; }
		public IObservable<bool> CanAddFileObservable { get; private set; }
		public IObservable<bool> CanImportFilesObservable { get; private set; }
		public IObservable<bool> CanImportKeysObservable { get; private set; }
		public IObservable<bool> AnySelectedEntryObservable { get; private set; }

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
			IsSubWindowOpen = false;

			if (confirm)
			{
				if (SelectedGroup != null && SelectedGroup.SelectedFile != null)
				{
					if(SelectedGroup.SelectedFile == SelectedGroup.CombinedEntries)
					{
						var selectedEntries = SelectedGroup.DataFiles.SelectMany(f => f.Entries.Where(x => x.Selected)).ToList();
						Log.Here().Important($"Deleting {selectedEntries.Count} keys.");
						List<LocaleEntryHistory> lastState = new List<LocaleEntryHistory>();
						
						foreach(var entry in selectedEntries)
						{
							lastState.Add(new LocaleEntryHistory
							{
								ParentFile = entry.Parent,
								Entry = entry,
								Index = entry.Parent.Entries.IndexOf(entry),
								ChangesUnsaved = entry.ChangesUnsaved,
								ParentChangesUnsaved = entry.Parent.ChangesUnsaved
							});
						}

						void undo()
						{
							foreach(var x in lastState)
							{
								var fileEntry = SelectedGroup.DataFiles.FirstOrDefault(f => f.SourcePath == x.ParentFile.SourcePath);
								if(fileEntry != null)
								{
									x.Entry.ChangesUnsaved = x.ChangesUnsaved;
									fileEntry.Entries.Insert(x.Index, x.Entry);
									fileEntry.ChangesUnsaved = x.ParentChangesUnsaved;
									if (fileEntry is LocaleNodeFileData nodeFileData && x.Entry is LocaleNodeKeyEntry nodeKeyEntry)
									{
										nodeFileData.RootRegion.AppendChild(nodeKeyEntry.Node);
									}
								}
							}
							SelectedGroup.UpdateCombinedData();
						}

						void redo()
						{
							foreach (var entry in selectedEntries)
							{
								entry.Parent.Entries.Remove(entry);
								if (entry is LocaleNodeKeyEntry nodeKeyEntry && entry.Parent is LocaleNodeFileData nodeFileData)
								{
									//Log.Here().Important($"Looking for container for '{nodeKeyEntry.Node.Name}' => {nodeKeyEntry.Key}.");
									var nodeContainer = nodeFileData.RootRegion.Children.Values.Where(l => l.Contains(nodeKeyEntry.Node));
									foreach (var list in nodeContainer)
									{
										//Log.Here().Important($"Removing node '{nodeKeyEntry.Node.Name}' from list.");
										list.Remove(nodeKeyEntry.Node);
									}
								}
								entry.Parent.ChangesUnsaved = true;
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
						var deleteEntries = selectedFile.Entries.Where(x => x.Selected).ToList();
						var lastChangesUnsaved = selectedFile.ChangesUnsaved;
						void undo()
						{
							selectedFile.Entries = new ObservableCollectionExtended<ILocaleKeyEntry>(last);
							SelectedGroup.UpdateCombinedData();
							selectedFile.ChangesUnsaved = lastChangesUnsaved;
						}
						void redo()
						{
							//selectedFile.Entries.RemoveAll(x => deleteEntries.Contains(x));
							Log.Here().Important($"Deleting {deleteEntries.Count} keys.");
							for (var i = 0; i < deleteEntries.Count; i++)
							{
								var entry = deleteEntries[i];
								entry.Parent.Entries.Remove(entry);
								if (selectedFile is LocaleNodeFileData nodeFileData && entry is LocaleNodeKeyEntry nodeKeyEntry)
								{
									//Log.Here().Important($"Looking for container for '{nodeKeyEntry.Node.Name}' => {nodeKeyEntry.Key}.");
									var nodeContainer = nodeFileData.RootRegion.Children.Values.Where(l => l.Contains(nodeKeyEntry.Node));
									foreach (var list in nodeContainer)
									{
										//Log.Here().Important($"Removing node '{nodeKeyEntry.Node.Name}' from list.");
										list.Remove(nodeKeyEntry.Node);
									}
								}
							}
							SelectedGroup.UpdateCombinedData();
							selectedFile.ChangesUnsaved = true;
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
				AnyEntrySelected = true;
			}
			else
			{
				AnyEntrySelected = SelectedGroup?.DataFiles.Any(t => t.Entries.Any(e => e.Selected == true)) == true;
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
			else if (!AnyEntrySelected)
			{
				SelectedEntry = null;
			}
		}

		public void SetLinkedData(ILocaleFileData fileData)
		{
			void OnFileSelected(string filePath)
			{
				IsSubWindowOpen = false;

				if (File.Exists(filePath))
				{
					var lastLinkData = fileData.FileLinkData;
					List<TextualLocaleEntry> lastValues = fileData.Entries.Select(x => new TextualLocaleEntry { Key = x.Key, Content = x.Content, Handle = x.Handle }).ToList();

					void undo()
					{
						fileData.FileLinkData = lastLinkData;

						foreach (var lastEntry in lastValues)
						{
							var entry = fileData.Entries.FirstOrDefault(x => x.Handle == lastEntry.Handle);
							if (entry != null)
							{
								entry.Key = lastEntry.Key;
								entry.Content = lastEntry.Content;
							}
						}
					}

					void redo()
					{
						fileData.FileLinkData = new LocaleFileLinkData()
						{
							ReadFrom = filePath,
							TargetFile = fileData.SourcePath
						};

						Log.Here().Activity($"Loading data for '{fileData.SourcePath}' from linked file '{filePath}'.");
						LocaleEditorCommands.RefreshLinkedData(fileData);

						if (fileData is LocaleNodeFileData nodeFileData)
						{
							if (nodeFileData.ModProject != null && !String.IsNullOrEmpty(nodeFileData.FileLinkData.ReadFrom))
							{
								Log.Here().Activity($"Looking for linked list for '{nodeFileData.ModProject.UUID}'.");
								var linkedList = LinkedLocaleData.FirstOrDefault(x => nodeFileData.ModProject.UUID == x.ProjectUUID);
								if (linkedList == null)
								{
									Log.Here().Activity("  Linked list not found. Creating new.");
									linkedList = new LocaleProjectLinkData()
									{
										ProjectUUID = nodeFileData.ModProject.UUID,
									};
									LinkedLocaleData.Add(linkedList);
								}

								linkedList.Links.Add(fileData.FileLinkData);

								LocaleEditorCommands.SaveLinkedData(ModuleData, linkedList);
							}
						}
					}

					CreateSnapshot(undo, redo);
					redo();
				}
			}
			IsSubWindowOpen = true;
			FileCommands.Load.OpenFileDialog(view, "Pick localization file to link...",
					CurrentEntryImportPath, OnFileSelected, "", new Action<string, FileDialogResult>((s,r) => IsSubWindowOpen = false), CommonFileFilters.DelimitedLocaleFiles);
		}

		public void RefreshLinkedData(ILocaleFileData fileData)
		{
			if(!String.IsNullOrEmpty(fileData.FileLinkData.ReadFrom))
			{
				List<TextualLocaleEntry> lastValues = fileData.Entries.Select(x => new TextualLocaleEntry { Key = x.Key, Content = x.Content, Handle = x.Handle }).ToList();
				void undo()
				{
					foreach(var lastEntry in lastValues)
					{
						var entry = fileData.Entries.FirstOrDefault(x => x.Handle == lastEntry.Handle);
						if (entry != null)
						{
							entry.Key = lastEntry.Key;
							entry.Content = lastEntry.Content;
						}
					}
				}
				void redo()
				{
					LocaleEditorCommands.RefreshLinkedData(fileData);
				}
				CreateSnapshot(undo, redo);
				redo();
			}
		}

		public void RemoveLinkedData(ILocaleFileData fileData)
		{
			var lastLinkData = fileData.FileLinkData;

			void onConfirm(bool confirmed)
			{
				if (confirmed)
				{
					var containingLists = LinkedLocaleData.Where(x => x.Links.Contains(fileData.FileLinkData));

					void undo()
					{
						fileData.FileLinkData = lastLinkData;
						foreach(var pdata in containingLists)
						{
							pdata.Links.Add(fileData.FileLinkData);
						}
					}

					void redo()
					{
						var removed = false;
						foreach (var pdata in containingLists)
						{
							pdata.Links.Remove(fileData.FileLinkData);
							removed = true;
						}

						if (removed)
						{
							fileData.ChangesUnsaved = true;
						}
					}

					CreateSnapshot(undo, redo);
					redo();
				}
			}

			FileCommands.OpenConfirmationDialog(view, "Remove Link", 
				$"Remove external link '{fileData.FileLinkData.ReadFrom}' from {fileData.FileLinkData.TargetFile}?", "", onConfirm);
		}

		public void CancelRenaming(ILocaleFileData fileData)
		{
			if (fileData.IsRenaming)
			{
				fileData.RenameText = fileData.Name;
				fileData.IsRenaming = false;
			}
		}

		public void ConfirmRenaming(ILocaleFileData fileData)
		{
			string lastName = fileData.Name;
			string nextName = fileData.RenameText;

			string ext = Path.GetExtension(fileData.SourcePath);
			string nextFilePath = Path.Combine(Path.GetDirectoryName(fileData.SourcePath), FileCommands.EnsureExtension(nextName, ext));

			Log.Here().Activity($"Renaming file '{fileData.SourcePath}' => '{nextFilePath}'.");

			void undo()
			{
				fileData.Name = lastName;
				fileData.RenameText = lastName;
				fileData.IsRenaming = false;
			}
			void redo()
			{
				if (File.Exists(fileData.SourcePath))
				{
					if(FileCommands.RenameFile(fileData.SourcePath, nextFilePath))
					{
						fileData.Name = fileData.RenameText;
					}
					else
					{
						fileData.RenameText = fileData.Name;
					}

					fileData.IsRenaming = false;
				}
				else
				{
					fileData.Name = fileData.RenameText;
					fileData.IsRenaming = false;
				}
			}
			CreateSnapshot(undo, redo);
			redo();
		}

		public void OnViewLoaded(LocaleEditorWindow v, DOS2DEModuleData moduleData, CompositeDisposable disposables)
		{
			view = v;
			ModuleData = moduleData;

			SubWindowOpenedObservable = this.WhenAnyValue(vm => vm.IsSubWindowOpen);

			GlobalCanActObservable = this.WhenAny(vm => vm.IsAddingNewFileTab, vm => vm.IsSubWindowOpen, (b1, b2) => !b1.Value && !b2.Value);
			AnySelectedEntryObservable = this.WhenAnyValue(vm => vm.AnyEntrySelected);
			CanImportFilesObservable = this.WhenAnyValue(vm => vm.CanAddFile);
			CanImportKeysObservable = this.WhenAnyValue(vm => vm.CanAddKeys);

			this.WhenAny(vm => vm.SelectedFile.IsRenaming, b => b.Value == true).Subscribe((b) =>
			{
				if (b)
				{
					view.FocusSelectedTab();
				}
			}).DisposeWith(disposables);

			this.WhenAnyValue(vm => vm.ChangesUnsaved).Subscribe((b) =>
			{
				if (b)
				{
					WindowTitle = "*Localization Editor";
				}
				else
				{
					WindowTitle = "Localization Editor";
				}
			}).DisposeWith(disposables);

			//this.WhenAnyValue(vm => vm.SelectedGroup.SelectedFile).BindTo(this, vm => vm.SelectedFile).DisposeWith(disposables);

			FileSelectedObservable = this.WhenAnyValue(vm => vm.SelectedGroup, vm => vm.SelectedFile, (g, f) => f != null && g != null && f != g.CombinedEntries);
			FileSelectedObservable.BindTo(this, vm => vm.AnyFileSelected).DisposeWith(disposables);
			FileSelectedObservable.Subscribe((b) =>
			{
				Log.Here().Activity($"AnyFileSelected? Subscribe: {b} AnyFileSelected: {AnyFileSelected}");
			}).DisposeWith(disposables);

			var onCancel = new Action<string, FileDialogResult>((s, r) => IsSubWindowOpen = false);

			AddFileCommand = ReactiveCommand.Create(() =>
			{
				Log.Here().Activity("Adding new file to group.");
				if(SelectedGroup != null)
				{
					var currentGroup = Groups.Where(g => g == SelectedGroup).First();
					var totalFiles = currentGroup.Tabs.Count;
					string futurePath = FileCommands.EnsureExtension(Path.Combine(currentGroup.SourceDirectories.First(), "NewFile" + totalFiles), ".lsb");
					var newFile = LocaleEditorCommands.CreateFileData(currentGroup, futurePath, "NewFile" + totalFiles);
					bool lastChangesUnsaved = currentGroup.ChangesUnsaved;

					void undo()
					{
						var list = currentGroup.DataFiles.ToList();
						list.Remove(newFile);
						currentGroup.DataFiles = new ObservableCollectionExtended<ILocaleFileData>(list);

						SelectedGroup.ChangesUnsaved = lastChangesUnsaved;
						SelectedGroup.UpdateCombinedData();
						SelectedGroup.SelectLast();
						view.FocusSelectedTab();
					}

					void redo()
					{
						currentGroup.DataFiles.Add(newFile);

						SelectedGroup.ChangesUnsaved = true;
						SelectedGroup.UpdateCombinedData();
						SelectedGroup.SelectLast();
						view.FocusSelectedTab();

						newFile.ChangesUnsaved = true;
						newFile.IsRenaming = true;
					}

					CreateSnapshot(undo, redo);
					redo();
				}
			}, CanImportFilesObservable).DisposeWith(disposables);

			ImportFileCommand = ReactiveCommand.Create(() =>
			{
				IsSubWindowOpen = true;
				FileCommands.Load.OpenMultiFileDialog(view, DOS2DETooltips.Button_Locale_ImportFile, 
					CurrentEntryImportPath, ImportFilesAsFileData, "", onCancel, DOS2DEFileFilters.AllLocaleFilesList.ToArray());
			}, CanImportFilesObservable).DisposeWith(disposables);

			ImportKeysCommand = ReactiveCommand.Create(() =>
			{
				IsSubWindowOpen = true;
				FileCommands.Load.OpenMultiFileDialog(view, DOS2DETooltips.Button_Locale_ImportKeys,
					CurrentEntryImportPath, ImportFilesAsKeys, "", onCancel, DOS2DEFileFilters.AllLocaleFilesList.ToArray());
			}, CanImportKeysObservable).DisposeWith(disposables);

			ExportFileAsTextualCommand = ReactiveCommand.Create<ILocaleFileData>(ExportFileAsText, GlobalCanActObservable).DisposeWith(disposables);

			OpenPreferencesCommand = ReactiveCommand.Create(() => { view.TogglePreferencesWindow(); }, GlobalCanActObservable).DisposeWith(disposables);

			DeleteKeysCommand = ReactiveCommand.Create(() =>
			{
				IsSubWindowOpen = true;
				FileCommands.OpenConfirmationDialog(view, "Delete Keys", "Delete selected keys?", "Changes will be lost.", DeleteSelectedKeys);
			}, AnySelectedEntryObservable).DisposeWith(disposables);

			//var selectedGroupObservable = this.WhenAny(vm => vm.SelectedGroup, x => x.Value);

			//var selectedFileObservable = this.WhenAny(vm => vm.SelectedGroup.SelectedFile, x => x.Value);
			//selectedFileObservable.Subscribe();
			//selectedFileObservable.ToProperty(this, vm => vm.SelectedFile, _selectedItem).DisposeWith(disposables);

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

			this.WhenAnyValue(vm => vm.SelectedFile, vm => vm.SelectedGroup).Subscribe((o) => clearSelectedEntry()).DisposeWith(disposables);

			CanExecutePopoutContentCommand = this.WhenAny(vm => vm.SelectedEntry, e => e.Value != null);

			LocaleEditorCommands.LoadSettings(ModuleData, this);

			UpdateCombinedGroup(true);

			ExportXMLCommand = ReactiveCommand.Create(OpenExportWindow, AnySelectedEntryObservable).DisposeWith(disposables);
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

			CloseFileCommand = ReactiveCommand.Create<ILocaleFileData>(CloseFileInGroup, GlobalCanActObservable).DisposeWith(disposables);
			ToggleRenameFileTabCommand = ReactiveCommand.Create<ILocaleFileData>((ILocaleFileData fileData) => 
			{
				fileData.IsRenaming = !fileData.IsRenaming;
				if(fileData.IsRenaming)
				{
					SelectedGroup.SelectedFileIndex = SelectedGroup.Tabs.IndexOf(fileData);
				}
			}, GlobalCanActObservable).DisposeWith(disposables);

			ConfirmRenameFileTabCommand = ReactiveCommand.Create<ILocaleFileData>(ConfirmRenaming).DisposeWith(disposables);
			CancelRenamingFileTabCommand = ReactiveCommand.Create<ILocaleFileData>(CancelRenaming).DisposeWith(disposables);

			SaveAllCommand = ReactiveCommand.CreateFromTask(SaveAll, GlobalCanActObservable).DisposeWith(disposables);
			SaveCurrentCommand = ReactiveCommand.CreateFromTask(SaveCurrent, GlobalCanActObservable).DisposeWith(disposables);
			GenerateHandlesCommand = ReactiveCommand.Create(GenerateHandles, AnySelectedEntryObservable).DisposeWith(disposables);
			AddNewKeyCommand = ReactiveCommand.Create(AddNewKey, CanImportKeysObservable).DisposeWith(disposables);

			AddFontTagCommand = ReactiveCommand.Create(AddFontTag, AnySelectedEntryObservable).DisposeWith(disposables);

			ToggleContentLightModeCommand = ReactiveCommand.Create(() => ContentLightMode = !ContentLightMode, GlobalCanActObservable).DisposeWith(disposables);
			ChangeContentFontSizeCommand = ReactiveCommand.Create<string>((fontSizeStr) => {
				this.RaisePropertyChanging("ContentFontSize");
				if (int.TryParse(fontSizeStr, out contentFontSize))
				{
					this.RaisePropertyChanged("ContentFontSize");
				}
			}, GlobalCanActObservable).DisposeWith(disposables);

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
			}, GlobalCanActObservable).DisposeWith(disposables);

			RefreshFileCommand = ReactiveCommand.Create<ILocaleFileData>((ILocaleFileData fileData) =>
			{
				if(fileData is LocaleNodeFileData nodeFile)
				{
					LocaleEditorCommands.RefreshFileData(nodeFile);
				}
			}, FileSelectedObservable).DisposeWith(disposables);

			ReloadFileLinkDataCommand = ReactiveCommand.Create<ILocaleFileData>(RefreshLinkedData, FileSelectedObservable).DisposeWith(disposables);
			SetFileLinkDataCommand = ReactiveCommand.Create<ILocaleFileData>(SetLinkedData, FileSelectedObservable).DisposeWith(disposables);
			RemoveFileLinkDataCommand = ReactiveCommand.Create<ILocaleFileData>(RemoveLinkedData, FileSelectedObservable).DisposeWith(disposables);

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

			var SelectAllEntriesCommand = ReactiveCommand.Create(() => { SelectedFile?.SelectAll(); });
			var DeselectAllEntriesCommand = ReactiveCommand.Create(() => { SelectedFile?.SelectNone(); });

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
			CanSave = AnyEntrySelected = CanAddFile = CanAddKeys = false;
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
		}
	}

	
}
