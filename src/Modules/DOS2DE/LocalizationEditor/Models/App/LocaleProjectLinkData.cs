using System.Runtime.Serialization;

namespace SCG.Modules.DOS2DE.LocalizationEditor.Models;

[DataContract]
public class LocaleProjectLinkData
{
	[DataMember]
	public string ProjectUUID { get; set; }

	[DataMember]
	public List<LocaleFileLinkData> Links = [];
}

[DataContract]
public class LocaleFileLinkData
{
	[DataMember]
	[Reactive]
	public string ReadFrom { get; set; }

	[DataMember]
	[Reactive]
	public string TargetFile { get; set; }
}
