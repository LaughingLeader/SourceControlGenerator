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

namespace SCG.Modules.DOS2DE.Data.View.Locale
{
	public class LocaleViewModel : HistoryBaseViewModel
	{
		private Dictionary<string, List<Action>> MenuEnabledLinks = new Dictionary<string, List<Action>>();

		private LocaleEditorWindow view;

		public override void OnPropertyNotify(string propertyName)
		{
			if (MenuEnabledLinks.ContainsKey(propertyName))
			{
				foreach (var setEnabledAction in MenuEnabledLinks[propertyName])
				{
					setEnabledAction.Invoke();
				}
			}
		}

		public LocaleEditorSettingsData Settings { get; set; }

		public DOS2DEModuleData ModuleData { get; set; }

		private LocaleMenuData menuData;

		public LocaleMenuData MenuData
		{
			get { return menuData; }
			set
			{
				Update(ref menuData, value);
			}
		}

		private bool changesUnsaved = false;

		public bool ChangesUnsaved
		{
			get { return changesUnsaved; }
			set
			{
				Update(ref changesUnsaved, value);
				Notify("WindowTitle");
			}
		}

		public string WindowTitle
		{
			get
			{
				return !ChangesUnsaved ? "Localization Editor" : "*Localization Editor";
			}
		}

		public ObservableCollectionExtended<LocaleTabGroup> Groups { get; set; }

		private int selectedGroupIndex = -1;

		public int SelectedGroupIndex
		{
			get { return selectedGroupIndex; }
			set
			{
				bool updateCanSave = selectedGroupIndex != value;
				
				Update(ref selectedGroupIndex, value);
				Notify("SelectedGroup");
				Notify("SelectedItem");
				Notify("CurrentImportPath");

				if (updateCanSave)
				{
					SelectedFileChanged(Groups[selectedGroupIndex], Groups[selectedGroupIndex].SelectedFile);
				}
			}
		}

		private LocaleTabGroup modsGroup;

		public LocaleTabGroup ModsGroup
		{
			get { return modsGroup; }
			set
			{
				Update(ref modsGroup, value);
			}
		}

		private LocaleTabGroup dialogGroup;

		public LocaleTabGroup DialogGroup
		{
			get { return dialogGroup; }
			set
			{
				Update(ref dialogGroup, value);
			}
		}

		private LocaleTabGroup publicGroup;

		public LocaleTabGroup PublicGroup
		{
			get { return publicGroup; }
			set
			{
				Update(ref publicGroup, value);
			}
		}

		private LocaleTabGroup combinedGroup;

		public LocaleTabGroup CombinedGroup
		{
			get { return combinedGroup; }
			private set
			{
				Update(ref combinedGroup, value);
			}
		}

		public LocaleTabGroup SelectedGroup
		{
			get
			{
				return SelectedGroupIndex > -1 ? Groups[SelectedGroupIndex] : null;
			}
		}

		private LocaleKeyEntry selectedEntry;

		public LocaleKeyEntry SelectedEntry
		{
			get { return selectedEntry; }
			set
			{
				Update(ref selectedEntry, value);
			}
		}

		public ILocaleFileData SelectedItem
		{
			get
			{
				return SelectedGroup?.SelectedFile;
			}
		}

		private bool canSave;

		public bool CanSave
		{
			get { return canSave; }
			set
			{
				Update(ref canSave, value);
				SaveCurrentMenuData.IsEnabled = canSave;
			}
		}

		private bool canAddFile;

		public bool CanAddFile
		{
			get { return canAddFile; }
			set
			{
				Update(ref canAddFile, value);
			}
		}

		private bool canAddKeys;

		public bool CanAddKeys
		{
			get { return canAddKeys; }
			set
			{
				Update(ref canAddKeys, value);
			}
		}

		private bool anySelected;

		public bool AnySelected
		{
			get { return anySelected; }
			set
			{
				Update(ref anySelected, value);
			}
		}

		private bool contentSelected = false;

		public bool ContentSelected
		{
			get { return contentSelected; }
			set
			{
				Update(ref contentSelected, value);
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
			get { return outputDate; }
			set
			{
				Update(ref outputDate, value);
			}
		}

		private string outputText;

		public string OutputText
		{
			get { return outputText; }
			set
			{
				Update(ref outputText, value);
			}
		}

		private LogType outputType;

		public LogType OutputType
		{
			get { return outputType; }
			set
			{
				Update(ref outputType, value);
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
				if (SelectedItem != null && SelectedItem is LocaleFileData data)
				{
					return Path.GetDirectoryName(data.SourcePath);
				}
				else if (SelectedGroup != null)
				{
					return SelectedGroup.SourceDirectory;
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

				List<HandleHistory> lastHandles = new List<HandleHistory>();
				List<HandleHistory> newHandles = new List<HandleHistory>();

				foreach (var entry in SelectedGroup.SelectedFile.Entries.Where(e => e.Selected))
				{
					if (entry.Handle.Equals("ls::TranslatedStringRepository::s_HandleUnknown", StringComparison.OrdinalIgnoreCase))
					{
						lastHandles.Add(new HandleHistory(entry, entry.Handle));
						entry.Handle = LocaleEditorCommands.CreateHandle();
						newHandles.Add(new HandleHistory(entry, entry.Handle));
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

			Notify("CombinedGroup");
			Notify("Groups");

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
				if (SelectedGroup.SelectedFile != null && SelectedGroup.SelectedFile is LocaleFileData keyFileData)
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
				if (SelectedGroup.DataFiles.First() is LocaleFileData keyFileData)
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
				Notify("CurrentImportPath");
				Notify("CurrentFileImportPath");
			}
			LocaleEditorWindow.instance.SaveSettings();

			ChangesUnsaved = true;
		}

		public void ImportFileAsKeys(IEnumerable<string> files)
		{
			Log.Here().Activity("Importing keys from files...");

			var currentItem = SelectedItem;
			var newKeys = LocaleEditorCommands.ImportFilesAsEntries(files, SelectedItem as LocaleFileData);

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
				Notify("CurrentImportPath");
				Notify("CurrentEntryImportPath");
			}
			LocaleEditorWindow.instance.SaveSettings();

			ChangesUnsaved = true;
		}

		private MenuData SaveCurrentMenuData { get; set; }
		//private MenuData SelectAllMenuData { get; set; }
		//private MenuData SelectNoneMenuData { get; set; }
		//private MenuData GenerateHandlesMenuData { get; set; }

		public ICommand AddFileCommand { get; set; }
		public ICommand ImportFileCommand { get; set; }
		public ICommand ImportKeysCommand { get; set; }
		public ICommand ExportXMLCommand { get; set; }

		public ICommand SaveAllCommand { get; set; }
		public ICommand SaveCurrentCommand { get; set; }
		public ICommand GenerateHandlesCommand { get; set; }

		public ICommand AddNewKeyCommand { get; set; }
		public ICommand DeleteKeysCommand { get; set; }

		public ICommand OpenPreferencesCommand { get; set; }

		public ICommand ExpandContentCommand { get; set; }

		public void AddNewKey()
		{
			if (SelectedGroup != null && SelectedGroup.SelectedFile != null)
			{
				if (SelectedGroup.SelectedFile is LocaleFileData fileData && fileData.Format == LSLib.LS.Enums.ResourceFormat.LSB)
				{
					LocaleKeyEntry localeEntry = LocaleEditorCommands.CreateNewLocaleEntry(fileData);
					localeEntry.SetHistoryFromObject(this);
					AddWithHistory(fileData.Entries, localeEntry);
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
			get { return exportText; }
			set
			{
				Update(ref exportText, value);
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
			MenuData.RegisterShortcuts(view);

			UpdateCombinedGroup(true);
		}

		public LocaleViewModel() : base()
		{
			MenuData = new LocaleMenuData();

			CombinedGroup = new LocaleTabGroup("All");
			ModsGroup = new LocaleTabGroup("Locale (Mods)");
			PublicGroup = new LocaleTabGroup("Locale (Public)");
			DialogGroup = new LocaleTabGroup("Dialog");

			Groups = new ObservableCollectionExtended<LocaleTabGroup>
			{
				CombinedGroup,
				ModsGroup,
				PublicGroup,
				DialogGroup
			};

			foreach (var g in Groups)
			{
				g.SelectedFileChanged = SelectedFileChanged;
			}

			ExportXMLCommand = ReactiveCommand.Create(OpenExportWindow);

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

			SaveAllCommand = ReactiveCommand.Create(SaveAll);
			SaveCurrentCommand = ReactiveCommand.Create(SaveCurrent);
			GenerateHandlesCommand = ReactiveCommand.Create(GenerateHandles);
			AddNewKeyCommand = ReactiveCommand.Create(AddNewKey);
			DeleteKeysCommand = new TaskCommand(DeleteSelectedKeys, LocaleEditorWindow.instance, "Delete Keys", 
				"Delete selected keys?", "Changes will be lost.");

			OpenPreferencesCommand = ReactiveCommand.Create(() => { LocaleEditorWindow.instance?.TogglePreferencesWindow(); });

			SaveCurrentMenuData = new MenuData("SaveCurrent", "Save", SaveCurrentCommand, Key.S, ModifierKeys.Control);

			MenuData.File.Add(SaveCurrentMenuData);
			MenuData.File.Add(new MenuData("File.SaveAll", "Save All", SaveAllCommand, Key.S, ModifierKeys.Control | ModifierKeys.Shift));
			
			MenuData.File.Add(CreateMenuDataWithLink(() => CanAddFile, "CanAddFile", "File.ImportFile",
				"Import File", ImportFileCommand));

			MenuData.File.Add(CreateMenuDataWithLink(() => CanAddKeys, "CanAddKeys", "File.ImportKeys",
				"Import File as Keys", ImportKeysCommand));

			MenuData.File.Add(CreateMenuDataWithLink(() => AnySelected, "AnySelected", "File.ExportSelected",
				DOS2DETooltips.Button_Locale_ExportToXML, ExportXMLCommand, Key.E, ModifierKeys.Control | ModifierKeys.Shift));

			MenuData.Edit.Add(new MenuData("Edit.Undo", "Undo", UndoCommand, Key.Z, ModifierKeys.Control));
			MenuData.Edit.Add(new MenuData("Edit.Redo", "Redo", RedoCommand, 
				new MenuShortcutInputBinding(Key.Z, ModifierKeys.Control | ModifierKeys.Shift),
				new MenuShortcutInputBinding(Key.Y, ModifierKeys.Control)
				)
			);

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
		}
	}

	
}
