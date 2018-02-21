using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LL.SCG.Util
{
	public static class EnumHelper
	{
		public static string Description(this System.Enum eValue)
		{
			var nAttributes = eValue.GetType().GetField(eValue.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);
			if (nAttributes.Any())
				return (nAttributes.First() as DescriptionAttribute).Description;

			// If no description is found, the least we can do is replace underscores with spaces
			// You can add your own custom default formatting logic here
			TextInfo oTI = CultureInfo.CurrentCulture.TextInfo;
			return oTI.ToTitleCase(oTI.ToLower(eValue.ToString().Replace("_", " ")));
		}

		public static IEnumerable<ValueDescription> GetAllValuesAndDescriptions(Type t)
		{
			if (!t.IsEnum)
				throw new ArgumentException("t must be an enum type");

			return System.Enum.GetValues(t).Cast<System.Enum>().Select((e) => new ValueDescription() { Value = e, Description = e.Description() }).ToList();
		}
	}

	public class ValueDescription
	{
		public object Value { get; set; }
		public object Description { get; set; }
	}
}
