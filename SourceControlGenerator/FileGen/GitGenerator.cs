using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LL.SCG.Data;
using LL.SCG.Data.App;
using LL.SCG.Data.View;
using LL.SCG.Interfaces;
using LL.SCG.Util;
using Microsoft.Win32;

namespace LL.SCG.FileGen
{
	public static class GitGenerator
	{
		public static string GenerateTemplateFile(string defaultText, string filePath, IProjectData projectData, MainAppData mainAppData, IModuleData moduleData)
		{
			if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
			{
				defaultText = File.ReadAllText(filePath);
			}

			defaultText = ReplaceKeywords(defaultText, projectData, mainAppData, moduleData);

			return defaultText;
		}

		public static bool CreateJunctions(string ProjectName, List<JunctionData> SourceFolders, IModuleData moduleData)
		{
			if(SourceFolders != null && SourceFolders.Count > 0)
			{
				string repositoryProjectDirectory = Path.Combine(moduleData.ModuleSettings.GitRootDirectory, ProjectName);

				Log.Here().Activity("Creating junctions for {0} at {1}.", ProjectName, repositoryProjectDirectory);

				int junctionsCreated = 0;
				int maxJunctions = SourceFolders.Count;

				foreach(var junctionData in SourceFolders)
				{
					var junctionTargetDirectory = Path.Combine(repositoryProjectDirectory, junctionData.BasePath);

					Log.Here().Activity("Looking for directory at {0}", junctionData.SourcePath);

					//Create the directory for future file adding (this is better than adding it later).
					Directory.CreateDirectory(junctionData.SourcePath);

					if(Directory.Exists(junctionData.SourcePath))
					{
						Log.Here().Important("Directory \"{0}\" found. Creating junction.", junctionData.BasePath);

						if(!JunctionHelper.Exists(junctionTargetDirectory))
						{
							try
							{
								JunctionHelper.Create(junctionData.SourcePath, junctionTargetDirectory, true);
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

		public static bool InitRepository(string RepoPath, string AuthorName = "", string Email = "")
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
				stream.WriteLine("git config core.longpaths true");
				stream.WriteLine("git config core.autocrlf true");
				stream.WriteLine("git config core.safecrlf false"); // Disable the warnings

				if(!String.IsNullOrWhiteSpace(AuthorName)) stream.WriteLine("git config user.name \"" + AuthorName + "\"");
				if(!String.IsNullOrWhiteSpace(Email)) stream.WriteLine("git config user.email \"" + Email + "\"");

				stream.Close();

				process.WaitForExit(1000 * 60 * 5);
				if (process.ExitCode == 0) return true;
			}
			catch(Exception ex)
			{
				Log.Here().Error("Error creating git repository: {0}", ex.ToString());
			}
			return false;
		}

		public static bool Commit(string RepoPath, string CommitMessage)
		{
			try
			{
				Process process = new Process();

				process.StartInfo.FileName = @"cmd.exe";
				process.StartInfo.UseShellExecute = false;
				process.StartInfo.RedirectStandardInput = true;
				process.StartInfo.CreateNoWindow = true;
				process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
				process.Start();

				StreamWriter stream = process.StandardInput;

				stream.WriteLine("cd \"" + RepoPath + "\"");
				stream.WriteLine("git add -A");
				stream.WriteLine("git commit -m \"" + CommitMessage + "\"");
				stream.Close();

				process.WaitForExit(1000 * 60 * 5);
				if (process.ExitCode == 0) return true;
			}
			catch (Exception ex)
			{
				Log.Here().Error("Error creating commit for git repository: {0}", ex.ToString());
			}
			return false;
		}

		public static bool Archive(string RepoPath, string OutputFileName, bool UseAttributesFile = true)
		{
			try
			{
				//Directory.CreateDirectory(Path.GetDirectoryName(OutputFileName));

				string command = "git archive master > --output=\"" + OutputFileName + "\"";
				command += Environment.NewLine + "tar -rf " + OutputFileName + " .git";
				if(UseAttributesFile)
				{
					//command += " --worktree-attributes";
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

				process.WaitForExit(1000 * 60 * 5);
				if (process.ExitCode == 0) return true;
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

		public static string ReplaceKeywords(string sourceText, IProjectData projectData, MainAppData mainAppData, IModuleData moduleData)
		{
			string replacedText = sourceText;

			foreach(var keywordData in mainAppData.DateKeyList.Where(k => KeywordIsValid(k)))
			{
				replacedText = replacedText.Replace(keywordData.KeywordName, keywordData.Replace?.Invoke(projectData));
			}

			foreach (var keywordData in moduleData.KeyList.Where(k => KeywordIsValid(k)))
			{
				replacedText = replacedText.Replace(keywordData.KeywordName, keywordData.Replace?.Invoke(projectData));
			}

			foreach (var keywordData in moduleData.UserKeywords.Keywords.Where(k => KeywordIsValid(k)))
			{
				replacedText = replacedText.Replace(keywordData.KeywordName, keywordData.Replace?.Invoke(projectData));
			}

			if(!String.IsNullOrEmpty(moduleData.UserKeywords.DateCustom))
			{
				replacedText = replacedText.Replace("$DateCustom", DateTime.Now.ToString(moduleData.UserKeywords.DateCustom));
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
