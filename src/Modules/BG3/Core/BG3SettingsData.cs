using SCG.Data;
using SCG.Interfaces;
using SCG.SCGEnum;

using System.Runtime.Serialization;

namespace SCG.BG3.Core;

[DataContract]
public class BG3SettingsData : ModuleSettingsData
{
	[VisibleToView("Data Directory", FileBrowseType.Directory)]
	[DataMember, Reactive]
	public string? DataDirectory { get; set; }

	[VisibleToView("Directory Layout", FileBrowseType.File)]
	[DataMember, Reactive]
	public string? DirectoryLayoutFile { get; set; }

	public bool FindDataDirectory()
	{
		
		return false;
	}

	public override void SetToDefault(IModuleData Data)
	{
		base.SetToDefault(Data);

		FindDataDirectory();

		DirectoryLayoutFile = Path.Join(DefaultPaths.ModuleSettingsFolder(Data), "DirectoryLayout.txt");
	}
}
