using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SCG.Extensions;

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
					try
					{
						BitmapImage image = new BitmapImage();
						image.BeginInit();
						image.CacheOption = BitmapCacheOption.OnLoad;
						//image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
						image.UriSource = new Uri(uri, UriKind.Absolute);
						image.EndInit();
						//Log.Here().Activity("Cached image: " + uri);
						return image;
					}
					catch(Exception ex)
					{
						Log.Here().Error($"Error converting string to BitmapImage: {ex.ToString()}");
					}
				}
			}
			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}
	}

	public class StringToCachedImageFallbackConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			var sourceImage = "";

			var param1 = (string)values.ValueOrDefault(0);
			var param2 = (string)values.ValueOrDefault(1);

			if (String.IsNullOrEmpty(param1) && !String.IsNullOrEmpty(param2))
			{
				sourceImage = param2;
			}
			else if(!String.IsNullOrEmpty(param1))
			{
				sourceImage = param1;
			}

			try
			{
				//Log.Here().Error("Images: " + String.Join(",", values));
				if (!String.IsNullOrWhiteSpace(sourceImage))
				{
					BitmapImage image = new BitmapImage();
					image.BeginInit();
					image.CacheOption = BitmapCacheOption.OnLoad;
					//image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
					image.UriSource = new Uri(sourceImage, UriKind.RelativeOrAbsolute);
					image.EndInit();
					return image;
				}
				else
				{
					string fallbackImage = @"pack://application:,,,/SourceControlGenerator;component/Resources/Icons/WarningRule_16x.png";
					BitmapImage image = new BitmapImage();
					image.BeginInit();
					image.CacheOption = BitmapCacheOption.OnLoad;
					//image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
					image.UriSource = new Uri(fallbackImage, UriKind.RelativeOrAbsolute);
					image.EndInit();
					return image;
				}
			}
			catch (Exception ex)
			{
				Log.Here().Error($"Error converting string to BitmapImage: {ex.ToString()}");
			}

			return null;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}