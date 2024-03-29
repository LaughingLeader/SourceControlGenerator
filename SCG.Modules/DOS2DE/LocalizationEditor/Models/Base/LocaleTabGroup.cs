﻿using DynamicData.Binding;
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
using SCG.Modules.DOS2DE.LocalizationEditor.ViewModels;
using SCG.Modules.DOS2DE.LocalizationEditor.Models;
using ReactiveUI.Fody.Helpers;

namespace SCG.Modules.DOS2DE.Data.View.Locale
{
	public class LocaleTabGroup : ReactiveObject
	{
		[Reactive] public string Name { get; set; }

		public List<string> SourceDirectories { get; set; } = new List<string>();

		[Reactive] public bool ChangesUnsaved { get; set; }

		private ObservableCollectionExtended<ILocaleFileData> dataFiles;

		public ObservableCollectionExtended<ILocaleFileData> DataFiles
		{
			get { return dataFiles; }
			set
			{
				dataFiles = value;
			}
		}

		public ObservableCollectionExtended<ILocaleFileData> Tabs { get; set; } = new ObservableCollectionExtended<ILocaleFileData>();

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
				if (SelectedFile != null) SelectedFile.Selected = false;
				this.RaiseAndSetIfChanged(ref selectedfileIndex, value);
				this.RaisePropertyChanged("SelectedFile");
				if (SelectedFileChanged != null)
				{
					SelectedFileChanged.Invoke(this, SelectedFile);
				}
				else if (SelectedFile != null)
				{
					SelectedFile.Selected = true;
				}
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

		[Reactive] public bool Visibility { get; set; } = true;

		[Reactive] public bool CanAddFiles { get; set; }

		[Reactive] public bool IsCustom { get; set; }

		public void UpdateCombinedData(bool all = false)
		{
			if (all || (Tabs.Count == 0 || CombinedEntries.Entries.Count == 0))
			{
				CombinedEntries.Entries.Clear();
				foreach (var obj in DataFiles)
				{
					CombinedEntries.Entries.AddRange(obj.Entries);
				}

				Tabs.Clear();
				Tabs.Add(CombinedEntries);
				Tabs.AddRange(DataFiles.OrderBy(x => x.Name));

				//CombinedEntries.Entries.OrderBy(e => e.Key);
				this.RaisePropertyChanged("CombinedEntries");
				this.RaisePropertyChanged("Tabs");
				Log.Here().Activity($"Updated combined entries for '{Name}'.");

				var nextIndex = -1;

				if (SelectedFile != null)
				{
					nextIndex = Tabs.IndexOf(SelectedFile);
				}
				if (SelectedFileIndex != nextIndex && nextIndex >= 0) SelectedFileIndex = nextIndex;
			}
			else
			{
				if (CombinedEntries.Entries.Count != DataFiles.Count || CombinedEntries.Entries.Count == 0)
				{
					CombinedEntries.Entries.Clear();
					foreach (var obj in DataFiles.OrderBy(x => x.Name))
					{
						CombinedEntries.Entries.AddRange(obj.Entries);
					}
					this.RaisePropertyChanged("CombinedEntries");
					this.RaisePropertyChanged("Tabs");
					Log.Here().Activity($"Updated combined entries for '{Name}'.");
				}
			}
			this.UpdateUnsavedChanges();
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

		public void UpdateUnsavedChanges()
		{
			ChangesUnsaved = Tabs.Any(f => f != CombinedEntries && f.ChangesUnsaved == true);
		}

		public void Clear()
		{
			SourceDirectories.Clear();
			CombinedEntries.Entries.Clear();
			DataFiles.Clear();
			Tabs.Clear();
		}

		public LocaleTabGroup(LocaleViewModel parent, string name = "")
		{
			Parent = parent;
			Name = name;
			CombinedEntries = new CombinedLocaleVirtualFileData(this, "All");
			CombinedEntries.IsCombinedData = true;
			CombinedEntries.Locked = true;
			this.WhenAnyValue(x => x.ChangesUnsaved).BindTo(CombinedEntries, x => x.ChangesUnsaved);
			DataFiles = new ObservableCollectionExtended<ILocaleFileData>();
			Tabs = new ObservableCollectionExtended<ILocaleFileData>();
			UpdateAllCommand = ReactiveCommand.Create(() => UpdateCombinedData(true));
		}
	}
}
