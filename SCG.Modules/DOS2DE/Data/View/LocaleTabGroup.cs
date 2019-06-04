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

		public Action<LocaleTabGroup, IKeyFileData> SelectedFileChanged { get; set; }

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

		public LocaleTabGroup(string name = "")
		{
			Name = name;
			CombinedEntries = new BaseLocaleFileData("All");
			CombinedEntries.Locked = true;
			DataFiles = new ObservableRangeCollection<IKeyFileData>();
			Tabs = new ObservableRangeCollection<IKeyFileData>();

			UpdateAllCommand = new ActionCommand(UpdateCombinedData);
		}
	}
}
