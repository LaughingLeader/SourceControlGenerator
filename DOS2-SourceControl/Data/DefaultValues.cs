using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LL.DOS2.SourceControl.Data
{
	public static class DefaultValues
	{
		public static string GitIgnore => Properties.Resources.DefaultGitIgnore;
		public static string Readme => Properties.Resources.DefaultReadme;
	}

	public static class DefaultPaths
	{
		public static string Backups
		{
			get => @"Projects_Backups";
		}

		public static string GitRoot
		{
			get => @"Projects_Git";
		}

		public static string GitIgnore
		{
			get => @"Settings/.gitignore.default";
		}

		public static string ReadmeTemplate
		{
			get => @"Settings/README.md.default";
		}

		public static string ChangelogTemplate
		{
			get => @"Settings/CHANGELOG.md.default";
		}

		public static string CustomLicense
		{
			get => @"Settings/LICENSE.default";
		}
	}
}
