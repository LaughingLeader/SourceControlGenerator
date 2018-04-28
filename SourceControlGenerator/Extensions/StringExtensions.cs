using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LL.SCG
{
	public static class StringExtensions
	{
		public static string ToDelimitedString<T>(this IEnumerable<T> source, Func<T, string> func)
		{
			return ToDelimitedString(source, ",", func);
		}

		public static string ToDelimitedString<T>(this IEnumerable<T> source, string delimiter, Func<T, string> func)
		{
			return String.Join(delimiter, source.Select(func).ToArray());
		}

		public static string Truncate(this string value, int maxLength)
		{
			if (string.IsNullOrEmpty(value)) return value;
			return value.Length <= maxLength ? value : value.Substring(0, maxLength);
		}

		public static string Truncate(this string value, int maxLength, string Append)
		{
			if (string.IsNullOrEmpty(value)) return value;
			return value.Length <= maxLength ? value : value.Substring(0, maxLength) + Append;
		}

		public static bool CaseInsensitiveContains(this string text, string value, StringComparison stringComparison = StringComparison.CurrentCultureIgnoreCase)
		{
			return text.IndexOf(value, stringComparison) >= 0;
		}
	}
}
