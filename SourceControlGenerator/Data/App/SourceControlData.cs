using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SCG.Data
{
	public class SourceControlData
	{
		public string ProjectName { get; set; }

		public string ProjectUUID { get; set; }

		[JsonIgnore] public string RepositoryPath { get; set; }
	}
}
