using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using LL.SCG.Controls;

namespace LL.SCG.Converters
{
	public class StringToFilterConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is string filterName)
			{
				FileBrowserFilter filter = CommonFileFilters.DefaultFilters.Where(f => f.Name.ToLower() == filterName.ToLower()).FirstOrDefault();
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

				if(filterNames.Length > 0)
				{
					List<FileBrowserFilter> filters = new List<FileBrowserFilter>();

					foreach (var filterName in filterNames)
					{
						FileBrowserFilter filter = CommonFileFilters.DefaultFilters.Where(f => f.Name.ToLower() == filterName.ToLower()).FirstOrDefault();
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
}