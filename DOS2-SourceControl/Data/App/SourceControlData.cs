using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LL.DOS2.SourceControl.Data
{
	public class SourceControlData
	{
		public string ProjectName { get; set; }

		[JsonIgnore] public string RepositoryPath { get; set; }
	}
}
