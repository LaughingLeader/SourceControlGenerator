using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LL.DOS2.SourceControl
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
	}
}
