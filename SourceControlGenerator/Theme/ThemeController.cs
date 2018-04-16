using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LL.SCG.Data.View;

namespace LL.SCG.Theme
{
	public static class ThemeController
	{
		public static Dictionary<string, ThemeColorData> Themes { get; set; }

		public static ThemeColorData CurrentTheme { get; set; }

		public static void Init()
		{
			Themes = new Dictionary<string, ThemeColorData>();

			Themes.Add("Dark", ThemeColorData.DefaultDark());
			Themes.Add("Light", ThemeColorData.DefaultLight());
			
			CurrentTheme = Themes.First().Value;
			CurrentTheme.RefreshTheme();
		}
	}

	public class DebugThemeColors : List<object>
	{
		public DebugThemeColors()
		{

		}
	}

	public class DebugThemeColorData : ThemeColorData
	{

	}
}
