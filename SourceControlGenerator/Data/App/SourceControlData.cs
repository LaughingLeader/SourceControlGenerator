using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LL.SCG.Data
{
	public class SourceControlData
	{
		public string ProjectName { get; set; }

		[JsonIgnore] public string RepositoryPath { get; set; }
	}
}
