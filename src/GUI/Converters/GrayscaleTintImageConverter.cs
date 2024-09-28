using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SCG.Converters;

public class GrayscaleTintImageConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		Log.Here().Activity("Tinting image");
		if (value is BitmapSource orgBmp)
		{
			if (orgBmp.Format == PixelFormats.Bgra32)
			{
				var orgPixels = new byte[orgBmp.PixelHeight * orgBmp.PixelWidth * 4];
				var newPixels = new byte[orgPixels.Length];
				orgBmp.CopyPixels(orgPixels, orgBmp.PixelWidth * 4, 0);
				for (var i = 3; i < orgPixels.Length; i += 4)
				{
					var grayVal = ((int)orgPixels[i - 3] +
					(int)orgPixels[i - 2] + (int)orgPixels[i - 1]);

					if (grayVal != 0) grayVal = grayVal / 3;
					newPixels[i] = orgPixels[i]; //Set AlphaChannel
					newPixels[i - 3] = (byte)grayVal;
					newPixels[i - 2] = (byte)grayVal;
					newPixels[i - 1] = (byte)grayVal;
				}
				return BitmapSource.Create(orgBmp.PixelWidth, orgBmp.PixelHeight, 96, 96, PixelFormats.Bgra32, null, newPixels, orgBmp.PixelWidth * 4);
			}
		}
		return value;
	}

	public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		return null;
	}
}