using System;
using System.Collections.Generic;
using System.Linq;
using SCG.Data;
using System.Collections.ObjectModel;
using System.Windows.Input;
using SCG.Commands;
using SCG.Modules.DOS2DE.Utilities;
using System.Windows;

namespace SCG.Modules.DOS2DE.Data.View
{
	public class DOS2DELocalizationViewData : PropertyChangedBase
	{
		public ObservableCollection<DOS2DELocalizationGroup> Groups { get; set; }

		private int selectedGroupIndex = 0;

		public int SelectedGroupIndex
		{
			get { return selectedGroupIndex; }
			set
			{
				selectedGroupIndex = value;
				RaisePropertyChanged("SelectedGroupIndex");
				RaisePropertyChanged("SelectedGroup");
			}
		}

		private DOS2DELocalizationGroup modsGroup;

		public DOS2DELocalizationGroup ModsGroup
		{
			get { return modsGroup; }
			set
			{
				modsGroup = value;
				RaisePropertyChanged("ModsGroup");
			}
		}

		private DOS2DELocalizationGroup dialogGroup;

		public DOS2DELocalizationGroup DialogGroup
		{
			get { return dialogGroup; }
			set
			{
				dialogGroup = value;
				RaisePropertyChanged("DialogGroup");
			}
		}

		private DOS2DELocalizationGroup publicGroup;

		public DOS2DELocalizationGroup PublicGroup
		{
			get { return publicGroup; }
			set
			{
				publicGroup = value;
				RaisePropertyChanged("PublicGroup");
			}
		}

		private DOS2DELocalizationGroup combinedGroup;

		public DOS2DELocalizationGroup CombinedGroup
		{
			get { return combinedGroup; }
			private set
			{
				combinedGroup = value;
				RaisePropertyChanged("CombinedGroup");
			}
		}

		public DOS2DELocalizationGroup SelectedGroup
		{
			get
			{
				return SelectedGroupIndex > -1 ? Groups[SelectedGroupIndex] : null;
			}
		}

		private bool exportKeys = true;

		public bool ExportKeys
		{
			get { return exportKeys; }
			set
			{
				exportKeys = value;
				RaisePropertyChanged("ExportKeys");
			}
		}

		private bool exportSource = true;

		public bool ExportSource
		{
			get { return exportSource; }
			set
			{
				exportSource = value;
				RaisePropertyChanged("ExportSource");
			}
		}

		public ICommand GenerateHandlesCommands { get; set; }

		public void GenerateHandles()
		{
			Log.Here().Activity("Generating handles");
			if (SelectedGroup != null && SelectedGroup.SelectedFile != null)
			{
				foreach (var entry in SelectedGroup.SelectedFile.Entries.Where(e => e.Selected))
				{
					if(entry.Handle.Equals("ls::TranslatedStringRepository::s_HandleUnknown", StringComparison.OrdinalIgnoreCase))
					{
						entry.Handle = DOS2DELocalizationEditor.NewHandle();
						Log.Here().Activity($"[{entry.Key}] New handle generated. [{entry.Handle}]");
					}
				}
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
			CombinedGroup.DataFiles = new ObservableRangeCollection<IKeyFileData>();
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

			Log.Here().Activity($"Setting selected group index to '{SelectedGroupIndex}' {CombinedGroup.Visibility}.");

			if(updateCombinedEntries)
			{
				foreach(var g in Groups)
				{
					g.UpdateCombinedData();
				}
			}
		}

		public DOS2DELocalizationViewData()
		{
			ModsGroup = new DOS2DELocalizationGroup("Locale (Mods)");
			DialogGroup = new DOS2DELocalizationGroup("Dialog");
			PublicGroup = new DOS2DELocalizationGroup("Locale (Public)");
			CombinedGroup = new DOS2DELocalizationGroup("All");
			Groups = new ObservableCollection<DOS2DELocalizationGroup>();
			Groups.Add(CombinedGroup);
			Groups.Add(ModsGroup);
			Groups.Add(PublicGroup);
			Groups.Add(DialogGroup);

			GenerateHandlesCommands = new ActionCommand(GenerateHandles);
		}
	}

	public class DOS2DELocalizationGroup : PropertyChangedBase
	{
		private string name;

		public string Name
		{
			get { return name; }
			set
			{
				name = value;
				RaisePropertyChanged("Name");
			}
		}

		private ObservableRangeCollection<IKeyFileData> dataFiles;

		public ObservableRangeCollection<IKeyFileData> DataFiles
		{
			get { return dataFiles; }
			set
			{
				dataFiles = value;
				UpdateCombinedData();
			}
		}

		public ObservableRangeCollection<IKeyFileData> Tabs { get; set; }

		private IKeyFileData combinedEntries;

		public IKeyFileData CombinedEntries
		{
			get { return combinedEntries; }
			private set
			{
				combinedEntries = value;
				RaisePropertyChanged("CombinedEntries");
			}
		}


		private int selectedfileIndex = 0;

		public int SelectedFileIndex
		{
			get { return selectedfileIndex; }
			set
			{
				selectedfileIndex = value;
				RaisePropertyChanged("SelectedFileIndex");
				RaisePropertyChanged("SelectedFile");
			}
		}

		public IKeyFileData SelectedFile
		{
			get
			{
				return SelectedFileIndex > -1 && Tabs.Count > 0 ? Tabs[SelectedFileIndex] : null;
			}
		}

		public ICommand UpdateAllCommand { get; set; }

		private bool visibility = true;

		public bool Visibility
		{
			get { return visibility; }
			set
			{
				visibility = value;
				RaisePropertyChanged("Visibility");
			}
		}

		public void UpdateCombinedData()
		{
			Tabs = new ObservableRangeCollection<IKeyFileData>();
			Tabs.Add(CombinedEntries);
			Tabs.AddRange(DataFiles);

			CombinedEntries.Entries.Clear();
			foreach (var obj in DataFiles)
			{
				CombinedEntries.Entries.AddRange(obj.Entries);
			}
			CombinedEntries.Entries.OrderBy(e => e.Key);
			RaisePropertyChanged("CombinedEntries");
			RaisePropertyChanged("Tabs");
			Log.Here().Activity($"Updated combined entries for '{Name}'.");
		}

		public DOS2DELocalizationGroup(string name="")
		{
			Name = name;
			CombinedEntries = new DOS2DEStringKeyFileDataBase("All");
			DataFiles = new ObservableRangeCollection<IKeyFileData>();
			Tabs = new ObservableRangeCollection<IKeyFileData>();

			UpdateAllCommand = new ActionCommand(UpdateCombinedData);
		}
	}

	public interface IKeyFileData
	{
		ObservableRangeCollection<DOS2DEKeyEntry> Entries { get; set; }

		string Name { get; set; }
		bool Active { get; set; }
		bool AllSelected { get; set; }

		void SelectAll();
		void SelectNone();
	}

	public class DOS2DEStringKeyFileDataBase : PropertyChangedBase, IKeyFileData
	{
		public ObservableRangeCollection<DOS2DEKeyEntry> Entries { get; set; }

		private string name;

		public string Name
		{
			get { return name; }
			set
			{
				name = value;
				RaisePropertyChanged("Name");
			}
		}

		private bool active = false;

		public bool Active
		{
			get { return active; }
			set
			{
				active = value;
				RaisePropertyChanged("Active");
			}
		}

		public void SelectAll()
		{
			foreach (var entry in Entries) { entry.Selected = true; }
		}

		public void SelectNone()
		{
			foreach (var entry in Entries) { entry.Selected = false; }
		}

		private bool allSelected;

		public bool AllSelected
		{
			get { return allSelected; }
			set
			{
				allSelected = value;
				RaisePropertyChanged("AllSelected");
				if (allSelected)
					SelectAll();
				else
					SelectNone();
			}
		}

		public DOS2DEStringKeyFileDataBase(string name = "")
		{
			Entries = new ObservableRangeCollection<DOS2DEKeyEntry>();

			Name = name;
		}
	}

	public class DOS2DEStringKeyFileData : DOS2DEStringKeyFileDataBase
	{
		public LSLib.LS.Resource Source { get; private set; }

		public LSLib.LS.Enums.ResourceFormat Format { get; set; }

		public string SourcePath { get; set; }

		public DOS2DEStringKeyFileData(LSLib.LS.Enums.ResourceFormat resourceFormat, LSLib.LS.Resource res, string sourcePath, string name = "") : base(name)
		{
			Source = res;
			SourcePath = sourcePath;
			Format = resourceFormat;
		}
	}

	public class DOS2DEKeyEntry : SCG.Data.PropertyChangedBase
	{
		private LSLib.LS.Node node;

		public LSLib.LS.NodeAttribute KeyAttribute { get; set; }

		public LSLib.LS.NodeAttribute TranslatedStringAttribute { get; set; }

		public LSLib.LS.TranslatedString TranslatedString { get; set; }

		private bool keyIsEditable = false;

		public bool KeyIsEditable
		{
			get { return keyIsEditable; }
			set
			{
				keyIsEditable = value;
				RaisePropertyChanged("KeyIsEditable");
			}
		}

		private string key = "None";

		public string Key
		{
			get { return KeyAttribute != null ? KeyAttribute.Value.ToString() : key; }
			set
			{
				if (KeyAttribute != null)
				{
					KeyAttribute.Value = value;
				}
				else
				{
					key = value;
				}

				RaisePropertyChanged("Key");
			}
		}

		public string Content
		{
			get { return TranslatedString != null ? TranslatedString.Value : "Content"; }
			set
			{
				TranslatedString.Value = value;
				RaisePropertyChanged("Content");
			}
		}

		public string Handle
		{
			get { return TranslatedString != null ? TranslatedString.Handle : "ls::TranslatedStringRepository::s_HandleUnknown"; }
			set
			{
				if (TranslatedString != null)
				{
					TranslatedString.Handle = value;
					RaisePropertyChanged("Handle");
				}
			}
		}

		private bool selected = false;

		public bool Selected
		{
			get { return selected; }
			set
			{
				selected = value;
				RaisePropertyChanged("Selected");
			}
		}

		public DOS2DEKeyEntry(LSLib.LS.Node resNode)
		{
			node = resNode;

			if (resNode != null)
			{
				

				
			}
		}
	}
}
