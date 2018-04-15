using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LL.SCG.Data
{
	public class ManagedProjectsData
	{
		public List<ProjectAppData> Projects { get; set; }

		public ManagedProjectsData()
		{
			Projects = new List<ProjectAppData>();
		}
	}

	public class ProjectAppData
	{
		public string Name { get; set; }
		public string UUID { get; set; }
		public string LastBackupUTC { get; set; }
	}
}
