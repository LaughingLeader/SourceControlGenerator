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

namespace LL.DOS2.SourceControl.Core
{
   public class SettingsController
	{
		private string AppSettingsFile = @"Settings/AppSettings.json";
		private string DirectoryLayoutFile = @"Settings/DirectoryLayout.txt";
		private string DirectoryLayoutDefaultFile = @"Settings/DirectoryLayout.default.txt";
		private string DOS2_SteamAppID = "435150";

		private List<string> ProjectDirectoryLayouts;
		private List<ModProjectData> AvailableProjects;
		private List<AddedProjectData> AddedProjects;

		public AppData AppSettings;

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
			if (AddedProjects == null)
			{
				AddedProjects = new List<AddedProjectData>();
			}
			else
			{
				AddedProjects.Clear();
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
							AddedProjects.Add(projectData);
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

										var existingProject = AddedProjects.Where(project => project.ProjectGUID == modUUID).FirstOrDefault();
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
	}
}
