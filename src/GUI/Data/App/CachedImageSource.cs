using System.Windows.Media.Imaging;

namespace SCG.Data.App;

public class CachedImageSource : ReactiveObject
{
	private BitmapImage source;

	public BitmapImage Source
	{
		get { return source; }
		set
		{
			this.RaiseAndSetIfChanged(ref source, value);
		}
	}

	private string sourcePath;

	public string SourcePath
	{
		get { return sourcePath; }
		set
		{
			this.RaiseAndSetIfChanged(ref sourcePath, value);
		}
	}

	public void Init(string imagePath)
	{
		RxApp.MainThreadScheduler.Schedule(() =>
		{
			SourcePath = imagePath;

			Source = new BitmapImage();
			Source.BeginInit();
			Source.CacheOption = BitmapCacheOption.OnLoad;
			//Source.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
			Source.UriSource = new Uri(SourcePath, UriKind.Absolute);
			Source.EndInit();

		});
	}
}
