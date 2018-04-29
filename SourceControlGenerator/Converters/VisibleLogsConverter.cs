using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using LL.SCG.Data.View;

namespace LL.SCG.Converters
{
	public class VisibleLogsConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if(values[0] is ObservableCollection<LogData> logs)
			{
				if(values[1] is string searchText && !String.IsNullOrWhiteSpace(searchText))
				{
					//Log.Here().Important($"Search val: {searchVal}");
					return logs.Where(l => l.IsVisible && l.Message.CaseInsensitiveContains(searchText))?.ToList();
				}
				else
				{
					return logs.Where(l => l.IsVisible)?.ToList();
				}
			}
			return null;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}