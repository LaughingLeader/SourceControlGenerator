using System;
using System.Collections.Generic;
using System.Linq;
using SCG.Data;
using System.Collections.ObjectModel;
using System.Windows.Input;
using SCG.Commands;
using SCG.Modules.DOS2DE.Utilities;

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

		public void UpdateCombinedGroup(bool updateCombinedEntries = false)
		{
			CombinedGroup.DataFiles = new ObservableCollection<DOS2DEStringKeyFileData>(ModsGroup.DataFiles.Union(PublicGroup.DataFiles).ToList());
			CombinedGroup.Visibility = (publicGroup.DataFiles.Count > 0 && modsGroup.DataFiles.Count > 0);
			RaisePropertyChanged("CombinedGroup");
			RaisePropertyChanged("Groups");

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
			}

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
			ModsGroup = new DOS2DELocalizationGroup("Mods");
			PublicGroup = new DOS2DELocalizationGroup("Public");
			CombinedGroup = new DOS2DELocalizationGroup("All");
			Groups = new ObservableCollection<DOS2DELocalizationGroup>();
			Groups.Add(CombinedGroup);
			Groups.Add(ModsGroup);
			Groups.Add(PublicGroup);

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

		private ObservableCollection<DOS2DEStringKeyFileData> dataFiles;

		public ObservableCollection<DOS2DEStringKeyFileData> DataFiles
		{
			get { return dataFiles; }
			set
			{
				dataFiles = value;
				UpdateCombinedData();
			}
		}

		public ObservableCollection<DOS2DEStringKeyFileData> Tabs { get; set; }

		private DOS2DEStringKeyFileData combinedEntries;

		public DOS2DEStringKeyFileData CombinedEntries
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

		public DOS2DEStringKeyFileData SelectedFile
		{
			get
			{
				return SelectedFileIndex > -1 ? Tabs[SelectedFileIndex] : null;
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
			Tabs = new ObservableCollection<DOS2DEStringKeyFileData>(DataFiles);
			Tabs.Insert(0, CombinedEntries);

			CombinedEntries.Entries.Clear();
			foreach (var obj in DataFiles)
			{
				CombinedEntries.Entries.AddRange(obj.Entries);
			}
			CombinedEntries.Entries.OrderBy(e => e.Key);
			RaisePropertyChanged("CombinedEntries");
			RaisePropertyChanged("Tabs");
		}

		public DOS2DELocalizationGroup(string name="")
		{
			Name = name;
			CombinedEntries = new DOS2DEStringKeyFileData(null, "All");
			DataFiles = new ObservableCollection<DOS2DEStringKeyFileData>();
			Tabs = new ObservableCollection<DOS2DEStringKeyFileData>();

			UpdateAllCommand = new ActionCommand(UpdateCombinedData);
		}
	}

	public class DOS2DEStringKeyFileData : PropertyChangedBase
	{
		public LSLib.LS.Resource Source { get; private set; }

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


		public DOS2DEStringKeyFileData(LSLib.LS.Resource res = null, string name = "")
		{
			Entries = new ObservableRangeCollection<DOS2DEKeyEntry>();

			Name = name;

			try
			{
				if (res != null)
				{
					Source = res;

					var rootNode = res.Regions.First().Value;
					foreach (var entry in rootNode.Children)
					{
						foreach (var node in entry.Value)
						{
							DOS2DEKeyEntry localeEntry = new DOS2DEKeyEntry(node);
							Entries.Add(localeEntry);

							//TraceNode(node);
						}

					}
				}
				
			}
			finally { }
		}

		public void Debug_TestEntries()
		{
			for (int i=0;i<4;i++)
			{
				var node = new LSLib.LS.Node();
				var att = new LSLib.LS.NodeAttribute(LSLib.LS.NodeAttribute.DataType.DT_TranslatedString);
				att.Value = new LSLib.LS.TranslatedString();

				var handle = Guid.NewGuid().ToString().Replace('-', 'g').Insert(0, "h");
				if (att.Value is LSLib.LS.TranslatedString str)
				{
					str.Handle = handle;
					str.Value = "Test";
				}

				node.Attributes.Add("Content", att);
				var entry = new DOS2DEKeyEntry(node);
				Entries.Add(entry);
			}
		}

		private void TraceNodes(KeyValuePair<string, List<LSLib.LS.Node>> keyValuePair, int indent = 0)
		{
			Log.Here().Activity($"{String.Concat(Enumerable.Repeat("\t", indent))}Key[{keyValuePair.Key}]|Value[{keyValuePair.Value}]");
			foreach (var next in keyValuePair.Value)
			{
				TraceNode(next, indent + 1);
			}
		}

		private void TraceNode(LSLib.LS.Node v, int indent = 0)
		{
			Log.Here().Activity($"{String.Concat(Enumerable.Repeat("\t", indent))}Name[{v.Name}]|{v.GetType()}|Parent: [{v.Parent.Name}]");
			foreach (var att in v.Attributes)
			{
				TraceAtt(att, indent + 1);
			}
			foreach (var child in v.Children)
			{
				TraceNodes(child, indent + 1);
			}
		}

		private void TraceAtt(KeyValuePair<string, LSLib.LS.NodeAttribute> attdict, int indent = 0)
		{
			Log.Here().Activity($"{String.Concat(Enumerable.Repeat("\t", indent))}Attribute: {attdict.Key} | {attdict.Value.Type} = {attdict.Value.Value}");
		}
	}

	public class DOS2DEKeyEntry : SCG.Data.PropertyChangedBase
	{
		private LSLib.LS.Node node;

		private LSLib.LS.NodeAttribute uuidNode;
		private LSLib.LS.NodeAttribute contentNode;

		public string Key
		{
			get { return uuidNode != null ? uuidNode.Value.ToString() : "Key"; }
			set
			{
				if(node != null) node.Attributes["UUID"].Value = value;
				RaisePropertyChanged("Key");
			}
		}

		public string Content
		{
			get { return contentNode != null ? contentNode.Value.ToString() : "Content"; }
			set
			{
				if (node != null) node.Attributes["Content"].Value = value;
				RaisePropertyChanged("Content");
			}
		}

		public string Handle
		{
			get
			{
				if (contentNode != null && contentNode.Value is LSLib.LS.TranslatedString str)
				{
					return str.Handle;
				}
				return "ls::TranslatedStringRepository::s_HandleUnknown";
			}
			set
			{
				if (contentNode != null && contentNode.Value is LSLib.LS.TranslatedString str)
				{
					str.Handle = value;
				}
				RaisePropertyChanged("Handle");
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
			if(resNode != null)
			{
				node = resNode;

				node.Attributes.TryGetValue("UUID", out uuidNode);
				node.Attributes.TryGetValue("Content", out contentNode);
			}
		}
	}
}
