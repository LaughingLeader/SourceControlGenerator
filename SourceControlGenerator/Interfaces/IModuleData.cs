using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SCG.Collections;
using SCG.Data;
using SCG.Data.App;
using SCG.Data.View;

namespace SCG.Interfaces
{
	public interface IModuleData
	{
		string AssemblyName { get; set; }
		string ModuleName { get; set; }
		string ModuleFolderName { get; set; }

		GitGenerationSettings GitGenerationSettings { get; set; }

		ObservableImmutableList<TemplateEditorData> Templates { get; set; }
		ObservableImmutableList<KeywordData> KeyList { get; set; }

		UserKeywordData UserKeywords { get; set; }

		bool AddTemplateControlVisible { get; set; }

		IModuleSettingsData ModuleSettings { get; }

		string LoadStringResource(string ResourceName);

		void InitializeSettings();
		void LoadSettings();

		void CreateNewTemplateData();
		void HideAddTemplateControl();
		void AddTemplate();

		//Events

		event EventHandler OnSettingsReverted;
	}

	public interface IModuleSettingsData : INotifyPropertyChanged
	{
		ObservableCollection<TemplateFileData> TemplateFiles { get; set; }

		bool FirstTimeSetup { get; set; }

		//Paths
		string GitRootDirectory { get; set; }
		string BackupRootDirectory { get; set; }
		string TemplateSettingsFile { get; set; }
		string UserKeywordsFile { get; set; }
		string AddedProjectsFile { get; set; }
		string GitGenSettingsFile { get; set; }

		string LastBackupPath { get; set; }

		BackupMode BackupMode { get; set; }

		//Methods

		void Init(IModuleData Data);
		void SetToDefault(IModuleData Data);

		void RaisePropertyChanged(string propertyName);
	}
}
