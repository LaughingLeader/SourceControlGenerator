﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using LL.SCG.Util;

namespace LL.SCG.Converters
{
	[ValueConversion(typeof(System.Enum), typeof(IEnumerable<ValueDescription>))]
	public class EnumToCollectionConverter : MarkupExtension, IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return EnumHelper.GetAllValuesAndDescriptions(value.GetType());
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			return this;
		}
	}
}
