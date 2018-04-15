using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using LL.SCG.Controls;
using LL.SCG.Data;
using LL.SCG.Data.View;
using LL.SCG.DOS2.Data.App;
using LL.SCG.DOS2.Data.View;
using LL.SCG.FileGen;
using LL.SCG.Windows;
using Microsoft.Win32;

namespace LL.SCG.DOS2.Core
{
	public static class DOS2Commands
	{
		public static void LoadAll(DOS2ModuleData Data)
		{
			LoadModProjects(Data);
			LoadManagedProjects(Data);
			LoadAvailableProjects(Data);
		}

		public static void LoadManagedProjects(DOS2ModuleData Data)
		{
			if (Data.ManagedProjects == null)
			{
				Data.ManagedProjects = new ObservableCollection<ModProjectData>();
			}
			else
			{
				Data.ManagedProjects.Clear();
			}

			string projectsAppDataPath = DefaultPaths.ProjectsAppData(Data);

			if (Data.Settings != null && File.Exists(Data.Settings.AddedProjectsFile))
			{
				projectsAppDataPath = Data.Settings.AddedProjectsFile;
			}

			if (!String.IsNullOrEmpty(projectsAppDataPath) && File.Exists(projectsAppDataPath))
			{
				//projectsAppDataPath = Path.GetFullPath(projectsAppDataPath);
				try
				{
					Data.ManagedProjectsData = JsonInterface.DeserializeObject<ManagedProjectsData>(projectsAppDataPath);
				}
				catch (Exception ex)
				{
					Log.Here().Error("Error deserializing managed projects data at {0}: {1}", projectsAppDataPath, ex.ToString());
				}
			}


			if (Data.ManagedProjectsData == null)
			{
				Data.ManagedProjectsData = new ManagedProjectsData();
			}
			else
			{
				foreach (var project in Data.ManagedProjectsData.Projects)
				{
					//var modProject = Data.ModProjects.Where(x => x.Name == project.Name && x.ModuleInfo.UUID == project.GUID).FirstOrDefault();
					var modProject = Data.ModProjects.Where(x => x.Name == project.Name && x.ID == project.UUID).FirstOrDefault();
					if (modProject != null)
					{
						Data.ManagedProjects.Add(modProject);

						DateTime lastBackup;
						var success = DateTime.TryParse(project.LastBackupUTC, out lastBackup);
						if (success)
						{
							Log.Here().Activity($"Successully parsed {modProject.LastBackup} to DateTime.");
							modProject.LastBackup = lastBackup.ToLocalTime();
						}
						else
						{
							Log.Here().Error($"Could not convert {project.LastBackupUTC} to DateTime.");
						}
					}
				}
			}
		}

		public static void LoadAvailableProjects(DOS2ModuleData Data)
		{
			if (Data.NewProjects == null)
			{
				Data.NewProjects = new ObservableCollection<AvailableProjectViewData>();
			}
			else
			{
				Data.NewProjects.Clear();
			}

			if (Data.ModProjects != null && Data.ModProjects.Count > 0)
			{
				foreach (var project in Data.ModProjects)
				{
					if (!string.IsNullOrEmpty(project.Name))
					{
						bool projectIsUnmanaged = true;

						if (Data.ManagedProjects != null)
						{
							if (Data.ManagedProjects.Any(p => p.Name == project.Name))
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
							Data.NewProjects.Add(availableProject);
						}
					}
				}
			}

		}

		public static void LoadModProjects(DOS2ModuleData Data)
		{
			if (Data.ModProjects == null)
			{
				Data.ModProjects = new ObservableCollection<ModProjectData>();
			}
			else
			{
				Data.ModProjects.Clear();
			}

			if (Data.Settings != null && !String.IsNullOrEmpty(Data.Settings.DataDirectory))
			{
				if (Directory.Exists(Data.Settings.DataDirectory))
				{
					string projectsPath = Path.Combine(Data.Settings.DataDirectory, "Projects");
					string modsPath = Path.Combine(Data.Settings.DataDirectory, "Mods");

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
									Log.Here().Activity("Finished reading meta files for mod: {0}", modProjectData.ModuleInfo.Name);
									Data.ModProjects.Add(modProjectData);
								}
							}
						}
					}
				}
				else
				{
					Log.Here().Error("Loading available projects failed. DOS2 data directory not found at {0}", Data.Settings.DataDirectory);
				}
			}

			if(Directory.Exists(Data.Settings.GitRootDirectory))
			{
				foreach(var project in Data.ModProjects)
				{
					var filePath = Path.Combine(Data.Settings.GitRootDirectory, project.Name, DefaultPaths.SourceControlGeneratorDataFile);
					var success = false;
					if(File.Exists(filePath))
					{
						var sourceControlData = FileCommands.Load.LoadSourceControlData(filePath);
						if(sourceControlData != null)
						{
							project.GitData = sourceControlData;
							success = true;
						}
					}
					
					if(success)
					{
						Log.Here().Important($"Source control file found in git repo for project {project.Name}.");
					}
					else
					{
						Log.Here().Activity($"Source control file not found for project {project.Name}.");
					}
				}
			}
		}


		public static bool SaveManagedProjects(DOS2ModuleData Data)
		{
			Log.Here().Important("Saving Managed Projects data to {0}", Data.Settings.AddedProjectsFile);

			if (Data.ManagedProjectsData != null && Data.ManagedProjectsData.Projects.Count > 0 && Data.Settings != null && FileCommands.IsValidPath(Data.Settings.AddedProjectsFile))
			{
				string json = JsonInterface.SerializeObject(Data.ManagedProjectsData);
				return FileCommands.WriteToFile(Data.Settings.AddedProjectsFile, json);
			}

			return false;
		}

		private static DOS2ModuleData MainData { get; set; }

		public static void SetData(DOS2ModuleData moduleData)
		{
			MainData = moduleData;
		}

		public static void OpenBackupFolder(ModProjectData modProjectData)
		{
			if(MainData != null)
			{
				string directory = Path.Combine(Path.GetFullPath(MainData.Settings.BackupRootDirectory), modProjectData.Name);
				if (!Directory.Exists(directory))
				{
					Directory.CreateDirectory(directory);
				}

				Process.Start(directory);
			}
			else
			{
				Log.Here().Error($"MainData is null!");
			}
		}

		public static void OpenGitFolder(ModProjectData modProjectData)
		{
			if (MainData != null)
			{
				string directory = Path.Combine(Path.GetFullPath(MainData.Settings.GitRootDirectory), modProjectData.Name);
				if (Directory.Exists(directory))
				{
					Process.Start(directory);
				}
				else
				{
					Process.Start(Path.GetFullPath(MainData.Settings.GitRootDirectory));
				}
			}
			else
			{
				Log.Here().Error($"MainData is null!");
			}
		}

		public static void OpenModsFolder(ModProjectData modProjectData)
		{
			Log.Here().Activity($"Attempting to open mods folder");

			if (MainData != null)
			{
				string startPath = Path.Combine(MainData.Settings.DataDirectory, "Mods");
				string directory = Path.Combine(Path.GetFullPath(startPath), modProjectData.FolderName);

				Log.Here().Activity($"Attempting to open directory {directory}");

				if (Directory.Exists(directory))
				{
					Process.Start(directory);
				}
				else
				{
					if(Directory.Exists(startPath))
					{
						Process.Start(startPath);
					}
					else
					{
						Log.Here().Error($"Mod directory for project {modProjectData.FolderName} does not exist!");
					}
				}
			}
		}

		public static void OpenPublicFolder(ModProjectData modProjectData)
		{
			if (MainData != null)
			{
				string startPath = Path.Combine(MainData.Settings.DataDirectory, "Public");
				string directory = Path.Combine(Path.GetFullPath(startPath), modProjectData.FolderName);
				if (Directory.Exists(directory))
				{
					Process.Start(directory);
				}
				else
				{
					if (Directory.Exists(startPath))
					{
						Process.Start(startPath);
					}
					else
					{
						Log.Here().Error($"Public directory for project {modProjectData.FolderName} does not exist!");
					}
				}
			}
		}

		public static void OpenEditorFolder(ModProjectData modProjectData)
		{
			if (MainData != null)
			{
				string startPath = Path.Combine(MainData.Settings.DataDirectory, "Editor/Mods");
				string directory = Path.Combine(Path.GetFullPath(startPath), modProjectData.FolderName);
				if (Directory.Exists(directory))
				{
					Process.Start(directory);
				}
				else
				{
					if (Directory.Exists(startPath))
					{
						Process.Start(startPath);
					}
					else
					{
						Log.Here().Error($"Editor directory for project {modProjectData.FolderName} does not exist!");
					}
				}
			}
		}

		public static void OpenProjectFolder(ModProjectData modProjectData)
		{
			if (MainData != null)
			{
				string startPath = Path.Combine(MainData.Settings.DataDirectory, "Projects");
				string directory = Path.Combine(Path.GetFullPath(startPath), modProjectData.FolderName);
				if (Directory.Exists(directory))
				{
					Process.Start(directory);
				}
				else
				{
					if (Directory.Exists(startPath))
					{
						Process.Start(startPath);
					}
					else
					{
						Log.Here().Error($"Projects directory for project {modProjectData.FolderName} does not exist!");
					}
				}
			}
		}
	}
}
