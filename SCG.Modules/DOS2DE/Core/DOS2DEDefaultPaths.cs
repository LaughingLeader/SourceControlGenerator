using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alphaleonis.Win32.Filesystem;
using SCG.Interfaces;
using SCG.Modules.DOS2DE.Data.View;

namespace SCG.Modules.DOS2DE.Core
{
	public static class DOS2DEDefaultPaths
	{
		public static string DirectoryLayout(IModuleData Data)
		{
			return DefaultPaths.ModuleSettingsFolder(Data) + @"\DirectoryLayout.txt";
		}

		public static readonly string DataDirectory_DefEd = @"\DefEd\Data";

		public static string LocalizationEditorFolder(IModuleData Data)
		{
			return Path.Combine(LocalizationEditorFolder(Data), @"\LocalizationEditor\");
		}

		public static string LocalizationEditorLinkFolder(IModuleData Data)
		{
			return Path.Combine(LocalizationEditorFolder(Data), @"\Links\");
		}

		public static string LocalizationEditorSettings(IModuleData Data)
		{
			return Path.Combine(LocalizationEditorFolder(Data), @"LocalizationEditorSettings.txt");
		}

		public static string CustomLocaleDirectory(DOS2DEModuleData Data, ModProjectData modProject)
		{
			return Path.Combine(Data.Settings.GitRootDirectory, modProject.ProjectName, @"LocaleEditor\");
		}
	}
}
