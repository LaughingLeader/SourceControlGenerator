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

namespace SCG.Data.App
{
	public class CachedImageSource : PropertyChangedBase
	{
		private BitmapImage source;

		public BitmapImage Source
		{
			get { return source; }
			set
			{
				source = value;
				RaisePropertyChanged("Source");
			}
		}

		private string sourcePath;

		public string SourcePath
		{
			get { return sourcePath; }
			set
			{
				sourcePath = value;
				RaisePropertyChanged("SourcePath");
			}
		}

		public void Init(string imagePath)
		{
			AppController.Main.MainWindow.Dispatcher.BeginInvoke(new Action(() =>
			{
				SourcePath = imagePath;

				Source = new BitmapImage();
				Source.BeginInit();
				Source.CacheOption = BitmapCacheOption.OnLoad;
				//Source.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
				Source.UriSource = new Uri(SourcePath, UriKind.Absolute);
				Source.EndInit();

			}), DispatcherPriority.Background);
		}
	}
}
