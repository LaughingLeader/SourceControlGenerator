using SCG.Core;

using System.Globalization;
using System.Windows.Data;

namespace SCG.Converters;

public class StringToFilterConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is string filterName)
		{
			var filter = CommonFileFilters.DefaultFilters.Where(f => f.Name.ToLower() == filterName.ToLower()).FirstOrDefault();
			if (filter != null)
			{
				return filter;
			}
		}

		return CommonFileFilters.All;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return null;
	}
}

public class StringToFilterListConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is string filterCollection)
		{
			var filterNames = filterCollection.Split(';');

			if (filterNames.Length > 0)
			{
				List<FileBrowserFilter> filters = [];

				foreach (var filterName in filterNames)
				{
					var filter = CommonFileFilters.DefaultFilters.Where(f => f.Name.ToLower() == filterName.ToLower()).FirstOrDefault();
					filters.Add(filter);
				}
			}
		}

		return CommonFileFilters.DefaultFilters;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return null;
	}
}