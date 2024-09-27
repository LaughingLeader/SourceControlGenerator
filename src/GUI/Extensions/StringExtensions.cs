using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCG.Extensions
{
	public static class StringExtensions
	{
		public static string ReplaceLastOccurrence(this string Source, string Find, string Replace)
		{
			int place = Source.LastIndexOf(Find);

			if (place == -1)
				return Source;

			string result = Source.Remove(place, Find.Length).Insert(place, Replace);
			return result;
		}
	}
}
