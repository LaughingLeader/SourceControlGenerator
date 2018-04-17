using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using LL.SCG.Data.View;

namespace LL.SCG.ThemeSystem
{
	public static class ThemeController
	{
		private static App mainApp;

		public static Dictionary<string, ResourceDictionary> Themes { get; set; }

		private static string currentTheme;
		public static string CurrentTheme
		{
			get => currentTheme;
			private set => currentTheme = value;
		}

		public static bool UnloadCurrentTheme()
		{
			if(!String.IsNullOrEmpty(CurrentTheme))
			{
				var dict = Themes[CurrentTheme];
				if(mainApp.Resources.MergedDictionaries.Remove(dict))
				{
					CurrentTheme = "";
					return true;
				}
			}
			return false;
		}

		public static bool SetTheme(string ThemeName)
		{
			if (Themes.ContainsKey(ThemeName))
			{
				UnloadCurrentTheme();

				CurrentTheme = ThemeName;
				var dict = Themes[ThemeName];
				mainApp.Resources.MergedDictionaries.Add(dict);
				Log.Here().Important($"Set theme to {ThemeName}");

				foreach(var dic in mainApp.Resources.MergedDictionaries)
				{
					Log.Here().Important(dic.ToString());
				}
			}
			return false;
		}

		public static bool AddTheme(string ThemeName, string FileName)
		{
			if(Themes.ContainsKey(ThemeName) == false)
			{
				var dictUri = new Uri(FileName, UriKind.Relative);
				var dict = Application.LoadComponent(dictUri) as ResourceDictionary;
				Themes.Add(ThemeName, dict);
				return true;
			}

			return false;
		}

		public static void Init(App MainApp)
		{
			mainApp = MainApp;

			Themes = new Dictionary<string, ResourceDictionary>(StringComparer.OrdinalIgnoreCase);

			//AddTheme("DarkMetro", @"pack://application:,,,/SourceControlGenerator;component/ThemeSystem/Themes/DarkMetro/DarkMetroStyle.xaml");
			AddTheme("DarkMetro", @"\ThemeSystem/Themes/DarkMetro/DarkMetroStyle.xaml");

			//SetTheme("DarkMetro");
		}
	}
}
