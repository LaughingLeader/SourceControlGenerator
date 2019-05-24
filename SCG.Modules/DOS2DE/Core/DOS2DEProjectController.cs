using SCG.Commands;
using SCG.Data;
using SCG.Data.App;
using SCG.Data.View;
using SCG.FileGen;
using SCG.Interfaces;
using SCG.Modules.DOS2DE.Controls;
using SCG.Modules.DOS2DE.Core;
using SCG.Modules.DOS2DE.Data.View;
using SCG.Modules.DOS2DE.Utilities;
using SCG.Modules.DOS2DE.Windows;
using SCG.Util;
using SCG.Windows;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using Alphaleonis.Win32.Filesystem;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace SCG.Core
{
	public class DOS2DEProjectController : IProjectController
	{
		private ProjectViewControl projectViewControl;

		public MainAppData MainAppData { get; set; }
		public DOS2DEModuleData Data { get; set; }

		public IModuleData ModuleData => Data;

		private bool saveModuleSettings = false;

		private CancellationTokenSource cancellationTokenSource;

		#region Git Generation

		private List<JunctionData> PrepareDirectories(ModProjectData project, List<string> DirectoryLayouts)
		{
			var sourceFolders = new List<JunctionData>();
			foreach (var directoryBaseName in DirectoryLayouts)
			{
				var subdirectoryName = directoryBaseName.Replace("ProjectName", project.ProjectName).Replace("ProjectFolder", project.ProjectFolder);
				if (project.ModuleInfo != null) subdirectoryName = subdirectoryName.Replace("ModUUID", project.ModuleInfo.UUID).Replace("ModFolder", project.ModuleInfo.Folder);

				//Log.Here().Important($"Directory: \"{subdirectoryName}\" for project {project.ProjectName}");

				var junctionSourceDirectory = Path.Combine(Data.Settings.DOS2DEDataDirectory, subdirectoryName);
				if (!Directory.Exists(junctionSourceDirectory))
				{
					Log.Here().Warning($"Directory {subdirectoryName} doesn't exist. Creating directory.");
					Directory.CreateDirectory(junctionSourceDirectory);
				}
				sourceFolders.Add(new JunctionData()
				{
					SourcePath = junctionSourceDirectory,
					BasePath = subdirectoryName
				});
			}
			return sourceFolders;
		}

		public void OpenGitGeneratorWindow()
		{
			if (Data.CanGenerateGit)
			{
				var selectedProjects = Data.ManagedProjects.Where(p => p.Selected && p.GitGenerated == false).ToList<IProjectData>();
				projectViewControl.MainWindow.OpenGitGenerationWindow(Data.GitGenerationSettings, selectedProjects, StartGitGeneration);
			}
		}

		public void StartGitGeneration()
		{
			if (Data.CanGenerateGit)
			{
				Data.CanGenerateGit = false;
				AppController.Main.StartProgress($"Generating Git Files... 0/{Data.GitGenerationSettings.ExportProjects.Count}", StartGitGenerationAsync);
			}
		}

		public async void StartGitGenerationAsync()
		{
			var totalSuccess = await OnGitGenerationAsync();

			if (totalSuccess >= Data.GitGenerationSettings.ExportProjects.Count)
			{
				MainWindow.FooterLog($"Successfully created git repositories for selected projects to git project root: {Data.Settings.GitRootDirectory}");
			}
			else
			{
				MainWindow.FooterError($"Problem occured. Check the log. {totalSuccess}/{Data.GitGenerationSettings.ExportProjects.Count} git repositories were created.");
			}
		}

		private async Task<int> OnGitGenerationAsync()
		{
			Log.Here().Important("Generating git repositories for selected projects.");
			int total = Data.GitGenerationSettings.ExportProjects.Count;
			AppController.Main.Data.ProgressValueMax = total;

			//Log.Here().Activity($"[Progress] Amount per tick set to {amountPerTick}");

			AppController.Main.UpdateProgressMessage("Parsing selected projects...");

			var totalSuccess = 0;

			var sortedProjects = Data.GitGenerationSettings.ExportProjects.OrderBy(p => p.ProjectName).ToImmutableList();

			if(Data.GitGenerationSettings.ExportProjects.TryOperation(c => c.Clear()))
			{
				Data.GitGenerationSettings.ExportProjects.DoOperation(c => c.AddRange(sortedProjects));
			}

			for (var i = 0; i < total; i++)
			{
				AppController.Main.UpdateProgressTitle($"Generating Git Files... {(i)}/{total}");

				var project = Data.GitGenerationSettings.ExportProjects[i];

				AppController.Main.UpdateProgressMessage($"Generating git files for project {project.ProjectName}...");

				//Log.Here().Activity($"[Progress] Target percentage for this iteration is {targetPercentage}, work should increase it by {totalPercentageAmount}");

				ModProjectData modProjectData = (ModProjectData)project;

				var success = await GenerateGitFilesAsync(modProjectData, Data.GitGenerationSettings).ConfigureAwait(false);

				if (success)
				{
					totalSuccess += 1;
					AppController.Main.UpdateProgressLog($"Successfuly generated git repo for project {project.DisplayName}.");
					Log.Here().Important("Git repository successfully generated for {0}.", project.DisplayName);
				}
				else
				{
					AppController.Main.UpdateProgressLog($"Error generating git repo for project {project.DisplayName}.");
					Log.Here().Error("Error generating git repository for {0}.", project.DisplayName);
				}

				AppController.Main.UpdateProgress();

				AppController.Main.UpdateProgressTitle($"Generating Git Files... {(i + 1)}/{total}");
			}

			AppController.Main.UpdateProgressTitle($"Generating Git Files... {total}/{total}");
			AppController.Main.UpdateProgressMessage("Finishing up...");
			AppController.Main.UpdateProgressLog("Git generation complete. We did it!");
			AppController.Main.FinishProgress();
			return totalSuccess;
		}

		private async Task<bool> GenerateGitFilesAsync(ModProjectData modProject, GitGenerationSettings generationSettings)
		{
			if (modProject == null) return false;
			//Log.Here().Activity($"[Progress] percentageIncrement is {percentageIncrement} / {endPercentage}");

			if (!string.IsNullOrEmpty(Data.Settings.GitRootDirectory))
			{
				string gitProjectRootDirectory = Path.Combine(Data.Settings.GitRootDirectory, modProject.ProjectName);

				AppController.Main.UpdateProgressLog("Creating git project directory...");

				var rootRepoDirectory = Directory.CreateDirectory(gitProjectRootDirectory);

				if (!Directory.Exists(gitProjectRootDirectory))
				{
					Directory.CreateDirectory(gitProjectRootDirectory);
				}

				if (generationSettings.InitGit)
				{
					AppController.Main.UpdateProgressLog("Initializing git repo...");

					var author = Data.Settings.DefaultAuthor;
					if(modProject.ModuleInfo != null && !String.IsNullOrWhiteSpace(modProject.ModuleInfo.Author))
					{
						author = modProject.ModuleInfo.Author;
					}

					var result = await GitGenerator.InitRepository(gitProjectRootDirectory, author);
					if (result)
					{
						Log.Here().Activity("Created git repository for project ({0}) at {1}", modProject.ProjectName, gitProjectRootDirectory);
					}
					else
					{
						Log.Here().Error("Error creating git repository for project {0}.", modProject.ProjectName);
					}
				}

				bool commitGit = false;
				string commitMessage = "";

				if (generationSettings.CreateJunctions)
				{
					AppController.Main.UpdateProgressLog("Creating junctions...");

					var sourceFolders = PrepareDirectories(modProject, Data.Settings.DirectoryLayouts);
					var result = await GitGenerator.CreateJunctions(modProject.ProjectName, sourceFolders, Data);

					if (result)
					{
						Log.Here().Activity("[{0}] Successfully created junctions.", modProject.ProjectName);
						commitGit = true;
						commitMessage = "Junctioned project folders";
					}
					else
					{
						Log.Here().Error("[{0}] Problem creating junctions.", modProject.ProjectName);
					}
				}

				if (generationSettings.TemplateSettings != null && generationSettings.TemplateSettings.Count > 0)
				{
					AppController.Main.UpdateProgressLog("Generating templates...");

					foreach (var templateSetting in generationSettings.TemplateSettings)
					{
						var templateData = Data.Templates.Where(t => t.Name == templateSetting.TemplateName).FirstOrDefault();
						if (templateData != null)
						{
							if (templateSetting.Enabled)
							{
								string outputFilePath = Path.Combine(gitProjectRootDirectory, templateData.ExportPath);
								string outputText = await GitGenerator.ReplaceKeywords(templateData.EditorText, modProject, MainAppData, Data);
								if (!FileCommands.WriteToFile(outputFilePath, outputText))
								{
									Log.Here().Error("[{0}] Failed to create template file at {1}", modProject.ProjectName, templateData.ExportPath);
								}
								else
								{
									commitGit = true;
									commitMessage += (", added " + templateData.Name);
								}
							}
							else
							{
								Log.Here().Activity("[{0}] Skipping {1}", modProject.ProjectName, templateSetting.TemplateName);
							}
						}
					}
				}

				if (generationSettings.SelectedLicense != LicenseType.None)
				{
					AppController.Main.UpdateProgressLog("Generating license...");

					string outputText = "";
					if (generationSettings.SelectedLicense == LicenseType.Custom)
					{
						var customLicenseTemplate = Data.Templates.Where(t => t.Name == "LICENSE").FirstOrDefault();
						if (customLicenseTemplate != null)
						{
							outputText = customLicenseTemplate.EditorText;
						}
					}
					else
					{
						switch (generationSettings.SelectedLicense)
						{
							case LicenseType.MIT:
								outputText = Properties.Resources.License_MIT;
								break;

							case LicenseType.Apache:
								outputText = Properties.Resources.License_Apache;
								break;

							case LicenseType.GPL:
								outputText = Properties.Resources.License_GPL;
								break;
						}
					}

					if (!String.IsNullOrEmpty(outputText))
					{
						outputText = await GitGenerator.ReplaceKeywords(outputText, modProject, MainAppData, Data);
					}

					string licenseFile = Path.Combine(gitProjectRootDirectory, "LICENSE");

					if (!FileCommands.WriteToFile(licenseFile, outputText))
					{
						Log.Here().Error("[{0}] Failed to write license template file at {1}", modProject.ProjectName, licenseFile);
					}
					else
					{
						commitGit = true;
						commitMessage += ", added license";
					}
				}

				if (generationSettings.InitGit && commitGit)
				{
					AppController.Main.UpdateProgressLog("Committing new files...");
					var result = await GitGenerator.Commit(gitProjectRootDirectory, commitMessage);
					if (result)
					{
						AppController.Main.UpdateProgressLog($"Successfully commited git repo for project {modProject.DisplayName}.");
					}
					else
					{
						AppController.Main.UpdateProgressLog($"Git repo failed to commit for project {modProject.DisplayName}.");
					}
				}

				AppController.Main.UpdateProgressLog("Generating source control data file...");

				SourceControlData sourceControlData = new SourceControlData()
				{
					ProjectName = modProject.ProjectName,
					ProjectUUID = modProject.UUID
				};

				modProject.GitData = sourceControlData;

				FileCommands.Save.SaveSourceControlData(sourceControlData, gitProjectRootDirectory);

				return true;
			}
			//}
			//catch(Exception ex)
			//{
			//	Log.Here().Error($"Error generating git files: {ex.ToString()}");
			//}
			return false;
		}

		#endregion Git Generation

		#region Backup

		public bool GenerateBackupFolder(ModProjectData modProject = null)
		{
			string projectBackupDirectory = Data.Settings.BackupRootDirectory;
			if (modProject != null) projectBackupDirectory = Path.Combine(Data.Settings.BackupRootDirectory, modProject.ProjectName);

			try
			{
				Directory.CreateDirectory(projectBackupDirectory);
				return true;
			}
			catch (Exception ex)
			{
				MainWindow.FooterError("Error creating backup directory at {0}: {1}", projectBackupDirectory, ex.Message);
			}

			return false;
		}

		private bool openFolderDialogOpen = false;

		public void BackupSelectedProjectsTo()
		{
			if (!openFolderDialogOpen)
			{
				openFolderDialogOpen = true;
				if (String.IsNullOrWhiteSpace(Data.Settings.LastBackupPath))
				{
					Data.Settings.LastBackupPath = Data.Settings.BackupRootDirectory;
				}

				FileCommands.Load.OpenFolderDialog(App.Current.MainWindow, "Select Archive Export Location", Data.Settings.LastBackupPath, (path) =>
				{
					Data.Settings.LastBackupPath = path;
					BackupSelectedProjects(path);
					openFolderDialogOpen = false;
				}, false);
			}
		}

		private string targetBackupOutputDirectory = "";

		private void CancelBackupProgress()
		{
			if(cancellationTokenSource != null)
			{
				Log.Here().Warning("Cancelling backup progress...");
				cancellationTokenSource.Cancel();
				AppController.Main.CancelProgress();
			}
		}

		public void BackupSelectedProjects(string OutputDirectory = "")
		{
			cancellationTokenSource = new CancellationTokenSource();

			targetBackupOutputDirectory = OutputDirectory;
			AppController.Main.StartProgress($"Backing up projects...", StartBackupSelectedProjectsAsync, "", 0, true, CancelBackupProgress);
		}

		public async void StartBackupSelectedProjectsAsync()
		{
			var sortedProjects = Data.ManagedProjects.Where(p => p.Selected).OrderByDescending(p => p.ProjectName);

			ConcurrentBag<ModProjectData> selectedProjects = new ConcurrentBag<ModProjectData>(sortedProjects);

			var totalSuccess = await BackupSelectedProjectsAsync(selectedProjects);
			if (totalSuccess >= selectedProjects.Count)
			{
				if (String.IsNullOrWhiteSpace(targetBackupOutputDirectory))
				{
					MainWindow.FooterLog($"Successfully backed up all selected projects to {Data.Settings.BackupRootDirectory}");
				}
				else
				{
					MainWindow.FooterLog($"Successfully backed up all selected projects to {targetBackupOutputDirectory}");
				}
			}
			else
			{
				if (!cancellationTokenSource.IsCancellationRequested)
				{
					MainWindow.FooterError($"Problem occured when backing up selected projects. Check the log. {totalSuccess}/{selectedProjects.Count} archives were created.");
				}
				else
				{
					MainWindow.FooterLog($"Cancelled backup process. {totalSuccess}/{selectedProjects.Count} archives were created.");
				}
			}
		}

		private async Task<int> BackupSelectedProjectsAsync(ConcurrentBag<ModProjectData> selectedProjects)
		{
			//+1 progress when done searching for files, +1 when done.
			AppController.Main.Data.ProgressValueMax = selectedProjects.Count * 2;

			//AppController.Main.Data.IsIndeterminate = true;
			

			int totalSuccess = 0;

			if (selectedProjects != null)
			{
				int i = 0;
				foreach (var project in selectedProjects)
				{
					if (cancellationTokenSource.IsCancellationRequested) break;

					AppController.Main.UpdateProgressTitle((selectedProjects.Count > 1 ? "Backing up projects..." : $"Backing up project... ") + $"{i}/{selectedProjects.Count}");

					//Log.Here().Activity($"[Progress-Backup] Target percentage for this backup iteration is {targetPercentage} => {totalPercentageAmount}. Amount per tick is {amountPerTick}.");

					AppController.Main.UpdateProgressMessage($"Creating archive for project {project.ProjectName}...");

					var backupSuccess = await BackupProjectAsync(project, targetBackupOutputDirectory, Data.Settings.BackupMode);

					if (cancellationTokenSource.IsCancellationRequested)
					{
						backupSuccess = BackupResult.Skipped;
					}

					if (backupSuccess == BackupResult.Success)
					{
						totalSuccess += 1;
						Log.Here().Activity("Successfully created archive for {0}.", project.ProjectName);
						project.LastBackup = DateTime.Now;
						var d = Data.ManagedProjectsData.Projects.Where(p => p.Name == project.ProjectName && p.UUID == project.UUID).FirstOrDefault();
						if (d != null) d.LastBackupUTC = project.LastBackup?.ToUniversalTime().ToString();

						AppController.Main.UpdateProgressLog("Archive created.");
					}
					else if (backupSuccess == BackupResult.Error)
					{
						Log.Here().Error("Failed to create archive for {0}.", project.ProjectName);
						AppController.Main.UpdateProgressLog("Archive creation failed.");
					}
					else
					{
						totalSuccess += 1;
						Log.Here().Activity("Skipped archive creation for {0}.", project.ProjectName);
					}

					AppController.Main.UpdateProgress();

					AppController.Main.UpdateProgressTitle((selectedProjects.Count > 1 ? "Backing up projects..." : $"Backing up project... ") + $"{i + 1}/{selectedProjects.Count}");
					i++;
				}
			}

			if (!cancellationTokenSource.IsCancellationRequested)
			{
				AppController.Main.UpdateProgressTitle((selectedProjects.Count > 1 ? "Backing up projects..." : $"Backing up project... ") + $"{selectedProjects.Count}/{selectedProjects.Count}");
				AppController.Main.UpdateProgressMessage("Finishing up...");
				AppController.Main.UpdateProgressLog("Backup quest complete. +5 XP");
				AppController.Main.FinishProgress();
			}

			if (totalSuccess > 0) DOS2DECommands.SaveManagedProjects(this.Data);

			return totalSuccess;
		}

		public async Task<BackupResult> BackupProjectAsync(ModProjectData modProject, string OutputDirectory = "", BackupMode mode = BackupMode.Zip)
		{
			if (String.IsNullOrWhiteSpace(OutputDirectory))
			{
				OutputDirectory = Path.Combine(Path.GetFullPath(Data.Settings.BackupRootDirectory), modProject.ProjectName);
				Directory.CreateDirectory(OutputDirectory);
			}
			else
			{
				OutputDirectory = Path.GetFullPath(OutputDirectory);
			}

			string sysFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.Replace("/", "-");

			//Log.Here().Important($"System date format: {sysFormat}");

			string archiveName = modProject.ProjectName + "_" + DateTime.Now.ToString(sysFormat + "_HH-mm-ss") + ".zip";
			string archivePath = Path.Combine(OutputDirectory, archiveName);
			string gitProjectDirectory = "";

			/*
			bool gitProjectDetected = false;

			if (!String.IsNullOrEmpty(Data.Settings.GitRootDirectory))
			{
				gitProjectDirectory = Path.Combine(Data.Settings.GitRootDirectory, modProject.ProjectName);
				if (Directory.Exists(gitProjectDirectory))
				{
					gitProjectDirectory = Path.Combine(Data.Settings.GitRootDirectory, modProject.ProjectName);
					gitProjectDetected = true;
					AppController.Main.UpdateProgressLog("Git repository detected.");
				}
			}
			*/

			var sourceFolders = PrepareDirectories(modProject, Data.Settings.DirectoryLayouts);

			if (!modProject.GitGenerated)
			{
				AppController.Main.UpdateProgressLog("Creating zip archive from project folders...");
				//Log.Here().Activity($"Git project not found. Archiving project {modProject.ProjectName} from project folders directly.");
				return await BackupGenerator.CreateArchiveFromRoot(Data.Settings.DOS2DEDataDirectory.Replace("/", "\\\\"), sourceFolders, archivePath, true, cancellationTokenSource.Token).ConfigureAwait(false);
			}
			else
			{
				gitProjectDirectory = Path.Combine(Data.Settings.GitRootDirectory, modProject.ProjectName);

				if (mode == BackupMode.GitArchive)
				{
					AppController.Main.UpdateProgressLog("Running git archive command...");
					var success = await GitGenerator.Archive(gitProjectDirectory, archivePath).ConfigureAwait(false);
					return success ? BackupResult.Success : BackupResult.Error;
				}
				else
				{
					AppController.Main.UpdateProgressLog("Creating zip archive...");
					return await BackupGenerator.CreateArchiveFromRepo(gitProjectDirectory, Data.Settings.DOS2DEDataDirectory.Replace("/", "\\\\"), sourceFolders, archivePath, true, cancellationTokenSource.Token).ConfigureAwait(false);
				}
				//Seems to have a problem with junctions and long paths
				//return BackupGenerator.CreateArchiveFromRepo(gitProjectDirectory, archivePath);
			}
		}

		#endregion Backup

		#region PackageCreation

		private void CancelPackageProgress()
		{
			if (cancellationTokenSource != null)
			{
				Log.Here().Warning("Cancelling package creation...");
				cancellationTokenSource.Cancel();
				AppController.Main.CancelProgress();
			}
		}

		public void PackageSelectedProjects()
		{
			cancellationTokenSource = new CancellationTokenSource();
			AppController.Main.StartProgress($"Packaging projects...", StartPackageSelectedProjectsAsync, "", 0, true, CancelPackageProgress);
		}

		public async void StartPackageSelectedProjectsAsync()
		{
			//Order by descending order, since it gets reversed in the bag
			var sortedProjects = Data.ManagedProjects.Where(p => p.Selected).OrderByDescending(p => p.ProjectName);

			ConcurrentBag<ModProjectData> selectedProjects = new ConcurrentBag<ModProjectData>(sortedProjects);

			string localModsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Larian Studios\Divinity Original Sin 2 Definitive Edition\Local Mods");

			var totalSuccess = await PackageSelectedProjectsAsync(selectedProjects, localModsFolder);
			if (totalSuccess >= selectedProjects.Count)
			{
				MainWindow.FooterLog($"Successfully packaged all selected projects to {localModsFolder}");
			}
			else
			{
				if (!cancellationTokenSource.IsCancellationRequested)
				{
					MainWindow.FooterError($"Problem occured when packaging selected projects. Check the log. {totalSuccess}/{selectedProjects.Count} packages were created.");
				}
				else
				{
					MainWindow.FooterLog($"Packaging was cancelled. {totalSuccess}/{selectedProjects.Count} packages were created.");
				}
			}
		}

		private async Task<int> PackageSelectedProjectsAsync(ConcurrentBag<ModProjectData> selectedProjects, string targetFolder)
		{
			//+1 progress when done searching for files, +1 when done.
			AppController.Main.Data.ProgressValueMax = selectedProjects.Count * 2;

			//AppController.Main.Data.IsIndeterminate = true;

			List<string> exportDirectories = new List<string>();
			exportDirectories.Add(Path.Combine(Data.Settings.DOS2DEDataDirectory, @"Mods\ModFolder"));
			exportDirectories.Add(Path.Combine(Data.Settings.DOS2DEDataDirectory, @"Public\ModFolder"));

			int totalSuccess = 0;

			if (selectedProjects != null)
			{
				int i = 0;
				foreach (var project in selectedProjects)
				{
					if (cancellationTokenSource.IsCancellationRequested) return totalSuccess;

					AppController.Main.UpdateProgressTitle((selectedProjects.Count > 1 ? "Packaging projects..." : $"Packaging project... ") + $"{i}/{selectedProjects.Count}");

					//Log.Here().Activity($"[Progress-Backup] Target percentage for this backup iteration is {targetPercentage} => {totalPercentageAmount}. Amount per tick is {amountPerTick}.");

					AppController.Main.UpdateProgressMessage($"Creating package for project {project.ProjectName}...");

					var backupSuccess = await PackageProjectAsync(project, targetFolder, exportDirectories);

					if (cancellationTokenSource.IsCancellationRequested)
					{
						backupSuccess = BackupResult.Skipped;
					}

					if (backupSuccess == BackupResult.Success)
					{
						totalSuccess += 1;
						Log.Here().Activity("Successfully created package for {0}.", project.ProjectName);
						project.LastBackup = DateTime.Now;
						var d = Data.ManagedProjectsData.Projects.Where(p => p.Name == project.ProjectName && p.UUID == project.UUID).FirstOrDefault();
						if (d != null) d.LastBackupUTC = project.LastBackup?.ToUniversalTime().ToString();

						AppController.Main.UpdateProgressLog("Package created.");
					}
					else if (backupSuccess == BackupResult.Error)
					{
						Log.Here().Error("Failed to create package for {0}.", project.ProjectName);
						AppController.Main.UpdateProgressLog("Package creation failed.");
					}
					else
					{
						totalSuccess += 1;
						Log.Here().Activity("Skipped package creation for {0}.", project.ProjectName);
					}

					AppController.Main.UpdateProgress();

					AppController.Main.UpdateProgressTitle((selectedProjects.Count > 1 ? "Packaging projects..." : $"Packaging project... ") + $"{i + 1}/{selectedProjects.Count}");
					i++;
				}
			}

			if(!cancellationTokenSource.IsCancellationRequested)
			{
				AppController.Main.UpdateProgressTitle((selectedProjects.Count > 1 ? "Packaging projects..." : $"Packaging project... ") + $"{selectedProjects.Count}/{selectedProjects.Count}");
				AppController.Main.UpdateProgressMessage("Finishing up...");
				AppController.Main.UpdateProgressLog("Packaging complete. +5 XP");
				AppController.Main.FinishProgress();
			}

			//if (totalSuccess > 0) DOS2DECommands.SaveManagedProjects(this.Data);

			return totalSuccess;
		}

		public List<string> IgnoredExportFiles { get; set; }

		public async Task<BackupResult> PackageProjectAsync(ModProjectData modProject, string outputDirectory, List<string> exportDirectories)
		{
			if (!Directory.Exists(outputDirectory))
			{
				Directory.CreateDirectory(outputDirectory);
			}

			//string inputDirectory = Path.Combine(Path.GetFullPath(Data.Settings.GitRootDirectory), modProject.ProjectName);
			//string outputPackage = Path.ChangeExtension(Path.Combine(outputDirectory, modProject.ProjectName + "_" + modProject.ModuleInfo.UUID), "pak");
			string outputPackage = Path.ChangeExtension(Path.Combine(outputDirectory, modProject.FolderName), "pak");

			//Imported Classic Projects
			if (!modProject.FolderName.Contains(modProject.ModuleInfo.UUID))
			{
				outputPackage = Path.ChangeExtension(Path.Combine(outputDirectory, modProject.FolderName + "_" + modProject.ModuleInfo.UUID), "pak");
			}
			
			try
			{
				var sourceFolders = new List<string>();
				foreach (var directoryBaseName in exportDirectories)
				{
					if (cancellationTokenSource.IsCancellationRequested) return BackupResult.Skipped;

					var subdirectoryName = directoryBaseName.Replace("ProjectName", modProject.ProjectName).Replace("ProjectFolder", modProject.ProjectFolder);
					if (modProject.ModuleInfo != null) subdirectoryName = subdirectoryName.Replace("ModUUID", modProject.ModuleInfo.UUID).Replace("ModFolder", modProject.ModuleInfo.Folder);

					var sourceDirectory = Path.Combine(Data.Settings.DOS2DEDataDirectory, subdirectoryName).Replace("/", "\\");
					if (!sourceDirectory.EndsWith(Path.DirectorySeparatorChar.ToString())) sourceDirectory += Path.DirectorySeparatorChar.ToString();
					if (Directory.Exists(sourceDirectory))
					{
						Log.Here().Important($"Adding source folder {directoryBaseName} => {sourceDirectory}");
						sourceFolders.Add(sourceDirectory);
					}
				}

				var result = await DOS2DEPackageCreator.CreatePackage(Data.Settings.DOS2DEDataDirectory.Replace("/", "\\"), sourceFolders, outputPackage, IgnoredExportFiles, cancellationTokenSource.Token);
				if (result)
				{
					return BackupResult.Success;
				}
				else
				{
					return BackupResult.Error;
				}

				//await ProcessHelper.RunCommandLineAsync(RepoPath, "git add -A");
				/*
				string command = $"divine -a create-package -g dos2 -s \"{inputDirectory}\" -d \"{outputPackage}\"";
				AppController.Main.UpdateProgressLog($"Running command [{command}]...");
				Log.Here().Activity($"Running command [{command}]...");
				var exitCode = await ProcessHelper.RunCommandLineAsync(divineFolder, command);
				if (exitCode == 0)
				{
					return BackupResult.Success;
				}
				*/
			}
			catch (Exception ex)
			{
				if (!cancellationTokenSource.IsCancellationRequested)
				{
					Log.Here().Error("Error creating package: {0}", ex.ToString());
					return BackupResult.Error;
				}
				else
				{
					Log.Here().Important("Cancelling package creation: {0}", ex.ToString());
					return BackupResult.Skipped;
				}
			}
		}
		#endregion

		public void AddProjects(List<AvailableProjectViewData> selectedItems)
		{
			bool bSaveData = false;

			foreach (AvailableProjectViewData project in selectedItems)
			{
				Log.Here().Activity($"Adding project {project.Name} data to managed projects.");
				var modData = Data.ModProjects.Where(p => p.ProjectName == project.Name).FirstOrDefault();
				if (modData != null)
				{
					//ManagedProjects.Add(new ProjectEntryData(modData.ProjectInfo, modData.ModInfo));
					Data.ManagedProjects.Add(modData);
					var availableProject = Data.NewProjects.Where(p => p.Name == project.Name).FirstOrDefault();
					if (availableProject != null) Data.NewProjects.Remove(availableProject);

					if (Data.ManagedProjectsData.Projects.Any(p => p.Name == modData.ProjectName))
					{
						if (modData.ProjectAppData == null)
						{
							ProjectAppData data = Data.ManagedProjectsData.Projects.Where(p => p.Name == modData.ProjectName && p.UUID == modData.ModuleInfo.UUID).FirstOrDefault();
							if (data != null)
							{
								modData.ProjectAppData = data;

								Log.Here().Activity($"Linked project {modData.ProjectName} data to managed project data.");
							}
						}
					}
					else
					{
						ProjectAppData data = new ProjectAppData()
						{
							Name = modData.ProjectName,
							UUID = modData.ModuleInfo.UUID,
							LastBackupUTC = null
						};
						Data.ManagedProjectsData.Projects.Add(data);
						modData.ProjectAppData = data;

						Log.Here().Activity($"Added project {modData.DisplayName} to managed projects.");

						bSaveData = true;
					}
				}
				else
				{
#if DEBUG
					Data.ManagedProjects.Add(ModProjectData.Test(project.Name));
#else
					MainWindow.FooterError($"Error adding project {project.Name} to managed projects: Mod data doesn't exist.");
#endif
				}
			}

			Data.RaisePropertyChanged("NewProjects");

			if (bSaveData)
			{
				if (DOS2DECommands.SaveManagedProjects(Data))
				{
					MainWindow.FooterLog("Saved Managed Projects data to {0}.", Data.Settings.AddedProjectsFile);
				}
				else
				{
					MainWindow.FooterError("Error saving Managed Projects data to {0}.", Data.Settings.AddedProjectsFile);
				}
			}
		}

		public void LoadDataDirectory()
		{
			if (!FileCommands.IsValidPath(Data.Settings.DOS2DEDataDirectory))
			{
				if (Data.Settings.FindDOS2DataDirectory())
				{
					Log.Here().Warning("DOS2 data directory found via the registry.");
					saveModuleSettings = true;
				}
				else
				{
					Log.Here().Warning("DOS2 data directory not found.");
				}
			}
			else
			{
				Log.Here().Activity("DOS2 data directory set to {0}", Data.Settings.DOS2DEDataDirectory);
			}
		}

		public void LoadDirectoryLayout()
		{
			if (Data.Settings.DirectoryLayouts == null)
			{
				Data.Settings.DirectoryLayouts = new List<string>();
			}
			else
			{
				Data.Settings.DirectoryLayouts.Clear();
			}

			string layoutFile = "";
			string layoutFileContents = "";

			if (File.Exists(Data.Settings.DirectoryLayoutFile))
			{
				Log.Here().Activity("DirectoryLayout file found at {0}. Reading directory layout.", Data.Settings.DirectoryLayoutFile);

				layoutFile = Data.Settings.DirectoryLayoutFile;
			}
			else
			{
				Log.Here().Warning("DirectoryLayout.txt file not found. Using default settings.");

				if (File.Exists(DOS2DEDefaultPaths.DirectoryLayout(Data)))
				{
					Log.Here().Activity("DirectoryLayout.default.txt found. Reading directory layout.");

					layoutFile = DOS2DEDefaultPaths.DirectoryLayout(Data);
				}
				else
				{
					Log.Here().Warning("Default DirectoryLayout.default.txt file not found. Using default settings stored in app.");

					layoutFileContents = SCG.Modules.DOS2DE.Properties.Resources.DirectoryLayout;
					FileCommands.WriteToFile(DOS2DEDefaultPaths.DirectoryLayout(Data), layoutFileContents);
				}

				Data.Settings.DirectoryLayoutFile = DOS2DEDefaultPaths.DirectoryLayout(Data);

				saveModuleSettings = true;
			}

			if (!String.IsNullOrEmpty(layoutFile) && File.Exists(layoutFile))
			{
				layoutFileContents = File.ReadAllText(layoutFile);
			}

			if (layoutFileContents != "")
			{
				using (var reader = new System.IO.StringReader(layoutFileContents))
				{
					for (string line = reader.ReadLine(); line != null; line = reader.ReadLine())
					{
						if (line.Trim().Length > 0)
						{
							//Get the first non-whitespace character index.
							int offset = line.TakeWhile(c => char.IsWhiteSpace(c)).Count();
							bool isComment = line.Substring(offset, 1) == "#";

							if (!isComment)
							{
								Data.Settings.DirectoryLayouts.Add(line);
								Log.Here().Activity("Added {0} to project directory path patterns.", line);
							}
						}
					}
				}
			}
		}

		public async void RefreshAllProjects()
		{
			if (MainAppData.ProgressActive) return;

			if (Data.CanClickRefresh)
			{
				Data.CanClickRefresh = false;

				await Task.Run(() =>
				{
					DOS2DECommands.LoadAll(Data);
					Data.CanClickRefresh = true;
				});
			}
			else
			{
				//Log.Here().Activity("Currently refreshing.");
			}
		}

		public async void RefreshAvailableProjects()
		{
			if (MainAppData.ProgressActive) return;

			if (Data.CanClickRefresh)
			{
				Data.CanClickRefresh = false;

				await Task.Run(() =>
				{
					DOS2DECommands.RefreshAvailableProjects(Data);
					Data.CanClickRefresh = true;
				});
			}
		}

		public async void RefreshModProjects()
		{
			if (MainAppData.ProgressActive) return;

			if (Data.CanClickRefresh)
			{
				Data.CanClickRefresh = false;

				await Task.Run(() =>
				{
					DOS2DECommands.RefreshManagedProjects(Data);
					Data.CanClickRefresh = true;
				});
			}
		}

		private ModProjectData ReplaceKeywordAction(IProjectData data)
		{
			if (data is ModProjectData modProjectData)
			{
				return modProjectData;
			}
			return null;
		}

		public UserControl GetProjectView(MainWindow mainWindow)
		{
			if (projectViewControl == null) projectViewControl = new ProjectViewControl(mainWindow, this);

			return projectViewControl;
		}

		private void InitModuleKeywords()
		{
			Data.KeyList.Add(new KeywordData()
			{
				KeywordName = "$Author",
				KeywordValue = "Mod Data: Author",
				Replace = (o) => { return ReplaceKeywordAction(o)?.ModuleInfo.Author; }
			});
			Data.KeyList.Add(new KeywordData()
			{
				KeywordName = "$Description",
				KeywordValue = "Mod Data: Description",
				Replace = (o) => { return ReplaceKeywordAction(o)?.ModuleInfo.Description; }
			});
			Data.KeyList.Add(new KeywordData()
			{
				KeywordName = "$ModType",
				KeywordValue = "Mod Data: Type",
				Replace = (o) => { return ReplaceKeywordAction(o)?.ModuleInfo.Type; }
			});
			Data.KeyList.Add(new KeywordData()
			{
				KeywordName = "$ProjectName",
				KeywordValue = "Mod Data: Name",
				Replace = (o) => { return ReplaceKeywordAction(o)?.ModuleInfo.Name; }
			});
			Data.KeyList.Add(new KeywordData()
			{
				KeywordName = "$Version",
				KeywordValue = "Mod Data: Version",
				Replace = (o) => { return ReplaceKeywordAction(o)?.Version; }
			});
			Data.KeyList.Add(new KeywordData()
			{
				KeywordName = "$UUID",
				KeywordValue = "Mod Data: UUID",
				Replace = (o) => { return ReplaceKeywordAction(o)?.ModuleInfo.UUID; }
			});
			Data.KeyList.Add(new KeywordData()
			{
				KeywordName = "$ModFolderName",
				KeywordValue = "Mod Data: Folder",
				Replace = (o) => { return ReplaceKeywordAction(o)?.ModuleInfo.Folder; }
			});
			Data.KeyList.Add(new KeywordData()
			{
				KeywordName = "$Targets",
				KeywordValue = "Mod Data: Targets",
				Replace = (o) => { return String.Join(",", ReplaceKeywordAction(o)?.ModuleInfo.TargetModes.ToArray()); }
			});
			Data.KeyList.Add(new KeywordData()
			{
				KeywordName = "$DependencyProjects",
				KeywordValue = "Mod Data: Dependencies",
				Replace = (o) => { return ReplaceKeywordAction(o)?.Dependencies.ToDelimitedString(d => d.Name); }
			});
		}

		public void OpenSetup(Action OnSetupFinished)
		{
			if (Data.Settings.FirstTimeSetup)
			{
				SetupWindow setupWindow = new SetupWindow(this, OnSetupFinished);
				setupWindow.Owner = App.Current.MainWindow;
				setupWindow.Show();
			}
		}

		public DOS2DEProjectController()
		{
			Data = new DOS2DEModuleData();

			IgnoredExportFiles = new List<string>();
			//IgnoredExportFiles.Add(".ailog");
			IgnoredExportFiles.Add("ReConHistory.txt");
			IgnoredExportFiles.Add("dialoglog.txt");
			IgnoredExportFiles.Add("errors.txt");
			IgnoredExportFiles.Add("gold.log");
			IgnoredExportFiles.Add("log.txt");
			IgnoredExportFiles.Add("network.log");
			IgnoredExportFiles.Add("osirislog.log");
			IgnoredExportFiles.Add("personallog.txt");
			IgnoredExportFiles.Add("story.debugInfo");
			IgnoredExportFiles.Add("story_orphanqueries_found.txt");
		}

		public void SelectionChanged()
		{
			BackupSelectedMenuData.IsEnabled = BackupSelectedToMenuData.IsEnabled = Data.ProjectSelected;
			StartGitGenerationMenuData.IsEnabled = Data.CanGenerateGit;
		}

		public void OpenFolder(string folderPath)
		{
			if (Directory.Exists(folderPath))
			{
				Process.Start(folderPath);
			}
		}

		private MenuData BackupSelectedMenuData { get; set; }
		private MenuData BackupSelectedToMenuData { get; set; }
		private MenuData StartGitGenerationMenuData { get; set; }
		private MenuData OpenLocalModsFolderMenuData { get; set; }

		public void Initialize(MainAppData mainAppData)
		{
			MainAppData = mainAppData;

			BackupSelectedMenuData = new MenuData("DOS2.BackupSelected")
			{
				Header = "Backup Selected Projects",
				ClickCommand = new ActionCommand(() => { BackupSelectedProjects(); }),
				IsEnabled = false
			};

			BackupSelectedToMenuData = new MenuData("DOS2.BackupSelectedTo")
			{
				Header = "Backup Selected Projects To...",
				ClickCommand = new ActionCommand(BackupSelectedProjectsTo),
				IsEnabled = false
			};

			StartGitGenerationMenuData = new MenuData("DOS2.StartGitGenerator")
			{
				Header = "Start Git Generator...",
				ClickCommand = new ActionCommand(OpenGitGeneratorWindow),
				IsEnabled = false
			};

			OpenLocalModsFolderMenuData = new MenuData("DOS2.OpenLocalModsFolder")
			{
				Header = "Open Local Mods Folder...",
				ClickCommand = new ActionCommand(() => {
					OpenFolder(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Larian Studios\Divinity Original Sin 2 Definitive Edition\Local Mods"));
				}),
				IsEnabled = true
			};

			var Debug_LocalizationTest = new MenuData("DOS2.ParseLocalizationLSB")
			{
				Header = "[Debug] Parse Localization",
				ClickCommand = new ActionCommand(() => { var f = DOS2DELocalizationEditor.LoadLSBAsync(@"G:\Divinity Original Sin 2\DefEd\Data\Mods\Nemesis_627c8d3a-7e6b-4fd2-8ce5-610d553fdbe9\Localization\LLMIME_MiscText.lsb"); }),
				IsEnabled = true
			};

			MainAppData.MenuBarData.File.Register(Data.ModuleName,
				new SeparatorData(),
				new MenuData("DOS2.RefreshProjects")
				{
					Header = "Refresh Projects",
					MenuItems = new ObservableCollection<IMenuData>()
					{
						new MenuData("DOS2.RefreshAll", "Refresh All", new ActionCommand(RefreshAllProjects)) { ShortcutKey = System.Windows.Input.Key.F5 },
						new MenuData("DOS2.RefreshManagedData", "Refresh Managed Data", new ActionCommand(RefreshModProjects)),
					}
				},
				new SeparatorData(),
				BackupSelectedMenuData,
				BackupSelectedToMenuData,
				StartGitGenerationMenuData,
				new SeparatorData(),
				OpenLocalModsFolderMenuData
#if DEBUG
				,
				new SeparatorData(),
				Debug_LocalizationTest
#endif
			);
		}

		public void Start()
		{
			DOS2DECommands.SetData(Data);

			LoadDataDirectory();
			LoadDirectoryLayout();
			InitModuleKeywords();

			DOS2DECommands.LoadAll(Data);

			AppController.Main.MainWindow.Dispatcher.BeginInvoke(new Action(() =>
			{
				Data.NewProjectsAvailable = Data.NewProjects != null && Data.NewProjects.Count > 0;
				Data.UpdateManageButtonsText();
			}), DispatcherPriority.Background);

			if (saveModuleSettings)
			{
				FileCommands.Save.SaveModuleSettings(Data);
				saveModuleSettings = false;
			}
#if DEBUG
			//TestView();
#endif
		}

		public void Unload()
		{
			MainAppData.MenuBarData.RemoveAllModuleMenus(Data.ModuleName);
		}
	}
}