using SCG.Data;
using SCG.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ReactiveUI;
using System.Reactive.Concurrency;

namespace SCG.Data.App
{
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
}
