using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using LL.SCG.Data;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml;
using System.ComponentModel;
using System.Collections.ObjectModel;
using LL.SCG.Data.View;
using LL.SCG.FileGen;
using LL.SCG.Windows;
using LL.SCG.Interfaces;
using static LL.SCG.Data.KeywordData;
using System.Reflection;
using LL.SCG.Data.Xml;
using LL.SCG.Data.App;
using LL.SCG.DOS2.Data.App;
using LL.SCG.DOS2.Core;
using LL.SCG.DOS2.Data.View;
using System.Windows.Controls;
using LL.SCG.DOS2.Controls;
using System.Globalization;

namespace LL.SCG.Core
{
	public class DOS2ProjectController : IProjectController
	{
		private ProjectViewControl projectViewControl;

		public MainAppData MainAppData { get; set; }
		public DOS2ModuleData Data { get; set; }

		public IModuleData ModuleData => Data;

		private List<JunctionData> PrepareDirectories(ModProjectData project, List<string> DirectoryLayouts)
		{
			var sourceFolders = new List<JunctionData>();
			foreach (var directoryBaseName in DirectoryLayouts)
			{
				var projectSubdirectoryName = directoryBaseName.Replace("ProjectName", project.Name).Replace("ProjectGUID", project.ModuleInfo.UUID);
				var junctionSourceDirectory = Path.Combine(Data.Settings.DataDirectory, projectSubdirectoryName);
				sourceFolders.Add(new JunctionData()
				{
					SourcePath = junctionSourceDirectory,
					BasePath = projectSubdirectoryName
				});
			}
			return sourceFolders;
		}

		public bool GenerateGitFiles(ModProjectData modProject, GitGenerationSettings generationSettings)
		{
			if (modProject == null) return false;

			if (!string.IsNullOrEmpty(Data.Settings.GitRootDirectory))
			{
				string gitProjectRootDirectory = Path.Combine(Data.Settings.GitRootDirectory, modProject.Name);

				AppController.Main.UpdateProgressMessage("Creating git project directory...");

				var rootRepoDirectory = Directory.CreateDirectory(gitProjectRootDirectory);

				AppController.Main.UpdateProgressMessage("Initializing git repo...");

				if (GitGenerator.InitRepository(gitProjectRootDirectory))
				{
					Log.Here().Activity("Created git repository for Project ({0}) at {1}", modProject.Name, gitProjectRootDirectory);

					AppController.Main.UpdateProgressMessage("Generating templates...");

					Parallel.ForEach(generationSettings.TemplateSettings, templateSetting =>
					{
						var templateData = Data.Templates.Where(t => t.Name == templateSetting.TemplateName).FirstOrDefault();
						if (templateData != null)
						{
							if (templateSetting.Enabled)
							{
								string outputFIlePath = Path.Combine(gitProjectRootDirectory, templateData.ExportPath);
								string outputText = GitGenerator.ReplaceKeywords(templateData.EditorText, modProject, MainAppData, Data);
								if (!FileCommands.WriteToFile(outputFIlePath, outputText))
								{
									Log.Here().Error("[{0}] Failed to create template file at {1}", modProject.Name, templateData.ExportPath);
								}
							}
							else
							{
								Log.Here().Activity("[{0}] Skipping {1}", modProject.Name, templateSetting.TemplateName);
							}
						}
					});

					if (generationSettings.SelectedLicense != LicenseType.None)
					{
						AppController.Main.UpdateProgressMessage("Generating license...");

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
								case LicenseType.GNU:
									outputText = Properties.Resources.License_GNU;
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
							Log.Here().Error("[{0}] Failed to write license template file at {1}", modProject.Name, licenseFile);
						}
					}

					AppController.Main.UpdateProgressMessage("Setting initial git commit...");

					GitGenerator.Commit(gitProjectRootDirectory, "Initial Commit");

					AppController.Main.UpdateProgressMessage("Creating junctions...");

					var sourceFolders = PrepareDirectories(modProject, Data.Settings.DirectoryLayouts);
					if (GitGenerator.CreateJunctions(modProject.Name, sourceFolders, Data))
					{
						Log.Here().Activity("[{0}] Successfully created junctions.", modProject.Name);
					}
					else
					{
						Log.Here().Error("[{0}] Problem creating junctions.", modProject.Name);
					}

					AppController.Main.UpdateProgressMessage("Committing new files...");

					GitGenerator.Commit(gitProjectRootDirectory, "Junctioned Project Folders");

					AppController.Main.UpdateProgressMessage("Generating source control data file...");

					SourceControlData sourceControlData = new SourceControlData()
					{
						ProjectName = modProject.Name,
						ProjectUUID = modProject.ID
					};

					modProject.GitData = sourceControlData;

					FileCommands.Save.SaveSourceControlData(sourceControlData, gitProjectRootDirectory);
				}
				else
				{
					Log.Here().Error("Error creating git repository for Project {0}.", modProject.Name);
				}

				return true;
			}
			return false;
		}

		public bool GenerateBackupFolder(ModProjectData modProject = null)
		{
			string projectBackupDirectory = Data.Settings.BackupRootDirectory;
			if (modProject != null) projectBackupDirectory = Path.Combine(Data.Settings.BackupRootDirectory, modProject.Name);

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
		#region Backup
		public async void BackupSelectedProjects(string OutputDirectory = "")
		{
			await BackupSelectedProjectsAsync(OutputDirectory);
		}

		public async Task BackupSelectedProjectsAsync(string OutputDirectory = "")
		{
			var selectedProjects = Data.ManagedProjects.Where(p => p.Selected).ToList();
			int amountPerTick = 100 / selectedProjects.Count;

			bool success = false;

			AppController.Main.StartProgress("Backing up projects...", 0);

			if (selectedProjects != null && selectedProjects.Count > 0)
			{
				foreach (var project in selectedProjects)
				{
					AppController.Main.UpdateProgressMessage("Creating archive...");

					if (BackupProject(project, OutputDirectory))
					{
						Log.Here().Activity("Successfully created archive for {0}.", project.Name);
						project.LastBackup = DateTime.Now;
						var d = Data.ManagedProjectsData.Projects.Where(p => p.Name == project.Name && p.UUID == project.ID).FirstOrDefault();
						if (d != null) d.LastBackupUTC = project.LastBackup.ToUniversalTime().ToString();
						success = true;
					}
					else
					{
						Log.Here().Error("Failed to create archive for {0}.", project.Name);
					}

					AppController.Main.UpdateProgress(amountPerTick);
				}
			}

			AppController.Main.UpdateProgressMessage("Finishing up...");
			AppController.Main.FinishProgress();

			if (success) DOS2Commands.SaveManagedProjects(this.Data);
		}

		public bool BackupProject(ModProjectData modProject, string OutputDirectory = "")
		{
			if (String.IsNullOrWhiteSpace(OutputDirectory))
			{
				OutputDirectory = Path.Combine(Path.GetFullPath(Data.Settings.BackupRootDirectory), modProject.Name);
				Directory.CreateDirectory(OutputDirectory);
			}
			else
			{
				OutputDirectory = Path.GetFullPath(OutputDirectory);
			}

			string sysFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.Replace("/", "-");

			Log.Here().Important($"System date format: {sysFormat}");

			string archiveName = modProject.Name + "_" + DateTime.Now.ToString(sysFormat + "_HH-mm-ss") + ".zip";
			string archivePath = Path.Combine(OutputDirectory, archiveName);
			string gitProjectDirectory = "";

			bool gitProjectDetected = false;

			if (!String.IsNullOrEmpty(Data.Settings.GitRootDirectory))
			{
				gitProjectDirectory = Path.Combine(Data.Settings.GitRootDirectory, modProject.Name);
				if (Directory.Exists(gitProjectDirectory))
				{
					gitProjectDirectory = Path.Combine(Data.Settings.GitRootDirectory, modProject.Name);
					gitProjectDetected = true;
				}
			}

			if (!gitProjectDetected)
			{
				Log.Here().Activity($"Git project not found. Archiving project {modProject.Name} from project folders directly.");
				var sourceFolders = PrepareDirectories(modProject, Data.Settings.DirectoryLayouts);
				return BackupGenerator.CreateArchiveFromRoot(Data.Settings.DataDirectory, sourceFolders, archivePath);
			}
			else
			{
				return BackupGenerator.CreateArchiveFromRepo(gitProjectDirectory, archivePath);
			}
		}

		#endregion

		public void AddProjects(List<AvailableProjectViewData> selectedItems)
		{
			bool bSaveData = false;

			foreach (AvailableProjectViewData project in selectedItems)
			{
				Log.Here().Activity($"Adding project {project.Name} data to managed projects.");
				var modData = Data.ModProjects.Where(p => p.Name == project.Name).FirstOrDefault();
				if (modData != null)
				{
					//ManagedProjects.Add(new ProjectEntryData(modData.ProjectInfo, modData.ModInfo));
					Data.ManagedProjects.Add(modData);
					var availableProject = Data.NewProjects.Where(p => p.Name == project.Name).FirstOrDefault();
					if (availableProject != null) Data.NewProjects.Remove(availableProject);

					if (Data.ManagedProjectsData.Projects.Any(p => p.Name == modData.Name))
					{
						if (modData.ProjectAppData == null)
						{
							ProjectAppData data = Data.ManagedProjectsData.Projects.Where(p => p.Name == modData.Name && p.UUID == modData.ModuleInfo.UUID).FirstOrDefault();
							if (data != null)
							{
								modData.ProjectAppData = data;

								Log.Here().Activity($"Linked project {modData.Name} data to managed project data.");
							}
						}
					}
					else
					{
						ProjectAppData data = new ProjectAppData()
						{
							Name = modData.Name,
							UUID = modData.ModuleInfo.UUID,
							LastBackupUTC = null
						};
						Data.ManagedProjectsData.Projects.Add(data);
						modData.ProjectAppData = data;

						Log.Here().Activity($"Added project {modData.Name} to managed projects.");

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
			if (!FileCommands.IsValidPath(Data.Settings.DataDirectory))
			{
				Log.Here().Important("DOS2 data directory not found. Reverting to default.");
				string dataDirectory = Helpers.DOS2.GetInstallPath();
				if (!String.IsNullOrEmpty(dataDirectory))
				{
					dataDirectory = dataDirectory + @"\Data";
					Data.Settings.DataDirectory = dataDirectory;
				}
			}
			else
			{
				Log.Here().Activity("DOS2 data directory set to {0}", Data.Settings.DataDirectory);
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
				Log.Here().Important("DirectoryLayout.txt file not found. Using default settings.");

				if (File.Exists(DOS2DefaultPaths.DirectoryLayout(Data)))
				{
					Log.Here().Activity("DirectoryLayout.default.txt found. Reading directory layout.");

					layoutFile = DOS2DefaultPaths.DirectoryLayout(Data);
				}
				else
				{
					Log.Here().Important("Default DirectoryLayout.default.txt file not found. Using default settings stored in app.");

					layoutFileContents = LL.SCG.DOS2.Properties.Resources.DirectoryLayout;
					FileCommands.WriteToFile(DOS2DefaultPaths.DirectoryLayout(Data), layoutFileContents);
				}

				Data.Settings.DirectoryLayoutFile = DOS2DefaultPaths.DirectoryLayout(Data);
			}

			if (!String.IsNullOrEmpty(layoutFile) && File.Exists(layoutFile))
			{
				layoutFileContents = File.ReadAllText(layoutFile);
			}

			if (layoutFileContents != "")
			{
				using (var reader = new StringReader(layoutFileContents))
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

		public async void StartGitGenerationAsync()
		{
			await RunGitGeneration();
		}

		public async Task RunGitGeneration()
		{
			Log.Here().Important("Generating git repositories for selected projects.");
			int amountPerTick = 100 / Data.GitGenerationSettings.ExportProjects.Count;

			AppController.Main.StartProgress("Generating Git Files...", 0);

			foreach (var project in Data.GitGenerationSettings.ExportProjects)
			{
				AppController.Main.UpdateProgress(amountPerTick);

				ModProjectData modProjectData = (ModProjectData)project;

				if (GenerateGitFiles(modProjectData, Data.GitGenerationSettings))
				{
					Log.Here().Important("Git repository successfully generated for {0}.", project.Name);
				}
				else
				{
					Log.Here().Error("Error generating git repository for {0}.", project.Name);
				}

				AppController.Main.UpdateProgressMessage("Finishing up...");
				AppController.Main.FinishProgress();
			}
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
				KeywordValue = "Mod Data: Name_UUID",
				Replace = (o) => { return ReplaceKeywordAction(o)?.Name + "_" + ReplaceKeywordAction(o)?.ModuleInfo.UUID; }
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

		public void Initialize(MainAppData mainAppData)
		{
			MainAppData = mainAppData;

			if(Data == null)
			{
				Data = new DOS2ModuleData();
			}
		}

		public void Start()
		{
			DOS2Commands.SetData(Data);

			LoadDataDirectory();
			LoadDirectoryLayout();
			InitModuleKeywords();

			DOS2Commands.LoadAll(Data);
#if DEBUG
			//TestView();
#endif
		}

	}
}
