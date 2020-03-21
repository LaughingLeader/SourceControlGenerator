using SCG.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCG.Modules.DOS2DE.Core
{
	public static class DOS2DEFileFilters
	{
		public static FileBrowserFilter LarianPakFile { get; private set; } = new FileBrowserFilter()
		{
			Name = "Divinity Mod Pak",
			Values = "*.pak"
		};

		public static FileBrowserFilter LarianBinaryFile { get; private set; } = new FileBrowserFilter()
		{
			Name = "Larian StringKey file",
			Values = "*.lsb"
		};

		public static FileBrowserFilter LarianJsonFile { get; private set; } = new FileBrowserFilter()
		{
			Name = "Larian Dialog file",
			Values = "*.lsj"
		};

		public static FileBrowserFilter LarianXMLFile { get; private set; } = new FileBrowserFilter()
		{
			Name = "Larian XML file",
			Values = "*.lsx"
		};

		public static FileBrowserFilter LarianLocalizationFiles { get; private set; } = new FileBrowserFilter()
		{
			Name = "Larian Localization files",
			Values = CommonFileFilters.CombineFilters(LarianBinaryFile, LarianXMLFile, LarianJsonFile)
		};

		public static FileBrowserFilter AllLocaleImportFiles { get; private set; } = new FileBrowserFilter()
		{
			Name = "Localization files",
			Values = CommonFileFilters.CombineFilters(CommonFileFilters.DelimitedLocaleFiles, LarianBinaryFile, LarianJsonFile)
		};

		public static List<FileBrowserFilter> AllLocaleFilesList { get; set; } = new List<FileBrowserFilter>()
		{
			AllLocaleImportFiles,
			LarianLocalizationFiles,
			CommonFileFilters.DelimitedLocaleFiles,
			CommonFileFilters.TabSeparatedFile,
			CommonFileFilters.CommaSeparatedFile,
			CommonFileFilters.All
		};
	}
}
