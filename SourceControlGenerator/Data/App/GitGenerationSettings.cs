using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LL.SCG.Interfaces;
using Newtonsoft.Json;

namespace LL.SCG.Data
{
	public enum LicenseType
	{
		[Description("None")]
		None,
		[Description("MIT")]
		MIT,
		[Description("Apache")]
		Apache,
		[Description("GNU")]
		GNU,
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

		public ObservableCollection<TemplateGenerationData> TemplateSettings { get; set; }

		[JsonIgnore]
		public ObservableCollection<IProjectData> ExportProjects { get; set; }


		public GitGenerationSettings()
		{
			TemplateSettings = new ObservableCollection<TemplateGenerationData>();
			ExportProjects = new ObservableCollection<IProjectData>();
			SelectedLicense = LicenseType.MIT;
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
