using DynamicData.Binding;
using Newtonsoft.Json;
using ReactiveUI;
using SCG.Data;
using SCG.Modules.DOS2DE.Data.Savable;
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

namespace SCG.Modules.DOS2DE.Data.View.Locale
{
	public class BaseLocaleFileData : ReactiveObject, ILocaleFileData
	{
		public ObservableCollectionExtended<ILocaleKeyEntry> Entries { get; set; } = new ObservableCollectionExtended<ILocaleKeyEntry>();

		private ReadOnlyObservableCollection<ILocaleKeyEntry> visibleEntries;
		public ReadOnlyObservableCollection<ILocaleKeyEntry> VisibleEntries => visibleEntries;

		public LocaleTabGroup Parent { get; private set; }

		public string SourcePath { get; set; }

		private LocaleFileLinkData fileLinkData;

		public LocaleFileLinkData FileLinkData
		{
			get => fileLinkData;
			set
			{
				this.RaiseAndSetIfChanged(ref fileLinkData, value);
				HasFileLink = !String.IsNullOrEmpty(fileLinkData.ReadFrom);
			}
		}


		private string name;

		public string Name
		{
			get { return name; }
			set
			{
				this.RaiseAndSetIfChanged(ref name, value);
				UpdateDisplayName();
				renamingName = name;
			}
		}

		private string displayName;

		public string DisplayName
		{
			get { return displayName; }
			private set
			{
				this.RaiseAndSetIfChanged(ref displayName, value);
			}
		}

		private void UpdateDisplayName()
		{
			//var str = $"<font size='8' align='left'>{Name}</font>";
			//DisplayName = !ChangesUnsaved ? str : "<font color='#FF0000'>*</font>" + str;
			//string name = Alphaleonis.Win32.Filesystem.Path.GetFileNameWithoutExtension(Name);
			DisplayName = !ChangesUnsaved ? Name : "*" + Name;
		}

		private bool selected = false;

		public bool Selected
		{
			get { return selected; }
			set
			{
				this.RaiseAndSetIfChanged(ref selected, value);
			}
		}

		private bool locked = false;

		public bool Locked
		{
			get { return locked; }
			set
			{
				this.RaiseAndSetIfChanged(ref locked, value);
			}
		}

		private bool changesUnsaved = false;

		public bool ChangesUnsaved
		{
			get { return changesUnsaved; }
			set
			{
				this.RaiseAndSetIfChanged(ref changesUnsaved, value);
				UpdateDisplayName();
			}
		}

		private bool canClose = false;

		public bool CanClose
		{
			get => canClose;
			set { this.RaiseAndSetIfChanged(ref canClose, value); }
		}

		private bool canRename = true;

		public bool CanRename
		{
			get => canRename;
			set { this.RaiseAndSetIfChanged(ref canRename, value); }
		}

		private bool canOverride = false;

		public bool CanOverride
		{
			get => canOverride;
			set { this.RaiseAndSetIfChanged(ref canOverride, value); }
		}

		private bool isRenaming = false;

		public bool IsRenaming
		{
			get => isRenaming;
			set { this.RaiseAndSetIfChanged(ref isRenaming, value); }
		}

		private string renamingName;

		public string RenameText
		{
			get => renamingName;
			set { this.RaiseAndSetIfChanged(ref renamingName, value); }
		}

		private bool hasFileLink;

		public bool HasFileLink
		{
			get => hasFileLink;
			set { this.RaiseAndSetIfChanged(ref hasFileLink, value); }
		}

		private bool canCreateFileLink;

		public bool CanCreateFileLink
		{
			get => canCreateFileLink;
			set { this.RaiseAndSetIfChanged(ref canCreateFileLink, value); }
		}

		public List<LocaleUnsavedChangesData> UnsavedChanges = new List<LocaleUnsavedChangesData>();

		public void SetChangesUnsaved(bool b, bool clearUnsaved = true)
		{
			if(!b && clearUnsaved)
			{
				UnsavedChanges.Clear();
			}

			foreach(var key in Entries)
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
			if(matchedChange != null)
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
			if(UnsavedChanges.Remove(change))
			{
				Parent?.UpdateUnsavedChanges();
				ChangesUnsaved = UnsavedChanges.Count > 0;
			}
		}

		public BaseLocaleFileData(LocaleTabGroup parent, string name = "")
		{
			Name = name;
			Parent = parent;
			var entryChangeSet = Entries.ToObservableChangeSet();
			//Setting ChangesUnsaved to true when any item in entries is unsaved
			var anyChanged = entryChangeSet.AutoRefresh(x => x.ChangesUnsaved).ToCollection();
			anyChanged.Any(x => x.Any(y => y.ChangesUnsaved == true)).ToProperty(this, x => x.ChangesUnsaved);
			entryChangeSet.AutoRefresh(x => x.Visible).Filter(x => x.Visible == true).ObserveOn(RxApp.MainThreadScheduler).Bind(out visibleEntries).Subscribe();

			this.WhenAnyValue(x => x.VisibleEntries.Count, x => x.Selected, (x,y) => x > 0 && y).
				ObserveOn(RxApp.MainThreadScheduler).Subscribe((x) =>
			{
				if(Selected && VisibleEntries.Count > 0)
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
