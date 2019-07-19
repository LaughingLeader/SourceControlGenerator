using DynamicData.Binding;
using SCG.Commands;
using SCG.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SCG.Modules.DOS2DE.Data.View.Locale
{
	public class LocaleTabGroup : PropertyChangedBase
	{
		private string name;

		public string Name
		{
			get { return name; }
			set
			{
				Update(ref name, value);
			}
		}

		private string sourceDirectory;

		public string SourceDirectory
		{
			get { return sourceDirectory; }
			set
			{
				Update(ref sourceDirectory, value);
			}
		}

		private bool changesUnsaved = true;

		public bool ChangesUnsaved
		{
			get { return changesUnsaved; }
			set
			{
				Update(ref changesUnsaved, value);
			}
		}

		private ObservableCollectionExtended<ILocaleFileData> dataFiles;

		public ObservableCollectionExtended<ILocaleFileData> DataFiles
		{
			get { return dataFiles; }
			set
			{
				dataFiles = value;
				UpdateCombinedData();
			}
		}

		public ObservableCollectionExtended<ILocaleFileData> Tabs { get; set; }

		private ILocaleFileData combinedEntries;

		public ILocaleFileData CombinedEntries
		{
			get { return combinedEntries; }
			private set
			{
				Update(ref combinedEntries, value);
			}
		}

		public Action<LocaleTabGroup, ILocaleFileData> SelectedFileChanged { get; set; }

		private int selectedfileIndex = 0;

		public int SelectedFileIndex
		{
			get { return selectedfileIndex; }
			set
			{
				Update(ref selectedfileIndex, value);
				Notify("SelectedFile");
				SelectedFileChanged?.Invoke(this, SelectedFile);
			}
		}

		public ILocaleFileData SelectedFile
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
				Update(ref visibility, value);
			}
		}

		public void UpdateCombinedData()
		{
			Tabs = new ObservableCollectionExtended<ILocaleFileData>();
			Tabs.Add(CombinedEntries);
			Tabs.AddRange(DataFiles);

			CombinedEntries.Entries = new ObservableCollectionExtended<LocaleKeyEntry>();
			foreach (var obj in DataFiles)
			{
				CombinedEntries.Entries.AddRange(obj.Entries);
			}
			CombinedEntries.Entries.OrderBy(e => e.Key);
			Notify("CombinedEntries");
			Notify("Tabs");
			Log.Here().Activity($"Updated combined entries for '{Name}'.");
		}

		public void SelectFirst()
		{
			SelectedFileIndex = 0;
		}

		public void SelectLast()
		{
			SelectedFileIndex = Tabs.Count - 1;
		}

		public LocaleTabGroup(string name = "")
		{
			
			Name = name;
			CombinedEntries = new BaseLocaleFileData("All");
			CombinedEntries.Locked = true;
			DataFiles = new ObservableCollectionExtended<ILocaleFileData>();
			Tabs = new ObservableCollectionExtended<ILocaleFileData>();
			UpdateAllCommand = new ActionCommand(UpdateCombinedData);
		}
	}
}
