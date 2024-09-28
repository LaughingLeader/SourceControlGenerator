namespace SCG.Data.Xml;

public class DependencyInfo : ReactiveObject
{
	public string Folder { get; set; }
	public string MD5 { get; set; }
	public string Name { get; set; }
	public string Version { get; set; }
}
