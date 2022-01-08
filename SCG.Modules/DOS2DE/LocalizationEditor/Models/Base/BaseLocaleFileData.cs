using DynamicData.Binding;
using Newtonsoft.Json;
using ReactiveUI;
using SCG.Data;
using System;
using System.Reactive;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using DynamicData;
using DynamicData.List;
using System.Reactive.Linq;
using ReactiveUI.Fody.Helpers;
using SCG.Modules.DOS2DE.Data.View.Locale;

namespace SCG.Modules.DOS2DE.LocalizationEditor.Models
{
	public class BaseLocaleFileData : ReactiveObject
	{
		public ObservableCollectionExtended<ILocaleKeyEntry> Entries { get; set; } = new ObservableCollectionExtended<ILocaleKeyEntry>();

		private ReadOnlyObservableCollection<ILocaleKeyEntry> visibleEntries;
		public ReadOnlyObservableCollection<ILocaleKeyEntry> VisibleEntries => visibleEntries;

		public LocaleTabGroup Parent { get; private set; }

		public string SourcePath { get; set; }

		[Reactive] public LocaleFileLinkData FileLinkData { get; set; }

		[Reactive] public string Name { get; set; }

		[Reactive] public bool ChangesUnsaved { get; set; }

		[Reactive] public string DisplayName { get; set; }

		[Reactive] public bool Selected { get; set; }

		[Reactive] public bool Locked { get; set; }

		[Reactive] public bool CanClose { get; set; }

		[Reactive] public bool CanRename { get; set; }

		[Reactive] public bool CanOverride { get; set; }

		[Reactive] public bool IsRenaming { get; set; }

		[Reactive] public string RenameText { get; set; }

		[Reactive] public bool HasFileLink { get; set; }

		[Reactive] public bool CanCreateFileLink { get; set; }

		[Reactive] public bool IsCombinedData { get; set; }

		public List<LocaleUnsavedChangesData> UnsavedChanges = new List<LocaleUnsavedChangesData>();

		private void UpdateDisplayName()
		{
			//var str = $"<font size='8' align='left'>{Name}</font>";
			//DisplayName = !ChangesUnsaved ? str : "<font color='#FF0000'>*</font>" + str;
			//string name = Alphaleonis.Win32.Filesystem.Path.GetFileNameWithoutExtension(Name);
			DisplayName = !ChangesUnsaved ? Name : "*" + Name;
		}

		public void SetChangesUnsaved(bool b, bool clearUnsaved = true)
		{
			if (!b && clearUnsaved)
			{
				UnsavedChanges.Clear();
			}

			foreach (var key in Entries)
			{
				key.ChangesUnsaved = false;
			}
			ChangesUnsaved = b;
		}

		public void SelectAll()
		{
			foreach (var entry in Entries) { entry.Selected = true; }
		}

		public void SelectNone()
		{
			foreach (var entry in Entries) { entry.Selected = false; }
		}

		public void OnSelectedKeyChanged(ILocaleKeyEntry key, bool selected)
		{
			Parent.OnSelectedKeyChanged(key, selected);
		}

		private bool allSelected;

		public bool AllSelected
		{
			get { return allSelected; }
			set
			{
				this.RaiseAndSetIfChanged(ref allSelected, value);
				if (allSelected)
					SelectAll();
				else
					SelectNone();
			}
		}

		private bool LocaleUnsavedChangeMatch(LocaleUnsavedChangesData a, LocaleUnsavedChangesData b)
		{
			return a.KeyEntry == b.KeyEntry && a.ChangeType == b.ChangeType && a.NewValue == b.LastValue;
		}

		public void AddUnsavedChange(ILocaleKeyEntry entry, LocaleUnsavedChangesData unsavedChange)
		{
			// Remove this unsaved change if it matches a previous one
			var matchedChange = UnsavedChanges.FirstOrDefault(x => LocaleUnsavedChangeMatch(x, unsavedChange));
			if (matchedChange != null)
			{
				//Log.Here().Activity($"Removing unsaved change as it matches a previous value '{matchedChange.ChangeType} | {matchedChange.LastValue} => {matchedChange.NewValue}'.");
				RemoveUnsavedChange(matchedChange);
				entry.ChangesUnsaved = false;
			}
			else
			{
				//Log.Here().Activity($"Added unsaved change '{unsavedChange.ChangeType} | {unsavedChange.LastValue} => {unsavedChange.NewValue}'.");
				UnsavedChanges.Add(unsavedChange);
				entry.ChangesUnsaved = ChangesUnsaved = Parent.ChangesUnsaved = true;
			}
		}

		public void RemoveUnsavedChange(LocaleUnsavedChangesData change)
		{
			if (UnsavedChanges.Remove(change))
			{
				Parent?.UpdateUnsavedChanges();
				ChangesUnsaved = UnsavedChanges.Count > 0;
			}
		}

		public BaseLocaleFileData(LocaleTabGroup parent, string name = "")
		{
			Name = name;
			Parent = parent;

			this.WhenAnyValue(x => x.Name, x => x.ChangesUnsaved).Subscribe((obs) =>
			{
				RenameText = obs.Item1;
				UpdateDisplayName();
			});

			this.WhenAnyValue(x => x.FileLinkData.ReadFrom, (f) => !string.IsNullOrEmpty(f)).ToProperty(this, x => x.HasFileLink);

			var entryChangeSet = Entries.ToObservableChangeSet();
			//Setting ChangesUnsaved to true when any item in entries is unsaved
			var anyChanged = entryChangeSet.AutoRefresh(x => x.ChangesUnsaved).ToCollection();
			anyChanged.Any(x => x.Any(y => y.ChangesUnsaved == true)).ToProperty(this, x => x.ChangesUnsaved);
			entryChangeSet.AutoRefresh(x => x.Visible).Filter(x => x.Visible == true).ObserveOn(RxApp.MainThreadScheduler).Bind(out visibleEntries).Subscribe();

			this.WhenAnyValue(x => x.VisibleEntries.Count, x => x.Selected, (x, y) => x > 0 && y).
				ObserveOn(RxApp.MainThreadScheduler).Subscribe((x) =>
			{
				if (Selected && VisibleEntries.Count > 0)
				{
					int index = 1;
					for (var i = 0; i < VisibleEntries.Count; i++)
					{
						var item = VisibleEntries[i];
						//System.Diagnostics.Trace.WriteLine($"{name}({Selected}) | items({i}) = {item.EntryKey}");
						item.Index = index;
						index++;
					}
				}
			});
		}
	}
}
