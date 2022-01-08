using System;
using System.Collections.Generic;
using System.Linq;
using SCG.Data;
using System.Collections.ObjectModel;
using System.Windows.Input;
using SCG.Commands;
using System.Windows;
using SCG.Data.View;
using LSLib.LS;
using SCG.Modules.DOS2DE.Windows;
using SCG.Modules.DOS2DE.Core;
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
using SCG.FileGen;
using DynamicData;
using System.Reactive.Linq;
using SCG.Controls;
using System.Windows.Controls;
using LSLib.LS.Enums;
using System.Reactive.Concurrency;
using System.Diagnostics;
using SCG.Modules.DOS2DE.LocalizationEditor.Models;
using SCG.Modules.DOS2DE.Data.View;
using SCG.Modules.DOS2DE.Data.View.Locale;
using SCG.Modules.DOS2DE.LocalizationEditor.Views;
using SCG.Modules.DOS2DE.LocalizationEditor.Utilities;

namespace SCG.Modules.DOS2DE.LocalizationEditor.ViewModels
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

		public void OnSettingsLoaded()
		{
			RootTemplatesGroup.Visibility = Settings.LoadRootTemplates;
			GlobalTemplatesGroup.Visibility = Settings.LoadGlobals;
			LevelDataGroup.Visibility = Settings.LoadLevelData;
		}

		private LocaleEditorProjectSettingsData activeProjectSettings;

		public LocaleEditorProjectSettingsData ActiveProjectSettings
		{
			get => activeProjectSettings;
			set { this.RaiseAndSetIfChanged(ref activeProjectSettings, value); }
		}

		public DOS2DEModuleData ModuleData { get; set; }

		public List<ModProjectData> LinkedProjects { get; set; } = new List<ModProjectData>();

		public List<LocaleProjectLinkData> LinkedLocaleData { get; set; } = new List<LocaleProjectLinkData>();

		public ModProjectData GetMainProject()
		{
			if (LinkedProjects.Count > 0)
			{
				return LinkedProjects.FirstOrDefault();
			}
			return null;
		}

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

		private bool isUnlocked = true;

		public bool IsUnlocked
		{
			get => isUnlocked;
			set { this.RaiseAndSetIfChanged(ref isUnlocked, value); }
		}

		private Visibility lockScreenVisibility = Visibility.Collapsed;

		public Visibility LockScreenVisibility
		{
			get { return lockScreenVisibility; }
			set
			{
				this.RaiseAndSetIfChanged(ref lockScreenVisibility, value);
				IsUnlocked = LockScreenVisibility != Visibility.Visible;
			}
		}

		public ObservableCollectionExtended<LocaleTabGroup> Groups { get; set; }

		private readonly ReadOnlyObservableCollection<LocaleTabGroup> visibleGroups;
		public ReadOnlyObservableCollection<LocaleTabGroup> VisibleGroups => visibleGroups;

		private int lastSelectedGroupIndex = 0;
		private int selectedGroupIndex = 0;

		public int SelectedGroupIndex
		{
			get => selectedGroupIndex;
			set
			{
				bool updateCanSave = selectedGroupIndex != value;
				var lastSelectedFile = SelectedFile;
				lastSelectedGroupIndex = selectedGroupIndex;

				if (value > Groups.Count)
				{
					value = 0;
				}
				else if (value < 0)
				{
					value = Groups.Count - 1;
				}

				var nextGroup = Groups[value];
				int nextIndex = 0;

				if (nextGroup != null)
				{
					//if (lastSelectedFile != null && nextGroup.DataFiles.Any(x => x.SourcePath == lastSelectedFile.SourcePath))
					//{
					//	nextIndex = nextGroup.Tabs.IndexOf(lastSelectedFile);
					//}
					if (nextGroup.SelectedFileIndex > nextGroup.Tabs.Count)
					{
						nextIndex = 0;
					}
					else
					{
						nextIndex = nextGroup.SelectedFileIndex;
					}

					CanRefreshFile = false;
				}

				//Log.Here().Activity($"{nextGroup.Name}: {nextGroup.SelectedFileIndex} => {nextIndex}");

				this.RaiseAndSetIfChanged(ref selectedGroupIndex, value);


				SelectedGroup = nextGroup;
				nextGroup.SelectedFileIndex = nextIndex;

				this.RaisePropertyChanged("CurrentImportPath");

				if (updateCanSave)
				{
					SelectedFileChanged(SelectedGroup, SelectedGroup.SelectedFile);
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

		private bool canRefreshFile = true;

		public bool CanRefreshFile
		{
			get => canRefreshFile;
			set { this.RaiseAndSetIfChanged(ref canRefreshFile, value); }
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
		private LocaleTabGroup journalGroup;

		public LocaleTabGroup JournalGroup
		{
			get => journalGroup;
			set
			{
				this.RaiseAndSetIfChanged(ref journalGroup, value);
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

		private LocaleTabGroup rootTemplatesGroup;

		public LocaleTabGroup RootTemplatesGroup
		{
			get => rootTemplatesGroup;
			set
			{
				this.RaiseAndSetIfChanged(ref rootTemplatesGroup, value);
			}
		}

		private LocaleTabGroup globalTemplatesGroup;

		public LocaleTabGroup GlobalTemplatesGroup
		{
			get => globalTemplatesGroup;
			set
			{
				this.RaiseAndSetIfChanged(ref globalTemplatesGroup, value);
			}
		}

		private LocaleTabGroup levelDataGroup;

		public LocaleTabGroup LevelDataGroup
		{
			get => levelDataGroup;
			set
			{
				this.RaiseAndSetIfChanged(ref levelDataGroup, value);
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
#if DEBUG
			return new List<LocaleTabGroup>() { PublicGroup, ModsGroup, DialogGroup, JournalGroup, CustomGroup };
#else
			return new List<LocaleTabGroup>() { PublicGroup, ModsGroup, DialogGroup, JournalGroup };
#endif
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
				if (selectedGroup != null)
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
				if (selectedFile != null && selectedFile != value)
				{
					selectedFile.IsRenaming = false;
				}
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
				if (SelectedEntry != null)
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

		private bool anyTextBoxFocused = false;

		public bool AnyTextBoxFocused
		{
			get => anyTextBoxFocused;
			set { this.RaiseAndSetIfChanged(ref anyTextBoxFocused, value); }
		}

		private bool clipboardHasText = false;

		public bool ClipboardHasText
		{
			get => clipboardHasText;
			set { this.RaiseAndSetIfChanged(ref clipboardHasText, value); }
		}

		private bool hideExtras = true;

		public bool HideExtras
		{
			get => hideExtras;
			set { this.RaiseAndSetIfChanged(ref hideExtras, value); }
		}


		public TextBox CurrentTextBox { get; set; }

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

		private Color? selectedColor;

		public Color? SelectedColor
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

		private bool missingKeyEntrySelected = false;

		public bool MissingKeyEntrySelected
		{
			get => missingKeyEntrySelected;
			set { this.RaiseAndSetIfChanged(ref missingKeyEntrySelected, value); }
		}


		private void SelectedFileChanged(LocaleTabGroup group, ILocaleFileData keyFileData)
		{
			SelectedFile = keyFileData;
			if (SelectedFile != null)
			{
				CanSave = group.CombinedEntries != keyFileData || group.DataFiles.Count == 1;
				SelectedFile.Selected = true;
				//Log.Here().Activity($"Selected file changed to {group.Name} | {keyFileData.Name}");
			}
			else
			{
				CanSave = false;
			}

			CanAddFile = group != CombinedGroup && group != DialogGroup && group != JournalGroup;
			CanAddKeys = SelectedGroup != null && SelectedGroup.SelectedFile != null && !SelectedGroup.SelectedFile.Locked;
			if (SelectedFile is LocaleNodeFileData fileData)
			{
				CanRefreshFile = fileData.Format != ResourceFormat.LSX;
			}
			else
			{
				CanRefreshFile = false;
			}
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
			if (JournalGroup?.DataFiles?.Count > 0) total += 1;
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
					var project = LinkedProjects.FirstOrDefault(x => data.SourcePath.Contains(x.FolderName));
					if (project != null)
					{
						string directory = Path.Combine(Path.GetFullPath(ModuleData.Settings.GitRootDirectory), project.ProjectName);
						if (Directory.Exists(directory))
						{
							return directory;
						}
					}
					return Path.GetDirectoryName(data.SourcePath);
				}
				else if (SelectedGroup != null)
				{
					return SelectedGroup.SourceDirectories.FirstOrDefault();
				}
				else if (LinkedProjects.Count > 0)
				{
					var project = LinkedProjects.First();
					string directory = Path.Combine(Path.GetFullPath(ModuleData.Settings.GitRootDirectory), project.ProjectName);
					if (Directory.Exists(directory))
					{
						return directory;
					}
				}
				else if (Directory.GetCurrentDirectory() == DefaultPaths.AppFolder)
				{
					return DefaultPaths.ModuleRootFolder(ModuleData);
				}
				return Directory.GetCurrentDirectory();
			}
		}

		/*
		public string CurrentFileImportPath
		{
			get
			{
				if (Settings != null && Directory.Exists(Settings.LastFileImportPath))
				{
					return Settings.LastFileImportPath;
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
		*/



		public void GenerateHandle(object targetObject)
		{
			if (targetObject is TextBox tb)
			{
				tb.Text = LocaleEditorCommands.CreateHandle();
			}
			else if (targetObject is ILocaleKeyEntry entry && entry.HandleIsEditable)
			{
				var lastHandle = new LocaleHandleHistory(entry, entry.Handle);
				var nextHandle = new LocaleHandleHistory(entry, entry.Handle);
				entry.Handle = LocaleEditorCommands.CreateHandle();
				Log.Here().Activity($"[{entry.Key}] New handle generated. [{entry.Handle}]");
				entry.Parent.ChangesUnsaved = true;

				CreateSnapshot(() =>
				{
					lastHandle.Key.Handle = lastHandle.Handle;
				}, () =>
				{
					nextHandle.Key.Handle = nextHandle.Handle;
				});
			}

			if (targetObject is IUnfocusable unfocusable)
			{
				unfocusable.Unfocus();
			}
		}

		private void GenerateHandlesThatMatch(string match = "", bool equals = true)
		{
			if (SelectedGroup != null && SelectedGroup.SelectedFile != null)
			{
				List<LocaleHandleHistory> lastHandles = new List<LocaleHandleHistory>();
				List<LocaleHandleHistory> newHandles = new List<LocaleHandleHistory>();

				List<ILocaleKeyEntry> list;

				if (!string.IsNullOrEmpty(match))
				{
					if (equals)
					{
						list = SelectedGroup.SelectedFile.Entries.Where(e => e.HandleIsEditable && e.Selected && e.Handle.Equals(match)).ToList();
					}
					else
					{
						list = SelectedGroup.SelectedFile.Entries.Where(e => e.HandleIsEditable && e.Selected && e.Handle.Contains(match)).ToList();
					}
				}
				else
				{
					list = SelectedGroup.SelectedFile.Entries.Where(e => e.HandleIsEditable && e.Selected).ToList();
				}

				if (list.Count > 0)
				{
					foreach (var entry in list)
					{
						lastHandles.Add(new LocaleHandleHistory(entry, entry.Handle));
						entry.Handle = LocaleEditorCommands.CreateHandle();
						newHandles.Add(new LocaleHandleHistory(entry, entry.Handle));
						Log.Here().Activity($"[{entry.Key}] New handle generated. [{entry.Handle}]");
						entry.Parent.ChangesUnsaved = true;
					}

					CreateSnapshot(() =>
					{
						foreach (var e in lastHandles)
						{
							e.Key.Handle = e.Handle;
						}
					}, () =>
					{
						foreach (var e in newHandles)
						{
							e.Key.Handle = e.Handle;
						}
					});
				}
			}
			else
			{
				Log.Here().Activity("No selected file found. Skipping handle generation.");
			}
		}

		public void GenerateHandles(bool forceRegenerate = false)
		{
			if (!forceRegenerate)
			{
				GenerateHandlesThatMatch(LocaleEditorCommands.UnsetHandle);
			}
			else
			{
				GenerateHandlesThatMatch();
			}
		}
		public void OverrideResHandles()
		{
			GenerateHandlesThatMatch("ResStr_", false);
		}

		public void UpdateSelectedGroupCombinedData(bool all = false)
		{
			if (SelectedGroup != null)
			{
				SelectedGroup.UpdateCombinedData(all);

				if (SelectedGroup != CombinedGroup)
				{
					CombinedGroup.UpdateCombinedData(all);
				}
			}
		}

		public void UpdateCombinedGroup(bool updateCombinedEntries = false, bool autoSelectGroup = false)
		{
			CombinedGroup.DataFiles = new ObservableCollectionExtended<ILocaleFileData>();
			CombinedGroup.DataFiles.AddRange(ModsGroup.DataFiles);
			CombinedGroup.DataFiles.AddRange(PublicGroup.DataFiles);
			CombinedGroup.DataFiles.AddRange(DialogGroup.DataFiles);
			CombinedGroup.DataFiles.AddRange(JournalGroup.DataFiles);
			if (Settings.LoadRootTemplates) CombinedGroup.DataFiles.AddRange(RootTemplatesGroup.DataFiles);
			if (Settings.LoadGlobals) CombinedGroup.DataFiles.AddRange(GlobalTemplatesGroup.DataFiles);
			if (Settings.LoadLevelData) CombinedGroup.DataFiles.AddRange(LevelDataGroup.DataFiles);
			CombinedGroup.DataFiles.AddRange(CustomGroup.DataFiles);
			//CombinedGroup.Visibility = MultipleGroupsEntriesFilled();
			CombinedGroup.Visibility = true;

			if (autoSelectGroup)
			{
				var index = 0;
				if (!CombinedGroup.Visibility)
				{
					if (PublicGroup.Visibility)
					{
						index++;
					}
					else if (ModsGroup.Visibility)
					{
						index++;
					}
					else if (DialogGroup.Visibility)
					{
						index++;
					}
					else if (JournalGroup.Visibility)
					{
						index++;
					}
					else if (RootTemplatesGroup.Visibility)
					{
						index++;
					}
					else if (LevelDataGroup.Visibility)
					{
						index++;
					}
				}
				SelectedGroupIndex = index;
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
			RxApp.TaskpoolScheduler.ScheduleAsync(async (s, t) =>
			{
				var backupSuccess = await LocaleEditorCommands.BackupDataFiles(this, ModuleData.Settings.BackupRootDirectory);
				if (backupSuccess == true)
				{
					int total = SelectedGroup.DataFiles.Where(f => f.ChangesUnsaved).Count();
					var successes = await LocaleEditorCommands.SaveDataFiles(this);
					Log.Here().Important($"Saved {successes} localization files.");
					OutputText = $"Saved {successes}/{total} files.";
					OutputType = LogType.Important;
				}
				else
				{
					OutputText = $"Problem occured when backing up files. Check the log.";
					OutputType = LogType.Error;
				}
				OutputDate = DateTime.Now.ToShortTimeString();

				LocaleEditorCommands.SaveAllLinkedData(ModuleData, LinkedLocaleData);
				return Disposable.Empty;
			});
		}

		public void SaveCurrent()
		{
			if (SelectedFile != null)
			{
				if (SelectedFile is LocaleNodeFileData keyFileData)
				{
					RxApp.TaskpoolScheduler.ScheduleAsync(async (s, t) =>
					{
						var backupSuccess = await LocaleEditorCommands.BackupDataFile(keyFileData.SourcePath, ModuleData.Settings.BackupRootDirectory);
						if (backupSuccess == true)
						{
							int result = await LocaleEditorCommands.SaveDataFile(keyFileData);
							if (result > 0)
							{
								OutputText = $"Saved '{keyFileData.Name}' to '{Directory.GetParent(keyFileData.SourcePath)}'.";
								OutputType = LogType.Important;

								keyFileData.SetChangesUnsaved(false, true);
								UpdateSelectedGroupCombinedData(false);
								ChangesUnsaved = false;
							}
							else
							{
								OutputText = $"Error saving '{keyFileData.SourcePath}'. Check the log.";
								OutputType = LogType.Error;
							}
							OutputDate = DateTime.Now.ToShortTimeString();
						}
						else
						{
							OutputText = $"Problem occured when backing up files. Check the log.";
							OutputType = LogType.Error;
						}

						LocaleEditorCommands.SaveLinkedDataForFile(ModuleData, LinkedLocaleData, SelectedFile);
						return Disposable.Empty;
					});
				}
				else
				{
					Log.Here().Warning($"Selected file saving of type [{SelectedFile.GetType()}] is not currently supported.");
				}
			}
			else
			{
				Log.Here().Error("No file is selected. Skipping saving.");
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
						SelectedGroup.UpdateCombinedData(true);
						SelectedGroup.SelectedFileIndex = SelectedGroup.Tabs.Count - 1;
					}
				}

				FileCommands.Save.OpenSaveDialog(view, "Create Localization File...",
					writeToFile, newFileName, sourceRoot, DOS2DEFileFilters.LarianBinaryFile);
			}
		}

		public void AddNewFile()
		{
			if (SelectedGroup != null)
			{
				var currentGroup = Groups.Where(g => g == SelectedGroup).First();
				var totalFiles = currentGroup.Tabs.Count;
				string futurePath = FileCommands.EnsureExtension(Path.Combine(currentGroup.SourceDirectories.First(), "NewFile" + totalFiles), ".lsb");
				var newFile = LocaleEditorCommands.CreateFileData(currentGroup, futurePath, "NewFile" + totalFiles);
				bool lastChangesUnsaved = currentGroup.ChangesUnsaved;
				bool lastGlobalChangesUnsaved = ChangesUnsaved;

				void undo()
				{
					var list = currentGroup.DataFiles.ToList();
					list.Remove(newFile);
					currentGroup.DataFiles = new ObservableCollectionExtended<ILocaleFileData>(list);

					currentGroup.ChangesUnsaved = lastChangesUnsaved;
					currentGroup.UpdateCombinedData();
					currentGroup.SelectLast();
					view.FocusSelectedTab();

					ChangesUnsaved = lastGlobalChangesUnsaved;
				}

				void redo()
				{
					currentGroup.DataFiles.Add(newFile);

					currentGroup.ChangesUnsaved = true;
					currentGroup.UpdateCombinedData(true);
					//currentGroup.SelectLast();

					currentGroup.SelectedFileIndex = currentGroup.Tabs.IndexOf(newFile);

					if (currentGroup != CombinedGroup)
					{
						CombinedGroup.DataFiles.Add(newFile);
						CombinedGroup.UpdateCombinedData(true);
						CombinedGroup.ChangesUnsaved = true;

						SelectedFileChanged(currentGroup, newFile);
					}

					view.FocusSelectedTab();

					ChangesUnsaved = true;

					newFile.ChangesUnsaved = true;
					newFile.IsRenaming = true;
				}

				CreateSnapshot(undo, redo);
				redo();
			}
		}

		public void ImportFilesAsFileData(IEnumerable<string> files)
		{
			IsSubWindowOpen = false;

			if (files.Count() > 0)
			{
				var currentGroup = Groups.Where(g => g == SelectedGroup).First();
				var newFileDataList = LocaleEditorCommands.ImportFilesAsData(files, SelectedGroup, LinkedProjects);

				var lastGroupChangesUnsaved = currentGroup.ChangesUnsaved;
				var lastChangesUnsaved = ChangesUnsaved;

				var lastFiles = currentGroup.DataFiles.ToList();
				var lastImportPath = CurrentImportPath;
				var lastLinked = LinkedLocaleData.ToList();

				var lastCustom = ActiveProjectSettings.CustomFiles.ToList();

				void undo()
				{
					foreach (var entry in newFileDataList)
					{
						if (lastFiles.Contains(entry)) lastFiles.Remove(entry);
					}
					currentGroup.DataFiles = new ObservableCollectionExtended<ILocaleFileData>(lastFiles);
					currentGroup.UpdateCombinedData(true);
					currentGroup.SelectLast();
					currentGroup.ChangesUnsaved = lastChangesUnsaved;
					view.FocusSelectedTab();

					if (currentGroup.IsCustom)
					{
						ActiveProjectSettings.CustomFiles.Clear();
						ActiveProjectSettings.CustomFiles.AddRange(lastCustom);
					}

					if (currentGroup != CombinedGroup)
					{
						CombinedGroup.UpdateCombinedData(true);
						currentGroup.ChangesUnsaved = true;
					}

					ChangesUnsaved = lastChangesUnsaved;

					SaveLastFileImportPath(lastImportPath);

					LinkedLocaleData = lastLinked;
				}

				void redo()
				{
					if (newFileDataList.Count > 0)
					{
						foreach (var entry in newFileDataList)
						{
							entry.ChangesUnsaved = true;
							if (entry.HasFileLink && entry is LocaleNodeFileData nodeFileData)
							{
								if (nodeFileData.ModProject != null)
								{
									var linkedList = LinkedLocaleData.FirstOrDefault(x => nodeFileData.ModProject.UUID == x.ProjectUUID);
									if (linkedList == null)
									{
										linkedList = new LocaleProjectLinkData()
										{
											ProjectUUID = nodeFileData.ModProject.UUID,
										};
										LinkedLocaleData.Add(linkedList);
									}

									linkedList.Links.RemoveAll(x => x.TargetFile == nodeFileData.FileLinkData.TargetFile);
									linkedList.Links.Add(nodeFileData.FileLinkData);

									LocaleEditorCommands.SaveLinkedData(ModuleData, linkedList);
								}
							}
						}

						var newFile = newFileDataList.Last();

						currentGroup.DataFiles.AddRange(newFileDataList);
						currentGroup.UpdateCombinedData(true);
						currentGroup.ChangesUnsaved = true;
						currentGroup.SelectedFileIndex = currentGroup.Tabs.IndexOf(newFile);
						view.FocusSelectedTab();

						if (currentGroup != CombinedGroup)
						{
							CombinedGroup.DataFiles.AddRange(newFileDataList);
							CombinedGroup.UpdateCombinedData(true);
							CombinedGroup.ChangesUnsaved = true;

							SelectedFileChanged(currentGroup, newFile);
						}

						if (currentGroup.IsCustom)
						{
							ActiveProjectSettings.CustomFiles.Add(newFile.SourcePath);
						}

						ChangesUnsaved = true;
					}

					string lastImport = files.FirstOrDefault();
					SaveLastFileImportPath(Path.GetDirectoryName(lastImport));

					this.RaisePropertyChanged("CurrentImportPath");
					this.RaisePropertyChanged("CurrentFileImportPath");
				}

				CreateSnapshot(undo, redo);
				redo();
			}

			IsAddingNewFileTab = false;
			IsSubWindowOpen = false;
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
					string delimiter = "\t";
					if (FileCommands.FileExtensionFound(path, ".csv"))
					{
						delimiter = ",";
					}
					string contents = $"Key{delimiter}Content\n";
					bool handleMode = localeFileData.Entries.Any(x => x.Handle != LocaleEditorCommands.UnsetHandle);
					if (handleMode)
					{
						contents = $"Key{delimiter}Content{delimiter}Handle\n";
					}
					var entries = localeFileData.Entries.OrderBy(x => x.Key).ToList();
					for (var i = 0; i < entries.Count; i++)
					{
						var entry = entries[i];
						if (!handleMode)
						{
							contents += entry.Key + delimiter + entry.Content;
						}
						else
						{
							contents += entry.Key + delimiter + entry.Content + delimiter + entry.Handle;
						}
						if (i < entries.Count - 1) contents += Environment.NewLine;
					}

					if (FileCommands.WriteToFile(path, contents.Trim()))
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

					SaveLastFileImportPath(Path.GetDirectoryName(path), localeFileData);
				}
			}

			string startImportPath = GetLastFileImportPath(localeFileData);

			//Log.Here().Activity("Import path: " + startImportPath);

			FileCommands.Save.OpenSaveDialog(view, "Save Locale File As...",
				writeToFile, exportName, startImportPath, DOS2DEFileFilters.AllLocaleFilesList.ToArray());
		}

		public void ExportAllToSpreadsheet()
		{
			var project = GetMainProject();
			string exportName = Path.GetFileNameWithoutExtension(project.ProjectName) + "-All.tsv";
			IsSubWindowOpen = true;
			void writeToFile(FileDialogResult result, string path)
			{
				IsSubWindowOpen = false;

				if (result == FileDialogResult.Ok)
				{
					string delimiter = "\t";
					if (FileCommands.FileExtensionFound(path, ".csv"))
					{
						delimiter = ",";
					}
					string contents = $"Key{delimiter}Content{delimiter}Handle{delimiter}Comment{delimiter}File/Template Name{delimiter}File Path\n";
					var entries = CombinedGroup.CombinedEntries.Entries.OrderBy(x => x.Parent.SourcePath).ToList();
					for (var i = 0; i < entries.Count; i++)
					{
						var entry = entries[i];
						var localPath = entry.Parent.SourcePath.Replace(ModuleData.Settings.DOS2DEDataDirectory + "\\", "");
						contents += string.Join(delimiter, entry.Key, entry.Content, entry.Handle, "", entry.Parent.Name, localPath);
						if (i < entries.Count - 1) contents += Environment.NewLine;
					}

					if (FileCommands.WriteToFile(path, contents.Trim()))
					{
						OutputType = LogType.Important;
						OutputText = $"Saved locale spreadsheet to '{path}'.";
						OutputDate = DateTime.Now.ToShortTimeString();
					}
					else
					{
						OutputType = LogType.Error;
						OutputText = $"Problem saving file '{path}'.";
						OutputDate = DateTime.Now.ToShortTimeString();
					}

					SaveLastFileImportPath(Path.GetDirectoryName(path));
				}
			}

			string startImportPath = GetLastFileImportPath();

			FileCommands.Save.OpenSaveDialog(view, "Save Locale Spreadsheet As...",
				writeToFile, exportName, startImportPath, DOS2DEFileFilters.AllLocaleFilesList.ToArray());
		}

		public void ImportFilesAsKeys(IEnumerable<string> files)
		{
			IsSubWindowOpen = false;

			if (SelectedFile != null && files.Count() > 0)
			{
				Log.Here().Activity("Importing keys from files...");

				var currentItem = SelectedFile;
				var newKeys = LocaleEditorCommands.ImportFilesAsEntries(files, SelectedFile);

				CreateSnapshot(() =>
				{
					foreach (var k in newKeys)
					{
						currentItem.Entries.Remove(k);
					}
				}, () =>
				{
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
				SelectedGroup?.UpdateCombinedData();

				//CurrentImportPath = Path.GetDirectoryName(files.FirstOrDefault());

				SaveLastFileImportPath(Path.GetDirectoryName(files.FirstOrDefault()), SelectedFile);

				this.RaisePropertyChanged("CurrentImportPath");
				this.RaisePropertyChanged("CurrentEntryImportPath");

				ChangesUnsaved = true;
			}
		}

		public ICommand AddFileCommand { get; private set; }
		public ICommand AddFileToGroupCommand { get; private set; }
		public ICommand ConfirmFileAddToGroupCommand { get; private set; }
		public ICommand CancelFileAddToGroupCommand { get; private set; }
		public ICommand CloseFileCommand { get; private set; }
		public ICommand ImportFileCommand { get; internal set; }
		public ICommand ImportKeysCommand { get; internal set; }
		public ICommand ExportAllXMLCommand { get; private set; }
		public ICommand ExportSelectedXMLCommand { get; private set; }
		public ICommand CheckForDuplicateKeysCommand { get; private set; }

		public ICommand SaveAllCommand { get; private set; }
		public ICommand SaveCurrentCommand { get; private set; }
		public ICommand SaveSettingsCommand { get; private set; }
		public ICommand GenerateHandlesCommand { get; private set; }
		public ICommand ForceGenerateHandlesCommand { get; private set; }
		public ICommand OverrideResHandlesCommand { get; private set; }

		public ICommand AddNewKeyCommand { get; private set; }
		public ICommand DeleteKeysCommand { get; private set; }
		public ICommand RefreshFileCommand { get; private set; }
		public ICommand ReloadFileCommand { get; private set; }
		public ICommand ReloadFileLinkDataCommand { get; private set; }
		public ICommand SetFileLinkDataCommand { get; private set; }
		public ICommand RemoveFileLinkDataCommand { get; private set; }
		public ICommand RefreshAllLinkedDataCommand { get; private set; }

		public ICommand OpenPreferencesCommand { get; private set; }

		//public ICommand ExpandContentCommand { get; set; }

		public ICommand AddFontTagCommand { get; private set; }

		// Content Context Menu
		public ICommand ToggleContentLightModeCommand { get; private set; }
		public ICommand ChangeContentFontSizeCommand { get; private set; }
		public ICommand PopoutContentCommand { get; set; }
		public ICommand OpenFileInExplorerCommand { get; private set; }
		public ICommand CopyToClipboardCommand { get; private set; }
		public ICommand ToggleRenameFileTabCommand { get; private set; }
		public ICommand ConfirmRenameFileTabCommand { get; private set; }
		public ICommand CancelRenamingFileTabCommand { get; private set; }
		public ICommand SelectNoneCommand { get; private set; }
		public ICommand NewHandleCommand { get; private set; }
		public ICommand ResetHandleCommand { get; private set; }
		public ICommand ExportFileAsTextualCommand { get; private set; }
		public ICommand ExportAllAsSpreadsheetCommand { get; private set; }
		public ICommand OnClipboardChangedCommand { get; private set; }
		public ICommand PasteIntoKeysCommand { get; private set; }
		public ICommand PasteIntoContentCommand { get; private set; }
		public ICommand PasteIntoHandlesCommand { get; private set; }

		public ICommand GenerateXMLCommand { get; private set; }
		public ICommand SaveXMLCommand { get; private set; }
		public ICommand SaveXMLAsCommand { get; private set; }
		public ICommand OpenXMLFolderCommand { get; private set; }
		public ICommand LanguageCheckedCommand { get; private set; }

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
				if (SelectedGroup.SelectedFile is LocaleNodeFileData fileData)
				{
					Log.Here().Activity($"File format is '{fileData.Format}'");
					LocaleNodeKeyEntry localeEntry = LocaleEditorCommands.CreateNewLocaleEntry(fileData);
					localeEntry.SetHistoryFromObject(this);
					AddWithHistory(fileData.Entries, localeEntry);
					SelectedGroup.UpdateCombinedData();
				}
				else if (SelectedGroup.SelectedFile is LocaleCustomFileData customFileData)
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
				else
				{
					Log.Here().Error($"Can't add to file type '{SelectedGroup.SelectedFile.GetType()}'");
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
					if (SelectedGroup.SelectedFile == SelectedGroup.CombinedEntries)
					{
						var selectedEntries = SelectedGroup.DataFiles.SelectMany(f => f.Entries.Where(x => x.Selected)).ToList();
						Log.Here().Important($"Deleting {selectedEntries.Count} keys.");
						List<LocaleEntryHistory> lastState = new List<LocaleEntryHistory>();

						var lastSelectedGroup = SelectedGroup;

						foreach (var entry in selectedEntries)
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
							foreach (var x in lastState)
							{
								var fileEntry = SelectedGroup.DataFiles.FirstOrDefault(f => f.SourcePath == x.ParentFile.SourcePath);
								if (fileEntry != null)
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
							lastSelectedGroup.UpdateCombinedData();
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
							lastSelectedGroup.UpdateCombinedData();
						}
						CreateSnapshot(undo, redo);
						redo();
					}
					else
					{
						var lastSelectedGroup = SelectedGroup;
						var selectedFile = lastSelectedGroup.SelectedFile;

						var last = selectedFile.Entries.ToList();
						var deleteEntries = selectedFile.Entries.Where(x => x.Selected).ToList();
						var lastChangesUnsaved = selectedFile.ChangesUnsaved;
						void undo()
						{
							selectedFile.Entries.Clear();
							selectedFile.Entries.AddRange(last);
							lastSelectedGroup.UpdateCombinedData();
							selectedFile.ChangesUnsaved = lastChangesUnsaved;
						}
						void redo()
						{
							//selectedFile.Entries.RemoveAll(x => deleteEntries.Contains(x));
							Log.Here().Important($"Deleting {deleteEntries.Count} keys.");
							for (var i = 0; i < deleteEntries.Count; i++)
							{
								var entry = deleteEntries[i];
								selectedFile.Entries.Remove(entry);
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
							lastSelectedGroup.UpdateCombinedData();
							selectedFile.ChangesUnsaved = true;
							//Log.Here().Activity($"Entries {selectedFile.Entries.Count} | {next.Count()}.");
						}
						CreateSnapshot(undo, redo);
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

		public List<string> Languages { get; set; } = new List<string>
		{
			"English",
			"Amlatspanish",
			"Chinese",
			"Chinesetraditional",
			"Czech",
			"French",
			"German",
			"Italian",
			"Japanese",
			"Korean",
			"Polish",
			"Russian",
			"Spanish",
		};

		public void GenerateXML()
		{
			if (view != null && view.ExportWindow != null)
			{
				ExportText = LocaleEditorCommands.ExportDataAsXML(this, view.ExportWindow.ExportAll, EnumLocaleLanguages.None);
				OutputText = $"Generated XML text.";
				OutputType = LogType.Important;
				OutputDate = DateTime.Now.ToShortTimeString();
			}
		}

		private bool SaveXMLFileTo(string localizationRoot, string language, bool showDialogWhenOverwriting = false)
		{
			string target = Path.Combine(localizationRoot, language, language.ToLower() + ".xml");
			EnumLocaleLanguages lang = (EnumLocaleLanguages)Enum.Parse(typeof(EnumLocaleLanguages), language);

			bool writeFile = true;

			if (showDialogWhenOverwriting && File.Exists(target))
			{
				writeFile = false;
				var result = MessageBoxEx.Show(view.ExportWindow, $"Overwrite {target}?", "This cannot be undone.", MessageBoxButton.YesNo);
				if (result == MessageBoxResult.Yes)
				{
					writeFile = true;
				}
			}

			if (writeFile)
			{
				ExportText = LocaleEditorCommands.ExportDataAsXML(this, view.ExportWindow.ExportAll, lang);
				if (FileCommands.WriteToFile(target, ExportText))
				{
					Log.Here().Important($"Saved language xml at {target}.");
					return true;
				}
				else
				{
					Log.Here().Error($"Error when saving {target}. Chck the log.");
				}
			}
			return false;
		}

		public void SaveXMLFile(string target = "")
		{
			var mainProject = GetMainProject();
			string localizationRoot = Path.GetFullPath(Path.Combine(ModuleData.Settings.DOS2DEDataDirectory, "Mods", mainProject.FolderName, "Localization"));

			if (!string.IsNullOrEmpty(target))
			{
				if (FileCommands.IsValidFilePath(target))
				{
					localizationRoot = Path.GetDirectoryName(target);
				}
				else if (FileCommands.IsValidDirectoryPath(target))
				{
					localizationRoot = target;
				}
			}

			var languages = ActiveProjectSettings.TargetLanguages.Split(';');
			var totalSuccess = 0;
			foreach (var lan in languages)
			{
				if (SaveXMLFileTo(localizationRoot, lan, false))
				{
					totalSuccess++;
				}
			}
			if (totalSuccess >= languages.Count())
			{
				OutputText = $"Saved all languages files.";
				OutputType = LogType.Important;
				OutputDate = DateTime.Now.ToShortTimeString();
			}
			else
			{
				OutputText = $"Error saving one or more files. Check the log.";
				OutputType = LogType.Error;
				OutputDate = DateTime.Now.ToShortTimeString();
			}
		}

		public void OpenExportWindow(bool exportAll = false)
		{
			ActiveProjectSettings = Settings.GetProjectSettings(LinkedProjects.FirstOrDefault());
			ActiveProjectSettings.SaveSettings = view.SaveSettings;

			ExportText = LocaleEditorCommands.ExportDataAsXML(this, exportAll);

			if (view != null && view.ExportWindow != null)
			{
				if (!view.ExportWindow.IsVisible)
				{
					view.ExportWindow.Show();
					view.ExportWindow.Owner = view;
					view.ExportWindow.ResetBindings();
					OutputText = "Generated XML text / opened export window.";
					OutputType = LogType.Important;
					OutputDate = DateTime.Now.ToShortTimeString();
				}
				else
				{
					OutputText = "Generated XML text.";
					OutputType = LogType.Important;
					OutputDate = DateTime.Now.ToShortTimeString();
				}
				view.ExportWindow.ExportAll = exportAll;
			}
		}

		public void AddCustomFileToGroup(LocaleTabGroup group)
		{
			if (!IsAddingNewFileTab)
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
					if (lastSelected != null)
					{
						lastSelected.Content = lastText;

						if (lastSelected == SelectedEntry)
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

		private bool openMissingEntriesViewOnLoad = false;
		private List<ILocaleKeyEntry> addMissingEntriesOnLoad = null;

		public ObservableCollectionExtended<ILocaleKeyEntry> MissingEntries { get; set; } = new ObservableCollectionExtended<ILocaleKeyEntry>();

		private Visibility missingEntriesViewVisible = Visibility.Collapsed;

		public Visibility MissingEntriesViewVisible
		{
			get => missingEntriesViewVisible;
			set { this.RaiseAndSetIfChanged(ref missingEntriesViewVisible, value); }
		}

		public ICommand RemoveSelectedMissingEntriesCommand { get; set; }
		public ICommand CloseMissingEntriesCommand { get; set; }
		public ICommand CopySimpleMissingEntriesCommand { get; set; }
		public ICommand CopyAllMissingEntriesCommand { get; set; }

		private string problemEntriesViewHeaderText;

		public string ProblemEntriesViewHeaderText
		{
			get => problemEntriesViewHeaderText;
			set { this.RaiseAndSetIfChanged(ref problemEntriesViewHeaderText, value); }
		}

		private string problemEntriesViewInfoText1;

		public string ProblemEntriesViewInfoText1
		{
			get => problemEntriesViewInfoText1;
			set { this.RaiseAndSetIfChanged(ref problemEntriesViewInfoText1, value); }
		}

		public string ProblemEntriesViewInfoText2
		{
			get => problemEntriesViewInfoText1;
			set { this.RaiseAndSetIfChanged(ref problemEntriesViewInfoText1, value); }
		}

		public void ShowMissingEntriesView(List<ILocaleKeyEntry> missingEntries, bool IsDuplicate = false)
		{
			if (!IsDuplicate)
			{
				ProblemEntriesViewHeaderText = "Missing Source Keys";
				ProblemEntriesViewInfoText1 = "These keys exist in a source file, but are missing from linked files.";
				ProblemEntriesViewInfoText2 = "Select which keys, if any, to remove from source files, or copy them to the clipboard.";
			}
			else
			{
				ProblemEntriesViewHeaderText = "Duplicate Keys";
				ProblemEntriesViewInfoText1 = "Duplicate keys in the same file result in an error in the Divinity Engine.";
				ProblemEntriesViewInfoText2 = "Select which keys, if any, to remove from source files, or copy them to the clipboard.";
			}

			if (missingEntries.Count > 0)
			{
				if (view != null && view.IsVisible)
				{
					openMissingEntriesViewOnLoad = false;
					foreach (var entry in missingEntries)
					{
						//Log.Here().Activity($"Checking for key: {entry.Key} | {RemovedEntries.Any(x => x.Key == entry.Key)}");
						//if (!MissingEntries.Any(x => x.Key == entry.Key))
						if (!MissingEntries.Contains(entry))
						{
							MissingEntries.Add(entry);
							entry.Selected = !IsDuplicate;
						}
					}

					MissingEntriesViewVisible = Visibility.Visible;

					if (!IsDuplicate)
					{
						Log.Here().Important($"Total missing entries: '{MissingEntries.Count}'.");
					}
					else
					{
						Log.Here().Important($"Total duplicate entries: '{MissingEntries.Count}'.");
					}

					//Log.Here().Important($"Removed entries:{String.Join(Environment.NewLine, RemovedEntries.Select(x => x.EntryKey))}");
				}
				else
				{
					openMissingEntriesViewOnLoad = true;
					if (addMissingEntriesOnLoad == null)
					{
						addMissingEntriesOnLoad = new List<ILocaleKeyEntry>();
					}
					addMissingEntriesOnLoad.AddRange(missingEntries);
				}
			}
		}

		public void ConfirmRemoveSelectedMissingEntries()
		{
			if (MissingEntries != null && MissingEntries.Any(x => x.Selected))
			{
				var lastRemovedEntries = MissingEntries.ToList();
				var selectedRemovedEntries = lastRemovedEntries.Where(x => x.Selected).ToList();

				List<LocaleEntryHistory> lastState = new List<LocaleEntryHistory>();

				foreach (var entry in selectedRemovedEntries)
				{
					if (entry.Parent.IsCombinedData)
					{
						var realParent = Groups.Select(g => g.DataFiles.FirstOrDefault(d => !d.IsCombinedData && d.Entries.Any(x => x.Handle == entry.Handle && x.Key == entry.Key))).FirstOrDefault();
						if (realParent != null)
						{
							var refEntry = realParent.Entries.First(x => x.Handle == entry.Handle && x.Key == entry.Key);
							lastState.Add(new LocaleEntryHistory
							{
								ParentFile = realParent,
								Entry = entry,
								Index = realParent.Entries.IndexOf(refEntry),
								ChangesUnsaved = entry.ChangesUnsaved,
								ParentChangesUnsaved = realParent.ChangesUnsaved
							});
						}
						else
						{
							lastState.Add(new LocaleEntryHistory
							{
								ParentFile = entry.Parent,
								Entry = entry,
								Index = entry.Index,
								ChangesUnsaved = entry.ChangesUnsaved,
								ParentChangesUnsaved = entry.Parent.ChangesUnsaved
							});
						}
					}
					else
					{
						lastState.Add(new LocaleEntryHistory
						{
							ParentFile = entry.Parent,
							Entry = entry,
							Index = entry.Index,
							ChangesUnsaved = entry.ChangesUnsaved,
							ParentChangesUnsaved = entry.Parent.ChangesUnsaved
						});
					}
				}

				void undo()
				{
					foreach (var x in lastState)
					{
						var fileEntries = Groups.SelectMany(g => g.DataFiles.Where(f => f != null && !f.IsCombinedData && f.SourcePath == x.ParentFile.SourcePath)).ToList();
						if (fileEntries.Count > 0)
						{
							foreach (var fileEntry in fileEntries)
							{
								if (fileEntry != null)
								{
									if (x.ChangesUnsaved) fileEntry.ChangesUnsaved = x.ChangesUnsaved;
									if (fileEntry.Entries.Count > x.Index)
									{
										try
										{
											fileEntry.Entries.Insert(x.Index, x.Entry);
										}
										catch (Exception ex)
										{
											fileEntry.Entries.Add(x.Entry);
										}
									}
									else
									{
										fileEntry.Entries.Add(x.Entry);
									}
									fileEntry.ChangesUnsaved = x.ParentChangesUnsaved;
									if (fileEntry is LocaleNodeFileData nodeFileData && x.Entry is LocaleNodeKeyEntry nodeKeyEntry)
									{
										nodeFileData.RootRegion.AppendChild(nodeKeyEntry.Node);
									}
								}
							}
						}
					}

					SelectedGroup?.UpdateCombinedData();

					MissingEntries.Clear();
					MissingEntries.AddRange(lastRemovedEntries);
					foreach (var entry in MissingEntries)
					{
						entry.Selected = selectedRemovedEntries.Contains(entry);
					}
					MissingEntriesViewVisible = Visibility.Visible;
				}

				void redo()
				{
					foreach (var entry in selectedRemovedEntries)
					{
						var parent = entry.Parent;
						if (parent != null)
						{
							parent.Entries.Remove(entry);

							if (entry is LocaleNodeKeyEntry nodeKeyEntry && parent is LocaleNodeFileData nodeFileData)
							{
								var nodeContainer = nodeFileData.RootRegion.Children.Values.Where(l => l.Contains(nodeKeyEntry.Node));
								foreach (var list in nodeContainer)
								{
									list.Remove(nodeKeyEntry.Node);
								}
							}
							parent.ChangesUnsaved = true;
						}
					}

					SelectedGroup?.UpdateCombinedData();
				}
				CreateSnapshot(undo, redo);
				redo();
			}
			MissingEntriesViewVisible = Visibility.Collapsed;
			MissingEntries.Clear();
		}

		public void CloseMissingEntriesView()
		{
			MissingEntriesViewVisible = Visibility.Collapsed;
			MissingEntries.Clear();
		}

		public string GetLastFileImportPath(ILocaleFileData fileData = null)
		{
			ModProjectData project = null;

			if (fileData == null) fileData = SelectedFile;

			if (fileData != null && fileData is LocaleNodeFileData data)
			{
				project = LinkedProjects.FirstOrDefault(x => data.SourcePath.Contains(x.FolderName));
			}
			else if (SelectedGroup != null)
			{
				project = LinkedProjects.FirstOrDefault(x => SelectedGroup.SourceDirectories.Any(y => y.Contains(x.FolderName)));
			}

			if (project != null)
			{
				var settings = Settings.GetProjectSettings(project);
				if (settings != null)
				{
					return settings.LastEntryImportPath;
				}
				else
				{
					string startPath = Path.Combine(ModuleData.Settings.DOS2DEDataDirectory, "Projects");
					string directory = Path.Combine(Path.GetFullPath(startPath), project.ProjectName);
					if (!Directory.Exists(directory))
					{
						directory = Path.Combine(Path.GetFullPath(startPath), project.ModuleInfo.Folder); // Imported projects
					}
					if (Directory.Exists(directory))
					{
						return directory;
					}
				}
			}
			return CurrentImportPath;
		}

		private void SaveLastFileImportPath(string lastFileImportPath, ILocaleFileData fileData = null)
		{
			ModProjectData project = null;

			if (fileData == null) fileData = SelectedFile;

			if (fileData != null && fileData is LocaleNodeFileData data)
			{
				project = LinkedProjects.FirstOrDefault(x => data.SourcePath.Contains(x.FolderName));
			}
			else if (SelectedGroup != null)
			{
				project = LinkedProjects.FirstOrDefault(x => SelectedGroup.SourceDirectories.Any(y => y.Contains(x.FolderName)));
			}

			if (project != null)
			{
				var settings = Settings.GetProjectSettings(project);
				if (settings != null)
				{
					if (!Directory.Exists(lastFileImportPath))
					{
						settings.LastEntryImportPath = Path.GetDirectoryName(lastFileImportPath);
					}
					else
					{
						settings.LastEntryImportPath = lastFileImportPath;
					}
					Log.Here().Activity($"Saved settings.LastEntryImportPath: {settings.LastEntryImportPath} | {lastFileImportPath}");
					view.SaveSettings();
				}
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

					SaveLastFileImportPath(Path.GetDirectoryName(filePath), fileData);

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
						var removed = LocaleEditorCommands.RefreshLinkedData(fileData);

						if (fileData is LocaleNodeFileData nodeFileData)
						{
							if (nodeFileData.ModProject != null && !string.IsNullOrEmpty(nodeFileData.FileLinkData.ReadFrom))
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

								linkedList.Links.RemoveAll(x => x.TargetFile == fileData.FileLinkData.TargetFile);
								linkedList.Links.Add(fileData.FileLinkData);

								LocaleEditorCommands.SaveLinkedData(ModuleData, linkedList);
							}
						}

						ShowMissingEntriesView(removed);
					}

					CreateSnapshot(undo, redo);
					redo();
				}
			}
			IsSubWindowOpen = true;

			string entryImportPath = GetLastFileImportPath(fileData);
			FileCommands.Load.OpenFileDialog(view, "Pick localization file to link...",
					entryImportPath, OnFileSelected, "", new Action<string, FileDialogResult>((s, r) => IsSubWindowOpen = false), CommonFileFilters.DelimitedLocaleFiles);
		}

		public void RefreshLinkedData(ILocaleFileData fileData)
		{
			if (!string.IsNullOrEmpty(fileData.FileLinkData.ReadFrom))
			{
				List<TextualLocaleEntry> lastValues = fileData.Entries.Select(x => new TextualLocaleEntry { Key = x.Key, Content = x.Content, Handle = x.Handle }).ToList();
				void undo()
				{
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
					var removed = LocaleEditorCommands.RefreshLinkedData(fileData);
					ShowMissingEntriesView(removed);
				}
				CreateSnapshot(undo, redo);
				redo();
			}
		}

		public void RefreshAllLinkedData()
		{
			foreach (var f in CombinedGroup.DataFiles)
			{
				RefreshLinkedData(f);
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
						foreach (var pdata in containingLists)
						{
							pdata.Links.Add(fileData.FileLinkData);
						}
						fileData.HasFileLink = true;
					}

					void redo()
					{
						var removed = false;
						foreach (var pdata in containingLists)
						{
							pdata.Links.Remove(fileData.FileLinkData);
							removed = true;
						}

						fileData.HasFileLink = false;

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
					if (FileCommands.RenameFile(fileData.SourcePath, nextFilePath))
					{
						Log.Here().Activity($"Renamed file '{fileData.SourcePath}' => '{nextFilePath}'.");
					}
					else
					{
						Log.Here().Error($"Error renaming file '{fileData.SourcePath}' => '{nextFilePath}'.");
					}
				}

				fileData.Name = fileData.RenameText;
				fileData.SourcePath = nextFilePath;
				fileData.IsRenaming = false;

				fileData.ChangesUnsaved = true;
				if (fileData.Parent != null) fileData.Parent.ChangesUnsaved = true;
				ChangesUnsaved = true;
			}
			CreateSnapshot(undo, redo);
			redo();
		}
		public void UpdateUnsavedChanges()
		{
			ChangesUnsaved = Groups.Any(g => g.ChangesUnsaved == true);
		}

		public void ReloadFileData(LocaleNodeFileData fileData)
		{
			FileCommands.OpenConfirmationDialog(view, "Reload Data?", "This current file will be reverted to what is saved, potentially restoring or changing keys.", "Unsaved changes will be lost.", (b) =>
			{
				if (b)
				{
					bool combinedSelected = SelectedGroup == CombinedGroup;
					var selectedGroup = SelectedGroup;
					var targetGroup = !combinedSelected ? SelectedGroup : Groups.FirstOrDefault(g => g.DataFiles.Contains(fileData));
					var oldFile = fileData;
					int index = targetGroup.DataFiles.IndexOf(fileData);
					int selectedIndex = selectedGroup.SelectedFileIndex;

					void undo()
					{
						targetGroup.DataFiles[index] = oldFile;
						targetGroup.UpdateCombinedData();
						if (selectedGroup != targetGroup) CombinedGroup.UpdateCombinedData();
						selectedGroup.SelectedFileIndex = selectedIndex;

						OutputText = $"Reverted file '{oldFile.SourcePath}'.";
						OutputType = LogType.Important;
					}

					void redo()
					{
						var newFile = LocaleEditorCommands.LoadResource(oldFile.Parent, oldFile.SourcePath, oldFile.Parent.Name == "Journal");
						var oldEntries = oldFile.Entries.ToList();
						oldFile.Entries.Clear();
						oldFile.Entries.AddRange(newFile.Entries);
						var changesUnsaved = 0;
						foreach (var entry in oldFile.Entries)
						{
							if (!oldEntries.Any(x => x.ValuesMatch(entry)))
							{
								entry.ChangesUnsaved = true;
								changesUnsaved++;
							}
						}
						targetGroup.UpdateCombinedData();
						if (selectedGroup != targetGroup) selectedGroup.UpdateCombinedData();
						selectedGroup.SelectedFileIndex = selectedIndex;

						OutputText = $"Reloaded file '{newFile.SourcePath}'";
						OutputType = LogType.Important;

						if (changesUnsaved > 0)
						{
							oldFile.ChangesUnsaved = targetGroup.ChangesUnsaved = ChangesUnsaved = true;
						}

						OnHideExtras(HideExtras);
					}
					CreateSnapshot(undo, redo);
					redo();
				}
				else
				{
					OutputText = $"Canceled reloading file.";
					OutputType = LogType.Activity;
				}
			});
		}

		private bool CanDisplayEntry(ILocaleKeyEntry entry)
		{
			if (!entry.KeyIsEditable && entry.Key.Equals("GameMasterSpawnSubSection") || !entry.HandleIsEditable)
			{
				return false;
			}
			else
			{
				if (entry.Parent is LocaleNodeFileData fileData)
				{
					if (fileData.Format == ResourceFormat.LSJ)
					{
						return true;
					}
					else if (fileData.Format == ResourceFormat.LSX)
					{
						return !LocaleEditorCommands.IgnoreHandle(entry.Handle, fileData);
					}
				}
			}
			return true;
		}

		private void OnHideExtras(bool enabled)
		{
			foreach (var g in Groups)
			{
				foreach (var f in g.Tabs)
				{
					foreach (var entry in f.Entries)
					{
						if (enabled)
						{
							entry.Visible = CanDisplayEntry(entry);
						}
						else
						{
							entry.Visible = true;
						}
					}
				}
			}
		}

		public void RefreshFileData(LocaleNodeFileData fileData)
		{
			FileCommands.OpenConfirmationDialog(view, "Refresh Data?", "Data for the current file's keys will be reloaded from the saved data, potentially changing keys, content, or handles.", "", (b) =>
			{
				if (b)
				{
					var selectedGroup = SelectedGroup;
					string lastFileSource = fileData.SourcePath;
					var entries = LocaleEditorCommands.LoadFromResource(fileData.Source, fileData.Format, false, fileData.Parent.Name == "Journal");

					if (entries.Count > 0)
					{
						List<LocaleEntryHistory> lastEntries = new List<LocaleEntryHistory>();
						List<ILocaleKeyEntry> newEntries = new List<ILocaleKeyEntry>();

						foreach (var entry in entries)
						{
							ILocaleKeyEntry existingEntry = null;
							if (fileData.Format == ResourceFormat.LSJ || fileData.Format == ResourceFormat.LSF)
							{
								existingEntry = fileData.Entries.FirstOrDefault(x => x.Content == entry.Content || x.Handle == entry.Handle &&
									x.Handle != LocaleEditorCommands.UnsetHandle);
							}
							else
							{
								existingEntry = fileData.Entries.FirstOrDefault(x => x.Key == entry.Key || x.Handle == entry.Handle &&
									x.Handle != LocaleEditorCommands.UnsetHandle);
							}

							if (existingEntry != null)
							{
								if (!existingEntry.ValuesMatch(entry))
								{
									lastEntries.Add(new LocaleEntryHistory
									{
										ParentFile = existingEntry.Parent,
										Entry = existingEntry,
										Index = existingEntry.Index,
										ChangesUnsaved = existingEntry.ChangesUnsaved,
										ParentChangesUnsaved = existingEntry.Parent.ChangesUnsaved,
										LastKey = entry.Key,
										LastContent = entry.Content,
										LastHandle = entry.Handle,
									});
								}
							}
							else
							{
								newEntries.Add(entry);
							}
						}

						void undo()
						{
							var fileEntry = selectedGroup.DataFiles.FirstOrDefault(f => f.SourcePath == lastFileSource);
							if (fileEntry != null)
							{
								foreach (var x in lastEntries)
								{
									if (x.Entry != null)
									{
										x.Entry.ChangesUnsaved = x.ChangesUnsaved;
										x.Entry.Key = x.LastKey;
										x.Entry.Content = x.LastContent;
										x.Entry.Handle = x.LastHandle;

										if (!fileEntry.Entries.Contains(x.Entry))
										{
											fileEntry.Entries.Insert(x.Index, x.Entry);
										}

										try
										{
											if (fileEntry is LocaleNodeFileData nodeFileData && x.Entry is LocaleNodeKeyEntry nodeKeyEntry
											&& !nodeFileData.RootRegion.Children.FirstOrDefault().Value.Contains(nodeKeyEntry.Node))
											{
												nodeFileData.RootRegion.AppendChild(nodeKeyEntry.Node);
											}
										}
										catch (Exception ex) { }
									}

									fileEntry.ChangesUnsaved = x.ParentChangesUnsaved;
								}
							}
						}

						void redo()
						{
							if (lastEntries.Count > 0 || newEntries.Count > 0)
							{
								int changes = 0;
								if (lastEntries.Count > 0)
								{
									foreach (var entry in entries)
									{
										ILocaleKeyEntry existingEntry = null;
										if (fileData.Format == ResourceFormat.LSJ)
										{
											existingEntry = fileData.Entries.FirstOrDefault(x => x.Content == entry.Content || x.Handle == entry.Handle &&
												x.Handle != LocaleEditorCommands.UnsetHandle);
										}
										else if (fileData.Format == ResourceFormat.LSX)
										{
											existingEntry = fileData.Entries.FirstOrDefault(x => x.Key == entry.Key && x.Handle == entry.Handle && entry.Content == x.Content);
										}
										else
										{
											existingEntry = fileData.Entries.FirstOrDefault(x => x.Key == entry.Key || x.Handle == entry.Handle &&
												x.Handle != LocaleEditorCommands.UnsetHandle);
										}

										if (fileData.Format == ResourceFormat.LSF || fileData.Format == ResourceFormat.LSJ)
										{
											existingEntry.Content = entry.Content;
											existingEntry.ChangesUnsaved = true;
										}
										else
										{
											existingEntry.Key = entry.Key;
											existingEntry.Content = entry.Content;
											existingEntry.ChangesUnsaved = true;
										}

										existingEntry.Handle = entry.Handle;

										if (existingEntry.ChangesUnsaved) changes++;
									}
								}

								if (newEntries.Count > 0)
								{
									foreach (var entry in newEntries)
									{
										fileData.Entries.Add(entry);
										changes++;
									}

									fileData.Parent.UpdateCombinedData();
								}

								if (changes > 0)
								{
									fileData.ChangesUnsaved = fileData.Parent.ChangesUnsaved = ChangesUnsaved = true;

									OutputText = $"Refreshed file. Found {changes} changes.";
									OutputType = LogType.Important;
								}
								else
								{
									OutputText = "No changes found.";
									OutputType = LogType.Activity;
								}
							}
							else
							{
								OutputText = "No changes found.";
								OutputType = LogType.Activity;
							}

							OnHideExtras(HideExtras);
						}
						CreateSnapshot(undo, redo);
						redo();
					}
				}
			});
		}

		private void OnClipboardChanged()
		{
			ClipboardHasText = Clipboard.ContainsText() && !string.IsNullOrEmpty(Clipboard.GetText());
		}

		public void OnViewLoaded(LocaleEditorWindow v, DOS2DEModuleData moduleData, CompositeDisposable disposables)
		{
			view = v;
			ModuleData = moduleData;

			//AppController.Main.Data.WhenAnyValue(x => x.IsUnlocked).BindTo(this, vm => vm.IsUnlocked).DisposeWith(disposables);
			AppController.Main.Data.WhenAnyValue(x => x.LockScreenVisibility).Subscribe((visibility) =>
			{
				LockScreenVisibility = visibility;
			}).DisposeWith(disposables);

			SubWindowOpenedObservable = this.WhenAnyValue(vm => vm.IsSubWindowOpen);

			GlobalCanActObservable = this.WhenAny(vm => vm.IsAddingNewFileTab, vm => vm.IsSubWindowOpen, (b1, b2) => !b1.Value && !b2.Value);
			AnySelectedEntryObservable = this.WhenAnyValue(vm => vm.AnyEntrySelected);
			CanAddFileObservable = this.WhenAnyValue(vm => vm.CanAddFile);
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
				//Log.Here().Activity($"AnyFileSelected? Subscribe: {b} AnyFileSelected: {AnyFileSelected}");
			}).DisposeWith(disposables);

			var onCancel = new Action<string, FileDialogResult>((s, r) => IsSubWindowOpen = false);

			AddFileCommand = ReactiveCommand.Create(AddNewFile, CanAddFileObservable).DisposeWith(disposables);

			ImportFileCommand = ReactiveCommand.Create(() =>
			{
				string entryImportPath = GetLastFileImportPath();
				IsSubWindowOpen = true;
				FileCommands.Load.OpenMultiFileDialog(view, DOS2DETooltips.Button_Locale_ImportFile,
					entryImportPath, ImportFilesAsFileData, "", onCancel, DOS2DEFileFilters.AllLocaleFilesList.ToArray());
				view.ResizeEntryKeyColumn();
			}, CanImportFilesObservable).DisposeWith(disposables);

			ImportKeysCommand = ReactiveCommand.Create(() =>
			{
				string entryImportPath = GetLastFileImportPath();

				IsSubWindowOpen = true;
				FileCommands.Load.OpenMultiFileDialog(view, DOS2DETooltips.Button_Locale_ImportKeys,
					entryImportPath, ImportFilesAsKeys, "", onCancel, DOS2DEFileFilters.AllLocaleFilesList.ToArray());
			}, CanImportKeysObservable).DisposeWith(disposables);

			ExportFileAsTextualCommand = ReactiveCommand.Create(() => ExportFileAsText(SelectedFile), FileSelectedObservable).DisposeWith(disposables);
			ExportAllAsSpreadsheetCommand = ReactiveCommand.Create(ExportAllToSpreadsheet).DisposeWith(disposables);

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

			this.WhenAny(vm => vm.SelectedEntry.EntryContent, vm => vm.Value).Subscribe((o) =>
			{
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

			LocaleEditorCommands.LoadProjectSettings(ModuleData, this);

			UpdateCombinedGroup(true);

			//ExportXMLCommand = ReactiveCommand.Create<bool>(OpenExportWindow, AnySelectedEntryObservable).DisposeWith(disposables);
			ExportAllXMLCommand = ReactiveCommand.Create(() => OpenExportWindow(true)).DisposeWith(disposables);
			ExportSelectedXMLCommand = ReactiveCommand.Create(() => OpenExportWindow(false)).DisposeWith(disposables);
			AddFileToGroupCommand = ReactiveCommand.Create<CustomLocaleTabGroup>(AddCustomFileToGroup, CanImportFilesObservable).DisposeWith(disposables);

			GenerateXMLCommand = ReactiveCommand.Create(GenerateXML);
			SaveXMLCommand = ReactiveCommand.Create(() => SaveXMLFile());
			SaveXMLAsCommand = ReactiveCommand.Create(() =>
			{
				var languages = ActiveProjectSettings.TargetLanguages.Split(';');

				if (languages.Length >= Languages.Count)
				{
					string startDirectory = Path.GetFullPath(Path.Combine(ModuleData.Settings.DOS2DEDataDirectory, "Mods", GetMainProject().FolderName, "Localization"));
					FileCommands.Load.OpenFolderDialog(view.ExportWindow, "Save Language XML Files To...", startDirectory, (path) =>
					{
						SaveXMLFile(path);
					});
				}
				else
				{
					var mainProject = GetMainProject();

					foreach (var lan in languages)
					{
						string startDirectory = Path.GetFullPath(Path.Combine(ModuleData.Settings.DOS2DEDataDirectory, "Mods", mainProject.FolderName, "Localization", lan));
						string defaultFileName = lan.ToLower() + ".xml";

						FileCommands.Save.OpenSaveDialog(view.ExportWindow, "Save Language XML As...", (result, path) =>
						{
							if (result == FileDialogResult.Ok)
							{
								FileCommands.WriteToFile(path, ExportText);
							}
						}, defaultFileName, startDirectory, CommonFileFilters.XMLLocaleFile);
					}
				}
			});
			OpenXMLFolderCommand = ReactiveCommand.Create(() =>
			{
				string startDirectory = Path.GetFullPath(Path.Combine(ModuleData.Settings.DOS2DEDataDirectory, "Mods", GetMainProject().FolderName, "Localization"));
				Directory.CreateDirectory(startDirectory);
				Process.Start("explorer.exe", startDirectory);
			});

			LanguageCheckedCommand = ReactiveCommand.Create((object item) =>
			{

			});

			var canConfirmAddFile = this.WhenAny(vm => vm.NewFileTabName, e => !string.IsNullOrWhiteSpace(e.Value));
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
			ToggleRenameFileTabCommand = ReactiveCommand.Create((ILocaleFileData fileData) =>
			{
				fileData.IsRenaming = !fileData.IsRenaming;
				if (fileData.IsRenaming)
				{
					SelectedGroup.SelectedFileIndex = SelectedGroup.Tabs.IndexOf(fileData);
				}
			}, GlobalCanActObservable).DisposeWith(disposables);

			ConfirmRenameFileTabCommand = ReactiveCommand.Create<ILocaleFileData>(ConfirmRenaming).DisposeWith(disposables);
			CancelRenamingFileTabCommand = ReactiveCommand.Create<ILocaleFileData>(CancelRenaming).DisposeWith(disposables);

			SaveAllCommand = ReactiveCommand.Create(SaveAll, GlobalCanActObservable).DisposeWith(disposables);
			SaveCurrentCommand = ReactiveCommand.Create(SaveCurrent, GlobalCanActObservable).DisposeWith(disposables);

			SaveSettingsCommand = ReactiveCommand.Create(view.SaveSettings).DisposeWith(disposables);
			Settings.SaveCommand = SaveSettingsCommand;

			GenerateHandlesCommand = ReactiveCommand.Create(() => { GenerateHandles(); }, AnySelectedEntryObservable).DisposeWith(disposables);
			ForceGenerateHandlesCommand = ReactiveCommand.Create(() =>
			{
				FileCommands.OpenConfirmationDialog(view, "Regenerate Handles Confirmation", "Replace all selected entry handles with new handles?", "", (b) =>
				{
					if (b) GenerateHandles(true);
				});
			}, AnySelectedEntryObservable).DisposeWith(disposables);
			OverrideResHandlesCommand = ReactiveCommand.Create(OverrideResHandles, AnySelectedEntryObservable).DisposeWith(disposables);
			NewHandleCommand = ReactiveCommand.Create<object>(GenerateHandle).DisposeWith(disposables);
			AddNewKeyCommand = ReactiveCommand.Create(AddNewKey, CanImportKeysObservable).DisposeWith(disposables);

			AddFontTagCommand = ReactiveCommand.Create(AddFontTag, AnySelectedEntryObservable).DisposeWith(disposables);

			ToggleContentLightModeCommand = ReactiveCommand.Create(() => ContentLightMode = !ContentLightMode, GlobalCanActObservable).DisposeWith(disposables);
			ChangeContentFontSizeCommand = ReactiveCommand.Create<string>((fontSizeStr) =>
			{
				this.RaisePropertyChanging("ContentFontSize");
				if (int.TryParse(fontSizeStr, out contentFontSize))
				{
					this.RaisePropertyChanged("ContentFontSize");
				}
			}, GlobalCanActObservable).DisposeWith(disposables);

			IObservable<bool> canSelectNone = this.WhenAnyValue(vm => vm.SelectedText, (text) => text != string.Empty);

			SelectNoneCommand = ReactiveCommand.Create<object>((targetObject) =>
			{
				if (targetObject is Xceed.Wpf.Toolkit.RichTextBox rtb)
				{
					rtb.Selection.Select(rtb.CaretPosition.DocumentStart, rtb.CaretPosition.DocumentStart);
				}
				else if (targetObject is TextBox tb)
				{
					tb.Select(tb.CaretIndex, 0);
				}
			}, canSelectNone).DisposeWith(disposables);

			ResetHandleCommand = ReactiveCommand.Create<object>((targetObject) =>
			{
				if (targetObject is TextBox tb)
				{
					tb.Text = LocaleEditorCommands.UnsetHandle;
				}
			}).DisposeWith(disposables);

			OpenFileInExplorerCommand = ReactiveCommand.Create<string>((path) =>
			{
				if (!string.IsNullOrEmpty(path))
				{
					if (File.Exists(path) || Directory.Exists(path))
					{
						Process.Start("explorer.exe", $"/select, \"{path}\"");
					}
					else
					{
						string parentDir = Path.GetDirectoryName(path);
						if (Directory.Exists(parentDir))
						{
							Process.Start("explorer.exe", parentDir);
						}
					}
				}
			});

			CopyToClipboardCommand = ReactiveCommand.Create<string>((str) =>
			{
				if (str != string.Empty)
				{
					string current = Clipboard.GetText(TextDataFormat.Text);
					void undo()
					{
						Clipboard.SetText(current);

						OutputText = $"Reverted clipboard text to '{current}'.";
						OutputType = LogType.Important;
					};
					void redo()
					{
						Clipboard.SetText(str, TextDataFormat.Text);
						OutputText = $"Copied text '{str}' to clipboard.";
						OutputType = LogType.Activity;
					}

					CreateSnapshot(undo, redo);
					redo();
				}
			}, GlobalCanActObservable).DisposeWith(disposables);

			RefreshFileCommand = ReactiveCommand.Create((ILocaleFileData fileData) =>
			{
				if (fileData is LocaleNodeFileData nodeFile)
				{
					RefreshFileData(nodeFile);
				}
			}, FileSelectedObservable).DisposeWith(disposables);

			ReloadFileCommand = ReactiveCommand.Create((ILocaleFileData fileData) =>
			{
				if (fileData is LocaleNodeFileData nodeFile)
				{
					ReloadFileData(nodeFile);
				}
			}, FileSelectedObservable).DisposeWith(disposables);

			ReloadFileLinkDataCommand = ReactiveCommand.Create<ILocaleFileData>(RefreshLinkedData, FileSelectedObservable).DisposeWith(disposables);
			SetFileLinkDataCommand = ReactiveCommand.Create<ILocaleFileData>(SetLinkedData, FileSelectedObservable).DisposeWith(disposables);
			RemoveFileLinkDataCommand = ReactiveCommand.Create<ILocaleFileData>(RemoveLinkedData, FileSelectedObservable).DisposeWith(disposables);

			RefreshAllLinkedDataCommand = ReactiveCommand.Create(RefreshAllLinkedData);

			CheckForDuplicateKeysCommand = ReactiveCommand.Create(() =>
			{
				List<ILocaleKeyEntry> duplicateEntries = new List<ILocaleKeyEntry>();
				Dictionary<string, ILocaleKeyEntry> keys = new Dictionary<string, ILocaleKeyEntry>();
				foreach (var g in Groups)
				{
					foreach (var f in g.DataFiles)
					{
						foreach (var entry in f.Entries)
						{
							if (!string.IsNullOrEmpty(entry.Key) && entry.KeyIsEditable) // Ignore dialog keys
							{
								if (keys.ContainsKey(entry.Key))
								{
									if (!keys.ContainsValue(entry))
									{
										var otherEntry = keys[entry.Key];
										duplicateEntries.Add(otherEntry);
										duplicateEntries.Add(entry);
									}
								}
								else
								{
									keys.Add(entry.Key, entry);
								}
							}
						}
					}
				}
				if (duplicateEntries.Count > 0)
				{
					ShowMissingEntriesView(duplicateEntries, true);
				}
			}).DisposeWith(disposables);

			MenuData.File.Add(new MenuData("File.AddNewFile", "Add New File", AddFileCommand, Key.N, ModifierKeys.Control));
			MenuData.File.Add(new SeparatorData());
			var SaveCurrentMenuData = new MenuData("SaveCurrent", "Save", SaveCurrentCommand, Key.S, ModifierKeys.Control);
			MenuData.File.Add(SaveCurrentMenuData);
			MenuData.File.Add(new MenuData("File.SaveAll", "Save All", SaveAllCommand, Key.S, ModifierKeys.Control | ModifierKeys.Shift));
			MenuData.File.Add(new SeparatorData());
			MenuData.File.Add(new MenuData("File.RefreshLinkedData", "Refresh All Linked Data", RefreshAllLinkedDataCommand, Key.F5));
			MenuData.File.Add(new SeparatorData());
			MenuData.File.Add(new MenuData("File.ImportFile", "Import File", ImportFileCommand));
			MenuData.File.Add(new MenuData("File.ImportKeys", "Import File as Keys", ImportKeysCommand));
			MenuData.File.Add(new SeparatorData());
			MenuData.File.Add(new MenuData("File.ExportTextSelected", "Export Selected File to Text File...", ExportFileAsTextualCommand));
			MenuData.File.Add(new MenuData("File.ExportTextSpreadsheet", "Export All to Spreadsheet...", ExportAllAsSpreadsheetCommand));
			MenuData.File.Add(new SeparatorData());
			MenuData.File.Add(new MenuData("File.ExportXMLAll", DOS2DETooltips.Button_Locale_ExportAllToXML, ExportAllXMLCommand, Key.E, ModifierKeys.Control));
			MenuData.File.Add(new MenuData("File.ExportXMLSelected", DOS2DETooltips.Button_Locale_ExportSelectedToXML, ExportSelectedXMLCommand, Key.E, ModifierKeys.Control | ModifierKeys.Shift));

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

			//var whenTextFocused = this.WhenAnyValue(vm => vm.AnyTextBoxFocused, b => b == false);

			var SelectAllEntriesCommand = ReactiveCommand.Create(() =>
			{
				if (CurrentTextBox == null)
				{
					if (SelectedFile != null)
					{
						SelectedFile.SelectAll();
					}
					else
					{
						SelectedGroup?.CombinedEntries.SelectAll();
					}
				}
				else
				{
					CurrentTextBox.SelectAll();
				}
			});

			var DeselectAllEntriesCommand = ReactiveCommand.Create(() =>
			{
				if (CurrentTextBox == null)
				{
					if (SelectedFile != null)
					{
						SelectedFile.SelectNone();
					}
					else
					{
						SelectedGroup?.CombinedEntries.SelectNone();
					}
				}
				else
				{
					CurrentTextBox.Select(0, 0);
				}
			});

			MenuData.Edit.Add(new MenuData("Edit.SelectAll", "Select All", SelectAllEntriesCommand, Key.A, ModifierKeys.Control));
			MenuData.Edit.Add(new MenuData("Edit.SelectNone", "Select None", DeselectAllEntriesCommand, Key.D, ModifierKeys.Control));
			MenuData.Edit.Add(new MenuData("Edit.GenerateHandles", "Generate Handles for Selected (Unset Only)", GenerateHandlesCommand, Key.G, ModifierKeys.Control | ModifierKeys.Shift));
			MenuData.Edit.Add(new MenuData("Edit.GenerateHandles", "Regenerate Handles for Selected", ForceGenerateHandlesCommand));
			MenuData.Edit.Add(new MenuData("Edit.AddKey", "Add Key", AddNewKeyCommand));
			MenuData.Edit.Add(new MenuData("Edit.DeleteSelectedKeys", "Delete Selected Keys", DeleteKeysCommand));

			var pasteSubMenu = new MenuData("Edit.PasteSubMenu", "Paste...");

			var canPasteObservable = this.WhenAnyValue(x => x.AnyEntrySelected, x => x.ClipboardHasText, (a, b) => a && b);
			canPasteObservable.Subscribe((b) =>
			{
				pasteSubMenu.IsEnabled = b;
			}).DisposeWith(disposables);

			PasteIntoKeysCommand = ReactiveCommand.Create(() =>
			{
				if (SelectedGroup != null && SelectedGroup.SelectedFile != null)
				{
					var selectedList = SelectedGroup.SelectedFile.Entries.Where(e => e.Selected && e.KeyIsEditable).ToList();
					if (selectedList.Count > 0)
					{
						var clipboardText = Clipboard.GetText();

						List<LocaleEntryHistory> lastEntryValues = new List<LocaleEntryHistory>(selectedList.Select(x => new LocaleEntryHistory()
						{
							Entry = x,
							ChangesUnsaved = x.ChangesUnsaved,
							ParentFile = x.Parent,
							LastKey = x.EntryKey
						}));

						void undo()
						{
							foreach (var entry in lastEntryValues)
							{
								if (entry.Entry != null)
								{
									entry.Entry.Key = entry.LastKey;
									entry.Entry.ChangesUnsaved = entry.ChangesUnsaved;
								}
							}
						};
						void redo()
						{
							foreach (var entry in selectedList)
							{
								entry.Key = clipboardText;
								entry.ChangesUnsaved = true;
							}
						}

						CreateSnapshot(undo, redo);
						redo();
					}
				}
			}, canPasteObservable).DisposeWith(disposables);

			PasteIntoContentCommand = ReactiveCommand.Create(() =>
			{
				if (SelectedGroup != null && SelectedGroup.SelectedFile != null)
				{
					var selectedList = SelectedGroup.SelectedFile.Entries.Where(e => e.Selected).ToList();
					if (selectedList.Count > 0)
					{
						var clipboardText = Clipboard.GetText();

						List<LocaleEntryHistory> lastEntryValues = new List<LocaleEntryHistory>(selectedList.Select(x => new LocaleEntryHistory()
						{
							Entry = x,
							ChangesUnsaved = x.ChangesUnsaved,
							ParentFile = x.Parent,
							LastContent = x.EntryContent,
						}));

						void undo()
						{
							foreach (var entry in lastEntryValues)
							{
								if (entry.Entry != null)
								{
									entry.Entry.Content = entry.LastContent;
									entry.Entry.ChangesUnsaved = entry.ChangesUnsaved;
								}
							}
						};
						void redo()
						{
							foreach (var entry in selectedList)
							{
								entry.Content = clipboardText;
								entry.ChangesUnsaved = true;
							}
						}

						CreateSnapshot(undo, redo);
						redo();
					}
				}
			}, canPasteObservable).DisposeWith(disposables);

			PasteIntoHandlesCommand = ReactiveCommand.Create(() =>
			{
				if (SelectedGroup != null && SelectedGroup.SelectedFile != null)
				{
					var selectedList = SelectedGroup.SelectedFile.Entries.Where(e => e.Selected).ToList();
					if (selectedList.Count > 0)
					{
						var clipboardText = Clipboard.GetText().Trim();

						Log.Here().Activity($"Pasting '{clipboardText}' into the handle for {selectedList.Count} entries.");

						List<LocaleEntryHistory> lastEntryValues = new List<LocaleEntryHistory>(selectedList.Select(x => new LocaleEntryHistory()
						{
							Entry = x,
							ChangesUnsaved = x.ChangesUnsaved,
							ParentFile = x.Parent,
							LastHandle = x.EntryHandle,
						}));

						void undo()
						{
							foreach (var entry in lastEntryValues)
							{
								if (entry.Entry != null)
								{
									entry.Entry.Handle = entry.LastHandle;
									entry.Entry.ChangesUnsaved = entry.ChangesUnsaved;
								}
							}
						};
						void redo()
						{
							foreach (var entry in selectedList)
							{
								entry.Handle = clipboardText;
								entry.ChangesUnsaved = true;
							}
						}

						CreateSnapshot(undo, redo);
						redo();
					}
				}
			}, canPasteObservable).DisposeWith(disposables);

			pasteSubMenu.Add(new MenuData("Edit.Paste.IntoKey", "Into Key", PasteIntoKeysCommand));
			pasteSubMenu.Add(new MenuData("Edit.Paste.IntoContent", "Into Content", PasteIntoContentCommand));
			pasteSubMenu.Add(new MenuData("Edit.Paste.IntoHandle", "Into Handle", PasteIntoHandlesCommand));
			MenuData.Edit.Add(pasteSubMenu);

			var wordWrapMenuItem = new MenuData("View.WordWrap", "Toggle Word Wrap");
			var wordWrapCommand = ReactiveCommand.Create(() =>
			{
				if (Settings != null)
				{
					Settings.WordWrapEnabled = !Settings.WordWrapEnabled;
					wordWrapMenuItem.IsChecked = Settings.WordWrapEnabled;
					view.SaveSettings();
				}
			});
			wordWrapMenuItem.ClickCommand = wordWrapCommand;
			MenuData.View.Add(wordWrapMenuItem);

			MenuData.Tools.Add(new MenuData("Tools.CheckForDuplicates", "Check for Duplicate Keys", CheckForDuplicateKeysCommand));

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

			//var anyRemovedEntriesSelected = this.WhenAnyValue(vm => vm.RemovedEntries);

			this.WhenAnyValue(x => x.HideExtras).Subscribe((b) =>
			{
				OnHideExtras(b);
			});

			/*
			this.WhenAnyValue(x => x.SelectedFile).Throttle(TimeSpan.FromMilliseconds(25)).ObserveOn(RxApp.MainThreadScheduler).Subscribe((x) =>
			{
				if(x != null)
				{
					int i = 1;
					foreach (var item in x.VisibleEntries)
					{
						item.Index = i;
						i++;
					}
				}
			}).DisposeWith(disposables);
			*/

			SelectedGroup = CombinedGroup;
			SelectedFile = CombinedGroup.CombinedEntries;
			SelectedFile.Selected = true;

			if (openMissingEntriesViewOnLoad)
			{
				RxApp.MainThreadScheduler.Schedule(TimeSpan.FromMilliseconds(150), () =>
				{
					openMissingEntriesViewOnLoad = false;
					ShowMissingEntriesView(addMissingEntriesOnLoad);
					addMissingEntriesOnLoad = null;
				});
			}

			//this.WhenAnyValue(x => x.Groups.WhenAnyValue(c => c.Select(g => g.ChangesUnsaved));
		}

		public MenuData UndoMenuData { get; private set; }
		public MenuData RedoMenuData { get; private set; }

		public LocaleViewModel() : base()
		{
			PropertyChanged += LocaleViewModel_PropertyChanged;
			MenuData = new LocaleMenuData();

			CombinedGroup = new LocaleTabGroup(this, "All");
			ModsGroup = new LocaleTabGroup(this, "Locale (Mods)");
			PublicGroup = new LocaleTabGroup(this, "Locale (Public)");
			DialogGroup = new LocaleTabGroup(this, "Dialog");
			DialogGroup.CanAddFiles = false;
			JournalGroup = new LocaleTabGroup(this, "Journal");
			JournalGroup.CanAddFiles = false;
			RootTemplatesGroup = new LocaleTabGroup(this, "RootTemplates");
			RootTemplatesGroup.CanAddFiles = false;
			GlobalTemplatesGroup = new LocaleTabGroup(this, "Globals");
			GlobalTemplatesGroup.CanAddFiles = false;
			LevelDataGroup = new LocaleTabGroup(this, "LevelData");
			LevelDataGroup.CanAddFiles = false;
			CustomGroup = new CustomLocaleTabGroup(this, "Custom");
			CustomGroup.IsCustom = true;

#if Debug
			Groups = new ObservableCollectionExtended<LocaleTabGroup>
			{
				CombinedGroup,
				ModsGroup,
				PublicGroup,
				DialogGroup,
				JournalGroup,
				RootTemplatesGroup,
				GlobalTemplatesGroup,
				LevelDataGroup
				CustomGroup
			};
#else
			Groups = new ObservableCollectionExtended<LocaleTabGroup>
			{
				CombinedGroup,
				ModsGroup,
				PublicGroup,
				DialogGroup,
				JournalGroup,
				RootTemplatesGroup,
				GlobalTemplatesGroup,
				LevelDataGroup,
				//CustomGroup
			};
#endif
			foreach (var g in Groups)
			{
				g.SelectedFileChanged = SelectedFileChanged;
			}

			Groups.ToObservableChangeSet().Filter(x => x.Visibility == true).ObserveOn(RxApp.MainThreadScheduler).Bind(out visibleGroups).Subscribe();

			var canRemoveEntries = this.WhenAnyValue(x => x.MissingKeyEntrySelected);

			RemoveSelectedMissingEntriesCommand = ReactiveCommand.Create(ConfirmRemoveSelectedMissingEntries, canRemoveEntries);
			CloseMissingEntriesCommand = ReactiveCommand.Create(CloseMissingEntriesView);
			CopySimpleMissingEntriesCommand = ReactiveCommand.Create(() =>
			{
				if (MissingEntries.Count > 0)
				{
					string current = Clipboard.GetText(TextDataFormat.Text);
					string removedEntriesStr = string.Join(Environment.NewLine, MissingEntries.Select(x => $"{x.EntryKey}\t{x.EntryContent}"));

					void undo()
					{
						Clipboard.SetText(current);
						OutputText = "Reverted clipboard text.";
						OutputType = LogType.Important;
					};
					void redo()
					{
						Clipboard.SetText(removedEntriesStr, TextDataFormat.Text);
						OutputText = "Copied removed entries to clipboard.";
						OutputType = LogType.Activity;
					}

					CreateSnapshot(undo, redo);
					redo();
				}
			});
			CopyAllMissingEntriesCommand = ReactiveCommand.Create(() =>
			{
				if (MissingEntries.Count > 0)
				{
					string current = Clipboard.GetText(TextDataFormat.Text);
					string removedEntriesStr = string.Join(Environment.NewLine, MissingEntries.Select(x => $"{x.EntryKey}\t{x.EntryContent}\t{x.EntryHandle}\t{x.Parent?.SourcePath}"));

					void undo()
					{
						Clipboard.SetText(current);
						OutputText = "Reverted clipboard text.";
						OutputType = LogType.Important;
					};
					void redo()
					{
						Clipboard.SetText(removedEntriesStr, TextDataFormat.Text);
						OutputText = "Copied removed entries to clipboard.";
						OutputType = LogType.Activity;
					}

					CreateSnapshot(undo, redo);
					redo();
				}
			});

			OnClipboardChangedCommand = ReactiveCommand.Create(OnClipboardChanged);
		}
	}


}
