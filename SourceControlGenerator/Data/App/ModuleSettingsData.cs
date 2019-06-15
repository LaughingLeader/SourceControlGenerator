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

	public class ModuleSettingsData : PropertyChangedBase, IModuleSettingsData
	{
		private string defaultAuthor;

		[VisibleToView("Default Git Author", SettingsViewPropertyType.Text)]
		[Bindable(BindableSupport.Yes, BindingDirection.TwoWay)]
		public string DefaultAuthor
		{
			get { return defaultAuthor; }
			set
			{
				Update(ref defaultAuthor, value);
			}
		}

		private string defaultEmail;

		[VisibleToView("Default Git Email", SettingsViewPropertyType.Text)]
		[Bindable(BindableSupport.Yes, BindingDirection.TwoWay)]
		public string DefaultEmail
		{
			get { return defaultEmail; }
			set
			{
				Update(ref defaultEmail, value);
			}
		}

		private string gitRootDirectory;

		[VisibleToView("Git Projects Root Directory", FileBrowseType.Directory)]
		[Bindable(BindableSupport.Yes, BindingDirection.TwoWay)]
		public string GitRootDirectory
		{
			get { return gitRootDirectory; }
			set { Update(ref gitRootDirectory, value);}
		}

		private string backupRootDirectory;

		[VisibleToView("Backup Root Directory", FileBrowseType.Directory)]
		[Bindable(BindableSupport.Yes, BindingDirection.TwoWay)]
		public string BackupRootDirectory
		{
			get { return backupRootDirectory; }
			set
			{
				Update(ref backupRootDirectory, value);
			}
		}

		private string templatesSettingsFile;

		[VisibleToView("Template Settings", FileBrowseType.File)]
		[Bindable(BindableSupport.Yes, BindingDirection.TwoWay)]
		public string TemplateSettingsFile
		{
			get { return templatesSettingsFile; }
			set
			{
				Update(ref templatesSettingsFile, value);
			}
		}

		private string userKeywordsFile;

		[VisibleToView("User Keywords", FileBrowseType.File)]
		[Bindable(BindableSupport.Yes, BindingDirection.TwoWay)]
		public string UserKeywordsFile
		{
			get { return userKeywordsFile; }
			set
			{
				Update(ref userKeywordsFile, value);
			}
		}

		private string addedProjectsFile;

		[VisibleToView("Added Projects", FileBrowseType.File)]
		[Bindable(BindableSupport.Yes, BindingDirection.TwoWay)]
		public string AddedProjectsFile
		{
			get { return addedProjectsFile; }
			set
			{
				Update(ref addedProjectsFile, value);
			}
		}

		private string gitGenerationSettings;

		[VisibleToView("Git Generation Settings", FileBrowseType.File)]
		[Bindable(BindableSupport.Yes, BindingDirection.TwoWay)]
		public string GitGenSettingsFile
		{
			get { return gitGenerationSettings; }
			set
			{
				Update(ref gitGenerationSettings, value);
			}
		}


		private string lastBackupPath = "";

		public string LastBackupPath
		{
			get { return lastBackupPath; }
			set
			{
				Update(ref lastBackupPath, value);
			}
		}

		public ObservableCollection<TemplateFileData> TemplateFiles { get; set; }

		private bool firstTimeSetup = true;

		public bool FirstTimeSetup
		{
			get { return firstTimeSetup; }
			set
			{
				Update(ref firstTimeSetup, value);
			}
		}

		private BackupMode backupMode = BackupMode.Zip;

		public BackupMode BackupMode
		{
			get { return backupMode; }
			set
			{
				Update(ref backupMode, value);
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
