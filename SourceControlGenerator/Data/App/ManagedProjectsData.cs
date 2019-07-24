using DynamicData.Binding;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using DynamicData;
using System.Collections.ObjectModel;
using Reactive.Bindings.Extensions;
using System.Reactive;
using System.Reactive.Linq;
using Newtonsoft.Json;

namespace SCG.Data
{
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

	public struct ProjectAppData
	{
		public string Name { get; set; }

		public string UUID { get; set; }

		public string LastBackupUTC { get; set; }

	}
}
