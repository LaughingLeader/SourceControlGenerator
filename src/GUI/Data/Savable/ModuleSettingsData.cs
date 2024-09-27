using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using SCG.Data.App;
using SCG.Data.View;
using SCG.Interfaces;
using Newtonsoft.Json;
using SCG.SCGEnum;
using ReactiveUI;
using System.Runtime.Serialization;

namespace SCG.Data
{
	public enum SettingsViewPropertyType
	{
		None = 0,
		Browser,
		Text
	}

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class VisibleToViewAttribute : Attribute
	{
		private bool visible = false;
		private string name;
		private FileBrowseType fileBrowseType;
		private SettingsViewPropertyType viewType;

		public string Name => name;
		public bool Visible => visible;
		public FileBrowseType FileBrowseType => fileBrowseType;
		public SettingsViewPropertyType ViewType => viewType;

		/*
		public VisibleToView(bool attributeValue, SettingsViewPropertyType viewType = SettingsViewPropertyType.None)
		{
			visible = attributeValue;
			this.viewType = viewType;
			if (!visible || viewType != SettingsViewPropertyType.Browser)
			{
				fileBrowseType = FileBrowseType.Disabled;
			}
			else
			{
				fileBrowseType = FileBrowseType.File;
			}
		}
		*/

		public VisibleToViewAttribute(string visibleName, FileBrowseType browseType = FileBrowseType.Disabled)
		{
			name = visibleName;
			fileBrowseType = browseType;
			if(fileBrowseType != FileBrowseType.Disabled)
			{
				viewType = SettingsViewPropertyType.Browser;
			}
			visible = fileBrowseType != FileBrowseType.Disabled;
		}

		public VisibleToViewAttribute(string visibleName, SettingsViewPropertyType viewType, FileBrowseType browseType = FileBrowseType.Disabled)
		{
			name = visibleName;
			this.viewType = viewType;
			visible = viewType != SettingsViewPropertyType.None;
			fileBrowseType = browseType;
		}

		public VisibleToViewAttribute()
		{
			viewType = SettingsViewPropertyType.None;
			fileBrowseType = FileBrowseType.Disabled;
			visible = false;
		}
	}

	[DataContract]
	public class ModuleSettingsData : ReactiveObject, IModuleSettingsData
	{
		private string defaultAuthor;

		[VisibleToView("Default Git Author", SettingsViewPropertyType.Text)]
		[Bindable(BindableSupport.Yes, System.ComponentModel.BindingDirection.TwoWay)]
		[DataMember]
		public string DefaultAuthor
		{
			get { return defaultAuthor; }
			set
			{
				this.RaiseAndSetIfChanged(ref defaultAuthor, value);
			}
		}

		private string defaultEmail;

		[VisibleToView("Default Git Email", SettingsViewPropertyType.Text)]
		[Bindable(BindableSupport.Yes, System.ComponentModel.BindingDirection.TwoWay)]
		[DataMember]
		public string DefaultEmail
		{
			get { return defaultEmail; }
			set
			{
				this.RaiseAndSetIfChanged(ref defaultEmail, value);
			}
		}

		private string gitRootDirectory;

		[VisibleToView("Git Projects Root Directory", FileBrowseType.Directory)]
		[Bindable(BindableSupport.Yes, System.ComponentModel.BindingDirection.TwoWay)]
		[DataMember]
		public string GitRootDirectory
		{
			get { return gitRootDirectory; }
			set { this.RaiseAndSetIfChanged(ref gitRootDirectory, value);}
		}

		private string backupRootDirectory;

		[VisibleToView("Backup Root Directory", FileBrowseType.Directory)]
		[Bindable(BindableSupport.Yes, System.ComponentModel.BindingDirection.TwoWay)]
		[DataMember]
		public string BackupRootDirectory
		{
			get { return backupRootDirectory; }
			set
			{
				this.RaiseAndSetIfChanged(ref backupRootDirectory, value);
			}
		}

		private string templatesSettingsFile;

		[VisibleToView("Template Settings", FileBrowseType.File)]
		[Bindable(BindableSupport.Yes, System.ComponentModel.BindingDirection.TwoWay)]
		[DataMember]
		public string TemplateSettingsFile
		{
			get { return templatesSettingsFile; }
			set
			{
				this.RaiseAndSetIfChanged(ref templatesSettingsFile, value);
			}
		}

		private string userKeywordsFile;

		[VisibleToView("User Keywords", FileBrowseType.File)]
		[Bindable(BindableSupport.Yes, System.ComponentModel.BindingDirection.TwoWay)]
		[DataMember]
		public string UserKeywordsFile
		{
			get { return userKeywordsFile; }
			set
			{
				this.RaiseAndSetIfChanged(ref userKeywordsFile, value);
			}
		}

		private string addedProjectsFile;

		[VisibleToView("Added Projects", FileBrowseType.File)]
		[Bindable(BindableSupport.Yes, System.ComponentModel.BindingDirection.TwoWay)]
		[DataMember]
		public string AddedProjectsFile
		{
			get { return addedProjectsFile; }
			set
			{
				this.RaiseAndSetIfChanged(ref addedProjectsFile, value);
			}
		}

		private string gitGenerationSettings;

		[VisibleToView("Git Generation Settings", FileBrowseType.File)]
		[Bindable(BindableSupport.Yes, System.ComponentModel.BindingDirection.TwoWay)]
		[DataMember]
		public string GitGenSettingsFile
		{
			get { return gitGenerationSettings; }
			set
			{
				this.RaiseAndSetIfChanged(ref gitGenerationSettings, value);
			}
		}


		private string lastBackupPath = "";

		[DataMember]
		public string LastBackupPath
		{
			get { return lastBackupPath; }
			set
			{
				this.RaiseAndSetIfChanged(ref lastBackupPath, value);
			}
		}

		[DataMember]
		public ObservableCollection<TemplateFileData> TemplateFiles { get; set; }

		private bool firstTimeSetup = true;

		[DataMember]
		public bool FirstTimeSetup
		{
			get { return firstTimeSetup; }
			set
			{
				this.RaiseAndSetIfChanged(ref firstTimeSetup, value);
			}
		}

		private BackupMode backupMode = BackupMode.Zip;

		[DataMember]
		public BackupMode BackupMode
		{
			get { return backupMode; }
			set
			{
				this.RaiseAndSetIfChanged(ref backupMode, value);
			}
		}

		public virtual void SetToDefault(IModuleData Data)
		{
			BackupRootDirectory = DefaultPaths.ModuleBackupsFolder(Data);
			GitRootDirectory = DefaultPaths.ModuleProjectsFolder(Data);
			AddedProjectsFile = DefaultPaths.ModuleAddedProjectsFile(Data);
			TemplateSettingsFile = DefaultPaths.ModuleTemplateSettingsFile(Data);
			UserKeywordsFile = DefaultPaths.ModuleKeywordsFile(Data);
			GitGenSettingsFile = DefaultPaths.ModuleGitGenSettingsFile(Data);
			//BackupMode = BackupMode.Zip;
		}

		public void Init(IModuleData Data)
		{
			SetToDefault(Data);
			TemplateFiles = new ObservableCollection<TemplateFileData>();
		}
	}
}
