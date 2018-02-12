using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LL.DOS2.SourceControl.Data;
using LL.DOS2.SourceControl.Data.View;
using LL.DOS2.SourceControl.Util;
using Microsoft.Win32;

namespace LL.DOS2.SourceControl.FileGen
{
	public static class GitGenerator
	{
		public static string GenerateTemplateFile(string defaultText, string filePath, ModProjectData modProjectData, MainAppData mainAppData)
		{
			if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
			{
				defaultText = File.ReadAllText(filePath);
			}

			defaultText = ReplaceKeywords(defaultText, modProjectData, mainAppData);

			return defaultText;
		}

		public static bool CreateJunctions(ModProjectData modProjectData, MainAppData mainAppData)
		{
			if(mainAppData.ProjectDirectoryLayouts != null && mainAppData.ProjectDirectoryLayouts.Count > 0)
			{
				string repositoryProjectDirectory = Path.Combine(mainAppData.AppSettings.GitRootDirectory, modProjectData.Name);

				Log.Here().Activity("Creating junctions for {0} at {1}. Building directory layouts.", modProjectData.Name, repositoryProjectDirectory);

				int junctionsCreated = 0;
				int maxJunctions = mainAppData.ProjectDirectoryLayouts.Count;

				foreach(var directoryBaseName in mainAppData.ProjectDirectoryLayouts)
				{
					var projectSubdirectoryName = directoryBaseName.Replace("ProjectName", modProjectData.Name).Replace("ProjectGUID", modProjectData.ModuleInfo.UUID);
					var junctionSourceDirectory = Path.Combine(mainAppData.AppSettings.DOS2DataDirectory, projectSubdirectoryName);
					var junctionTargetDirectory = Path.Combine(repositoryProjectDirectory, projectSubdirectoryName);

					Log.Here().Activity("Looking for mod directory at {0}", junctionSourceDirectory);
					if(Directory.Exists(junctionSourceDirectory))
					{
						Log.Here().Important("Directory \"{0}\" found. Creating junction.", projectSubdirectoryName);

						if(!JunctionHelper.Exists(junctionTargetDirectory))
						{
							try
							{
								JunctionHelper.Create(junctionSourceDirectory, junctionTargetDirectory, true);
								junctionsCreated++;
								Log.Here().Important("Junction successfully created at {0}", junctionTargetDirectory);
							}
							catch(Exception ex)
							{
								Log.Here().Error("Error creating junction at {0}: {1}", junctionTargetDirectory, ex.ToString());
							}
						}
						else
						{
							Log.Here().Important("Junction already exists at \"{0}\". Skipping.", junctionTargetDirectory);
							if (maxJunctions > 0) maxJunctions--;
						}
					}
				}

				if(junctionsCreated >= maxJunctions)
				{
					return true;
				}
			}
			return false;
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
				process.StartInfo.CreateNoWindow = true;
				process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
				process.Start();

				StreamWriter stream = process.StandardInput;

				stream.WriteLine("cd \"" + RepoPath + "\"");
				stream.WriteLine("git init");
				stream.Close();

				process.WaitForExit();
				return true;
			}
			catch(Exception ex)
			{
				Log.Here().Error("Error creating git repository: {0}", ex.ToString());
			}
			return false;
		}

		public static bool Archive(string RepoPath, string OutputFileName, bool IgnoreGitFiles = true)
		{
			try
			{
				string command = "git archive -o \"" + OutputFileName + "\"";
				if(IgnoreGitFiles)
				{
					command += " --worktree-attributes";
				}

				Process process = new Process();

				process.StartInfo.FileName = @"cmd.exe";
				process.StartInfo.UseShellExecute = false;
				process.StartInfo.RedirectStandardInput = true;
				process.StartInfo.CreateNoWindow = true;
				process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
				process.Start();

				StreamWriter stream = process.StandardInput;

				stream.WriteLine("cd \"" + RepoPath + "\"");
				stream.WriteLine(command);
				stream.Close();

				process.WaitForExit();
				return true;
			}
			catch (Exception ex)
			{
				Log.Here().Error("Error creating git archive: {0}", ex.ToString());
			}
			return false;
		}

		private static bool KeywordIsValid(KeywordData k)
		{
			return k.Replace != null && !String.IsNullOrEmpty(k.KeywordName) && !String.IsNullOrEmpty(k.KeywordValue);
		}

		public static string ReplaceKeywords(string sourceText, ModProjectData modProjectData, MainAppData mainAppData)
		{
			string replacedText = sourceText;

			foreach(var keywordData in mainAppData.AppKeyList.Where(k => KeywordIsValid(k)))
			{
				replacedText = replacedText.Replace(keywordData.KeywordName, keywordData.Replace?.Invoke(modProjectData));
			}

			foreach(var keywordData in mainAppData.DateKeyList.Where(k => KeywordIsValid(k)))
			{
				replacedText = replacedText.Replace(keywordData.KeywordName, keywordData.Replace?.Invoke(modProjectData));
			}

			foreach (var keywordData in mainAppData.UserKeywords.Keywords.Where(k => KeywordIsValid(k)))
			{
				replacedText = replacedText.Replace(keywordData.KeywordName, keywordData.Replace?.Invoke(modProjectData));
			}

			if(!String.IsNullOrEmpty(mainAppData.UserKeywords.DateCustom))
			{
				replacedText = replacedText.Replace("$DateCustom", DateTime.Now.ToString(mainAppData.UserKeywords.DateCustom));
			}

			return replacedText;
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
