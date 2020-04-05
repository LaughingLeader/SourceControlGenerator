using SCG.Commands;
using SCG.Data;
using SCG.Data.App;
using SCG.Data.View;
using SCG.FileGen;
using SCG.Interfaces;
using SCG.Modules.DOS2DE.Views;
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
using SCG.Modules.DOS2DE.Data.View.Locale;
using DynamicData.Binding;
using ReactiveUI;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using DynamicData;
using System.Reactive.Disposables;
using LSLib.LS;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using SCG.Data.Xml;
using System.Reactive;

namespace SCG.Core
{
	public class DOS2DEProjectController : IProjectController
	{
		private DOS2DEProjectsView projectViewControl;

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

				var success = await GenerateGitFilesAsync(modProjectData, Data.GitGenerationSettings);

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
				string gitProjectRootDirectory = Path.Combine(Data.Settings.GitRootDirectory, modProject.ProjectFolder);

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
					var result = GitGenerator.CreateJunctions(modProject.ProjectFolder, sourceFolders, Data);

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
								string outputText = GitGenerator.ReplaceKeywords(templateData.EditorText, modProject, MainAppData, Data);
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
						outputText = GitGenerator.ReplaceKeywords(outputText, modProject, MainAppData, Data);
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
					ProjectUUID = modProject.UUID,
					RepositoryPath = gitProjectRootDirectory,
					SourceFile = Path.Combine(gitProjectRootDirectory, DefaultPaths.SourceControlGeneratorDataFile)
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
			if (modProject != null) projectBackupDirectory = Path.Combine(Data.Settings.BackupRootDirectory, modProject.ProjectFolder);

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

			//+1 progress when done searching for files, +1 when done.
			AppController.Main.Data.ProgressValueMax = selectedProjects.Count * 2;

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
						backupSuccess = FileCreationTaskResult.Skipped;
					}

					if (backupSuccess == FileCreationTaskResult.Success)
					{
						totalSuccess += 1;
						project.LastBackup = DateTime.Now;
						Log.Here().Activity("Successfully created archive for {0}.", project.ProjectName);

						RxApp.MainThreadScheduler.Schedule(() =>
						{
							Data.ManagedProjectsData.SavedProjects.AddOrUpdate(new ProjectAppData
							{
								Name = project.ProjectName,
								UUID = project.UUID,
								LastBackupUTC = DateTime.Now.ToUniversalTime().ToString()
							});

							Log.Here().Activity("Saved backup time {0}.", project.ProjectName);
						});

						AppController.Main.UpdateProgressLog("Archive created.");
					}
					else if (backupSuccess == FileCreationTaskResult.Error)
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

			if (totalSuccess > 0)
			{
				DOS2DECommands.SaveManagedProjects(this.Data);
			}

			return totalSuccess;
		}

		public async Task<FileCreationTaskResult> BackupProjectAsync(ModProjectData modProject, string OutputDirectory = "", BackupMode mode = BackupMode.Zip)
		{
			if (String.IsNullOrWhiteSpace(OutputDirectory))
			{
				OutputDirectory = Path.Combine(Path.GetFullPath(Data.Settings.BackupRootDirectory), modProject.ProjectFolder);
				Directory.CreateDirectory(OutputDirectory);
			}
			else
			{
				OutputDirectory = Path.GetFullPath(OutputDirectory);
			}

			string sysFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.Replace("/", "-");

			//Log.Here().Important($"System date format: {sysFormat}");

			string archiveName = modProject.ProjectFolder + "_" + DateTime.Now.ToString(sysFormat + "_HH-mm-ss") + ".zip";
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
				return await BackupGenerator.CreateArchiveFromRoot(Data.Settings.DOS2DEDataDirectory.Replace("/", "\\\\"), sourceFolders, archivePath, true, cancellationTokenSource.Token);
			}
			else
			{
				gitProjectDirectory = Path.Combine(Data.Settings.GitRootDirectory, modProject.ProjectFolder);

				if (mode == BackupMode.GitArchive)
				{
					AppController.Main.UpdateProgressLog("Running git archive command...");
					var success = await GitGenerator.Archive(gitProjectDirectory, archivePath);
					return success ? FileCreationTaskResult.Success : FileCreationTaskResult.Error;
				}
				else
				{
					AppController.Main.UpdateProgressLog("Creating zip archive...");
					return await BackupGenerator.CreateArchiveFromRepo(gitProjectDirectory, Data.Settings.DOS2DEDataDirectory.Replace("/", "\\\\"), sourceFolders, archivePath, true, cancellationTokenSource.Token);
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

			//+1 progress when done searching for files, +1 when done.
			AppController.Main.Data.ProgressValueMax = selectedProjects.Count * 2;

			var allTaskResults = await PackageSelectedProjectsAsync(selectedProjects, localModsFolder);
			int successCount = allTaskResults.Where(x => x.Result != FileCreationTaskResult.Error).Count();
			if (successCount >= selectedProjects.Count)
			{
				MainWindow.FooterLog($"Successfully packaged all selected projects to {localModsFolder}");
			}
			else
			{
				if (!cancellationTokenSource.IsCancellationRequested)
				{
					MainWindow.FooterError($"Problem occured when packaging selected projects. Check the log. {successCount}/{selectedProjects.Count} packages were created.");
				}
				else
				{
					MainWindow.FooterLog($"Packaging was cancelled. {successCount}/{selectedProjects.Count} packages were created.");
				}
			}
		}

		private async Task<List<FileCreationTaskData>> PackageSelectedProjectsAsync(ConcurrentBag<ModProjectData> selectedProjects, string targetFolder, bool finishProgressOnDone = true)
		{
			//AppController.Main.Data.IsIndeterminate = true;

			List<string> exportDirectories = new List<string>();
			exportDirectories.Add(Path.Combine(Data.Settings.DOS2DEDataDirectory, @"Mods\ModFolder"));
			exportDirectories.Add(Path.Combine(Data.Settings.DOS2DEDataDirectory, @"Public\ModFolder"));

			List<FileCreationTaskData> taskResults = new List<FileCreationTaskData>();

			if (selectedProjects != null)
			{
				int i = 0;
				foreach (var project in selectedProjects)
				{
					if (cancellationTokenSource.IsCancellationRequested) return taskResults;

					AppController.Main.UpdateProgressTitle((selectedProjects.Count > 1 ? "Packaging projects..." : $"Packaging project... ") + $"{i}/{selectedProjects.Count}");

					//Log.Here().Activity($"[Progress-Backup] Target percentage for this backup iteration is {targetPercentage} => {totalPercentageAmount}. Amount per tick is {amountPerTick}.");

					AppController.Main.UpdateProgressMessage($"Creating package for project {project.ProjectName}...");
					AppController.Main.UpdateProgress();

					FileCreationTaskData packageTask = await PackageProjectAsync(project, targetFolder, exportDirectories);
					taskResults.Add(packageTask);

					if (cancellationTokenSource.IsCancellationRequested)
					{
						packageTask.Result = FileCreationTaskResult.Skipped;
					}

					if (packageTask.Result == FileCreationTaskResult.Success)
					{
						Log.Here().Activity("Successfully created package for {0}.", project.ProjectName);
						project.LastBackup = DateTime.Now;
						Data.ManagedProjectsData.SavedProjects.AddOrUpdate(new ProjectAppData
						{
							Name = project.ProjectName,
							UUID = project.UUID,
							LastBackupUTC = DateTime.Now.ToUniversalTime().ToString()
						});

						AppController.Main.UpdateProgressLog("Package created.");
					}
					else if (packageTask.Result == FileCreationTaskResult.Error)
					{
						Log.Here().Error("Failed to create package for {0}.", project.ProjectName);
						AppController.Main.UpdateProgressLog("Package creation failed.");
					}
					else
					{
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

			return taskResults;
		}

		public List<string> IgnoredPackageFiles = new List<string>(){
			"ReConHistory.txt",
			"dialoglog.txt",
			"errors.txt",
			"log.txt",
			"personallog.txt",
			"story_orphanqueries_found.txt",
			".ailog",
			".log",
			".debugInfo",
			".dmp",
			"goals.div",
			"goals.raw",
			"story.div",
			"story_ac.dat",
			"story_definitions.div",
		};

		public async Task<FileCreationTaskData> PackageProjectAsync(ModProjectData modProject, string outputDirectory, List<string> exportDirectories)
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

			FileCreationTaskData taskData = new FileCreationTaskData()
			{
				TargetPath = outputPackage,
				Result = FileCreationTaskResult.None,
				ID = modProject.UUID
			};

			try
			{
				var sourceFolders = new List<string>();
				foreach (var directoryBaseName in exportDirectories)
				{
					if (cancellationTokenSource.IsCancellationRequested)
					{
						taskData.Result = FileCreationTaskResult.Skipped;
						return taskData;
					}

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

				var ignoredFiles = IgnoredPackageFiles.ToList();
				if(modProject.ModuleInfo != null && modProject.ModuleInfo.Type.Equals("Add-on", StringComparison.OrdinalIgnoreCase))
				{
					ignoredFiles.Add("story.div.osi");
				}

				var result = await DOS2DEPackageCreator.CreatePackage(Data.Settings.DOS2DEDataDirectory.Replace("/", "\\"), 
					sourceFolders, outputPackage, ignoredFiles, cancellationTokenSource.Token);
				if (result)
				{
					taskData.Result = FileCreationTaskResult.Success;
				}
				else
				{
					taskData.Result = FileCreationTaskResult.Error;
				}

				return taskData;

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
					taskData.Result = FileCreationTaskResult.Error;
				}
				else
				{
					Log.Here().Important("Cancelling package creation: {0}", ex.ToString());
					taskData.Result = FileCreationTaskResult.Skipped;
				}
			}

			return taskData;
		}
		#endregion

		#region ReleaseCreation
		private void CancelReleaseProgress()
		{
			if (cancellationTokenSource != null)
			{
				Log.Here().Warning("Cancelling package creation...");
				cancellationTokenSource.Cancel();
				AppController.Main.CancelProgress();
			}
		}

		public void CreateReleasesForSelectedProjects()
		{
			cancellationTokenSource = new CancellationTokenSource();
			AppController.Main.StartProgress($"Creating release zips for projects...", StartReleaseSelectedProjectsAsync, "", 0, true, CancelReleaseProgress);
		}

		public async void StartReleaseSelectedProjectsAsync()
		{
			//Order by descending order, since it gets reversed in the bag
			var sortedProjects = Data.ManagedProjects.Where(p => p.Selected).OrderByDescending(p => p.ProjectName);

			ConcurrentBag<ModProjectData> selectedProjects = new ConcurrentBag<ModProjectData>(sortedProjects);

			string localModsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Larian Studios\Divinity Original Sin 2 Definitive Edition\Local Mods");

			//+1 progress when done searching for files, +1 when done.
			AppController.Main.Data.ProgressValueMax = selectedProjects.Count * 4;

			var allTaskResults = await PackageSelectedProjectsAsync(selectedProjects, localModsFolder);
			int successCount = allTaskResults.Where(x => x.Result != FileCreationTaskResult.Error).Count();

			var targets = allTaskResults.Where(x => x.Result == FileCreationTaskResult.Success);
			int total = targets.Count();
			int i = 0;

			foreach (var task in targets)
			{
				if (cancellationTokenSource.IsCancellationRequested) break;

				var mod = selectedProjects.FirstOrDefault(x => x.UUID == task.ID);
				AppController.Main.UpdateProgressMessage($"Creating zip for project package {mod.ProjectName}...");

				string zipName = mod.ProjectFolder.Replace(mod.UUID, "");
				if(!zipName.EndsWith("_"))
				{
					zipName = zipName + "_";
				}
				zipName = Path.Combine(localModsFolder, zipName + "v" + mod.Version + ".zip");

				AppController.Main.UpdateProgressTitle((total > 1 ? "Zipping packages..." : "Zipping project... ") + $"{i}/{total}");
				AppController.Main.UpdateProgress();
				var result = await BackupGenerator.CreateArchiveFromFile(task.TargetPath, zipName, cancellationTokenSource.Token);

				if (result == FileCreationTaskResult.Success)
				{
					Log.Here().Activity("Successfully created zip for {0}.", mod.ProjectName);
					AppController.Main.UpdateProgressLog("Zip created.");
				}
				else if (result == FileCreationTaskResult.Error)
				{
					Log.Here().Error("Failed to create zip for {0}.", mod.ProjectName);
					AppController.Main.UpdateProgressLog("Zip creation failed.");
				}
				else
				{
					Log.Here().Activity("Skipped package creation for {0}.", mod.ProjectName);
				}

				AppController.Main.UpdateProgress();
				AppController.Main.UpdateProgressTitle((total > 1 ? "Zipping packages..." : "Zipping project... ") + $"{i + 1}/{total}");
				i++;
			}

			successCount += i;

			if (successCount >= selectedProjects.Count)
			{
				MainWindow.FooterLog($"Successfully packaged and zipped all selected projects to {localModsFolder}");
			}
			else
			{
				if (!cancellationTokenSource.IsCancellationRequested)
				{
					MainWindow.FooterError($"Problem occured when releasing selected projects. Check the log. {successCount}/{selectedProjects.Count*4} releases were created.");
				}
				else
				{
					MainWindow.FooterLog($"Packaging/Zipping was cancelled. {successCount}/{selectedProjects.Count*4} releases were created.");
				}
			}

			if (!cancellationTokenSource.IsCancellationRequested)
			{
				AppController.Main.UpdateProgressMessage("Finishing up...");
				AppController.Main.UpdateProgressLog("Packaging/zipping complete. +15 XP");
				AppController.Main.FinishProgress();
			}
		}
		#endregion

		public void AddProjects(IEnumerable<string> selectedItems)
		{
			bool bSaveData = false;

			Log.Here().Activity($"Selected Projects: {String.Join(";", selectedItems)}");

			foreach (string uuid in selectedItems)
			{
				ModProjectData modData = Data.ModProjects.Items.FirstOrDefault(m => m.UUID == uuid);
				if(modData != null)
				{
					Log.Here().Activity($"Adding project {modData.DisplayName} data to managed projects.");
					modData.IsManaged = true;

					modData.ProjectAppData = new ProjectAppData()
					{
						Name = modData.ProjectName,
						UUID = modData.ModuleInfo.UUID,
						LastBackupUTC = null
					};
					Data.ManagedProjectsData.SavedProjects.AddOrUpdate(modData.ProjectAppData);

					Log.Here().Activity($"Added project {modData.DisplayName} to managed projects.");

					bSaveData = true;
				}
			}

			if (bSaveData)
			{
				Data.ManagedProjectsData.Sort();
				
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

		private void RefreshAllProjects_OnFadingDone()
		{
			RxApp.TaskpoolScheduler.ScheduleAsync(async (s, t) =>
			{
				await RefreshAllProjectsAsync();
				await s.Yield();
				return Disposable.Empty;
			});
		}

		public void RefreshAllProjects_Start()
		{
			Data.CanClickRefresh = false;
			if (projectViewControl != null)
			{
				projectViewControl.FadeLoadingPanel(false, RefreshAllProjects_OnFadingDone);
			}
			else
			{
				RefreshAllProjects_OnFadingDone();
			}
		}

		public async Task<Unit> RefreshAllProjectsAsync()
		{
			if (Thread.CurrentThread.IsBackground)
			{
				await Observable.Start(() => {
					Data.ModProjects.Clear();
					return Unit.Default;
				}, RxApp.MainThreadScheduler);
			}
			else
			{
				Data.ModProjects.Clear();
			}

			Log.Here().Activity("Refreshing all data.");
			var newMods = await DOS2DECommands.LoadAllAsync(Data);
			Log.Here().Activity("Done Refreshing all data.");

			if (Thread.CurrentThread.IsBackground)
			{
				return await Observable.Start(() => {
					Data.ModProjects.AddRange(newMods);
					projectViewControl.FadeLoadingPanel(true, () => { Data.CanClickRefresh = true; });
					Data.UpdateManageButtonsText();
					return Unit.Default;
				}, RxApp.MainThreadScheduler);
			}
			else
			{
				Data.ModProjects.AddRange(newMods);
				projectViewControl.FadeLoadingPanel(true, () => { Data.CanClickRefresh = true; });
				Data.UpdateManageButtonsText();
				return Unit.Default;
			}
		}
		private async Task<Unit> RefreshModProjectsAsync()
		{
			await DOS2DECommands.RefreshManagedProjects(Data);
			if (Thread.CurrentThread.IsBackground)
			{
				return await Observable.Start(() => {
					projectViewControl.FadeLoadingPanel(true, () => { Data.CanClickRefresh = true; });
					return Unit.Default;
				}, RxApp.MainThreadScheduler);
			}
			else
			{
				projectViewControl.FadeLoadingPanel(true, () => { Data.CanClickRefresh = true; });
				return Unit.Default;
			}
		}

		private void RefreshModProjects_OnFadingDone()
		{
			RxApp.TaskpoolScheduler.ScheduleAsync(async (s, t) =>
			{
				await RefreshModProjectsAsync();
				await s.Yield();
				return Disposable.Empty;
			});
		}

		public void RefreshModProjects_Start()
		{
			Data.CanClickRefresh = false;
			if (projectViewControl != null)
			{
				projectViewControl.FadeLoadingPanel(false, RefreshModProjects_OnFadingDone);
			}
			else
			{
				RefreshModProjects_OnFadingDone();
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
			if (projectViewControl == null) projectViewControl = new DOS2DEProjectsView(mainWindow, this);

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

		private IObservable<bool> canRefresh;
		private IObservable<bool> anySelected;

		public DOS2DEProjectController()
		{
			Data = new DOS2DEModuleData();

			canRefresh = this.WhenAnyValue(vm => vm.Data.CanClickRefresh);
			anySelected = this.WhenAnyValue(vm => vm.Data.ProjectSelected);
			Data.RefreshAllCommand = ReactiveCommand.Create(RefreshAllProjects_Start, canRefresh);
		}

		public void SelectionChanged()
		{
			BackupSelectedMenuData.IsEnabled = BackupSelectedToMenuData.IsEnabled = Data.ProjectSelected;
			StartGitGenerationMenuData.IsEnabled = Data.CanGenerateGit;
			OpenLocalizationEditorMenuData.IsEnabled = Data.ProjectSelected;
		}

		public void OpenFolder(string folderPath)
		{
			if (Directory.Exists(folderPath))
			{
				Process.Start(folderPath);
			}
		}

		private void RebuildJunctions()
		{
			List<ModProjectData> selectedProjects = Data.ManagedProjects.Where(x => x.Selected).ToList();

			foreach(var modProject in selectedProjects)
			{
				Log.Here().Important($"Rebuilding junctions for project '{modProject.ProjectName}'.");
				var sourceFolders = PrepareDirectories(modProject, Data.Settings.DirectoryLayouts);
				var result = GitGenerator.CreateJunctions(modProject.ProjectFolder, sourceFolders, Data, true);
				if(result)
				{
					Log.Here().Activity($"Rebuilt junction for '{modProject.ProjectName}'.");
				}
			}
		}

		private void CreateEditorProjectFromPak_Start()
		{
			string documentsFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			string startDirectory = documentsFolder;

			string larianDocumentsFolder = Path.Combine(documentsFolder, @"Larian Studios\Divinity Original Sin 2 Definitive Edition");
			if (Directory.Exists(larianDocumentsFolder))
			{
				startDirectory = larianDocumentsFolder;
				string modPakFolder = Path.Combine(larianDocumentsFolder, "Mods");
				if (Directory.Exists(modPakFolder))
				{
					startDirectory = modPakFolder;
				}
				else
				{
					modPakFolder = Path.Combine(larianDocumentsFolder, "Local Mods");
					if (Directory.Exists(modPakFolder))
					{
						startDirectory = modPakFolder;
					}
				}
			}

			FileCommands.Load.OpenFileDialog(this.projectViewControl.MainWindow, "Create Divinity Editor Project from Mod Pak...",
					startDirectory, CreateEditorProjectFromPak_OnDialogDone, "", null, DOS2DEFileFilters.LarianPakFile);
		}

		private void CreateEditorProjectFromPak_OnDialogDone(string path)
		{
			AppController.Main.Data.ProgressValueMax = 100;
			AppController.Main.StartProgress($"Creating project from pak... ", () => CreateEditorProjectFromPak(path), "", 0, true);
		}

		private void CreateEditorProjectFromPak(string path)
		{
			RxApp.TaskpoolScheduler.ScheduleAsync(async (ctrl, t) =>
			{
				if (await CreateEditorProjectFromPakAsync(path))
				{
					MainWindow.FooterLog($"Successfully created project from {Path.GetFileName(path)}");
				}
				else
				{
					MainWindow.FooterError($"Problem occurred when creating project from {Path.GetFileName(path)}. Check the log (F8).");
				}
				AppController.Main.FinishProgress();
				return Disposable.Empty;
			});
		}

		private async Task<bool> CreateEditorProjectFromPakAsync(string path)
		{
			string outputDirectory = Data.Settings.DOS2DEDataDirectory;
			AppController.Main.UpdateProgressMessage($"Checking data directory {outputDirectory}...");
			if (Directory.Exists(outputDirectory))
			{
				try
				{
					ModuleInfo moduleInfo = null;

					AppController.Main.UpdateProgressMessage($"Reading Mods meta.lsx in pak...");

					using (var pr = new LSLib.LS.PackageReader(path))
					{
						string pakName = Path.GetFileNameWithoutExtension(path);

						var pak = pr.Read();
						var metaFiles = pak?.Files?.Where(pf => DOS2DEXMLUtils.IsModMetaFile(pakName, pf));
						AbstractFileInfo metaFile = null;
						foreach (var f in metaFiles)
						{
							var parentDir = Directory.GetParent(f.Name);
							// A pak may have multiple meta.lsx files for overriding NumPlayers or something. Match against the pak name in that case.
							if (parentDir.Name == pakName)
							{
								metaFile = f;
								break;
							}
						}
						if (metaFile == null) metaFile = metaFiles.FirstOrDefault();
						if (metaFile != null)
						{
							Log.Here().Activity($"Parsing meta.lsx for mod pak '{path}'.");
							using (var stream = metaFile.MakeStream())
							{
								using (var sr = new System.IO.StreamReader(stream))
								{
									string metaContents = await sr.ReadToEndAsync();

									AppController.Main.UpdateProgressLog(@"Parsing Mods\..\meta.lsx.");
									moduleInfo = new ModuleInfo();
									moduleInfo.LoadFromXml(XDocument.Parse(DOS2DEXMLUtils.EscapeXmlAttributes(metaContents)), true);

									AppController.Main.UpdateProgressLog(@"Loaded Mods\..\meta.lsx.");
									AppController.Main.UpdateProgress(20);
								}
							}
						}
						else
						{
							Log.Here().Error($"Error: No meta.lsx for mod pak '{path}'.");
						}
					}

					if(moduleInfo != null && !String.IsNullOrWhiteSpace(moduleInfo.UUID) && !String.IsNullOrWhiteSpace(moduleInfo.Folder))
					{
						AppController.Main.UpdateProgressMessage($"Preparing pak extraction...");

						string projectMetaFile = Path.Combine(Data.Settings.DOS2DEDataDirectory, $"Projects/{moduleInfo.Folder}/meta.lsx");

						if (!Data.ModProjects.Items.Any(x => x.ModuleInfo.UUID == moduleInfo.UUID))
						{
							AppController.Main.UpdateProgressMessage($"Extracting...");
							AppController.Main.UpdateProgressLog($"Extracting pak...");
							if (await DOS2DEPackageCreator.ExtractPackageAsync(path, outputDirectory, CancellationToken.None))
							{
								Log.Here().Important($"Successfully extracted pak {path}.");
								AppController.Main.UpdateProgressLog($"Pak extracted!");
							}
						}
						else
						{
							AppController.Main.UpdateProgressLog($"Project already extracted? Skipping so we don't kill your files.");
							Log.Here().Warning($"Mod project {moduleInfo.Name}({moduleInfo.UUID}) already exists in data directory. Skipping extraction.");
						}

						AppController.Main.UpdateProgress(60);

						AppController.Main.UpdateProgressMessage($"Creating Project meta.lsx...");

						if (!File.Exists(projectMetaFile))
						{
							AppController.Main.UpdateProgressLog($"Creating meta.lsx for project at {projectMetaFile}");
							System.IO.FileInfo file = new System.IO.FileInfo(projectMetaFile);
							file.Directory.Create(); // If the directory already exists, this method does nothing.
							File.WriteAllText(file.FullName, DOS2DEXMLUtils.CreateProjectMetaString(moduleInfo.UUID, DOS2DEXMLUtils.UnescapeXml(moduleInfo.Name), moduleInfo.Type));
							Log.Here().Important($"Successfully extracted '{path}' and turned it into an editor project. {moduleInfo.Name}({moduleInfo.UUID}).");
							AppController.Main.UpdateProgressMessage($"Success! Refreshing projects...");
							AppController.Main.UpdateProgressTitle($"Refreshing projects...");
							await RefreshAllProjectsAsync();
						}
						else
						{
							Log.Here().Warning($"Project meta file {projectMetaFile} already exists. Skipping.");
						}
						AppController.Main.UpdateProgress(20);
						return true;
					}
				}
				catch (Exception ex)
				{
					Log.Here().Error($"Error extracting package: {ex.ToString()}");
				}
			}
			else
			{
				Log.Here().Error($"Data directory does not exist!");
			}
			return false;
		}

		private MenuData BackupSelectedMenuData { get; set; }
		private MenuData BackupSelectedToMenuData { get; set; }
		private MenuData StartGitGenerationMenuData { get; set; }
		private MenuData OpenLocalModsFolderMenuData { get; set; }
		private MenuData OpenLocalizationEditorMenuData { get; set; }

		public void Initialize(MainAppData mainAppData)
		{
			MainAppData = mainAppData;

			BackupSelectedMenuData = new MenuData("DOS2DE.BackupSelected")
			{
				Header = "Backup Selected Projects",
				ClickCommand = new ActionCommand(() => { BackupSelectedProjects(); }),
				IsEnabled = false
			};

			BackupSelectedToMenuData = new MenuData("DOS2DE.BackupSelectedTo")
			{
				Header = "Backup Selected Projects To...",
				ClickCommand = new ActionCommand(BackupSelectedProjectsTo),
				IsEnabled = false
			};

			StartGitGenerationMenuData = new MenuData("DOS2DE.StartGitGenerator")
			{
				Header = "Start Git Generator...",
				ClickCommand = new ActionCommand(OpenGitGeneratorWindow),
				IsEnabled = false
			};

			OpenLocalModsFolderMenuData = new MenuData("DOS2DE.OpenLocalModsFolder")
			{
				Header = "Open Local Mods Folder...",
				ClickCommand = new ActionCommand(() => {
					OpenFolder(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Larian Studios\Divinity Original Sin 2 Definitive Edition\Local Mods"));
				}),
				IsEnabled = true
			};

			MainAppData.MenuBarData.File.Register(Data.ModuleName,
				new SeparatorData(),
				new MenuData("DOS2DE.RefreshProjects")
				{
					Header = "Refresh Projects",
					MenuItems = new ObservableCollectionExtended<IMenuData>()
					{
						new MenuData("DOS2DE.RefreshAll", "Refresh All", Data.RefreshAllCommand, System.Windows.Input.Key.F5),
						new MenuData("DOS2DE.RefreshManagedData", "Refresh Managed Data", ReactiveCommand.Create(RefreshModProjects_Start, canRefresh)),
					}
				},
				new SeparatorData(),
				BackupSelectedMenuData,
				BackupSelectedToMenuData,
				StartGitGenerationMenuData,
				new SeparatorData(),
				OpenLocalModsFolderMenuData,
				new SeparatorData(),
				new MenuData("DOS2DE.RebuildJunctions", "Rebuild Junctions for Selected...", ReactiveCommand.Create(RebuildJunctions, anySelected))
			);
			OpenLocalizationEditorMenuData = new MenuData("DOS2DE.LocalizationEditor", 
				"Localization Editor", ReactiveCommand.Create(DOS2DEProjectsView.ToggleDOS2DELocalizationEditor), System.Windows.Input.Key.F7);
			OpenLocalizationEditorMenuData.IsEnabled = false;

			MainAppData.MenuBarData.Tools.Register(Data.ModuleName,
				new SeparatorData(),
				OpenLocalizationEditorMenuData,
				new SeparatorData(),
				new MenuData("DOS2DE.CreateEditorProject",
				"Create Editor Project from Pak...", ReactiveCommand.Create(CreateEditorProjectFromPak_Start))
			);

			Data.OnLockScreenChangedAction = new Action<System.Windows.Visibility, bool>((v, b) =>
			{
				if(projectViewControl != null && projectViewControl.LocaleEditorWindow != null)
				{
					if(projectViewControl.LocaleEditorWindow.ViewModel != null)
					{
						projectViewControl.LocaleEditorWindow.ViewModel.LockScreenVisibility = v;
						Log.Here().Activity("Lock screen changed");
					}
				}
			});
		}

		public void Start()
		{
			DOS2DECommands.SetData(Data);

			LoadDataDirectory();
			LoadDirectoryLayout();
			InitModuleKeywords();
#if Debug
			var watch = new System.Diagnostics.Stopwatch();
			watch.Start();
#endif
			//RxApp.MainThreadScheduler.ScheduleAsync(async (scheduler, token) =>
			//{
			//	var newMods = await DOS2DECommands.LoadAllAsync(Data);
			//	Data.ModProjects.AddRange(newMods);
			//	Data.UpdateManageButtonsText();

			//	if (saveModuleSettings)
			//	{
			//		FileCommands.Save.SaveModuleSettings(Data);
			//		saveModuleSettings = false;
			//	}
			//	await scheduler.Yield();
			//	return Disposable.Empty;
			//});
			RefreshAllProjects_Start();

			//var task = DOS2DECommands.LoadAllAsync(Data, false, true).ConfigureAwait(true).;

			//var newMods = task.GetAwaiter().GetResult();
			//Data.ModProjects.AddRange(newMods);
			//Data.UpdateManageButtonsText();

			if (saveModuleSettings)
			{
				FileCommands.Save.SaveModuleSettings(Data);
				saveModuleSettings = false;
			}

			//var task = Task.Run(() => RefreshAllProjectsAsync());
			//task.Wait();

			//if (saveModuleSettings)
			//{
			//	FileCommands.Save.SaveModuleSettings(Data);
			//	saveModuleSettings = false;
			//}

#if Debug
			watch.Stop();
			Log.Here().Important($"Loading time: {watch.ElapsedMilliseconds} ms");
#endif
		}

		public void Unload()
		{
			MainAppData.MenuBarData.RemoveAllModuleMenus(Data.ModuleName);
		}
	}
}