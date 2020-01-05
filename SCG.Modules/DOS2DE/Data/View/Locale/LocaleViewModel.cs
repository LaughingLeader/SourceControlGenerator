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
using DynamicData;
using System.Reactive.Linq;
using SCG.Controls;
using System.Windows.Controls;
using LSLib.LS.Enums;
using System.Reactive.Concurrency;

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

		public void OnSettingsLoaded()
		{
			this.RootTemplatesGroup.Visibility = Settings.LoadRootTemplates;
			this.GlobalTemplatesGroup.Visibility = Settings.LoadGlobals;
			this.LevelDataGroup.Visibility = Settings.LoadLevelData;
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
			if(LinkedProjects.Count > 0)
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
				else if(value < 0)
				{
					value = Groups.Count - 1;
				}

				var nextGroup = Groups[value];
				int nextIndex = 0;

				if (nextGroup != null)
				{
					if (lastSelectedFile != null && nextGroup.DataFiles.Any(x => x.SourcePath == lastSelectedFile.SourcePath))
					{
						nextIndex = nextGroup.Tabs.IndexOf(lastSelectedFile);
					}
					else if (nextGroup.SelectedFileIndex > nextGroup.Tabs.Count)
					{
						nextIndex = 0;
					}
					else
					{
						nextIndex = nextGroup.SelectedFileIndex;
					}
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

		private bool hideExtras = false;

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
				CanSave = (group.CombinedEntries != keyFileData) || group.DataFiles.Count == 1;
				//Log.Here().Activity($"Selected file changed to {group.Name} | {keyFileData.Name}");
			}
			else
			{
				CanSave = false;
			}

			CanAddFile = group != CombinedGroup && group != DialogGroup;
			CanAddKeys = SelectedGroup != null && SelectedGroup.SelectedFile != null && !SelectedGroup.SelectedFile.Locked;

			//view.LocaleEntryDataGrid_BuildIndexes();

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
			if (targetObject is System.Windows.Controls.TextBox tb)
			{
				tb.Text = LocaleEditorCommands.CreateHandle();
			}
			else if (targetObject is ILocaleKeyEntry entry)
			{
				var lastHandle = new LocaleHandleHistory(entry, entry.Handle);
				var nextHandle = new LocaleHandleHistory(entry, entry.Handle);
				entry.Handle = LocaleEditorCommands.CreateHandle();
				Log.Here().Activity($"[{entry.Key}] New handle generated. [{entry.Handle}]");
				entry.Parent.ChangesUnsaved = true;

				CreateSnapshot(() => {
					lastHandle.Key.Handle = lastHandle.Handle;
				}, () => {
					nextHandle.Key.Handle = nextHandle.Handle;
				});
			}

			if (targetObject is IUnfocusable unfocusable)
			{
				unfocusable.Unfocus();
			}
		}

		private void GenerateHandlesThatMatch(string match, bool equals = true)
		{
			if (SelectedGroup != null && SelectedGroup.SelectedFile != null)
			{
				List<LocaleHandleHistory> lastHandles = new List<LocaleHandleHistory>();
				List<LocaleHandleHistory> newHandles = new List<LocaleHandleHistory>();

				List<ILocaleKeyEntry> list;

				if (equals)
				{
					list = SelectedGroup.SelectedFile.Entries.Where(e => e.Selected && e.Handle.Equals(match)).ToList();
				}
				else
				{
					list = SelectedGroup.SelectedFile.Entries.Where(e => e.Selected && e.Handle.Contains(match)).ToList();
				}

				if(list.Count > 0)
				{
					foreach (var entry in list)
					{
						lastHandles.Add(new LocaleHandleHistory(entry, entry.Handle));
						entry.Handle = LocaleEditorCommands.CreateHandle();
						newHandles.Add(new LocaleHandleHistory(entry, entry.Handle));
						Log.Here().Activity($"[{entry.Key}] New handle generated. [{entry.Handle}]");
						entry.Parent.ChangesUnsaved = true;
					}

					CreateSnapshot(() => {
						foreach (var e in lastHandles)
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
			}
			else
			{
				Log.Here().Activity("No selected file found. Skipping handle generation.");
			}
		}

		public void GenerateHandles()
		{
			GenerateHandlesThatMatch(LocaleEditorCommands.UnsetHandle);
		}
		public void OverrideResHandles()
		{
			GenerateHandlesThatMatch("ResStr_", false);
		}

		public void UpdateCombinedGroup(bool updateCombinedEntries = false)
		{
			CombinedGroup.DataFiles = new ObservableCollectionExtended<ILocaleFileData>();
			CombinedGroup.DataFiles.AddRange(ModsGroup.DataFiles);
			CombinedGroup.DataFiles.AddRange(PublicGroup.DataFiles);
			CombinedGroup.DataFiles.AddRange(DialogGroup.DataFiles);
			if (Settings.LoadRootTemplates) CombinedGroup.DataFiles.AddRange(RootTemplatesGroup.DataFiles);
			if (Settings.LoadGlobals) CombinedGroup.DataFiles.AddRange(GlobalTemplatesGroup.DataFiles);
			if (Settings.LoadLevelData) CombinedGroup.DataFiles.AddRange(LevelDataGroup.DataFiles);
			CombinedGroup.DataFiles.AddRange(CustomGroup.DataFiles);
			//CombinedGroup.Visibility = MultipleGroupsEntriesFilled();
			CombinedGroup.Visibility = true;

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
			RxApp.TaskpoolScheduler.ScheduleAsync(async (s,t) =>
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
			if(SelectedFile != null)
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
								OutputText = $"Saved '{keyFileData.SourcePath}'";
								OutputType = LogType.Important;

								keyFileData.SetChangesUnsaved(false, true);
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

				var lastChangesUnsaved = currentGroup.ChangesUnsaved;

				var lastFiles = currentGroup.DataFiles.ToList();

				CreateSnapshot(() => {
					foreach (var entry in newFileDataList)
					{
						if (lastFiles.Contains(entry)) lastFiles.Remove(entry);
					}
					currentGroup.DataFiles = new ObservableCollectionExtended<ILocaleFileData>(lastFiles);
					currentGroup.UpdateCombinedData();
					currentGroup.SelectLast();
					currentGroup.ChangesUnsaved = lastChangesUnsaved;
					view.FocusSelectedTab();
				}, () => {
					currentGroup.DataFiles.AddRange(newFileDataList);
				});

				SelectedGroup.DataFiles.AddRange(newFileDataList);

				Log.Here().Activity("Saving linked data for new files.");
				foreach(var file in newFileDataList)
				{
					LocaleEditorCommands.SaveLinkedDataForFile(ModuleData, LinkedLocaleData, file);
				}

				SelectedGroup.ChangesUnsaved = true;
				SelectedGroup.UpdateCombinedData();
				SelectedGroup.SelectLast();
				view.FocusSelectedTab();

				foreach(var p in LinkedProjects)
				{
					var settings = Settings.GetProjectSettings(p);
					if (settings != null)
					{
						settings.LastFileImportPath = Path.GetDirectoryName(files.FirstOrDefault());
						settings.LastEntryImportPath = Path.GetDirectoryName(files.FirstOrDefault());

						view.SaveSettings();
					}
				}

				this.RaisePropertyChanged("CurrentImportPath");
				this.RaisePropertyChanged("CurrentFileImportPath");

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

			string importPath = CurrentImportPath;

			if (LinkedProjects.Count == 1)
			{
				var settings = Settings.GetProjectSettings(LinkedProjects.FirstOrDefault());
				if (settings != null) importPath = settings.LastEntryImportPath;
			}

			FileCommands.Save.OpenSaveDialog(view, "Save Locale File As...",
				writeToFile, exportName, importPath, DOS2DEFileFilters.AllLocaleFilesList.ToArray());
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

				//CurrentImportPath = Path.GetDirectoryName(files.FirstOrDefault());

				if (LinkedProjects.Count == 1)
				{
					var settings = Settings.GetProjectSettings(LinkedProjects.FirstOrDefault());
					if (settings != null) settings.LastEntryImportPath = Path.GetDirectoryName(files.FirstOrDefault());
				}

				this.RaisePropertyChanged("CurrentImportPath");
				this.RaisePropertyChanged("CurrentEntryImportPath");
				view.SaveSettings();

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
		public ICommand ExportXMLCommand { get; private set; }
		public ICommand CheckForDuplicateKeysCommand { get; private set; }

		public ICommand SaveAllCommand { get; private set; }
		public ICommand SaveCurrentCommand { get; private set; }
		public ICommand SaveSettingsCommand { get; private set; }
		public ICommand GenerateHandlesCommand { get; private set; }
		public ICommand OverrideResHandlesCommand { get; private set; }

		public ICommand AddNewKeyCommand { get; private set; }
		public ICommand DeleteKeysCommand { get; private set; }
		public ICommand RefreshFileCommand { get; private set; }
		public ICommand ReloadFileCommand { get; private set; }
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
		public ICommand NewHandleCommand { get; private set; }
		public ICommand ResetHandleCommand { get; private set; }
		public ICommand ExportFileAsTextualCommand { get; private set; }
		public ICommand OnClipboardChangedCommand { get; private set; }
		public ICommand PasteIntoKeysCommand { get; private set; }
		public ICommand PasteIntoContentCommand { get; private set; }
		public ICommand PasteIntoHandlesCommand { get; private set; }

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
					if(SelectedGroup.SelectedFile == SelectedGroup.CombinedEntries)
					{
						var selectedEntries = SelectedGroup.DataFiles.SelectMany(f => f.Entries.Where(x => x.Selected)).ToList();
						Log.Here().Important($"Deleting {selectedEntries.Count} keys.");
						List<LocaleEntryHistory> lastState = new List<LocaleEntryHistory>();

						var lastSelectedGroup = SelectedGroup;
						
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
						this.CreateSnapshot(undo, redo);
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
							lastSelectedGroup.UpdateCombinedData();
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
			ActiveProjectSettings = Settings.GetProjectSettings(LinkedProjects.FirstOrDefault());

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

			if(missingEntries.Count > 0)
			{
				if(view != null && view.IsVisible)
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
					if(addMissingEntriesOnLoad == null)
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
					lastState.Add(new LocaleEntryHistory
					{
						ParentFile = entry.Parent,
						Entry = entry,
						Index = entry.Index,
						ChangesUnsaved = entry.ChangesUnsaved,
						ParentChangesUnsaved = entry.Parent.ChangesUnsaved
					});
				}

				void undo()
				{
					foreach (var x in lastState)
					{
						var fileEntries = Groups.Select(g => g.DataFiles.FirstOrDefault(f => f.SourcePath == x.ParentFile.SourcePath));
						foreach (var fileEntry in fileEntries)
						{
							if (x.ChangesUnsaved) fileEntry.ChangesUnsaved = x.ChangesUnsaved;
							fileEntry.Entries.Insert(x.Index, x.Entry);
							fileEntry.ChangesUnsaved = x.ParentChangesUnsaved;
							if (fileEntry is LocaleNodeFileData nodeFileData && x.Entry is LocaleNodeKeyEntry nodeKeyEntry)
							{
								nodeFileData.RootRegion.AppendChild(nodeKeyEntry.Node);
							}
						}
					}
					SelectedGroup?.UpdateCombinedData();

					MissingEntries.Clear();
					MissingEntries.AddRange(lastRemovedEntries);
					foreach(var entry in MissingEntries)
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
				this.CreateSnapshot(undo, redo);
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
						var removed = LocaleEditorCommands.RefreshLinkedData(fileData);

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

						ShowMissingEntriesView(removed);
					}

					CreateSnapshot(undo, redo);
					redo();
				}
			}
			IsSubWindowOpen = true;

			string entryImportPath = CurrentImportPath;

			if (LinkedProjects.Count == 1)
			{
				var settings = Settings.GetProjectSettings(LinkedProjects.FirstOrDefault());
				if (settings != null) entryImportPath = settings.LastEntryImportPath;
			}

			FileCommands.Load.OpenFileDialog(view, "Pick localization file to link...",
					entryImportPath, OnFileSelected, "", new Action<string, FileDialogResult>((s,r) => IsSubWindowOpen = false), CommonFileFilters.DelimitedLocaleFiles);
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
					var removed = LocaleEditorCommands.RefreshLinkedData(fileData);
					ShowMissingEntriesView(removed);
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
		public void UpdateUnsavedChanges()
		{
			ChangesUnsaved = Groups.Any(g => g.ChangesUnsaved == true);
		}

		public void ReloadFileData(LocaleNodeFileData fileData)
		{
			FileCommands.OpenConfirmationDialog(view, "Reload Data?", "Reload data in selected file?", "Unsaved changes will be lost", (b) =>
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
						if(selectedGroup != targetGroup) CombinedGroup.UpdateCombinedData();
						selectedGroup.SelectedFileIndex = selectedIndex;

						OutputText = $"Reverted file '{oldFile.SourcePath}'.";
						OutputType = LogType.Important;
					}

					void redo()
					{
						var newFile = LocaleEditorCommands.LoadResource(oldFile.Parent, oldFile.SourcePath);
						targetGroup.DataFiles[index] = newFile;
						targetGroup.UpdateCombinedData();
						if (selectedGroup != targetGroup) selectedGroup.UpdateCombinedData();
						selectedGroup.SelectedFileIndex = selectedIndex;

						OutputText = $"Reloaded file '{newFile.SourcePath}'";
						OutputType = LogType.Important;
					}
					this.CreateSnapshot(undo, redo);
					redo();
				}
			});
		}

		public void RefreshFileData(LocaleNodeFileData fileData)
		{
			FileCommands.OpenConfirmationDialog(view, "Refresh Data?", "Refresh data in selected file?", "", (b) =>
			{
				if (b)
				{
					var selectedGroup = SelectedGroup;
					string lastFileSource = fileData.SourcePath;
					var entries = LocaleEditorCommands.LoadFromResource(fileData.Source, fileData.Format);

					if (entries.Count > 0)
					{
						List<LocaleEntryHistory> lastEntries = new List<LocaleEntryHistory>();
						List<ILocaleKeyEntry> newEntries = new List<ILocaleKeyEntry>();

						foreach (var entry in entries)
						{
							ILocaleKeyEntry existingEntry = null;
							if (fileData.Format == ResourceFormat.LSJ || fileData.Format == ResourceFormat.LSF)
							{
								existingEntry = fileData.Entries.FirstOrDefault(x => x.Content == entry.Content || (x.Handle == entry.Handle &&
									x.Handle != LocaleEditorCommands.UnsetHandle));
							}
							else
							{
								existingEntry = fileData.Entries.FirstOrDefault(x => x.Key == entry.Key || (x.Handle == entry.Handle &&
									x.Handle != LocaleEditorCommands.UnsetHandle));
							}

							if (existingEntry != null)
							{
								if(!existingEntry.ValuesMatch(entry))
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
											&& !nodeFileData.RootRegion.Children.FirstOrDefault().Value.Contains((nodeKeyEntry.Node)))
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
											existingEntry = fileData.Entries.FirstOrDefault(x => x.Content == entry.Content || (x.Handle == entry.Handle &&
												x.Handle != LocaleEditorCommands.UnsetHandle));
										}
										else
										{
											existingEntry = fileData.Entries.FirstOrDefault(x => x.Key == entry.Key || (x.Handle == entry.Handle &&
												x.Handle != LocaleEditorCommands.UnsetHandle));
										}

										if ((fileData.Format == ResourceFormat.LSF || fileData.Format == ResourceFormat.LSJ))
										{
											existingEntry.Content = entry.Content;
										}
										else
										{
											existingEntry.Key = entry.Key;
											existingEntry.Content = entry.Content;
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

								if(changes > 0)
								{
									fileData.ChangesUnsaved = true;

									OutputText = $"Refreshed file. Found {changes} changes.";
									OutputType = LogType.Important;
								}
								else
								{
									OutputText = "No changes found.";
									OutputType = LogType.Activity;
								}
							}
						}
						this.CreateSnapshot(undo, redo);
						redo();
					}
				}
			});
		}

		private void OnClipboardChanged()
		{
			ClipboardHasText = Clipboard.ContainsText() && !String.IsNullOrEmpty(Clipboard.GetText());
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
				string entryImportPath = CurrentImportPath;

				if (LinkedProjects.Count == 1)
				{
					var settings = Settings.GetProjectSettings(LinkedProjects.FirstOrDefault());
					if (settings != null) entryImportPath = settings.LastEntryImportPath;
				}

				IsSubWindowOpen = true;
				FileCommands.Load.OpenMultiFileDialog(view, DOS2DETooltips.Button_Locale_ImportFile,
					entryImportPath, ImportFilesAsFileData, "", onCancel, DOS2DEFileFilters.AllLocaleFilesList.ToArray());
				this.view.ResizeEntryKeyColumn();
			}, CanImportFilesObservable).DisposeWith(disposables);

			ImportKeysCommand = ReactiveCommand.Create(() =>
			{
				string entryImportPath = CurrentImportPath;

				if (LinkedProjects.Count == 1)
				{
					var settings = Settings.GetProjectSettings(LinkedProjects.FirstOrDefault());
					entryImportPath = settings.LastEntryImportPath;
				}

				IsSubWindowOpen = true;
				FileCommands.Load.OpenMultiFileDialog(view, DOS2DETooltips.Button_Locale_ImportKeys,
					entryImportPath, ImportFilesAsKeys, "", onCancel, DOS2DEFileFilters.AllLocaleFilesList.ToArray());
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

			LocaleEditorCommands.LoadProjectSettings(ModuleData, this);

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

			SaveAllCommand = ReactiveCommand.Create(SaveAll, GlobalCanActObservable).DisposeWith(disposables);
			SaveCurrentCommand = ReactiveCommand.Create(SaveCurrent, GlobalCanActObservable).DisposeWith(disposables);

			SaveSettingsCommand = ReactiveCommand.Create(view.SaveSettings).DisposeWith(disposables);
			Settings.SaveCommand = this.SaveSettingsCommand;

			GenerateHandlesCommand = ReactiveCommand.Create(GenerateHandles, AnySelectedEntryObservable).DisposeWith(disposables);
			OverrideResHandlesCommand = ReactiveCommand.Create(OverrideResHandles, AnySelectedEntryObservable).DisposeWith(disposables);
			NewHandleCommand = ReactiveCommand.Create<object>(GenerateHandle).DisposeWith(disposables);
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

			ResetHandleCommand = ReactiveCommand.Create<object>((targetObject) =>
			{
				if (targetObject is System.Windows.Controls.TextBox tb)
				{
					tb.Text = LocaleEditorCommands.UnsetHandle;
				}
			}).DisposeWith(disposables);

			CopyToClipboardCommand = ReactiveCommand.Create<string>((str) =>
			{
				if (str != String.Empty)
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

			RefreshFileCommand = ReactiveCommand.Create<ILocaleFileData>((ILocaleFileData fileData) =>
			{
				if(fileData is LocaleNodeFileData nodeFile)
				{
					RefreshFileData(nodeFile);
				}
			}, FileSelectedObservable).DisposeWith(disposables);

			ReloadFileCommand = ReactiveCommand.Create<ILocaleFileData>((ILocaleFileData fileData) =>
			{
				if(fileData is LocaleNodeFileData nodeFile)
				{
					ReloadFileData(nodeFile);
				}
			}, FileSelectedObservable).DisposeWith(disposables);

			ReloadFileLinkDataCommand = ReactiveCommand.Create<ILocaleFileData>(RefreshLinkedData, FileSelectedObservable).DisposeWith(disposables);
			SetFileLinkDataCommand = ReactiveCommand.Create<ILocaleFileData>(SetLinkedData, FileSelectedObservable).DisposeWith(disposables);
			RemoveFileLinkDataCommand = ReactiveCommand.Create<ILocaleFileData>(RemoveLinkedData, FileSelectedObservable).DisposeWith(disposables);

			CheckForDuplicateKeysCommand = ReactiveCommand.Create(() =>
			{
				List<ILocaleKeyEntry> duplicateEntries = new List<ILocaleKeyEntry>();
				Dictionary<string, ILocaleKeyEntry> keys = new Dictionary<string, ILocaleKeyEntry>();
				foreach(var g in Groups)
				{
					foreach(var f in g.DataFiles)
					{
						foreach(var entry in f.Entries)
						{
							if(!String.IsNullOrEmpty(entry.Key) && entry.KeyIsEditable) // Ignore dialog keys
							{
								if(keys.ContainsKey(entry.Key))
								{
									if(!keys.ContainsValue(entry))
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
				if(duplicateEntries.Count > 0)
				{
					ShowMissingEntriesView(duplicateEntries, true);
				}
			}).DisposeWith(disposables);

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

			//var whenTextFocused = this.WhenAnyValue(vm => vm.AnyTextBoxFocused, b => b == false);

			var SelectAllEntriesCommand = ReactiveCommand.Create(() => {
				if (CurrentTextBox == null)
				{
					if(SelectedFile != null)
					{
						SelectedFile.SelectAll();
					}
					else
					{
						this.SelectedGroup?.CombinedEntries.SelectAll();
					}
				}
				else
				{
					CurrentTextBox.SelectAll();
				}
			});

			var DeselectAllEntriesCommand = ReactiveCommand.Create(() => {
				if (CurrentTextBox == null)
				{
					if (SelectedFile != null)
					{
						SelectedFile.SelectNone();
					}
					else
					{
						this.SelectedGroup?.CombinedEntries.SelectNone();
					}
				}
				else
				{
					CurrentTextBox.Select(0,0);
				}
			});

			MenuData.Edit.Add(new MenuData("Edit.SelectAll", "Select All", SelectAllEntriesCommand, Key.A, ModifierKeys.Control));
			MenuData.Edit.Add(new MenuData("Edit.SelectNone", "Select None", DeselectAllEntriesCommand, Key.D, ModifierKeys.Control));
			MenuData.Edit.Add(new MenuData("Edit.GenerateHandles", "Generate Handles for Selected", GenerateHandlesCommand, Key.G, ModifierKeys.Control | ModifierKeys.Shift));
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

			if (openMissingEntriesViewOnLoad)
			{
				view.Dispatcher.Invoke(new Action(() =>
				{
					openMissingEntriesViewOnLoad = false;
					ShowMissingEntriesView(addMissingEntriesOnLoad);
					addMissingEntriesOnLoad = null;
				}), System.Windows.Threading.DispatcherPriority.Loaded);
			}

			this.WhenAnyValue(x => x.HideExtras).Subscribe((b) =>
			{
				//int totalHidden = 0;
				if(SelectedGroup != null && SelectedGroup.SelectedFile != null)
				{
					foreach(var entry in SelectedGroup.SelectedFile.Entries)
					{
						if(b)
						{
							if (!entry.KeyIsEditable && entry.Key.Equals("GameMasterSpawnSubSection"))
							{
								entry.Visible = false;
								//totalHidden += 1;
							}
							else
							{
								entry.Visible = true;
							}
						}
						else
						{
							entry.Visible = true;
						}
					}
				}
				//Log.Here().Activity($"Updated extra key visibility {totalHidden}");
			});

			this.WhenAnyValue(x => x.SelectedFile).Throttle(TimeSpan.FromMilliseconds(25)).ObserveOn(RxApp.MainThreadScheduler).Subscribe((x) =>
			{
				if(x != null)
				{
					for (var i = 0; i < SelectedFile.VisibleEntries.Count; i++)
					{
						SelectedFile.VisibleEntries[i].Index = i + 1;
					}
				}
			}).DisposeWith(disposables);

			SelectedGroup = CombinedGroup;
			SelectedFile = CombinedGroup.CombinedEntries;

			//this.WhenAnyValue(x => x.Groups.WhenAnyValue(c => c.Select(g => g.ChangesUnsaved));
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
			DialogGroup.CanAddFiles = false;
			RootTemplatesGroup = new LocaleTabGroup(this, "RootTemplates");
			RootTemplatesGroup.CanAddFiles = false;
			GlobalTemplatesGroup = new LocaleTabGroup(this, "Globals");
			GlobalTemplatesGroup.CanAddFiles = false;
			LevelDataGroup = new LocaleTabGroup(this, "LevelData");
			LevelDataGroup.CanAddFiles = false;
			CustomGroup = new CustomLocaleTabGroup(this, "Custom");

#if Debug
			Groups = new ObservableCollectionExtended<LocaleTabGroup>
			{
				CombinedGroup,
				ModsGroup,
				PublicGroup,
				DialogGroup,
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
				RootTemplatesGroup,
				GlobalTemplatesGroup,
				LevelDataGroup
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
			CopySimpleMissingEntriesCommand = ReactiveCommand.Create(() => {
				if (MissingEntries.Count > 0)
				{
					string current = Clipboard.GetText(TextDataFormat.Text);
					string removedEntriesStr = String.Join(Environment.NewLine, MissingEntries.Select(x => $"{x.EntryKey}\t{x.EntryContent}"));

					void undo()
					{
						Clipboard.SetText(current);
						OutputText = $"Reverted clipboard text.";
						OutputType = LogType.Important;
					};
					void redo()
					{
						Clipboard.SetText(removedEntriesStr, TextDataFormat.Text);
						OutputText = $"Copied removed entries to clipboard.";
						OutputType = LogType.Activity;
					}

					CreateSnapshot(undo, redo);
					redo();
				}
			});
			CopyAllMissingEntriesCommand = ReactiveCommand.Create(() => {
				if (MissingEntries.Count > 0)
				{
					string current = Clipboard.GetText(TextDataFormat.Text);
					string removedEntriesStr = String.Join(Environment.NewLine, MissingEntries.Select(x => $"{x.EntryKey}\t{x.EntryContent}\t{x.EntryHandle}\t{x.Parent?.SourcePath}"));

					void undo()
					{
						Clipboard.SetText(current);
						OutputText = $"Reverted clipboard text.";
						OutputType = LogType.Important;
					};
					void redo()
					{
						Clipboard.SetText(removedEntriesStr, TextDataFormat.Text);
						OutputText = $"Copied removed entries to clipboard.";
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
