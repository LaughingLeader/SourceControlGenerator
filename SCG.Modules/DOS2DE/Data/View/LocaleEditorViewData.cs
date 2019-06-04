﻿using System;
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

namespace SCG.Modules.DOS2DE.Data.View
{
	public class LocaleViewData : PropertyChangedBase
	{
		public LocaleEditorSettingsData Settings { get; set; }

		public DOS2DEModuleData ModuleData { get; set; }

		private LocaleMenuData menuData;

		public LocaleMenuData MenuData
		{
			get { return menuData; }
			set
			{
				menuData = value;
				RaisePropertyChanged("MenuData");
			}
		}

		public ObservableCollection<LocaleTabGroup> Groups { get; set; }

		private int selectedGroupIndex = -1;

		public int SelectedGroupIndex
		{
			get { return selectedGroupIndex; }
			set
			{
				bool updateCanSave = selectedGroupIndex != value;
				
				selectedGroupIndex = value;
				RaisePropertyChanged("SelectedGroupIndex");
				RaisePropertyChanged("SelectedGroup");
				RaisePropertyChanged("SelectedItem");
				RaisePropertyChanged("CurrentImportPath");

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
				modsGroup = value;
				RaisePropertyChanged("ModsGroup");
			}
		}

		private LocaleTabGroup dialogGroup;

		public LocaleTabGroup DialogGroup
		{
			get { return dialogGroup; }
			set
			{
				dialogGroup = value;
				RaisePropertyChanged("DialogGroup");
			}
		}

		private LocaleTabGroup publicGroup;

		public LocaleTabGroup PublicGroup
		{
			get { return publicGroup; }
			set
			{
				publicGroup = value;
				RaisePropertyChanged("PublicGroup");
			}
		}

		private LocaleTabGroup combinedGroup;

		public LocaleTabGroup CombinedGroup
		{
			get { return combinedGroup; }
			private set
			{
				combinedGroup = value;
				RaisePropertyChanged("CombinedGroup");
			}
		}

		public LocaleTabGroup SelectedGroup
		{
			get
			{
				return SelectedGroupIndex > -1 ? Groups[SelectedGroupIndex] : null;
			}
		}

		public ILocaleFileData SelectedItem
		{
			get
			{
				return SelectedGroup != null ? SelectedGroup.SelectedFile : null;
			}
		}

		private bool exportKeys = false;

		public bool ExportKeys
		{
			get { return exportKeys; }
			set
			{
				exportKeys = value;
				RaisePropertyChanged("ExportKeys");
				Log.Here().Activity($"ExportKeys set to {exportKeys}");
			}
		}

		private bool exportSource = false;

		public bool ExportSource
		{
			get { return exportSource; }
			set
			{
				exportSource = value;
				RaisePropertyChanged("ExportSource");
				Log.Here().Activity($"ExportSource set to {exportSource}");
			}
		}

		private bool canSave;

		public bool CanSave
		{
			get { return canSave; }
			set
			{
				canSave = value;
				SaveCurrentMenuData.IsEnabled = canSave;
				RaisePropertyChanged("CanSave");
			}
		}

		private bool canAddFile = false;

		public bool CanAddFile
		{
			get { return canAddFile; }
			set
			{
				canAddFile = value;
				RaisePropertyChanged("CanAddFile");
			}
		}

		private bool canAddKeys = true;

		public bool CanAddKeys
		{
			get { return canAddKeys; }
			set
			{
				canAddKeys = value;
				RaisePropertyChanged("CanAddKeys");
			}
		}

		private bool anySelected = false;

		public bool AnySelected
		{
			get { return anySelected; }
			set
			{
				anySelected = value;
				RaisePropertyChanged("AnySelected");
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

			if (group == DialogGroup)
			{
				DOS2DETooltips.Button_Locale_ImportDisabled = "Import Disabled for Dialog";
				DOS2DETooltips.TooltipChanged("Button_Locale_ImportDisabled");
			}
			else
			{
				DOS2DETooltips.Button_Locale_ImportDisabled = "Import Disabled";
				DOS2DETooltips.TooltipChanged("Button_Locale_ImportDisabled");
			}
			//Log.Here().Activity($"Can add file: {CanAddFile}");
		}

		private string outputDate;

		public string OutputDate
		{
			get { return outputDate; }
			set
			{
				outputDate = value;
				RaisePropertyChanged("OutputDate");
			}
		}

		private string outputText;

		public string OutputText
		{
			get { return outputText; }
			set
			{
				outputText = value;
				RaisePropertyChanged("OutputText");
			}
		}

		private LogType outputType;

		public LogType OutputType
		{
			get { return outputType; }
			set
			{
				outputType = value;
				RaisePropertyChanged("OutputType");
			}
		}

		public ICommand ImportFileCommand { get; set; }

		public ICommand SaveAllCommand { get; set; }
		public ICommand SaveCurrentCommand { get; set; }
		public ICommand GenerateHandlesCommand { get; set; }

		public ICommand AddNewKeyCommand { get; set; }
		public ICommand DeleteKeysCommand { get; set; }
		public ICommand ImportKeysCommand { get; set; }

		public void AddNewKey()
		{
			if (SelectedGroup != null && SelectedGroup.SelectedFile != null)
			{
				if (SelectedGroup.SelectedFile is LocaleFileData fileData && fileData.Format == LSLib.LS.Enums.ResourceFormat.LSB)
				{
					LocaleKeyEntry localeEntry = LocaleEditorCommands.CreateNewLocaleEntry(fileData);
					fileData.Entries.Add(localeEntry);
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
			if(confirm)
			{
				if (SelectedGroup != null && SelectedGroup.SelectedFile != null)
				{
					foreach(var entry in SelectedGroup.SelectedFile.Entries.Where(e => e.Selected).ToList())
					{
						SelectedGroup.SelectedFile.Entries.Remove(entry);
					}

					SelectedGroup.UpdateCombinedData();
				}
				else
				{
					Log.Here().Activity("No selected file(s) found. Skipping delete operation.");
				}
			}
			
		}

		public void GenerateHandles()
		{
			if (SelectedGroup != null && SelectedGroup.SelectedFile != null)
			{
				Log.Here().Activity("Generating handles...");
				foreach (var entry in SelectedGroup.SelectedFile.Entries.Where(e => e.Selected))
				{
					if(entry.Handle.Equals("ls::TranslatedStringRepository::s_HandleUnknown", StringComparison.OrdinalIgnoreCase))
					{
						entry.Handle = LocaleEditorCommands.CreateHandle();
						Log.Here().Activity($"[{entry.Key}] New handle generated. [{entry.Handle}]");
					}
				}
			}
			else
			{
				Log.Here().Activity("No selected file found. Skipping handle generation.");
			}
		}

		private bool MultipleGroupsEntriesFilled()
		{
			int total = 0;
			if (ModsGroup.DataFiles.Count > 0) total += 1;
			if (PublicGroup.DataFiles.Count > 0) total += 1;
			if (DialogGroup.DataFiles.Count > 0) total += 1;
			return total > 1;
		}

		public void UpdateCombinedGroup(bool updateCombinedEntries = false)
		{
			CombinedGroup.DataFiles = new ObservableRangeCollection<ILocaleFileData>();
			CombinedGroup.DataFiles.AddRange(ModsGroup.DataFiles);
			CombinedGroup.DataFiles.AddRange(PublicGroup.DataFiles);
			CombinedGroup.DataFiles.AddRange(DialogGroup.DataFiles);
			CombinedGroup.Visibility = MultipleGroupsEntriesFilled();

			if(!CombinedGroup.Visibility)
			{
				if (PublicGroup.Visibility)
				{
					SelectedGroupIndex = 1;
				}
				else if(ModsGroup.Visibility)
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

			RaisePropertyChanged("CombinedGroup");
			RaisePropertyChanged("Groups");

			//Log.Here().Activity($"Setting selected group index to '{SelectedGroupIndex}' {CombinedGroup.Visibility}.");

			if(updateCombinedEntries)
			{
				foreach(var g in Groups)
				{
					g.UpdateCombinedData();
				}
			}
		}

		public int TotalFiles
		{
			get
			{
				try
				{
					return CombinedGroup.CombinedEntries.Entries.Count;
				}
				catch { }
				return 0;
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
				}
			}
		}

		public string CurrentImportPath
		{
			get
			{
				if(SelectedItem != null && SelectedItem is LocaleFileData data)
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


		public void ImportFileAsFileData(IEnumerable<string> files)
		{
			var newFileDataList = LocaleEditorCommands.ImportFilesAsData(files, SelectedGroup);
			foreach (var k in newFileDataList)
			{
				SelectedGroup.DataFiles.Add(k);
			}
			SelectedGroup.ChangesUnsaved = true;
			SelectedGroup.UpdateCombinedData();
			SelectedGroup.SelectLast();
			LocaleEditorWindow.instance.SaveSettings();
		}

		public void ImportFileAsKeys(IEnumerable<string> files)
		{
			Log.Here().Activity("Importing keys from files...");
			var newKeys = LocaleEditorCommands.ImportFilesAsEntries(files, SelectedItem as LocaleFileData);
			foreach (var k in newKeys)
			{
				SelectedItem.Entries.Add(k);
			}
			SelectedItem.ChangesUnsaved = true;
			SelectedGroup.UpdateCombinedData();
			LocaleEditorWindow.instance.SaveSettings();
		}

		private MenuData SaveCurrentMenuData { get; set; }

		public LocaleViewData()
		{
			MenuData = new LocaleMenuData();

			ModsGroup = new LocaleTabGroup("Locale (Mods)");
			DialogGroup = new LocaleTabGroup("Dialog");
			PublicGroup = new LocaleTabGroup("Locale (Public)");
			CombinedGroup = new LocaleTabGroup("All");
			Groups = new ObservableCollection<LocaleTabGroup>();
			Groups.Add(CombinedGroup);
			Groups.Add(ModsGroup);
			Groups.Add(PublicGroup);
			Groups.Add(DialogGroup);

			foreach(var g in Groups)
			{
				g.SelectedFileChanged = SelectedFileChanged;
			}

			ImportFileCommand = new OpenFileBrowserCommand(ImportFileAsFileData)
			{
				Title = DOS2DETooltips.Button_Locale_ImportFile,
				ParentWindow = LocaleEditorWindow.instance,
				UseFolderBrowser = false,
				AllowMultipleFiles = true,
				Filters = DOS2DEFileFilters.AllLocaleFilesList.ToArray(),
				StartPath = CurrentImportPath
			};

			SaveAllCommand = new ActionCommand(SaveAll);
			SaveCurrentCommand = new ActionCommand(SaveCurrent);
			GenerateHandlesCommand = new ActionCommand(GenerateHandles);
			AddNewKeyCommand = new ActionCommand(AddNewKey);
			DeleteKeysCommand = new TaskCommand(DeleteSelectedKeys, LocaleEditorWindow.instance, "Delete Keys", "Delete selected keys?", "Changes will be lost.");
			ImportKeysCommand = new OpenFileBrowserCommand(ImportFileAsKeys)
			{
				Title = DOS2DETooltips.Button_Locale_ImportKeys,
				ParentWindow = LocaleEditorWindow.instance,
				UseFolderBrowser = false,
				AllowMultipleFiles = true,
				Filters = DOS2DEFileFilters.AllLocaleFilesList.ToArray(),
				StartPath = CurrentImportPath
			};

			SaveCurrentMenuData = new MenuData("SaveCurrent", "Save", SaveCurrentCommand, Key.S, ModifierKeys.Control);

			MenuData.File.Add(SaveCurrentMenuData);
			MenuData.File.Add(new MenuData("SaveAll", "Save All", SaveAllCommand, Key.S, ModifierKeys.Control | ModifierKeys.Shift));

			CanSave = false;
			ExportKeys = true;
			ExportSource = false;
		}
	}

	
}
