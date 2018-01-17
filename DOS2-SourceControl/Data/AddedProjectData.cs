using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LL.DOS2.SourceControl.Data
{
	/// <summary>
	/// Simple data container for projects that are added to the source control/backup manager.
	/// </summary>
	public class AddedProjectData
	{
		public string ProjectName { get; set; }
		public string ProjectGUID { get; set; }

		public AddedProjectData()
		{
			ProjectName = "";
			ProjectGUID = "";
		}

		public AddedProjectData(string Name, string GUID)
		{
			ProjectName = Name;
			ProjectGUID = GUID;
		}
	}
}
