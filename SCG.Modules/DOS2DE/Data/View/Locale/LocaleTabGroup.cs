using DynamicData.Binding;
using ReactiveUI;
using Reactive.Bindings.Extensions;
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
	public class LocaleTabGroup : ReactiveObject
	{
		private string name;

		public string Name
		{
			get { return name; }
			set
			{
				this.RaiseAndSetIfChanged(ref name, value);
			}
		}

		public List<string> SourceDirectories { get; set; } = new List<string>();

		private bool changesUnsaved = true;

		public bool ChangesUnsaved
		{
			get { return changesUnsaved; }
			set
			{
				this.RaiseAndSetIfChanged(ref changesUnsaved, value);
			}
		}

		private ObservableCollectionExtended<ILocaleFileData> dataFiles;

		public ObservableCollectionExtended<ILocaleFileData> DataFiles
		{
			get { return dataFiles; }
			set
			{
				dataFiles = value;
			}
		}

		public ObservableCollectionExtended<ILocaleFileData> Tabs { get; set; }

		private ILocaleFileData combinedEntries;

		public ILocaleFileData CombinedEntries
		{
			get { return combinedEntries; }
			private set
			{
				this.RaiseAndSetIfChanged(ref combinedEntries, value);
			}
		}

		public Action<LocaleTabGroup, ILocaleFileData> SelectedFileChanged { get; set; }

		private int selectedfileIndex = 0;

		public int SelectedFileIndex
		{
			get { return selectedfileIndex; }
			set
			{
				this.RaiseAndSetIfChanged(ref selectedfileIndex, value);
				this.RaisePropertyChanged("SelectedFile");
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
				this.RaiseAndSetIfChanged(ref visibility, value);
			}
		}

		private bool canAddFiles = false;

		public bool CanAddFiles
		{
			get => canAddFiles;
			set { this.RaiseAndSetIfChanged(ref canAddFiles, value); }
		}

		public void UpdateCombinedData()
		{
			Tabs = new ObservableCollectionExtended<ILocaleFileData>();
			Tabs.Add(CombinedEntries);
			Tabs.AddRange(DataFiles);

			CombinedEntries.Entries = new ObservableCollectionExtended<ILocaleKeyEntry>();
			foreach (var obj in DataFiles)
			{
				CombinedEntries.Entries.AddRange(obj.Entries);
			}
			CombinedEntries.Entries.OrderBy(e => e.Key);
			this.RaisePropertyChanged("CombinedEntries");
			this.RaisePropertyChanged("Tabs");
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

		public LocaleViewModel Parent { get; private set; }

		public void OnSelectedKeyChanged(ILocaleKeyEntry keyEntry, bool selected)
		{
			Parent.KeyEntrySelected(keyEntry, selected);
		}

		public LocaleTabGroup(LocaleViewModel parent, string name = "")
		{
			Parent = parent;
			Name = name;
			CombinedEntries = new BaseLocaleFileData(this, "All");
			CombinedEntries.Locked = true;
			DataFiles = new ObservableCollectionExtended<ILocaleFileData>();
			Tabs = new ObservableCollectionExtended<ILocaleFileData>();
			UpdateAllCommand = new ActionCommand(UpdateCombinedData);
		}
	}
}
