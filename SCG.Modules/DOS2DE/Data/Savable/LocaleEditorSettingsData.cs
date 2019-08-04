using SCG.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alphaleonis.Win32.Filesystem;
using System.Runtime.Serialization;
using ReactiveUI;

namespace SCG.Modules.DOS2DE.Data
{
	[DataContract]
	public class LocaleEditorSettingsData : ReactiveObject
	{
		private string lastFileImportPath = "";

		[DataMember]
		public string LastFileImportPath
		{
			get { return lastFileImportPath; }
			set
			{
				this.RaiseAndSetIfChanged(ref lastFileImportPath, value);
			}
		}

		private string lastEntryimportPath = "";

		[DataMember]
		public string LastEntryImportPath
		{
			get { return lastEntryimportPath; }
			set
			{
				this.RaiseAndSetIfChanged(ref lastEntryimportPath, value);
			}
		}

		private bool exportKeys = false;

		[DataMember]
		public bool ExportKeys
		{
			get { return exportKeys; }
			set
			{
				this.RaiseAndSetIfChanged(ref exportKeys, value);
				Log.Here().Activity($"ExportKeys set to {exportKeys}");
			}
		}

		private bool exportSource = false;

		[DataMember]
		public bool ExportSource
		{
			get { return exportSource; }
			set
			{
				this.RaiseAndSetIfChanged(ref exportSource, value);
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
