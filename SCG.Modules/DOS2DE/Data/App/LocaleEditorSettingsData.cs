using SCG.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alphaleonis.Win32.Filesystem;

namespace SCG.Modules.DOS2DE.Data.App
{
	public class LocaleEditorSettingsData : PropertyChangedBase
	{
		private string lastFileImportPath = "";

		public string LastFileImportPath
		{
			get { return lastFileImportPath; }
			set
			{
				lastFileImportPath = value;
				RaisePropertyChanged("LastFileImportPath");
			}
		}

		private string lastEntryimportPath = "";

		public string LastEntryImportPath
		{
			get { return lastEntryimportPath; }
			set
			{
				lastEntryimportPath = value;
				RaisePropertyChanged("LastEntryImportPath");
			}
		}

		private bool exportKeys = false;

		public bool ExportKeys
		{
			get { return exportKeys; }
			set
			{
				exportKeys = value;
				RaisePropertyChanged("ExportKeys");
				Log.Here().Activity($"ExportKeys set to {exportKeys}");
			}
		}

		private bool exportSource = false;

		public bool ExportSource
		{
			get { return exportSource; }
			set
			{
				exportSource = value;
				RaisePropertyChanged("ExportSource");
				Log.Here().Activity($"ExportSource set to {exportSource}");
			}
		}

		public LocaleEditorSettingsData()
		{
			
		}
	}

	public class LocaleEditorSettingsDesignData : LocaleEditorSettingsData
	{
		public LocaleEditorSettingsDesignData() : base()
		{
			LastFileImportPath = "G:\\Divinity Original Sin 2\\DefEd\\Data\\Mods\\Nemesis_627c8d3a-7e6b-4fd2-8ce5-610d553fdbe9\\Localization";
			LastEntryImportPath = "D:\\Projects\\_Visual_Studio\\SourceControlGenerator\\SourceControlGenerator\\bin";
		}
	}
}
