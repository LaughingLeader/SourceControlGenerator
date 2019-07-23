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

namespace SCG.Data
{
	[DataContract]
	public class ManagedProjectsData : ReactiveObject
	{
		[DataMember]
		public ObservableCollectionExtended<ProjectAppData> Projects { get; set; } = new ObservableCollectionExtended<ProjectAppData>();

		private readonly ReadOnlyObservableCollection<ProjectAppData> _sortedProjects;
		public ReadOnlyObservableCollection<ProjectAppData> SortedProjects => _sortedProjects;

		public ManagedProjectsData()
		{
			Projects.ToObservableChangeSet().Sort(SortExpressionComparer<ProjectAppData>.Descending(p => p.Name)).Bind(out _sortedProjects).Subscribe();
		}
	}

	[DataContract]
	public class ProjectAppData : ReactiveObject
	{
		private string name;

		[DataMember]
		public string Name
		{
			get => name;
			set { this.RaiseAndSetIfChanged(ref name, value); }
		}


		private string uuid;

		[DataMember]
		public string UUID
		{
			get => uuid;
			set { this.RaiseAndSetIfChanged(ref uuid, value); }
		}


		private string lastBackupUTC;

		[DataMember]
		public string LastBackupUTC
		{
			get => lastBackupUTC;
			set { this.RaiseAndSetIfChanged(ref lastBackupUTC, value); }
		}

	}
}
