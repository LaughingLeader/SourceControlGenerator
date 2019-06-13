using System;
using System.Collections.Generic;
using System.Diagnostics;
using Alphaleonis.Win32.Filesystem;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SCG.Data;
using SCG.Data.App;
using SCG.Data.View;
using SCG.Interfaces;
using SCG.Util;
using Microsoft.Win32;

namespace SCG.FileGen
{
	public static class GitGenerator
	{
		public static async Task<string> GenerateTemplateFile(string defaultText, string filePath, IProjectData projectData, MainAppData mainAppData, IModuleData moduleData)
		{
			if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
			{
				defaultText = File.ReadAllText(filePath);
			}

			defaultText = await ReplaceKeywords(defaultText, projectData, mainAppData, moduleData).ConfigureAwait(false);

			return defaultText;
		}

		public static async Task<bool> CreateJunctions(string ProjectName, List<JunctionData> SourceFolders, IModuleData moduleData)
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

		public static async Task<bool> InitRepository(string RepoPath, string AuthorName = "", string Email = "")
		{
			try
			{
				if(!Directory.Exists(RepoPath))
				{
					Directory.CreateDirectory(RepoPath);
				}

				var commands = new List<string>()
				{
					"git init",
					"git config core.longpaths true",
					"git config core.autocrlf true",
					"git config core.safecrlf false"
				};

				if(!String.IsNullOrWhiteSpace(AuthorName)) commands.Add("git config user.name \"" + AuthorName + "\"");
				if(!String.IsNullOrWhiteSpace(Email)) commands.Add("git config user.email \"" + Email + "\"");

				var exitCode = await ProcessHelper.RunCommandLineAsync(RepoPath, commands.ToArray()).ConfigureAwait(false);
				return exitCode == 0;
			}
			catch(Exception ex)
			{
				Log.Here().Error("Error creating git repository: {0}", ex.ToString());
			}
			return false;
		}

		public static async Task<bool> Commit(string RepoPath, string CommitMessage)
		{
			try
			{
				//await ProcessHelper.RunCommandLineAsync(RepoPath, "git add -A");
				var exitCode = await ProcessHelper.RunCommandLineAsync(RepoPath, "git add -A", "git commit -m \"" + CommitMessage + "\"").ConfigureAwait(false);
				return exitCode == 0;
			}
			catch (Exception ex)
			{
				Log.Here().Error("Error creating commit for git repository: {0}", ex.ToString());
			}
			return false;
		}

		public static async Task<bool> Archive(string RepoPath, string OutputFileName, bool UseAttributesFile = true)
		{
			try
			{
				//Directory.CreateDirectory(Path.GetDirectoryName(OutputFileName));

				string command = "git archive --format=zip HEAD --output=\"" + OutputFileName + "\"";

				//string command = "git archive master > --output=\"" + OutputFileName + "\"";
				//command += Environment.NewLine + "tar -rf " + OutputFileName + " .git";
				if (UseAttributesFile)
				{
					//command += " --worktree-attributes";
				}

				var exitCode = await ProcessHelper.RunCommandLineAsync(RepoPath, command).ConfigureAwait(false);
				return exitCode == 0;
			}
			catch (Exception ex)
			{
				Log.Here().Error("Error creating git archive: {0}", ex.ToString());
			}
			return false;
		}

		private static bool KeywordIsValid(KeywordData k)
		{
			return k != null && !String.IsNullOrEmpty(k.KeywordName) && !String.IsNullOrEmpty(k.KeywordValue);
		}

		public static async Task<string> ReplaceKeywords(string sourceText, IProjectData projectData, MainAppData mainAppData, IModuleData moduleData)
		{
			string replacedText = sourceText;

			if (mainAppData != null)
			{
				if(projectData == null)
				{
					Log.Here().Error($"Error replacing keywords for module {moduleData.ModuleName}: projectData is null.");

					return replacedText;
				}

				if (mainAppData.DateKeyList != null)
				{
					//Reverse so $Date is replace after all the other variations
					var keywords = mainAppData.DateKeyList.Where(keywordData => KeywordIsValid(keywordData) && replacedText.Contains(keywordData.KeywordName)).Reverse();
					foreach (var keywordData in keywords)
					{
						replacedText = keywordData.ReplaceText(replacedText, projectData);
					}
					/*
					var tasks = mainAppData.DateKeyList.Where(keywordData => KeywordIsValid(keywordData) && replacedText.Contains(keywordData.KeywordName)).Reverse().Select(keywordData => Task.Run(() =>
					{
						replacedText = keywordData.ReplaceText(replacedText, projectData);
					}));

					await Task.WhenAll(tasks);
					*/
				}
				else
				{
					Log.Here().Error($"Error parsing DateKeyList for module {moduleData.ModuleName}: DateKeyList is null.");
				}

				if (mainAppData.AppKeyList != null)
				{
					var keywords = mainAppData.AppKeyList.Where(keywordData => KeywordIsValid(keywordData) && replacedText.Contains(keywordData.KeywordName));
					foreach (var keywordData in keywords)
					{
						replacedText = keywordData.ReplaceText(replacedText, projectData);
					}
					/*
					var tasks = mainAppData.AppKeyList.Where(keywordData => KeywordIsValid(keywordData) && replacedText.Contains(keywordData.KeywordName)).Select(keywordData => Task.Run(() =>
					{
						Log.Here().Activity($"Replacing {keywordData.KeywordName} with {keywordData.KeywordValue}");
						replacedText = keywordData.ReplaceText(replacedText, projectData);
					}));

					await Task.WhenAll(tasks);
					*/
				}
				else
				{
					Log.Here().Error($"Error parsing DateKeyList for module {moduleData.ModuleName}: DateKeyList is null.");
				}

				if (moduleData.UserKeywords != null && moduleData.UserKeywords.Keywords != null)
				{
					var keywords = moduleData.UserKeywords.Keywords.Where(keywordData => KeywordIsValid(keywordData) && replacedText.Contains(keywordData.KeywordName));
					foreach (var keywordData in keywords)
					{
						replacedText = keywordData.ReplaceText(replacedText, projectData);
					}
					/*
					var tasks = moduleData.UserKeywords.Keywords.Where(keywordData => KeywordIsValid(keywordData) && replacedText.Contains(keywordData.KeywordName)).Select(keywordData => Task.Run(() =>
					{
						replacedText = keywordData.ReplaceText(replacedText, projectData);
					}));

					await Task.WhenAll(tasks);
					*/
				}
				else
				{
					Log.Here().Error($"Error parsing user keywords for module {moduleData.ModuleName}: UserKeywords is null.");
				}

				if (moduleData.UserKeywords != null && !String.IsNullOrEmpty(moduleData.UserKeywords.DateCustom) && replacedText.Contains("$DateCustom"))
				{
					replacedText = replacedText.Replace("$DateCustom", DateTime.Now.ToString(moduleData.UserKeywords.DateCustom));
				}
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
