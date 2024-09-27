using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

using Alphaleonis.Win32.Filesystem;

using Newtonsoft.Json;

using SCG.FileGen;

namespace SCG.Data
{
	[DataContract]
	public class SourceControlData
	{
		[DataMember]
		public string ProjectName { get; set; }

		[DataMember]
		public string ProjectUUID { get; set; }

		public string RepositoryPath { get; set; }

		public string SourceFile { get; set; }

		public static SourceControlData FromPath(string path)
		{
			var projectData = JsonInterface.DeserializeObject<SourceControlData>(path);
			if (projectData != null)
			{
				projectData.SourceFile = path;
				projectData.RepositoryPath = Path.GetDirectoryName(path);
			}
			return projectData;
		}
		public static async Task<SourceControlData> FromPathAsync(string path)
		{
			var projectData = await JsonInterface.DeserializeObjectAsync<SourceControlData>(path);
			if (projectData != null)
			{
				projectData.SourceFile = path;
				projectData.RepositoryPath = Path.GetDirectoryName(path);
			}
			return projectData;
		}
	}
}
