using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LL.SCG.Data.View;
using LL.SCG.Interfaces;

namespace LL.SCG
{
	public static class DefaultPaths
	{
		public static string MainAppSettings => @"Settings/MainSettings.json";

		public static string SourceControlGeneratorDataFile => @"SourceControlGenerator.json";

		public static string AppSettings(IModuleData Data)
		{
			return @"Settings/" + Data.ModuleFolderName + @"/AppSettings.json";
		}

		public static string ProjectsAppData(IModuleData Data)
		{
			return @"Settings/" + Data.ModuleFolderName + @"/AddedProjects.json";
		}

		public static string TemplateSettings(IModuleData Data)
		{
			return @"Settings/" + Data.ModuleFolderName + @"/Templates.xml";
		}

		public static string TemplateFiles(IModuleData Data)
		{
			return @"Templates/" + Data.ModuleFolderName + @"/";
		}

		public static string Keywords(IModuleData Data)
		{
			return @"Settings/" + Data.ModuleFolderName + @"/Keywords.json";
		}

		public static string GitGenSettings(IModuleData Data)
		{
			return @"Settings/" + Data.ModuleFolderName + @"/GitGeneration.json";
		}

		//Folders

		public static string Backups(IModuleData Data)
		{
			return @"Backups/" + Data.ModuleFolderName;
		}

		public static string GitRoot(IModuleData Data)
		{
			return @"Projects/" + Data.ModuleFolderName;
		}
	}
}
