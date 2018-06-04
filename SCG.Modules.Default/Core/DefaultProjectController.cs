using SCG.Data.View;
using SCG.Interfaces;
using SCG.Windows;
using SCG.Modules.Default.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SCG.Commands;
using System.Collections.ObjectModel;
using SCG.Data.App;
using SCG.Core;
using SCG.Data;
using System.IO;
using SCG.FileGen;
using System.Collections.Concurrent;
using System.Globalization;
using SCG.Modules.Default.Controls;
using System.Windows.Controls;

namespace SCG.Modules.Default.Core
{
	public class DefaultProjectController : IProjectController
	{
		public MainAppData MainAppData { get; set; }
		public IModuleData ModuleData => Data;

		public DefaultModuleData Data { get; set; }

		private ProjectViewControl projectViewControl;

		public void RefreshProjects()
		{

		}

		public void SelectionChanged()
		{

		}

		#region Git Generation

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
			int amountPerTick = AppController.Main.Data.ProgressValueMax / total;

			Log.Here().Activity($"[Progress] Amount per tick set to {amountPerTick}");

			AppController.Main.UpdateProgressMessage("Parsing selected projects...");

			var totalSuccess = 0;

			for (var i = 0; i < total; i++)
			{
				AppController.Main.UpdateProgressTitle($"Generating Git Files... {(i)}/{total}");

				var project = Data.GitGenerationSettings.ExportProjects[i];
				int targetPercentage = amountPerTick * (i + 1);
				int totalPercentageAmount = targetPercentage - AppController.Main.Data.ProgressValue;

				//Log.Here().Activity($"[Progress] Target percentage for this iteration is {targetPercentage}, work should increase it by {totalPercentageAmount}");

				DefaultProjectData projectData = (DefaultProjectData)project;

				var success = await GenerateGitFilesAsync(projectData, Data.GitGenerationSettings, totalPercentageAmount).ConfigureAwait(false);

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

				AppController.Main.SetProgress(targetPercentage);

				AppController.Main.UpdateProgressTitle($"Generating Git Files... {(i + 1)}/{total}");
			}

			AppController.Main.UpdateProgressTitle($"Generating Git Files... {total}/{total}");
			AppController.Main.UpdateProgressMessage("Finishing up...");
			AppController.Main.UpdateProgressLog("Git generation complete. We did it!");
			AppController.Main.FinishProgress();
			return totalSuccess;
		}

		private async Task<bool> GenerateGitFilesAsync(DefaultProjectData modProject, GitGenerationSettings generationSettings, int endPercentage)
		{
			if (modProject == null) return false;

			int percentageIncrement = endPercentage / 9;

			Log.Here().Activity($"[Progress] percentageIncrement is {percentageIncrement} / {endPercentage}");

			if (!string.IsNullOrEmpty(Data.Settings.GitRootDirectory))
			{
				string gitProjectRootDirectory = Path.Combine(Data.Settings.GitRootDirectory, modProject.ProjectName);

				AppController.Main.UpdateProgress(percentageIncrement, "Creating git project directory...");

				var rootRepoDirectory = Directory.CreateDirectory(gitProjectRootDirectory);

				if (!Directory.Exists(gitProjectRootDirectory))
				{
					Directory.CreateDirectory(gitProjectRootDirectory);
				}

				if (generationSettings.InitGit)
				{
					AppController.Main.UpdateProgress(percentageIncrement, "Initializing git repo...");

					var result = await GitGenerator.InitRepository(gitProjectRootDirectory, Data.Settings.DefaultAuthor);
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

				if (generationSettings.TemplateSettings != null && generationSettings.TemplateSettings.Count > 0)
				{
					AppController.Main.UpdateProgress(percentageIncrement, "Generating templates...");

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
					AppController.Main.UpdateProgress(percentageIncrement, "Generating license...");

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
					AppController.Main.UpdateProgress(percentageIncrement, "Committing new files...");
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

				AppController.Main.UpdateProgress(percentageIncrement, "Generating source control data file...");

				SourceControlData sourceControlData = new SourceControlData()
				{
					ProjectName = modProject.ProjectName,
					ProjectUUID = modProject.UUID
				};

				modProject.GitData = sourceControlData;

				FileCommands.Save.SaveSourceControlData(sourceControlData, gitProjectRootDirectory);

				AppController.Main.UpdateProgress(percentageIncrement);

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

		public bool GenerateBackupFolder(DefaultProjectData modProject = null)
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

		public void BackupSelectedProjects(string OutputDirectory = "")
		{
			targetBackupOutputDirectory = OutputDirectory;
			AppController.Main.StartProgress($"Backing up projects...", StartBackupSelectedProjectsAsync, "Creating archive...");
		}

		public async void StartBackupSelectedProjectsAsync()
		{
			ConcurrentBag<DefaultProjectData> selectedProjects = new ConcurrentBag<DefaultProjectData>(Data.ManagedProjects.Where(p => p.Selected));

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

		private async Task<int> BackupSelectedProjectsAsync(ConcurrentBag<DefaultProjectData> selectedProjects)
		{
			var total = selectedProjects.Count;
			int amountPerTick = AppController.Main.Data.ProgressValueMax / total;

			int totalSuccess = 0;

			if (selectedProjects != null)
			{
				AppController.Main.UpdateProgressMessage("Creating archives...");

				int i = 0;
				foreach (var project in selectedProjects)
				{
					AppController.Main.UpdateProgressTitle($"Backing up projects... {i}/{total}");

					int targetPercentage = amountPerTick * (i + 1);
					int totalPercentageAmount = targetPercentage - AppController.Main.Data.ProgressValue;

					//Log.Here().Activity($"[Progress-Backup] Target percentage for this backup iteration is {targetPercentage} => {totalPercentageAmount}. Amount per tick is {amountPerTick}.");

					AppController.Main.UpdateProgressLog("Creating archive...");

					var backupSuccess = await BackupProjectAsync(project, targetBackupOutputDirectory, Data.Settings.BackupMode, totalPercentageAmount);

					if (backupSuccess)
					{
						totalSuccess += 1;
						Log.Here().Activity("Successfully created archive for {0}.", project.ProjectName);
						project.LastBackup = DateTime.Now;
						//var d = Data.ManagedProjectsData.Projects.Where(p => p.Name == project.ProjectName && p.UUID == project.UUID).FirstOrDefault();
						//if (d != null) d.LastBackupUTC = project.LastBackup?.ToUniversalTime().ToString();

						AppController.Main.UpdateProgressLog("Archive created.");
					}
					else
					{
						Log.Here().Error("Failed to create archive for {0}.", project.ProjectName);
						AppController.Main.UpdateProgressLog("Archive creation failed.");
					}

					AppController.Main.UpdateProgressTitle($"Backing up projects... {i + 1}/{total}");
					AppController.Main.SetProgress(targetPercentage);

					i++;
				}
			}

			AppController.Main.UpdateProgressTitle($"Backing up projects... {total}/{total}");
			AppController.Main.UpdateProgressMessage("Finishing up...");
			AppController.Main.UpdateProgressLog("Backup quest complete. +5 XP");
			AppController.Main.FinishProgress();

			//if (totalSuccess > 0) DOS2Commands.SaveManagedProjects(this.Data);

			return totalSuccess;
		}

		public async Task<bool> BackupProjectAsync(DefaultProjectData modProject, string OutputDirectory = "", BackupMode mode = BackupMode.Zip, int totalPercentageAmount = -1)
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

			if (!modProject.GitGenerated)
			{
				AppController.Main.UpdateProgressLog("Creating zip archive from project folders...");
				//Log.Here().Activity($"Git project not found. Archiving project {modProject.ProjectName} from project folders directly.");
				return await BackupGenerator.CreateArchiveFromDirectory(modProject.Directory, archivePath, totalPercentageAmount).ConfigureAwait(false);
			}
			else
			{
				gitProjectDirectory = Path.Combine(Data.Settings.GitRootDirectory, modProject.ProjectName);

				if (mode == BackupMode.GitArchive)
				{
					AppController.Main.UpdateProgressLog("Running git archive command...");
					return await GitGenerator.Archive(gitProjectDirectory, archivePath).ConfigureAwait(false);
				}
				else
				{
					AppController.Main.UpdateProgressLog("Creating zip archive...");
					return await BackupGenerator.CreateArchiveFromDirectory(modProject.Directory, archivePath, totalPercentageAmount).ConfigureAwait(false);
				}
				//Seems to have a problem with junctions and long paths
				//return BackupGenerator.CreateArchiveFromRepo(gitProjectDirectory, archivePath);
			}
		}

		#endregion Backup

		public void AddProjects(List<AvailableProjectViewData> selectedItems)
		{
			foreach(var item in selectedItems)
			{
				Data.ManagedProjects.Add(new DefaultProjectData() { ProjectName = item.Name });
			}
		}

		public UserControl GetProjectView(MainWindow mainWindow)
		{
			if (projectViewControl == null) projectViewControl = new ProjectViewControl(mainWindow, this);

			return projectViewControl;
		}

		public void OpenSetup(Action OnSetupFinished)
		{
			//if (Data.Settings.FirstTimeSetup)
			//{
			//	SetupWindow setupWindow = new SetupWindow(this, OnSetupFinished);
			//	setupWindow.Owner = App.Current.MainWindow;
			//	setupWindow.Show();
			//}
			OnSetupFinished.Invoke();
		}


		public void Start()
		{
			
		}

		public void Unload()
		{
			MainAppData.MenuBarData.RemoveAllModuleMenus(Data.ModuleName);
		}

		private MenuData BackupSelectedMenuData { get; set; }
		private MenuData BackupSelectedToMenuData { get; set; }
		private MenuData StartGitGenerationMenuData { get; set; }

		public void Initialize(MainAppData mainAppData)
		{
			MainAppData = mainAppData;

			BackupSelectedMenuData = new MenuData("Default.BackupSelected")
			{
				Header = "Backup Selected Projects",
				ClickCommand = new ActionCommand(() => { BackupSelectedProjects(); }),
				IsEnabled = false
			};

			BackupSelectedToMenuData = new MenuData("Default.BackupSelectedTo")
			{
				Header = "Backup Selected Projects To...",
				ClickCommand = new ActionCommand(BackupSelectedProjectsTo),
				IsEnabled = false
			};

			StartGitGenerationMenuData = new MenuData("Default.StartGitGenerator")
			{
				Header = "Start Git Generator...",
				ClickCommand = new ActionCommand(OpenGitGeneratorWindow),
				IsEnabled = false
			};

			MainAppData.MenuBarData.File.Register(Data.ModuleName,
				new SeparatorData(),
				BackupSelectedMenuData,
				BackupSelectedToMenuData,
				StartGitGenerationMenuData
			);
		}

		public DefaultProjectController()
		{
			Data = new DefaultModuleData();
		}

	}
}
