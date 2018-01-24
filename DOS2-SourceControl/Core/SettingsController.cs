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

namespace LL.DOS2.SourceControl.Core
{
   public class SettingsController
	{
		private string AppSettingsFile = @"Settings/AppSettings.json";
		private string DirectoryLayoutFile = @"Settings/DirectoryLayout.txt";
		private string DirectoryLayoutDefaultFile = @"Settings/DirectoryLayout.default.txt";
		private string DOS2_SteamAppID = "435150";

		private AppData appSettings;
		private List<string> projectDirectoryLayouts;
		private List<ModProjectData> availableProjects;
		private List<AddedProjectData> managedProjects;

		private string defaultGitIgnoreText;

		public List<string> ProjectDirectoryLayouts { get => projectDirectoryLayouts; set => projectDirectoryLayouts = value; }
		public List<ModProjectData> AvailableProjects { get => availableProjects; set => availableProjects = value; }
		public List<AddedProjectData> ManagedProjects { get => managedProjects; set => managedProjects = value; }

		public AppData AppSettings { get => appSettings; set => appSettings = value; }

		public string DefaultGitIgnoreText { get => defaultGitIgnoreText; set => defaultGitIgnoreText = value; }

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged(String property)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(property));
			}
		}

		public bool WriteToFile(string FPath, string Contents)
		{
			try
			{
				FileInfo file = new FileInfo(FPath);
				file.Directory.Create();
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
				FileInfo file = new FileInfo(FPath);
				file.Directory.Create();
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

		public void LoadAppSettings()
		{
			if(File.Exists(AppSettingsFile))
			{
				Log.Here().Activity("Loading AppSettings from {0}", AppSettingsFile);

				AppSettings = JsonConvert.DeserializeObject<AppData>(File.ReadAllText(AppSettingsFile));
			}
			else
			{
				Log.Here().Activity("AppSettings file at {0} not found. Creating new file.", AppSettingsFile);
				AppSettings = new AppData();
				string dataDirectory = Core.CoreHelper.GetDOS2Directory();
				if (!String.IsNullOrEmpty(dataDirectory))
				{
					dataDirectory = dataDirectory + @"\Data";
					AppSettings.DOS2DataDirectory = dataDirectory;
				}
				SaveAppSettings();
			}

			
			if (String.IsNullOrEmpty(AppSettings.DOS2DataDirectory))
			{
				Log.Here().Important("DOS2 data directory not found.");
			}
			else
			{
				Log.Here().Activity("DOS2 data directory found at {0}", AppSettings.DOS2DataDirectory);
			}

			if(File.Exists(AppSettings.GitIgnoreFile))
			{
				DefaultGitIgnoreText = File.ReadAllText(AppSettings.GitIgnoreFile);
			}
			else
			{
				if(File.Exists(AppData.DefaultGitIgnorePath()))
				{
					DefaultGitIgnoreText = File.ReadAllText(AppData.DefaultGitIgnorePath());
				}
				else
				{
					DefaultGitIgnoreText = Properties.Resources.DefaultGitIgnore;
					File.WriteAllText(AppData.DefaultGitIgnorePath(), DefaultGitIgnoreText);
				}
			}
		}

		public void SaveAppSettings()
		{
			Log.Here().Activity("Saving AppSettings to {0}", AppSettingsFile);

			if(AppSettings != null)
			{
				string json = JsonConvert.SerializeObject(AppSettings, Newtonsoft.Json.Formatting.Indented);
				WriteToFile(AppSettingsFile, json);
			}
		}

		public void SaveGitIgnore()
		{
			if (AppSettings != null)
			{
				Log.Here().Activity("Saving .gitignore.default to {0}", AppSettings.GitIgnoreFile);

				if (IsPathValid(AppSettings.GitIgnoreFile))
				{
					WriteToFile(AppSettings.GitIgnoreFile, DefaultGitIgnoreText);
				}
				else
				{
					Log.Here().Error("Invalid path for default .gitignore file: {0}. Using default path: {1}", AppSettings.GitIgnoreFile, AppData.DefaultGitIgnorePath());
					WriteToFile(AppData.DefaultGitIgnorePath(), DefaultGitIgnoreText);
				}
			}
		}

		public void LoadDirectoryLayout()
		{
			if (ProjectDirectoryLayouts == null)
			{
				ProjectDirectoryLayouts = new List<string>();
			}
			else
			{
				ProjectDirectoryLayouts.Clear();
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
								ProjectDirectoryLayouts.Add(line);
								Log.Here().Activity("Added {0} to project directory path patterns.", line);
							}
						}
					}
				}
			}
		}

		public void LoadAddedProjects()
		{
			if (ManagedProjects == null)
			{
				ManagedProjects = new List<AddedProjectData>();
			}
			else
			{
				ManagedProjects.Clear();
			}

			if (AppSettings != null && !String.IsNullOrEmpty(AppSettings.GitRootDirectory) && Directory.Exists(AppSettings.GitRootDirectory))
			{
				Log.Here().Activity("Scanning git root directory for added projects.");

				var projects = Directory.GetFiles(AppSettings.GitRootDirectory, "project-sourcecontrol.json");
				if(projects != null && projects.Length > 0)
				{
					foreach(var projectFilePath in projects)
					{
						if (File.Exists(projectFilePath))
						{
							AddedProjectData projectData = JsonConvert.DeserializeObject<AddedProjectData>(File.ReadAllText(projectFilePath));
							ManagedProjects.Add(projectData);
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
			if (AvailableProjects == null)
			{
				AvailableProjects = new List<ModProjectData>();
			}
			else
			{
				AvailableProjects.Clear();
			}

			if (AppSettings != null && !String.IsNullOrEmpty(AppSettings.DOS2DataDirectory))
			{
				if(Directory.Exists(AppSettings.DOS2DataDirectory))
				{
					string projectsPath = Path.Combine(AppSettings.DOS2DataDirectory, "Mods");

					if (Directory.Exists(projectsPath))
					{
						Log.Here().Activity("Loading DOS2 projects from projects directory at: {0}", projectsPath);

						DirectoryInfo projectsRoot = new DirectoryInfo(projectsPath);
						var projectFolders = projectsRoot.GetDirectories();

						if(projectFolders != null)
						{
							foreach (DirectoryInfo projectInfo in projectFolders)
							{
								var projectFolderName = projectInfo.Name;
								Log.Here().Activity("Checking project folder: {0}", projectFolderName);

								var metaFile = projectInfo.GetFiles("meta.lsx").FirstOrDefault();
								if (metaFile != null)
								{
									Log.Here().Activity("Meta file found for project {0}. Reading file.", projectFolderName);

									var metaContents = XDocument.Load(metaFile.OpenRead());
									var moduleInfo = metaContents.XPathSelectElement("save/region/node/children/node[@id='ModuleInfo']");

									if (moduleInfo != null)
									{
										Log.Here().Activity("Module info selected.");

										string projectName = moduleInfo.Descendants("attribute").FirstOrDefault(x => (string)x.Attribute("id") == "Name").Attribute("value").Value;
										string modUUID = moduleInfo.Descendants("attribute").FirstOrDefault(x => (string)x.Attribute("id") == "UUID").Attribute("value").Value;
										string modAuthor = moduleInfo.Descendants("attribute").FirstOrDefault(x => (string)x.Attribute("id") == "Author").Attribute("value").Value;

										ModProjectData projectData = new ModProjectData()
										{
											ProjectName = projectName,
											ProjectGUID = modUUID,
											Author = modAuthor,
											TargetModes = new List<string>()
										};

										var modTargets = moduleInfo.Descendants("node").FirstOrDefault(x => (string)x.Attribute("id") == "TargetModes").Descendants("node").Descendants("attribute");

										if (modTargets != null && modTargets.Count() > 0)
										{
											foreach (var target in modTargets)
											{
												var targetVal = target.Attribute("value");
												if (targetVal != null && !String.IsNullOrEmpty(targetVal.Value)) projectData.TargetModes.Add(targetVal.Value);
											}
										}

										Log.Here().Activity("Added mod project to available projects: {0}[{1}] | Modes: {2}", projectName, modUUID, projectData.TargetModes.ToString());
										AvailableProjects.Add(projectData);

										var existingProject = ManagedProjects.Where(project => project.ProjectGUID == modUUID).FirstOrDefault();
										if(existingProject != null)
										{
											Log.Here().Activity("Mod project already added. Hiding from available mod projects.");
											projectData.Hide = true;
										}

									}
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

		public void LoadSettings()
		{
			LoadAppSettings();
			LoadDirectoryLayout();
		}

		public void Start()
		{
			LoadSettings();
			LoadAddedProjects();
			LoadAvailableProjects();
		}

		public SettingsController()
		{
			Start();
		}
	}
}
