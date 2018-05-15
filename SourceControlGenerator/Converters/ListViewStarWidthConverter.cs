using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace SCG.Converters
{
	public class ListViewStarWidthConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			ListView listView = value as ListView;
			double width = listView.Width;

			GridView gridView = listView.View as GridView;

			for (int i = 0; i < gridView.Columns.Count; i++)
			{
				if (!Double.IsNaN(gridView.Columns[i].Width))
				{
					width -= gridView.Columns[i].Width;
				}	
			}

			// this is to take care of margin/padding
			width = width - 5;

			Log.Here().Important($"ListView column width is {width.ToString()}");

			return width;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
