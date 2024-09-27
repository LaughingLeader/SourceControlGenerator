using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Windows.Media
{
	public static class ColorExtensions
	{
		public static string ToHexString(this System.Windows.Media.Color c)
		{
			return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
		}

		public static string ToRGBString(this System.Windows.Media.Color c)
		{
			return "RGB(" + c.R.ToString() + "," + c.G.ToString() + "," + c.B.ToString() + ")";
		}
	}
}
