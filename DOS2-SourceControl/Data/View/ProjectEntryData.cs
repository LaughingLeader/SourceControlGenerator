using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LL.DOS2.SourceControl.Data.Xml;

namespace LL.DOS2.SourceControl.Data.View
{
	public class ProjectEntryData : PropertyChangedBase
	{
		private ModuleInfo moduleInfo;

		[Browsable(false)]
		public ModuleInfo ModuleInfo
		{
			get { return moduleInfo; }
			set
			{
				moduleInfo = value;
				RaisePropertyChanged("ModuleInfo");
			}
		}

		private ProjectInfo projectInfo;

		[Browsable(false)]
		public ProjectInfo ProjectInfo
		{
			get { return projectInfo; }
			set
			{
				projectInfo = value;
				RaisePropertyChanged("ProjectInfo");
			}
		}

		public string Name => ModuleInfo.Name;
		public string Author => ModuleInfo.Author;
		public string Type => ModuleInfo.Type;
		public string Version => ModuleInfo.Version;

		public ProjectEntryData(ProjectInfo pInfo, ModuleInfo mInfo)
		{
			this.ProjectInfo = pInfo;
			this.ModuleInfo = mInfo;
		}
	}
}
