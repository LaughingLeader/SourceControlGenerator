using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using LL.SCG.Data.App;
using LL.SCG.Data.View;
using LL.SCG.Interfaces;
using Newtonsoft.Json;
using LL.SCG.Enum;

namespace LL.SCG.Data
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
		private string gitRootDirectory;

		[VisibleToView("Git Projects Root Directory", FileBrowseType.Directory)]
		[Bindable(BindableSupport.Yes, BindingDirection.TwoWay)]
		public string GitRootDirectory
		{
			get { return gitRootDirectory; }
			set { gitRootDirectory = value; RaisePropertyChanged("GitRootDirectory"); }
		}

		private string backupRootDirectory;

		[VisibleToView("Backup Root Directory", FileBrowseType.Directory)]
		[Bindable(BindableSupport.Yes, BindingDirection.TwoWay)]
		public string BackupRootDirectory
		{
			get { return backupRootDirectory; }
			set
			{
				backupRootDirectory = value;
				RaisePropertyChanged("BackupRootDirectory");
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
				templatesSettingsFile = value;
				RaisePropertyChanged("TemplateSettingsFile");
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
				userKeywordsFile = value;
				RaisePropertyChanged("UserKeywordsFile");
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
				addedProjectsFile = value;
				RaisePropertyChanged("AddedProjectsFile");
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
				gitGenerationSettings = value;
				RaisePropertyChanged("GitGenSettingsFile");
			}
		}


		private string lastBackupPath = "";

		[Bindable(BindableSupport.No)]
		public string LastBackupPath
		{
			get { return lastBackupPath; }
			set
			{
				lastBackupPath = value;
				RaisePropertyChanged("LastBackupPath");
			}
		}

		[Bindable(BindableSupport.No)]
		public ObservableCollection<TemplateFileData> TemplateFiles { get; set; }

		public virtual void SetToDefault(IModuleData Data)
		{
			BackupRootDirectory = DefaultPaths.Backups(Data);
			GitRootDirectory = DefaultPaths.GitRoot(Data);
			AddedProjectsFile = DefaultPaths.ProjectsAppData(Data);
			TemplateSettingsFile = DefaultPaths.TemplateSettings(Data);
			UserKeywordsFile = DefaultPaths.Keywords(Data);
			GitGenSettingsFile = DefaultPaths.GitGenSettings(Data);
		}

		public void Init(IModuleData Data)
		{
			SetToDefault(Data);
			TemplateFiles = new ObservableCollection<TemplateFileData>();
		}
	}
}
