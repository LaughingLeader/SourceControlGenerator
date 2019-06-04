using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SCG.Interfaces;

namespace SCG.Modules.DOS2DE.Core
{
	public static class DOS2DEDefaultPaths
	{
		public static string DirectoryLayout(IModuleData Data)
		{
			return DefaultPaths.ModuleSettingsFolder(Data) + @"\DirectoryLayout.txt";
		}

		public static readonly string DataDirectory_DefEd = @"\DefEd\Data";

		public static string LocalizationEditorSettings(IModuleData Data)
		{
			return DefaultPaths.ModuleSettingsFolder(Data) + @"\LocalizationEditorSettings.txt";
		}
	}
}
