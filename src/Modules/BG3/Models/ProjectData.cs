using LSLib.LS;

using System.Windows.Media.Imaging;

namespace SCG.BG3.Models;
public class ProjectData : ReactiveObject
{
	[Reactive] public string? UUID { get; set; }
	[Reactive] public string? Module { get; set; }
	[Reactive] public string? Name { get; set; }
	[Reactive] public string? GameProject { get; set; }

	[Reactive] public string? ProjectMetaFilePath { get; set; }
	[Reactive] public string? ThumbnailFilePath { get; set; }
	[Reactive] public string? ModMetaFilePath { get; set; }
	[Reactive] public ModuleData? Mod { get; set; }


	[Reactive] public BitmapImage? Thumbnail { get; private set; }

	private static readonly string[] _thumbnailImageTypes = [".png", ".jpg", ".jpeg"];

	private async Task ParseMetaAsync(CancellationToken token)
	{
		if (string.IsNullOrEmpty(ProjectMetaFilePath) || !File.Exists(ProjectMetaFilePath)) return;

		using var fs = new FileStream(ProjectMetaFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
		if(fs != null)
		{
			var result = new byte[fs.Length];
			await fs.ReadAsync(result.AsMemory(0, (int)fs.Length), token);
			fs.Position = 0;
			using var reader = new LSXReader(fs);
			reader.SerializationSettings.DefaultByteSwapGuids = true;
			var resource = reader.Read();

			if(resource != null && resource.Regions.TryGetValue("MetaData", out var rootNode))
			{
				UUID = rootNode.GetAttributeValue(nameof(UUID));
				Name = rootNode.GetAttributeValue(nameof(Name));
				Module = rootNode.GetAttributeValue(nameof(Module));
				GameProject = rootNode.GetAttributeValue(nameof(GameProject));

				var projectFolder = Path.GetDirectoryName(ProjectMetaFilePath)!;

				if (token.IsCancellationRequested) return;

				var thumbnail = Directory.EnumerateFiles(projectFolder, "*", DefaultResources.FlatSearchOptions)
					.Where(x => _thumbnailImageTypes.Any(y => x.EndsWith(y, StringComparison.OrdinalIgnoreCase)))
					.FirstOrDefault();
				ThumbnailFilePath = thumbnail;
			}
		}
	}

	public static async Task<ProjectData> FromMetaAsync(string path, CancellationToken token)
	{
		var modProject = new ProjectData()
		{
			ProjectMetaFilePath = path
		};

		await modProject.ParseMetaAsync(token);

		return modProject;
	}

	private IDisposable? _loadThumbnailDisp;

	private async Task LoadThumbnailAsync(IScheduler sch, CancellationToken token)
	{
		var cacheService = Locator.Current.GetService<FileCacheService>();
		if(cacheService != null)
		{
			var imageBytes = await cacheService.GetBytesAsync(ThumbnailFilePath!, token);
			if(imageBytes != null)
			{
				var image = new BitmapImage();
				using (var mem = new MemoryStream(imageBytes))
				{
					mem.Position = 0;
					image.BeginInit();
					image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
					image.CacheOption = BitmapCacheOption.OnLoad;
					image.UriSource = null;
					image.StreamSource = mem;
					image.EndInit();
				}
				image.Freeze();
				RxApp.MainThreadScheduler.Schedule(() =>
				{
					Thumbnail = image;
				});
			}
		}
	}

	public ProjectData()
	{
		this.WhenAnyValue(x => x.ThumbnailFilePath).Subscribe(path =>
		{
			_loadThumbnailDisp?.Dispose();
			if(!string.IsNullOrEmpty(path))
			{
				_loadThumbnailDisp = RxApp.TaskpoolScheduler.ScheduleAsync(LoadThumbnailAsync);
			}
		});
	}
}
