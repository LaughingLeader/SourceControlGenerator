using SCG.Commands;

using System.Globalization;
using System.Windows.Data;

namespace SCG.Converters;

public class ParamsToOpenFileBrowserParamsConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is string pathParam && !String.IsNullOrEmpty(pathParam))
		{
			Log.Here().Activity(pathParam);
			return new OpenFileBrowserParams()
			{
				StartDirectory = pathParam
			};
		}
		else if (value is OpenFileBrowserParams openFileBrowserParams)
		{
			return openFileBrowserParams;
		}
		else if (value is object[] values)
		{
			var fileBrowserParams = new OpenFileBrowserParams();
			for (var i = 0; i < values.Length; i++)
			{
				var val = values[i];
				if (val is string strParam)
				{
					if (i == 0)
					{
						fileBrowserParams.StartDirectory = strParam;
					}
					else
					{
						fileBrowserParams.Title = strParam;
					}
				}
			}
			return fileBrowserParams;
		}
		else
		{
			Log.Here().Activity($"{value}");
		}
		return null;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}