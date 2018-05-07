using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace LL.SCG.Converters
{
	public class EnumDescriptionConverter : IValueConverter
	{
		private string GetEnumDescription(System.Enum enumObj)
		{
			FieldInfo fieldInfo = enumObj.GetType().GetField(enumObj.ToString());

			object[] attribArray = fieldInfo.GetCustomAttributes(false);

			if (attribArray.Length == 0)
			{
				return enumObj.ToString();
			}
			else
			{
				DescriptionAttribute attrib = attribArray.OfType<DescriptionAttribute>().FirstOrDefault();
				if(attrib != null) return attrib.Description;
			}

			return String.Empty;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			System.Enum myEnum = (System.Enum)value;
			string description = GetEnumDescription(myEnum);
			return description;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return string.Empty;
		}
	}
}