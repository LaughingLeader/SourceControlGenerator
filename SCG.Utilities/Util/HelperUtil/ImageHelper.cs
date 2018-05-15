using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCG.Util.HelperUtil
{
	public enum ImageType
	{
		None = 0,
		BMP = 0x4D42,
		JPG = 0xD8FF,
		GIF = 0x4947,
		PCX = 0x050A,
		PNG = 0x5089,
		PSD = 0x4238,
		RAS = 0xA659,
		SGI = 0xDA01,
		TIFF = 0x4949
	}

	public class ImageHelper
	{
		private Dictionary<int, ImageType> _imageTag;
		public ImageHelper()
		{
			_imageTag = new Dictionary<int, ImageType>();
			_imageTag[(int)ImageType.BMP] = ImageType.BMP;
			_imageTag[(int)ImageType.JPG] = ImageType.JPG;
			_imageTag[(int)ImageType.GIF] = ImageType.GIF;
			_imageTag[(int)ImageType.PCX] = ImageType.PCX;
			_imageTag[(int)ImageType.PNG] = ImageType.PNG;
			_imageTag[(int)ImageType.PSD] = ImageType.PSD;
			_imageTag[(int)ImageType.RAS] = ImageType.RAS;
			_imageTag[(int)ImageType.SGI] = ImageType.SGI;
			_imageTag[(int)ImageType.TIFF] = ImageType.TIFF;
		}

		public ImageType CheckImageType(string path)
		{
			byte[] buf = new byte[2];
			try
			{
				using (StreamReader sr = new StreamReader(path))
				{
					int i = sr.BaseStream.Read(buf, 0, buf.Length);
					if (i != buf.Length)
					{
						return ImageType.None;
					}
				}
			}
			catch (Exception ex)
			{
				Log.Here().Error($"Error reading image: {ex.ToString()}");
				return ImageType.None;
			}
			return CheckImageType(buf);
		}

		public ImageType CheckImageType(byte[] buf)
		{
			if (buf == null || buf.Length < 2)
			{
				return ImageType.None;
			}

			int key = (buf[1] << 8) + buf[0];
			ImageType s;
			if (_imageTag.TryGetValue(key, out s))
			{
				return s;
			}
			return ImageType.None;
		}
	}
}
