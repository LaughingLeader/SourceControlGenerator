using SCG.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCG.Modules.DOS2DE.Data.View.Locale
{
	public class BaseLocaleFileData : PropertyChangedBase, ILocaleFileData
	{
		public ObservableRangeCollection<LocaleKeyEntry> Entries { get; set; }

		public string SourcePath { get; set; }

		private string name;

		public string Name
		{
			get { return name; }
			set
			{
				name = value;
				RaisePropertyChanged("Name");
				UpdateDisplayName();
			}
		}

		private string displayName;

		public string DisplayName
		{
			get { return displayName; }
			private set
			{
				displayName = value;
				RaisePropertyChanged("DisplayName");
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
				active = value;
				RaisePropertyChanged("Active");
			}
		}

		private bool locked = false;

		public bool Locked
		{
			get { return locked; }
			set
			{
				locked = value;
				RaisePropertyChanged("Locked");
			}
		}

		private bool changesUnsaved = false;

		public bool ChangesUnsaved
		{
			get { return changesUnsaved; }
			set
			{
				changesUnsaved = value;
				RaisePropertyChanged("ChangesUnsaved");
				UpdateDisplayName();
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

		public BaseLocaleFileData(string name = "")
		{
			Entries = new ObservableRangeCollection<LocaleKeyEntry>();

			Name = name;
		}
	}

	public class LocaleFileData : BaseLocaleFileData
	{
		public LSLib.LS.Resource Source { get; private set; }

		public LSLib.LS.Enums.ResourceFormat Format { get; set; }

		public LocaleFileData(LSLib.LS.Enums.ResourceFormat resourceFormat, LSLib.LS.Resource res, string sourcePath, string name = "") : base(name)
		{
			Source = res;
			SourcePath = sourcePath;
			Format = resourceFormat;
		}
	}
}
