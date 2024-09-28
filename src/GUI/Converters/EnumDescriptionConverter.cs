using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;

namespace SCG.Converters;

public class EnumDescriptionConverter : IValueConverter
{
	private string GetEnumDescription(System.Enum enumObj)
	{
		var fieldInfo = enumObj.GetType().GetField(enumObj.ToString());

		var attribArray = fieldInfo.GetCustomAttributes(false);

		if (attribArray.Length == 0)
		{
			return enumObj.ToString();
		}
		else
		{
			var attrib = attribArray.OfType<DescriptionAttribute>().FirstOrDefault();
			if (attrib != null) return attrib.Description;
		}

		return String.Empty;
	}

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		var myEnum = (System.Enum)value;
		var description = GetEnumDescription(myEnum);
		return description;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return value;
	}
}