using System.Globalization;
using System.Windows.Data;

namespace SCG.Converters;

public class DateTimeToStringConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		if (value is DateTime date)
		{
			var ci = CultureInfo.CurrentUICulture;

			if (parameter != null && parameter is string formatstring)
			{
				var format = formatstring.ToCharArray().First();
				var pattern = ci.DateTimeFormat.GetAllDateTimePatterns(format).FirstOrDefault();

				if (pattern != null)
				{
					//Log.Here().Activity($"Format {format} = pattern {pattern}");
				}
				else
				{
					pattern = formatstring;
				}

				//Add leading zeroes for a nice datagrid display
				pattern = pattern.Replace("d", "dd").Replace("dddd", "dd");
				pattern = pattern.Replace("M", "MM").Replace("MMMM", "MM");
				pattern = pattern.Replace("m", "mm").Replace("mmmm", "mm");
				pattern = pattern.Replace("h", "hh").Replace("hhhh", "hh");
				pattern = pattern.Replace("H", "HH").Replace("HHHH", "HH");
				pattern = pattern.Replace("s", "ss").Replace("ssss", "ss");

				return date.ToLocalTime().ToString(pattern);
			}

			return date.ToLocalTime().ToString();
		}

		return "";
	}

	public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		return null;
	}
}