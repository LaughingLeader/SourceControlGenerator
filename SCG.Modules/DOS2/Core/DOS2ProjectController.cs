﻿using SCG.Commands;
using SCG.Data;
using SCG.Data.App;
using SCG.Data.View;
using SCG.FileGen;
using SCG.Interfaces;
using SCG.Modules.DOS2.Controls;
using SCG.Modules.DOS2.Core;
using SCG.Modules.DOS2.Data.View;
using SCG.Modules.DOS2.Windows;
using SCG.Windows;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Globalization;
using Alphaleonis.Win32.Filesystem;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using ReactiveUI;
using DynamicData.Binding;
using DynamicData;

namespace SCG.Core
{
	public class DOS2ProjectController : IProjectController
	{
		private ProjectViewControl projectViewControl;

		public MainAppData MainAppData { get; set; }
		public DOS2ModuleData Data { get; set; }

		public IModuleData ModuleData => Data;

		private bool saveModuleSettings = false;

		#region Git Generation

		private List<JunctionData> PrepareDirectories(ModProjectData project, List<string> DirectoryLayouts)
		{
			var sourceFolders = new List<JunctionData>();
			foreach (var directoryBaseName in DirectoryLayouts)
			{
				var projectSubdirectoryName = directoryBaseName.Replace("ProjectName", project.ProjectName).Replace("ProjectGUID", project.ModuleInfo.UUID);

				var junctionSourceDirectory = Path.Combine(Data.Settings.DOS2DataDirectory, projectSubdirectoryName);
				if (!Directory.Exists(junctionSourceDirectory))
				{
					Log.Here().Important($"Directory {projectSubdirectoryName} doesn't exist. Creating directory.");
					Directory.CreateDirectory(junctionSourceDirectory);
				}
				sourceFolders.Add(new JunctionData()
				{
					SourcePath = junctionSourceDirectory,
					BasePath = projectSubdirectoryName
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

				AppController.Main.UpdateProgress(1);

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
					var result = GitGenerator.CreateJunctions(modProject.ProjectName, sourceFolders, Data);

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
					ProjectUUID = modProject.UUID
				};

				modProject.GitData = sourceControlData;

				FileCommands.Save.SaveSourceControlData(sourceControlData, gitProjectRootDirectory);

				AppController.Main.UpdateProgressAndMax(1);

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
		private CancellationTokenSource cancellationTokenSource;

		private void CancelBackupProgress()
		{
			if(cancellationTokenSource != null)
			{
				Log.Here().Warning("Cancelling backup progress...");
				cancellationTokenSource.Cancel();
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
			var sortedProjects = Data.ManagedProjects.Where(p => p.Selected).OrderBy(p => p.DisplayName);

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
				MainWindow.FooterError($"Problem occured when backing up selected projects. Check the log. {totalSuccess}/{selectedProjects.Count} archives were created.");
			}
		}

		private async Task<int> BackupSelectedProjectsAsync(ConcurrentBag<ModProjectData> selectedProjects)
		{
			AppController.Main.Data.ProgressValueMax = selectedProjects.Count * 2;

			int totalSuccess = 0;

			if (selectedProjects != null)
			{
				int i = 0;
				foreach (var project in selectedProjects)
				{
					AppController.Main.UpdateProgressTitle((selectedProjects.Count > 1 ? "Backing up projects..." : $"Backing up project... ") + $"{i}/{selectedProjects.Count}");

					//Log.Here().Activity($"[Progress-Backup] Target percentage for this backup iteration is {targetPercentage} => {totalPercentageAmount}. Amount per tick is {amountPerTick}.");

					AppController.Main.UpdateProgressMessage($"Creating archive for project {project.ProjectName}...");

					var backupSuccess = await BackupProjectAsync(project, targetBackupOutputDirectory, Data.Settings.BackupMode);

					if (backupSuccess == FileCreationTaskResult.Success)
					{
						totalSuccess += 1;
						Log.Here().Activity("Successfully created archive for {0}.", project.ProjectName);
						project.LastBackup = DateTime.Now;
						var d = Data.ManagedProjectsData.SavedProjects.Items.Where(p => p.UUID == project.UUID).FirstOrDefault();
						if (d != null) d.LastBackupUTC = project.LastBackup?.ToUniversalTime().ToString();

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

					AppController.Main.UpdateProgress(1);

					AppController.Main.UpdateProgressTitle((selectedProjects.Count > 1 ? "Backing up projects..." : $"Backing up project... ") + $"{i + 1}/{selectedProjects.Count}");
					i++;
				}
			}

			AppController.Main.UpdateProgressTitle((selectedProjects.Count > 1 ? "Backing up projects..." : $"Backing up project... ") + $"{selectedProjects.Count}/{selectedProjects.Count}");
			AppController.Main.UpdateProgressMessage("Finishing up...");
			AppController.Main.UpdateProgressLog("Backup quest complete. +5 XP");
			AppController.Main.FinishProgress();

			if (totalSuccess > 0) DOS2Commands.SaveManagedProjects(this.Data);

			return totalSuccess;
		}

		public async Task<FileCreationTaskResult> BackupProjectAsync(ModProjectData modProject, string OutputDirectory = "", BackupMode mode = BackupMode.Zip)
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
				return await BackupGenerator.CreateArchiveFromRoot(Data.Settings.DOS2DataDirectory.Replace("/", "\\\\"), sourceFolders, archivePath, true, cancellationTokenSource.Token).ConfigureAwait(false);
			}
			else
			{
				gitProjectDirectory = Path.Combine(Data.Settings.GitRootDirectory, modProject.ProjectName);

				if (mode == BackupMode.GitArchive)
				{
					AppController.Main.UpdateProgressLog("Running git archive command...");
					var success = await GitGenerator.Archive(gitProjectDirectory, archivePath).ConfigureAwait(false);
					return success ? FileCreationTaskResult.Success : FileCreationTaskResult.Error;
				}
				else
				{
					AppController.Main.UpdateProgressLog("Creating zip archive...");
					return await BackupGenerator.CreateArchiveFromRepo(gitProjectDirectory, Data.Settings.DOS2DataDirectory.Replace("/", "\\\\"), sourceFolders, archivePath, true, cancellationTokenSource.Token).ConfigureAwait(false);
				}
				//Seems to have a problem with junctions and long paths
				//return BackupGenerator.CreateArchiveFromRepo(gitProjectDirectory, archivePath);
			}
		}

		#endregion Backup

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

					if (Data.ManagedProjectsData.SavedProjects.Items.Any(p => p.UUID == modData.UUID))
					{
						if (modData.ProjectAppData == null)
						{
							ProjectAppData data = Data.ManagedProjectsData.SavedProjects.Items.Where(p => p.UUID == modData.ModuleInfo.UUID).FirstOrDefault();
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
						Data.ManagedProjectsData.SavedProjects.AddOrUpdate(data);
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
				if (DOS2Commands.SaveManagedProjects(Data))
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
			if (!FileCommands.IsValidPath(Data.Settings.DOS2DataDirectory))
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
				Log.Here().Activity("DOS2 data directory set to {0}", Data.Settings.DOS2DataDirectory);
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

				if (File.Exists(DOS2DefaultPaths.DirectoryLayout(Data)))
				{
					Log.Here().Activity("DirectoryLayout.default.txt found. Reading directory layout.");

					layoutFile = DOS2DefaultPaths.DirectoryLayout(Data);
				}
				else
				{
					Log.Here().Warning("Default DirectoryLayout.default.txt file not found. Using default settings stored in app.");

					layoutFileContents = SCG.Modules.DOS2.Properties.Resources.DirectoryLayout;
					FileCommands.WriteToFile(DOS2DefaultPaths.DirectoryLayout(Data), layoutFileContents);
				}

				Data.Settings.DirectoryLayoutFile = DOS2DefaultPaths.DirectoryLayout(Data);

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
					DOS2Commands.LoadAll(Data);
					Data.CanClickRefresh = true;
				});
			}
			else
			{
				//Log.Here().Activity("Currently refreshing.");
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
					DOS2Commands.RefreshManagedProjects(Data);
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

		public void TestView()
		{
			/*
			Data.UserKeywords.RemoveEmpty();

			for (int i = 0; i < 50; i++)
			{
				string testStr = "test_" + i;

				var tdata = new TemplateEditorData()
				{
					ID = testStr,
					Name = testStr
				};
				tdata.Init(Data);
				Data.Templates.Add(tdata);

				var kdata = new KeywordData()
				{
					KeywordName = testStr,
					KeywordValue = ""
				};
				Data.UserKeywords.Keywords.Add(kdata);
			}
			*/

			Data.NewProjects.Add(new AvailableProjectViewData()
			{
				Name = "SJjjsjdiasjdiasidiahdisahdihaisdhddddddddddddddddddddddddddddddddddddddddiasdias"
			});

			for (var i = 0; i < 15; i++)
			{
				Data.NewProjects.Add(new AvailableProjectViewData()
				{
					Name = "Project_" + i
				});
			}

			Data.ManagedProjects.Add(ModProjectData.Test("Test Project"));
			Data.ManagedProjects.Add(ModProjectData.Test("Test Project 2"));
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

		public DOS2ProjectController()
		{
			Data = new DOS2ModuleData();
		}

		public void SelectionChanged()
		{
			BackupSelectedMenuData.IsEnabled = BackupSelectedToMenuData.IsEnabled = Data.ProjectSelected;
			StartGitGenerationMenuData.IsEnabled = Data.CanGenerateGit;
		}

		private MenuData BackupSelectedMenuData { get; set; }
		private MenuData BackupSelectedToMenuData { get; set; }
		private MenuData StartGitGenerationMenuData { get; set; }

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

			MainAppData.MenuBarData.File.Register(Data.ModuleName,
				new SeparatorData(),
				new MenuData("DOS2.RefreshProjects")
				{
					Header = "Refresh Projects",
					MenuItems = new ObservableCollectionExtended<IMenuData>()
					{
						new MenuData("DOS2.RefreshAll", "Refresh All", new ActionCommand(RefreshAllProjects), System.Windows.Input.Key.F5),
						new MenuData("DOS2.RefreshManagedData", "Refresh Managed Data", new ActionCommand(RefreshModProjects)),
					}
				},
				new SeparatorData(),
				BackupSelectedMenuData,
				BackupSelectedToMenuData,
				StartGitGenerationMenuData
			);
		}

		public void Start()
		{
			DOS2Commands.SetData(Data);

			LoadDataDirectory();
			LoadDirectoryLayout();
			InitModuleKeywords();

			DOS2Commands.LoadAll(Data);

			Data.UpdateManageButtonsText();

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