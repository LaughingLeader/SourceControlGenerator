using DynamicData.Binding;

using SCG.Data.View;

using System.Globalization;
using System.Windows.Data;

namespace SCG.Converters;

public class VisibleLogsConverter : IMultiValueConverter
{
	public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		if (values[0] is ObservableCollectionExtended<LogData> logs)
		{
			if (values[1] is string searchText && !String.IsNullOrWhiteSpace(searchText))
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