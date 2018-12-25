using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SCG.Converters
{
	public class StringToCachedImageConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if(value is string uri)
			{
				if(!String.IsNullOrWhiteSpace(uri))
				{
					BitmapImage image = new BitmapImage();
					image.BeginInit();
					image.CacheOption = BitmapCacheOption.OnLoad;
					//image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
					image.UriSource = new Uri(uri, UriKind.Absolute);
					image.EndInit();
					return image;
				}
			}
			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}
	}
}