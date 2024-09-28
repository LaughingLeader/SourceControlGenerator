using DynamicData;

using Newtonsoft.Json;

using System.Runtime.Serialization;

namespace SCG.Data;

[DataContract]
public class ManagedProjectsData : ReactiveObject
{
	public SourceCache<ProjectAppData, string> SavedProjects { get; set; } = new SourceCache<ProjectAppData, string>(x => x.UUID);

	[DataMember]
	[JsonProperty("Projects")]
	public List<ProjectAppData> SortedProjects { get; set; }

	public void Sort()
	{
		SortedProjects = SavedProjects.Items.OrderBy(m => m.Name).ToList();
	}
}

[DataContract]
public class ProjectAppData
{
	[DataMember]
	public string Name { get; set; }

	[DataMember]
	public string UUID { get; set; }

	[DataMember]
	public string LastBackupUTC { get; set; }
}
