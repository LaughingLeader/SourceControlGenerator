using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace LL.DOS2.SourceControl.Controls
{
	public class EmptyListVisibilityConverter : IValueConverter
	{
		private bool EmptyList(object value)
		{
			if (value == null) return true;
			if (value is IList && value.GetType().GetGenericArguments().Length > 0)
			{
				IList list = (IList)value;
				return list.Count <= 0;
			}
			return true;
		}

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return !EmptyList(value) ? Visibility.Collapsed : Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
