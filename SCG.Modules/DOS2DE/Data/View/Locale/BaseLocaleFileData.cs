﻿using DynamicData.Binding;
using Newtonsoft.Json;
using ReactiveUI;
using SCG.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCG.Modules.DOS2DE.Data.View.Locale
{
	[JsonObject(MemberSerialization.OptIn)]
	public class BaseLocaleFileData : ReactiveObject, ILocaleFileData
	{
		[JsonProperty]
		public ObservableCollectionExtended<ILocaleKeyEntry> Entries { get; set; } = new ObservableCollectionExtended<ILocaleKeyEntry>();

		public string SourcePath { get; set; }

		private string name;

		[JsonProperty]
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

		private bool canRename = false;

		public bool CanRename
		{
			get => canRename;
			set { this.RaiseAndSetIfChanged(ref canRename, value); }
		}


		private bool isRenaming;

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
				this.RaiseAndSetIfChanged(ref allSelected, value);
				if (allSelected)
					SelectAll();
				else
					SelectNone();
			}
		}

		public BaseLocaleFileData(string name = "")
		{
			Name = name;
		}
	}
}