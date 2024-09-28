using LSLib.LS;

namespace SCG.BG3.Models;
public class ModuleData : ReactiveObject
{
	[Reactive] public string? FilePath { get; set; }

	[Reactive] public string? UUID { get; set; }
	[Reactive] public string? Name { get; set; }
	[Reactive] public string? Description { get; set; }
	[Reactive] public string? Author { get; set; }
	[Reactive] public string? Folder { get; set; }
	[Reactive] public string? Version64 { get; set; }


	[ObservableAsProperty] public string? VersionDisplayValue { get; }

	private async Task ParseMetaAsync(CancellationToken token)
	{
		if (string.IsNullOrEmpty(FilePath) || !File.Exists(FilePath)) return;

		using var fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
		if (fs != null)
		{
			var result = new byte[fs.Length];
			await fs.ReadAsync(result.AsMemory(0, (int)fs.Length), token);
			fs.Position = 0;
			using var reader = new LSXReader(fs);
			reader.SerializationSettings.DefaultByteSwapGuids = true;
			var resource = reader.Read();
			if (resource != null && resource.Regions.TryGetValue("Config", out var rootNode)
				&& rootNode.Children.TryGetValue("ModuleInfo", out var moduleInfoList)
				&& moduleInfoList?.FirstOrDefault() is Node moduleInfo)
			{
				UUID = moduleInfo.GetAttributeValue(nameof(UUID));
				Name = moduleInfo.GetAttributeValue(nameof(Name));
				Description = moduleInfo.GetAttributeValue(nameof(Description));
				Folder = moduleInfo.GetAttributeValue(nameof(Folder));
				Version64 = moduleInfo.GetAttributeValue(nameof(Version64));
			}
		}
	}

	public static async Task<ModuleData> FromMetaAsync(string path, CancellationToken token)
	{
		var modProject = new ModuleData()
		{
			FilePath = path
		};

		await modProject.ParseMetaAsync(token);

		return modProject;
	}

	private static string? VersionToDisplayValue(string? versionStr)
	{
		if(ulong.TryParse(versionStr, out var versionNum))
		{
			var major = versionNum >> 55;
			var minor = (versionNum >> 47) & 0xFF;
			var revision = (versionNum >> 31) & 0xFFFF;
			var build = versionNum & 0x7FFFFFFFUL;
			return $"{major}.{minor}.{revision}.{build}";
		}
		return null;
	}

	public ModuleData()
	{
		this.WhenAnyValue(x => x.Version64).Select(VersionToDisplayValue).ToUIProperty(this, x => x.VersionDisplayValue);
	}
}
