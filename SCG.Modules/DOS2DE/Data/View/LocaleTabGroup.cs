using SCG.Commands;
using SCG.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SCG.Modules.DOS2DE.Data.View
{
	public class LocaleTabGroup : PropertyChangedBase
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

		private string sourceDirectory;

		public string SourceDirectory
		{
			get { return sourceDirectory; }
			set
			{
				sourceDirectory = value;
				RaisePropertyChanged("SourceDirectory");
			}
		}

		private bool changesUnsaved = true;

		public bool ChangesUnsaved
		{
			get { return changesUnsaved; }
			set
			{
				changesUnsaved = value;
				RaisePropertyChanged("ChangesUnsaved");
			}
		}

		private ObservableRangeCollection<ILocaleFileData> dataFiles;

		public ObservableRangeCollection<ILocaleFileData> DataFiles
		{
			get { return dataFiles; }
			set
			{
				dataFiles = value;
				UpdateCombinedData();
			}
		}

		public ObservableRangeCollection<ILocaleFileData> Tabs { get; set; }

		private ILocaleFileData combinedEntries;

		public ILocaleFileData CombinedEntries
		{
			get { return combinedEntries; }
			private set
			{
				combinedEntries = value;
				RaisePropertyChanged("CombinedEntries");
			}
		}

		public Action<LocaleTabGroup, ILocaleFileData> SelectedFileChanged { get; set; }

		private int selectedfileIndex = 0;

		public int SelectedFileIndex
		{
			get { return selectedfileIndex; }
			set
			{
				selectedfileIndex = value;
				RaisePropertyChanged("SelectedFileIndex");
				RaisePropertyChanged("SelectedFile");
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
				visibility = value;
				RaisePropertyChanged("Visibility");
			}
		}

		public void UpdateCombinedData()
		{
			Tabs = new ObservableRangeCollection<ILocaleFileData>();
			Tabs.Add(CombinedEntries);
			Tabs.AddRange(DataFiles);

			CombinedEntries.Entries = new ObservableRangeCollection<LocaleKeyEntry>();
			foreach (var obj in DataFiles)
			{
				CombinedEntries.Entries.AddRange(obj.Entries);
			}
			CombinedEntries.Entries.OrderBy(e => e.Key);
			RaisePropertyChanged("CombinedEntries");
			RaisePropertyChanged("Tabs");
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
			DataFiles = new ObservableRangeCollection<ILocaleFileData>();
			Tabs = new ObservableRangeCollection<ILocaleFileData>();

			UpdateAllCommand = new ActionCommand(UpdateCombinedData);
		}
	}
}
