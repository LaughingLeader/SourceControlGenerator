using LSLib.LS;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace SCG.BG3.Models;
public class ModProject : ReactiveObject
{
	[Reactive] public string? ProjectMetaFilePath { get; set; }
	[Reactive] public string? ThumbnailFilePath { get; set; }
	[Reactive] public string? ModMetaFilePath { get; set; }

	[Reactive] public string? ProjectGUID { get; set; }
	[Reactive] public string? ModuleGUID { get; set; }
	[Reactive] public string? ProjectName { get; set; }

	[Reactive] public BitmapImage? Thumbnail { get; private set; }

	private static readonly string[] _thumbnailImageTypes = [".png", ".jpg", ".jpeg"];

	private async Task ParseMetaAsync(CancellationToken token)
	{
		if (string.IsNullOrEmpty(ProjectMetaFilePath) || !File.Exists(ProjectMetaFilePath)) return;

		using var fs = new FileStream(ProjectMetaFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
		if(fs != null)
		{
			await fs.ReadAsync(new byte[fs.Length], 0, (int)fs.Length, token);
			fs.Position = 0;
			using var reader = new LSXReader(fs);
			reader.SerializationSettings.DefaultByteSwapGuids = true;
			var resource = reader.Read();
			if(resource != null && resource.TryGetNodeById("root", out var rootNode))
			{
				ProjectGUID = rootNode.GetNodeValueById("UUID");
				ProjectName = rootNode.GetNodeValueById("Name");
				ModuleGUID = rootNode.GetNodeValueById("Module");

				var projectFolder = Path.GetDirectoryName(ProjectMetaFilePath)!;

				if (token.IsCancellationRequested) return;

				var thumbnail = Directory.EnumerateFiles(projectFolder, "*", DefaultResources.FlatSearchOptions)
					.Where(x => _thumbnailImageTypes.Any(y => x.EndsWith(y, StringComparison.OrdinalIgnoreCase)))
					.FirstOrDefault();
				ThumbnailFilePath = thumbnail;
			}
		}
	}

	public static async Task<ModProject> FromProjectMetaAsync(string path, CancellationToken token)
	{
		var modProject = new ModProject()
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

	public ModProject()
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
