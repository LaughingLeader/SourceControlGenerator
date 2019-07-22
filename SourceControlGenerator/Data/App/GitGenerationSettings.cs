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

	public class GitGenerationSettings : ReactiveObject
	{
		private LicenseType licenseType;

		public LicenseType SelectedLicense
		{
			get { return licenseType; }
			set
			{
				Update(ref licenseType, value);
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
				Update(ref createJunctions, value);
			}
		}

		private bool initGit = true;

		public bool InitGit
		{
			get { return initGit; }
			set
			{
				Update(ref initGit, value);
			}
		}

		private bool initialGitCommit = true;

		public bool InitialGitCommit
		{
			get { return initialGitCommit; }
			set
			{
				Update(ref initialGitCommit, value);
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

	public class TemplateGenerationData : ReactiveObject
	{
		private string id;

		public string ID
		{
			get { return id; }
			set
			{
				Update(ref id, value);
			}
		}

		private string templateName;

		public string TemplateName
		{
			get { return templateName; }
			set
			{
				Update(ref templateName, value);
			}
		}

		private bool enabled;

		public bool Enabled
		{
			get { return enabled; }
			set
			{
				Update(ref enabled, value);
			}
		}

		private string tooltip;

		[JsonIgnore]
		public string TooltipText
		{
			get { return tooltip; }
			set
			{
				Update(ref tooltip, value);
			}
		}

	}
}
