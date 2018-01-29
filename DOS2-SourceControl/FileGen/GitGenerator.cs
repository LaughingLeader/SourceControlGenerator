using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LL.DOS2.SourceControl.Data;
using LL.DOS2.SourceControl.Data.View;
using Microsoft.Win32;

namespace LL.DOS2.SourceControl.FileGen
{
	public static class GitGenerator
	{
		public static string GenerateReadmeText(AppSettingsData AppSettings, string ProjectName)
		{
			string defaultText = Properties.Resources.DefaultReadme;
			if (!string.IsNullOrEmpty(AppSettings.ReadmeTemplateFile) && File.Exists(AppSettings.ReadmeTemplateFile))
			{
				defaultText = File.ReadAllText(AppSettings.ReadmeTemplateFile);
			}

			if (defaultText.Contains("$ProjectName")) defaultText.Replace("$ProjectName", ProjectName);

			return defaultText;
		}

		public static string GenerateChangelogText(AppSettingsData AppSettings, string ProjectName)
		{
			string defaultText = Properties.Resources.DefaultChangelog;
			if (!string.IsNullOrEmpty(AppSettings.ChangelogTemplateFile) && File.Exists(AppSettings.ChangelogTemplateFile))
			{
				defaultText = File.ReadAllText(AppSettings.ChangelogTemplateFile);
			}

			if (defaultText.Contains("$ProjectName")) defaultText.Replace("$ProjectName", ProjectName);

			return defaultText;
		}

		public static string GenerateLicense(AppSettingsData AppSettings, string ProjectName, GitGenerationSettings generationSettings)
		{
			string defaultText = Properties.Resources.License_MIT;

			switch (generationSettings.LicenseType)
			{
				case LicenseType.Custom:
					if (!string.IsNullOrEmpty(AppSettings.CustomLicenseFile) && File.Exists(AppSettings.CustomLicenseFile))
					{
						defaultText = File.ReadAllText(AppSettings.CustomLicenseFile);
					}
					break;
				case LicenseType.Apache:
					defaultText = Properties.Resources.License_Apache;
					break;
				case LicenseType.GNU:
					defaultText = Properties.Resources.License_GNU;
					break;
			}

			if (defaultText.Contains("$Author")) defaultText.Replace("$Author", generationSettings.Author);
			if (defaultText.Contains("$Year")) defaultText.Replace("$Year", DateTime.Now.Year.ToString());

			return defaultText;
		}

		public static bool CreateRepository(string RepoPath)
		{
			try
			{
				if(!Directory.Exists(RepoPath))
				{
					Directory.CreateDirectory(RepoPath);
				}

				Process process = new Process();

				process.StartInfo.FileName = @"cmd.exe";
				process.StartInfo.UseShellExecute = false;
				process.StartInfo.RedirectStandardInput = true;
				process.Start();

				StreamWriter stream = process.StandardInput;

				stream.WriteLine("cd \"" + RepoPath + "\"");
				stream.WriteLine("git init");
				stream.Close();

				process.WaitForExit();
			}
			catch(Exception ex)
			{
				Log.Here().Error("Error creating git repository: {0}", ex.ToString());
			}
			return false;
		}

		public static string GetGitInstallPath()
		{
			string installPath = Helpers.Registry.GetRegistryKeyValue(RegistryView.Registry64, "InstallPath", @"SOFTWARE\GitForWindows");
			if (String.IsNullOrEmpty(installPath))
			{
				installPath = Helpers.Registry.GetRegistryKeyValue(RegistryView.Registry32, "InstallPath", @"SOFTWARE\GitForWindows");
			}

			if (!String.IsNullOrEmpty(installPath)) return installPath;

			return "";
		}
	}
}
