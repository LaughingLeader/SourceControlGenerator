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
using SCG.Modules.DOS2DE.Data.View.Locale;

namespace SCG.Modules.DOS2DE.Data
{
	[DataContract]
	public class LocaleEditorSettingsData : ReactiveObject
	{
		[DataMember]
		public ObservableCollectionExtended<LocaleEditorProjectSettingsData> Projects { get; set; }

		private bool loadRootTemplates = true;

		[DataMember]
		public bool LoadRootTemplates
		{
			get { return loadRootTemplates; }
			set
			{
				this.RaiseAndSetIfChanged(ref loadRootTemplates, value);
			}
		}

		private bool loadGlobals = true;

		[DataMember]
		public bool LoadGlobals
		{
			get { return loadGlobals; }
			set
			{
				this.RaiseAndSetIfChanged(ref loadGlobals, value);
			}
		}

		private bool loadLevelData = false;

		[DataMember]
		public bool LoadLevelData
		{
			get { return loadLevelData; }
			set
			{
				this.RaiseAndSetIfChanged(ref loadLevelData, value);
			}
		}

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
