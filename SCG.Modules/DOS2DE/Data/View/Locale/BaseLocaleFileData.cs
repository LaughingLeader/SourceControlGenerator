using DynamicData.Binding;
using Newtonsoft.Json;
using ReactiveUI;
using SCG.Data;
using SCG.Modules.DOS2DE.Data.Savable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SCG.Modules.DOS2DE.Data.View.Locale
{
	public class BaseLocaleFileData : ReactiveObject, ILocaleFileData
	{
		public ObservableCollectionExtended<ILocaleKeyEntry> Entries { get; set; } = new ObservableCollectionExtended<ILocaleKeyEntry>();

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

		private bool active = false;

		public bool Active
		{
			get { return active; }
			set
			{
				this.RaiseAndSetIfChanged(ref active, value);
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
				if(changesUnsaved == true && value == false)
				{
					foreach(var entry in Entries)
					{
						entry.ChangesUnsaved = false;
					}
				}

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
		public void AddUnsavedChange(LocaleUnsavedChangesData unsavedChange)
		{
			// Remove this unsaved change if it matches a previous one
			var matchedChange = UnsavedChanges.FirstOrDefault(x => x.Equals(unsavedChange));
			if(matchedChange.ChangeType != LocaleChangedField.None)
			{
				RemoveUnsavedChange(matchedChange);
			}
			else
			{
				UnsavedChanges.Add(unsavedChange);
				ChangesUnsaved = Parent.ChangesUnsaved = true;
			}
		}

		public void RemoveUnsavedChange(LocaleUnsavedChangesData unsavedChange)
		{
			UnsavedChanges.Remove(unsavedChange);
			ChangesUnsaved = UnsavedChanges.Count > 0;
			Parent.UpdateUnsavedChanges();
		}

		public BaseLocaleFileData(LocaleTabGroup parent, string name = "")
		{
			Name = name;
			Parent = parent;
		}
	}
}
