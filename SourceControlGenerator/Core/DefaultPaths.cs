using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LL.SCG.Data.View;
using LL.SCG.Interfaces;

namespace LL.SCG
{
	public static class DefaultPaths
	{
		public static string DefaultPortableRootFolder => @"Data\";
		public static string DefaultMyDocumentsRootFolder => @"SourceControlGenerator\";

		public static string PortableSettingsFile => @"portable";

		public static string RootFolder { get; set; } = DefaultPortableRootFolder;

		public static string MainSettingsFolder => RootFolder + @"Settings\";

		public static string MainAppSettingsFile => MainSettingsFolder + @"MainSettings.json";

		public static string SourceControlGeneratorDataFile => @"SourceControlGenerator.json";

		//Module
		public static string ModuleRootFolder(IModuleData Data)
		{
			return Path.Combine(RootFolder, Data.ModuleFolderName);
		}

		public static string ModuleBackupsFolder(IModuleData Data)
		{
			return RootFolder + Data.ModuleFolderName + @"\Backups\";
		}

		public static string ModuleProjectsFolder(IModuleData Data)
		{
			return RootFolder + Data.ModuleFolderName + @"\Projects\";
		}

		public static string ModuleSettingsFolder(IModuleData Data)
		{
			return RootFolder + Data.ModuleFolderName + @"\Settings\";
		}

		public static string ModuleExportFolder(IModuleData Data)
		{
			return RootFolder + Data.ModuleFolderName + @"\Export\";
		}

		public static string ModuleTemplatesFolder(IModuleData Data)
		{
			return RootFolder + Data.ModuleFolderName + @"\Templates\";
		}

		public static string ModuleSettingsFile(IModuleData Data)
		{
			return ModuleSettingsFolder(Data) + @"ModuleSettings.json";
		}

		public static string ModuleAddedProjectsFile(IModuleData Data)
		{
			return ModuleSettingsFolder(Data) + @"AddedProjects.json";
		}

		public static string ModuleTemplateSettingsFile(IModuleData Data)
		{
			return ModuleSettingsFolder(Data) + @"Templates.xml";
		}

		public static string ModuleKeywordsFile(IModuleData Data)
		{
			return ModuleSettingsFolder(Data) + @"Keywords.json";
		}

		public static string ModuleGitGenSettingsFile(IModuleData Data)
		{
			return ModuleSettingsFolder(Data) + @"GitGeneration.json";
		}

		public static string ModuleMarkdownConverterSettingsFile(IModuleData Data)
		{
			return ModuleSettingsFolder(Data) + @"MarkdownConverterSettings.json";
		}
	}
}
