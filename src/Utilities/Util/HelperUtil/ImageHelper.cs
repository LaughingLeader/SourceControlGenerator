namespace SCG.Util.HelperUtil;

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
	private readonly Dictionary<int, ImageType> _imageTag;
	public ImageHelper()
	{
		_imageTag = new Dictionary<int, ImageType>
		{
			[(int)ImageType.BMP] = ImageType.BMP,
			[(int)ImageType.JPG] = ImageType.JPG,
			[(int)ImageType.GIF] = ImageType.GIF,
			[(int)ImageType.PCX] = ImageType.PCX,
			[(int)ImageType.PNG] = ImageType.PNG,
			[(int)ImageType.PSD] = ImageType.PSD,
			[(int)ImageType.RAS] = ImageType.RAS,
			[(int)ImageType.SGI] = ImageType.SGI,
			[(int)ImageType.TIFF] = ImageType.TIFF
		};
	}

	public ImageType CheckImageType(string path)
	{
		var buf = new byte[2];
		try
		{
			using (var sr = new System.IO.StreamReader(path))
			{
				var i = sr.BaseStream.Read(buf, 0, buf.Length);
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

		var key = (buf[1] << 8) + buf[0];
		ImageType s;
		if (_imageTag.TryGetValue(key, out s))
		{
			return s;
		}
		return ImageType.None;
	}
}
