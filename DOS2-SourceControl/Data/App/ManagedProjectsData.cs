using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LL.DOS2.SourceControl.Data
{
	public class ManagedProjectsData
	{
		public List<ProjectAppData> ManagedProjects { get; set; }

		public ManagedProjectsData()
		{
			ManagedProjects = new List<ProjectAppData>();
		}
	}

	public class ProjectAppData
	{
		public string Name { get; set; }
		public string GUID { get; set; }
		public DateTime? LastBackup { get; set; }
	}
}
