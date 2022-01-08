using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SCG.Modules.DOS2DE.Data.Savable
{
	[DataContract]
	public class LocaleProjectLinkData
	{
		[DataMember]
		public string ProjectUUID { get; set; }

		[DataMember]
		public List<LocaleFileLinkData> Links = new List<LocaleFileLinkData>();
	}

	[DataContract]
	public struct LocaleFileLinkData
	{
		[DataMember]
		[Reactive]
		public string ReadFrom { get; set; }

		[DataMember]
		[Reactive]
		public string TargetFile { get; set; }
	}
}
