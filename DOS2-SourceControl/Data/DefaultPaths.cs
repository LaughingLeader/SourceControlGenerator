﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LL.DOS2.SourceControl.Data
{
	public static class DefaultValues
	{
		//public static string GitIgnore => Properties.Resources.DefaultGitIgnore;
		//public static string Readme => Properties.Resources.DefaultReadme;
		public static string DOS2_SteamAppID => "435150";
		public static string SourceControlDataFileName => @"DOS2SourceControl.json";

		public static string TemplateID_Ignore = "1";
		public static string TemplateID_Readme = "2";
		public static string TemplateID_Changelog = "3";
		public static string TemplateID_License = "4";
		public static string TemplateID_Attributes = "5";
	}

	public static class DefaultPaths
	{
		public static string AppSettings => @"Settings/AppSettings.json";

		public static string ProjectsAppData => @"Settings/ManagedProjects.json";

		public static string DirectoryLayout => @"Settings/DirectoryLayout.default.txt";

		public static string TemplateSettings => @"Settings/Templates.xml";

		public static string TemplateFiles => @"Templates/";

		/*
		public static string GitIgnore => @"Settings/.gitignore.default";

		public static string GitAttributes => @"Settings/attributes.default";

		public static string ReadmeTemplate => @"Settings/README.md.default";

		public static string ChangelogTemplate => @"Settings/CHANGELOG.md.default";
		*/

		//public static string CustomLicense  => @"Settings/LICENSE.default";

		public static string Keywords => @"Settings/Keywords.json";

		public static string GitGenSettings => @"Settings/GitGeneration.json";

		//Folders

		public static string Backups => @"Backups";

		public static string GitRoot => @"Projects";
	}
}