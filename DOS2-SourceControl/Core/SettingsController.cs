using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using LL.DOS2.SourceControl.Data;
using Newtonsoft.Json;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml;
using System.ComponentModel;
using System.Collections.ObjectModel;
using LL.DOS2.SourceControl.Data.View;
using LL.DOS2.SourceControl.FileGen;

namespace LL.DOS2.SourceControl.Core
{
   public class SettingsController
	{
		private string AppSettingsFile = @"Settings/AppSettings.json";
		private string DirectoryLayoutFile = @"Settings/DirectoryLayout.txt";
		private string DirectoryLayoutDefaultFile = @"Settings/DirectoryLayout.default.txt";
		private string ManagedFileName = @"DOS2SourceControl.json";
		private string DOS2_SteamAppID = "435150";

		public MainAppData Data { get; set; }

		private MainWindow mainWindow;

		public bool WriteToFile(string FPath, string Contents)
		{
			try
			{
				if(!Directory.Exists(FPath)) Directory.CreateDirectory(FPath);

				FileInfo file = new FileInfo(FPath);
				File.WriteAllText(FPath, Contents);

				Log.Here().Activity("Created file: {0}", FPath);
				return true;
			}
			catch(Exception e)
			{
				Log.Here().Error("Error creating file at {0} - {1}", FPath, e.ToString());
				return false;
			}
		}

		public bool CreateFile(string FPath)
		{
			try
			{
				if (!Directory.Exists(FPath)) Directory.CreateDirectory(FPath);

				FileInfo file = new FileInfo(FPath);
				file.Create();
				Log.Here().Activity("Created file: {0}", FPath);
				return true;
			}
			catch (Exception e)
			{
				Log.Here().Error("Error creating file at {0} - {1}", FPath, e.ToString());
				return false;
			}
		}

		public bool IsPathValid(String pathString)
		{
			Uri pathUri;
			Boolean isValidUri = Uri.TryCreate(pathString, UriKind.Absolute, out pathUri);
			return isValidUri && pathUri != null && pathUri.IsLoopback;
		}

		#region Loading

		public void LoadAppSettings()
		{
			if(File.Exists(AppSettingsFile))
			{
				Log.Here().Activity("Loading AppSettings from {0}", AppSettingsFile);

				Data.AppSettings = JsonConvert.DeserializeObject<AppSettingsData>(File.ReadAllText(AppSettingsFile));
			}
			else
			{
				Log.Here().Activity("AppSettings file at {0} not found. Creating new file.", AppSettingsFile);
				Data.AppSettings = new AppSettingsData();
				string dataDirectory = Helpers.DOS2.GetInstallPath();
				if (!String.IsNullOrEmpty(dataDirectory))
				{
					dataDirectory = dataDirectory + @"\Data";
					Data.AppSettings.DOS2DataDirectory = dataDirectory;
				}
				SaveAppSettings();
			}

			
			if (String.IsNullOrEmpty(Data.AppSettings.DOS2DataDirectory))
			{
				Log.Here().Important("DOS2 data directory not found.");
			}
			else
			{
				Log.Here().Activity("DOS2 data directory found at {0}", Data.AppSettings.DOS2DataDirectory);
			}

			if(File.Exists(Data.AppSettings.GitIgnoreFile))
			{
				Data.DefaultGitIgnoreText = File.ReadAllText(Data.AppSettings.GitIgnoreFile);
			}
			else
			{
				if(File.Exists(DefaultPaths.GitIgnore))
				{
					Data.DefaultGitIgnoreText = File.ReadAllText(DefaultPaths.GitIgnore);
				}
				else
				{
					Data.DefaultGitIgnoreText = Properties.Resources.DefaultGitIgnore;
					File.WriteAllText(DefaultPaths.GitIgnore, Data.DefaultGitIgnoreText);
				}
			}
		}

		public void LoadDirectoryLayout()
		{
			if (Data.ProjectDirectoryLayouts == null)
			{
				Data.ProjectDirectoryLayouts = new List<string>();
			}
			else
			{
				Data.ProjectDirectoryLayouts.Clear();
			}

			string layoutFile = "";
			string layoutFileContents = "";

			if (File.Exists(DirectoryLayoutFile))
			{
				Log.Here().Activity("DirectoryLayout.txt found. Reading directory layout.");

				layoutFile = DirectoryLayoutFile;
			}
			else
			{
				Log.Here().Important("DirectoryLayout.txt file not found. Using default settings.");

				if (File.Exists(DirectoryLayoutDefaultFile))
				{
					Log.Here().Activity("DirectoryLayout.default.txt found. Reading directory layout.");

					layoutFile = DirectoryLayoutDefaultFile;
				}
				else
				{
					Log.Here().Important("DirectoryLayout.default.txt file not found. Using default settings stored in app.");

					layoutFileContents = Properties.Resources.DirectoryLayout;
					WriteToFile(DirectoryLayoutDefaultFile, layoutFileContents);
					WriteToFile(DirectoryLayoutFile, layoutFileContents);
				}
			}

			if(!String.IsNullOrEmpty(layoutFile) && File.Exists(layoutFile))
			{
				layoutFileContents = File.ReadAllText(DirectoryLayoutFile);
			}

			if(layoutFileContents != "")
			{
				using (var reader = new StringReader(layoutFileContents))
				{
					for (string line = reader.ReadLine(); line != null; line = reader.ReadLine())
					{
						if(line.Trim().Length > 0)
						{
							//Get the first non-whitespace character index.
							int offset = line.TakeWhile(c => char.IsWhiteSpace(c)).Count();
							bool isComment = line.Substring(offset, 1) == "#";

							if (!isComment)
							{
								Data.ProjectDirectoryLayouts.Add(line);
								Log.Here().Activity("Added {0} to project directory path patterns.", line);
							}
						}
					}
				}
			}
		}

		public void LoadManagedProjects()
		{
			if (Data.ManagedProjects == null)
			{
				Data.ManagedProjects = new ObservableCollection<SourceControlData>();
			}
			else
			{
				Data.ManagedProjects.Clear();
			}

			if (Data.AppSettings != null && !String.IsNullOrEmpty(Data.AppSettings.GitRootDirectory) && Directory.Exists(Data.AppSettings.GitRootDirectory))
			{
				Log.Here().Activity("Scanning git root directory for added projects.");

				var projects = Directory.GetFiles(Data.AppSettings.GitRootDirectory, ManagedFileName);
				if(projects != null && projects.Length > 0)
				{
					foreach(var projectFilePath in projects)
					{
						if (File.Exists(projectFilePath))
						{
							SourceControlData projectData = JsonConvert.DeserializeObject<SourceControlData>(File.ReadAllText(projectFilePath));
							Data.ManagedProjects.Add(projectData);
							Log.Here().Activity("Source control project file found for project {0}. Adding to active projects.", projectData.ProjectName);
						}
					}
				}
			}
			else
			{
				Log.Here().Important("No git root directory not found. Skipping.");
			}
		}

		public void LoadAvailableProjects()
		{
			if (Data.AvailableProjects == null)
			{
				Data.AvailableProjects = new ObservableCollection<AvailableProjectViewData>();
			}
			else
			{
				Data.AvailableProjects.Clear();
			}

			if(Data.ModProjects != null && Data.ModProjects.Count > 0)
			{
				foreach(var project in Data.ModProjects)
				{
					if(!string.IsNullOrEmpty(project.Name))
					{
						bool projectIsUnmanaged = (Data.ManagedProjects == null || Data.ManagedProjects != null && Data.ManagedProjects.Count <= 0);

						if(projectIsUnmanaged && Data.ManagedProjects != null)
						{
							if(Data.ManagedProjects.Any(p => p.ProjectName == project.Name))
							{
								projectIsUnmanaged = false;
							}
						}

						if (projectIsUnmanaged)
						{
							AvailableProjectViewData availableProject = new AvailableProjectViewData()
							{
								Name = project.Name,
								Tooltip = project.Tooltip
							};
							Data.AvailableProjects.Add(availableProject);
						}
					}
				}
			}
		}

		public void LoadModProjects()
		{
			if(Data.ModProjects == null)
			{
				Data.ModProjects = new List<ModProjectData>();
			}
			else
			{
				Data.ModProjects.Clear();
			}

			if (Data.AppSettings != null && !String.IsNullOrEmpty(Data.AppSettings.DOS2DataDirectory))
			{
				if (Directory.Exists(Data.AppSettings.DOS2DataDirectory))
				{
					string projectsPath = Path.Combine(Data.AppSettings.DOS2DataDirectory, "Projects");
					string modsPath = Path.Combine(Data.AppSettings.DOS2DataDirectory, "Mods");

					if (Directory.Exists(modsPath))
					{
						Log.Here().Activity("Loading DOS2 projects from mods directory at: {0}", modsPath);

						DirectoryInfo modsRoot = new DirectoryInfo(modsPath);
						var modFolders = modsRoot.GetDirectories();

						if (modFolders != null)
						{
							foreach (DirectoryInfo modFolderInfo in modFolders)
							{
								var modFolderName = modFolderInfo.Name;
								Log.Here().Activity("Checking project mod folder: {0}", modFolderName);

								var metaFile = modFolderInfo.GetFiles("meta.lsx").FirstOrDefault();
								if (metaFile != null)
								{
									Log.Here().Activity("Meta file found for project {0}. Reading file.", modFolderName);
									ModProjectData modProjectData = new ModProjectData(metaFile, projectsPath);
									Log.Here().Activity("Finished reading meta files for mod: {0}", modProjectData.ModInfo.Name);
									Data.ModProjects.Add(modProjectData);
								}
							}
						}
					}
				}
				else
				{
					Log.Here().Error("Loading available projects failed. DOS2 data directory not found.");
				}
			}
		}

		#endregion

		#region Saving

		public void SaveAppSettings()
		{
			Log.Here().Activity("Saving AppSettings to {0}", AppSettingsFile);

			if (Data.AppSettings != null)
			{
				string json = JsonConvert.SerializeObject(Data.AppSettings, Newtonsoft.Json.Formatting.Indented);
				WriteToFile(AppSettingsFile, json);
			}
		}

		public void SaveGitIgnore()
		{
			if (Data.AppSettings != null)
			{
				Log.Here().Activity("Saving .gitignore.default to {0}", Data.AppSettings.GitIgnoreFile);

				if (IsPathValid(Data.AppSettings.GitIgnoreFile))
				{
					WriteToFile(Data.AppSettings.GitIgnoreFile, Data.DefaultGitIgnoreText);
				}
				else
				{
					Log.Here().Error("Invalid path for default .gitignore file: {0}. Using default path: {1}", Data.AppSettings.GitIgnoreFile, DefaultPaths.GitIgnore);
					WriteToFile(DefaultPaths.GitIgnore, Data.DefaultGitIgnoreText);
				}
			}
		}

		#endregion

		public bool GenerateGitFiles(AvailableProjectViewData project, GitGenerationSettings generationSettings)
		{
			if(!string.IsNullOrEmpty(Data.AppSettings.GitRootDirectory))
			{
				string gitProjectRootDirectory = Path.Combine(Data.AppSettings.GitRootDirectory, project.Name);
				string sourceControlFile = Path.Combine(gitProjectRootDirectory, ManagedFileName);
				string readmeFile = Path.Combine(gitProjectRootDirectory, "README.md");
				string changelogFile = Path.Combine(gitProjectRootDirectory, "CHANGELOG.md");
				string licenseFile = Path.Combine(gitProjectRootDirectory, "LICENSE");

				Directory.CreateDirectory(gitProjectRootDirectory);

				SourceControlData sourceControlData = new SourceControlData();
				sourceControlData.ProjectName = project.Name;

				string json = JsonConvert.SerializeObject(sourceControlData, Newtonsoft.Json.Formatting.Indented);
				if(!WriteToFile(sourceControlFile, json))
				{
					Log.Here().Error("[{0}] Failed to write {1}", project.Name, sourceControlFile);
				}

				if(generationSettings.GenerateReadme)
				{
					string readmeText = GitGenerator.GenerateReadmeText(Data.AppSettings, project.Name);
					if (!WriteToFile(readmeFile, readmeText))
					{
						Log.Here().Error("[{0}] Failed to write {1}", project.Name, readmeFile);
					}
				}
				else
				{
					Log.Here().Activity("[{0}] Skipping README.md.", project.Name);
				}
				
				if(generationSettings.GenerateChangelog)
				{
					string changelogText = GitGenerator.GenerateChangelogText(Data.AppSettings, project.Name);
					if (!WriteToFile(changelogFile, changelogText))
					{
						Log.Here().Error("[{0}] Failed to write {1}", project.Name, changelogFile);
					}
				}
				else
				{
					Log.Here().Activity("[{0}] Skipping CHANGELOG.md.", project.Name);
				}

				if(generationSettings.GenerateLicense)
				{
					string licenseText = GitGenerator.GenerateLicense(Data.AppSettings, project.Name, generationSettings);
					if (!WriteToFile(licenseFile, licenseText))
					{
						Log.Here().Error("[{0}] Failed to write {1}", project.Name, licenseFile);
					}
				}

				if(GitGenerator.CreateRepository(gitProjectRootDirectory))
				{
					Log.Here().Activity("Created git repository for project ({0}) at {1}", project.Name, gitProjectRootDirectory);
				}
				else
				{
					Log.Here().Error("Error creating git repository for project {0}.", project.Name);
				}
				
				

				return true;
			}
			return false;
		}

		public void AddProjectsToManaged(List<AvailableProjectViewData> selectedItems)
		{
			foreach(var project in selectedItems)
			{
				var modData = Data.ModProjects.Where(p => p.Name == project.Name).FirstOrDefault();
				if(modData != null)
				{

				}
			}
		}

		public void Start()
		{
			Log.Here().Important("Starting application.");
			LoadAppSettings();
			LoadDirectoryLayout();
			LoadModProjects();
			LoadManagedProjects();
			LoadAvailableProjects();
		}

		public SettingsController(MainWindow MainAppWindow)
		{
			mainWindow = MainAppWindow;
			Data = new MainAppData();
			Start();
		}
	}
}
