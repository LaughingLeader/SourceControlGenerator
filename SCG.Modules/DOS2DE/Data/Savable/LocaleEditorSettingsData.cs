using SCG.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alphaleonis.Win32.Filesystem;
using System.Runtime.Serialization;
using ReactiveUI;
using SCG.Modules.DOS2DE.Data.View;
using DynamicData.Binding;
using DynamicData;
using System.Windows.Input;

namespace SCG.Modules.DOS2DE.Data
{
	[DataContract]
	public class LocaleEditorSettingsData : ReactiveObject
	{
		[DataMember]
		public ObservableCollectionExtended<LocaleEditorProjectSettingsData> Projects { get; set; }

		public LocaleEditorProjectSettingsData GetProjectSettings(ModProjectData modProjectData)
		{
			if(modProjectData != null)
			{
				var entry = Projects.FirstOrDefault(x => x.FolderName == modProjectData.FolderName);
				if (entry == null)
				{
					var data = new LocaleEditorProjectSettingsData()
					{
						Name = modProjectData.DisplayName,
						FolderName = modProjectData.FolderName
					};
					Projects.Add(data);
					return data;
				}
				else
				{
					return entry;
				}
			}
			return null;
		}

		public ICommand SaveCommand { get; set; }

		public LocaleEditorSettingsData()
		{
			Projects = new ObservableCollectionExtended<LocaleEditorProjectSettingsData>();
		}
	}

	[DataContract]
	public class LocaleEditorProjectSettingsData : ReactiveObject
	{
		private string name;

		public string Name
		{
			get => name;
			set { this.RaiseAndSetIfChanged(ref name, value); }
		}

		private string foldername = "";

		[DataMember]
		public string FolderName
		{
			get { return foldername; }
			set
			{
				this.RaiseAndSetIfChanged(ref foldername, value);
			}
		}

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
	}

	public class LocaleEditorSettingsDesignData : LocaleEditorSettingsData
	{
		public LocaleEditorSettingsDesignData() : base()
		{
			Projects.Add(new LocaleEditorProjectSettingsData()
			{
				LastFileImportPath = "G:\\Divinity Original Sin 2\\DefEd\\Data\\Mods\\Nemesis_627c8d3a-7e6b-4fd2-8ce5-610d553fdbe9\\Localization",
				LastEntryImportPath = "D:\\Projects\\_Visual_Studio\\SourceControlGenerator\\SourceControlGenerator\\bin",
				Name = "Test Name"
			});
		}
	}
}
