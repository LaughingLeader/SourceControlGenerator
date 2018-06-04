using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using SCG.Collections;
using SCG.Interfaces;
using Newtonsoft.Json;

namespace SCG.Data
{
	public enum LicenseType
	{
		[Description("None")]
		None,
		[Description("MIT")]
		MIT,
		[Description("Apache")]
		Apache,
		[Description("GPL")]
		GPL,
		[Description("Custom")]
		Custom
	}

	public class GitGenerationSettings : PropertyChangedBase
	{
		private LicenseType licenseType;

		public LicenseType SelectedLicense
		{
			get { return licenseType; }
			set
			{
				licenseType = value;
				RaisePropertyChanged("SelectedLicense");
			}
		}

		public ObservableImmutableList<TemplateGenerationData> TemplateSettings { get; set; }

		[JsonIgnore]
		public ObservableImmutableList<IProjectData> ExportProjects { get; set; }

		private bool createJunctions = true;

		public bool CreateJunctions
		{
			get { return createJunctions; }
			set
			{
				createJunctions = value;
				RaisePropertyChanged("CreateJunctions");
			}
		}

		private bool initGit = true;

		public bool InitGit
		{
			get { return initGit; }
			set
			{
				initGit = value;
				RaisePropertyChanged("InitGit");
			}
		}

		private bool initialGitCommit = true;

		public bool InitialGitCommit
		{
			get { return initialGitCommit; }
			set
			{
				initialGitCommit = value;
				RaisePropertyChanged("InitialGitCommit");
			}
		}

		public object TemplateSettingsLock { get; private set; } = new object();

		public object ExportProjectsLock { get; private set; } = new object();

		public void SetTemplateSettings(ObservableImmutableList<TemplateGenerationData> newList)
		{
			if(TemplateSettings != null) BindingOperations.DisableCollectionSynchronization(TemplateSettings);
			TemplateSettings = newList;
			BindingOperations.EnableCollectionSynchronization(TemplateSettings, TemplateSettingsLock);
		}

		public void SetExportProjects(ObservableImmutableList<IProjectData> newList)
		{
			if (ExportProjects != null) BindingOperations.DisableCollectionSynchronization(ExportProjects);
			ExportProjects = newList;
			BindingOperations.EnableCollectionSynchronization(ExportProjects, ExportProjectsLock);
		}

		public GitGenerationSettings()
		{
			TemplateSettings = new ObservableImmutableList<TemplateGenerationData>();
			ExportProjects = new ObservableImmutableList<IProjectData>();
			SelectedLicense = LicenseType.MIT;

			BindingOperations.EnableCollectionSynchronization(TemplateSettings, TemplateSettingsLock);
			BindingOperations.EnableCollectionSynchronization(ExportProjects, ExportProjectsLock);
		}
	}

	public class TemplateGenerationData : PropertyChangedBase
	{
		private string id;

		public string ID
		{
			get { return id; }
			set
			{
				id = value;
				RaisePropertyChanged("ID");
			}
		}

		private string templateName;

		public string TemplateName
		{
			get { return templateName; }
			set
			{
				templateName = value;
				RaisePropertyChanged("TemplateName");
			}
		}

		private bool enabled;

		public bool Enabled
		{
			get { return enabled; }
			set
			{
				enabled = value;
				RaisePropertyChanged("Enabled");
			}
		}

		private string tooltip;

		[JsonIgnore]
		public string TooltipText
		{
			get { return tooltip; }
			set
			{
				tooltip = value;
				RaisePropertyChanged("TooltipText");
			}
		}

	}
}
