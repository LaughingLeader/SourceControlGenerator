using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCG.Extensions
{
	public static class ArrayExtensions
	{
		public static T ValueOrDefault<T>(this T[] array, int index)
		{
			if(index < array.Length)
			{
				return array[index];
			}
			return default(T);
		}
	}
}
