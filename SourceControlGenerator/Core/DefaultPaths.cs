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
		public static string DefaultPortableRootFolder => @"Data/";
		public static string DefaultMyDocumentsRootFolder => @"SourceControlGenerator/";

		public static string PortableSettingsFile => @"portable";

		public static string RootFolder { get; set; } = DefaultPortableRootFolder;
		public static string MainAppSettings => RootFolder + @"Settings/MainSettings.json";

		public static string SourceControlGeneratorDataFile => @"SourceControlGenerator.json";

		public static string ModuleSettings(IModuleData Data)
		{
			return RootFolder + @"Settings/" + Data.ModuleFolderName + @"/ModuleSettings.json";
		}

		public static string ProjectsAppData(IModuleData Data)
		{
			return RootFolder + @"Settings/" + Data.ModuleFolderName + @"/AddedProjects.json";
		}

		public static string TemplateSettings(IModuleData Data)
		{
			return RootFolder + @"Settings/" + Data.ModuleFolderName + @"/Templates.xml";
		}

		public static string TemplateFiles(IModuleData Data)
		{
			return RootFolder + @"Templates/" + Data.ModuleFolderName + @"/";
		}

		public static string Keywords(IModuleData Data)
		{
			return RootFolder + @"Settings/" + Data.ModuleFolderName + @"/Keywords.json";
		}

		public static string GitGenSettings(IModuleData Data)
		{
			return RootFolder + @"Settings/" + Data.ModuleFolderName + @"/GitGeneration.json";
		}

		//Folders

		public static string Backups(IModuleData Data)
		{
			return RootFolder + @"Backups/" + Data.ModuleFolderName;
		}

		public static string GitRoot(IModuleData Data)
		{
			return RootFolder + @"Projects/" + Data.ModuleFolderName;
		}
	}
}
