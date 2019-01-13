using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Alphaleonis.Win32.Filesystem;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml.Linq;
using SCG.Collections;
using SCG.Controls;
using SCG.Data;
using SCG.Data.View;
using SCG.Modules.DOS2.Data.App;
using SCG.Modules.DOS2.Data.View;
using SCG.FileGen;
using SCG.Windows;
using Microsoft.Win32;

namespace SCG.Modules.DOS2.Core
{
	public static class DOS2Commands
	{
		public static void LoadAll(DOS2ModuleData Data)
		{
			LoadModProjects(Data);
			LoadManagedProjects(Data);
			LoadSourceControlData(Data);
			LoadAvailableProjects(Data);
		}

		public static void LoadManagedProjects(DOS2ModuleData Data, bool ClearExisting = true)
		{
			if (Data.ManagedProjects == null)
			{
				Data.ManagedProjects = new ObservableImmutableList<ModProjectData>();
				BindingOperations.EnableCollectionSynchronization(Data.ManagedProjects, Data.ManagedProjectsLock);
			}
			else
			{
				if(ClearExisting)
				{
					Data.ManagedProjects.DoOperation(data => data.Clear());
					BindingOperations.EnableCollectionSynchronization(Data.ManagedProjects, Data.ManagedProjectsLock);
				}
			}

			string projectsAppDataPath = DefaultPaths.ModuleAddedProjectsFile(Data);

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
					var modProject = Data.ModProjects.Where(x => x.ProjectName == project.Name && x.UUID == project.UUID).FirstOrDefault();
					if (modProject != null)
					{
						ModProjectData existingData = null;

						if(!ClearExisting)
						{
							existingData = Data.ManagedProjects.Where(p => p.ProjectName == project.Name && p.UUID == project.UUID).FirstOrDefault();
						}

						if (ClearExisting || existingData == null)
						{
							Data.ManagedProjects.Add(modProject);

							if (!String.IsNullOrWhiteSpace(project.LastBackupUTC))
							{
								DateTime lastBackup;

								var success = DateTime.TryParse(project.LastBackupUTC, out lastBackup);
								if (success)
								{
									//Log.Here().Activity($"Successully parsed {modProject.LastBackup} to DateTime.");
									modProject.LastBackup = lastBackup.ToLocalTime();
								}
								else
								{
									Log.Here().Error($"Could not convert {project.LastBackupUTC} to DateTime.");
								}
							}
						}
						else if(existingData != null)
						{
							existingData.ReloadData();
						}
					}
				}
			}
		}

		public static void LoadAvailableProjects(DOS2ModuleData Data, bool ClearExisting = false)
		{
			if (Data.NewProjects == null)
			{
				Data.NewProjects = new ObservableImmutableList<AvailableProjectViewData>();
				BindingOperations.EnableCollectionSynchronization(Data.NewProjects, Data.NewProjectsLock);
			}
			else
			{
				if (ClearExisting)
				{
					Data.NewProjects.DoOperation(data => data.Clear());
					BindingOperations.EnableCollectionSynchronization(Data.NewProjects, Data.NewProjectsLock);
				}
			}

			if (Data.ModProjects != null)
			{
				foreach (var project in Data.ModProjects)
				{
					if (!string.IsNullOrEmpty(project.ProjectName))
					{
						bool projectIsUnmanaged = true;

						if (Data.ManagedProjects != null)
						{
							if (Data.ManagedProjects.Any(p => p.ProjectName == project.ProjectName))
							{
								projectIsUnmanaged = false;
							}
						}

						if (projectIsUnmanaged)
						{
							if(ClearExisting || Data.NewProjects.Where(p => p.Name == project.ProjectName).FirstOrDefault() == null)
							{
								AvailableProjectViewData availableProject = new AvailableProjectViewData()
								{
									Name = project.ProjectName,
									Tooltip = project.Tooltip
								};
								Data.NewProjects.DoOperation(data => data.Add(availableProject));
							}
						}
					}
				}
			}

			Data.NewProjectsAvailable = Data.NewProjects != null && Data.NewProjects.Count > 0;
		}

		private static string[] ignoredFolders = new string[7] { "Origins", "DivinityOrigins_1301db3d-1f54-4e98-9be5-5094030916e4", "Shared", "Arena", "DOS2_Arena", "Game_Master", "GameMaster" };

		public static void LoadModProjects(DOS2ModuleData Data, bool ClearPrevious = true)
		{
			if (Data.ModProjects == null)
			{
				Data.ModProjects = new ObservableImmutableList<ModProjectData>();
				BindingOperations.EnableCollectionSynchronization(Data.ModProjects, Data.ModProjectsLock);
			}
			else
			{
				if (ClearPrevious)
				{
					Data.ModProjects.DoOperation(data => data.Clear());
					BindingOperations.EnableCollectionSynchronization(Data.ModProjects, Data.ModProjectsLock);
				}
			}

			if (Data.Settings != null && !String.IsNullOrEmpty(Data.Settings.DOS2DataDirectory))
			{
				if (Directory.Exists(Data.Settings.DOS2DataDirectory))
				{
					string projectsPath = Path.Combine(Data.Settings.DOS2DataDirectory, "Projects");
					string modsPath = Path.Combine(Data.Settings.DOS2DataDirectory, "Mods");

					if (Directory.Exists(modsPath))
					{
						Log.Here().Activity("Loading DOS2 projects from mods directory at: {0}", modsPath);

						DirectoryInfo modsRoot = new DirectoryInfo(modsPath);
						var modFolders = modsRoot.GetDirectories().Where(s => !ignoredFolders.Contains(s.Name));

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

									if(!ClearPrevious)
									{
										var previous = Data.ModProjects.Where(p => p.FolderName == modProjectData.FolderName).FirstOrDefault();
										if(previous != null)
										{
											if(previous.DataIsNewer(modProjectData))
											{
												previous.UpdateData(modProjectData);
											}
										}
										else
										{
											Data.ModProjects.DoOperation(data => data.Add(modProjectData));
										}
									}
									else
									{
										Data.ModProjects.DoOperation(data => data.Add(modProjectData));
									}
								}
							}
						}
					}
				}
				else
				{
					Log.Here().Error("Loading available projects failed. DOS2 data directory not found at {0}", Data.Settings.DOS2DataDirectory);
				}
			}
		}

		public static bool LoadSourceControlData(DOS2ModuleData Data)
		{
			if (Directory.Exists(Data.Settings.GitRootDirectory))
			{
				bool overallSuccess = false;
				foreach (var project in Data.ModProjects)
				{
					var filePath = Path.Combine(Data.Settings.GitRootDirectory, project.ProjectName, DefaultPaths.SourceControlGeneratorDataFile);
					var success = false;
					if (File.Exists(filePath))
					{
						var sourceControlData = FileCommands.Load.LoadSourceControlData(filePath);
						if (sourceControlData != null)
						{
							project.GitData = sourceControlData;
							success = true;
							overallSuccess = true;
						}
					}

					if (success)
					{
						//Log.Here().Important($"Source control file found in git repo for project {project.ProjectName}.");
					}
					else
					{
						//Log.Here().Warning($"Source control file not found for project {project.ProjectName}.");
					}
				}

				return overallSuccess;
			}
			return false;
		}

		#region Refresh

		public static void RefreshAvailableProjects(DOS2ModuleData Data)
		{
			LoadModProjects(Data, true);
			LoadAvailableProjects(Data, true);
			LoadManagedProjects(Data, true);
		}

		public static void RefreshManagedProjects(DOS2ModuleData Data)
		{
			if (Data.ManagedProjects != null && Data.ManagedProjects.Count > 0)
			{
				if (Data.Settings != null && !String.IsNullOrEmpty(Data.Settings.DOS2DataDirectory))
				{
					if (Directory.Exists(Data.Settings.DOS2DataDirectory))
					{
						string projectsPath = Path.Combine(Data.Settings.DOS2DataDirectory, "Projects");
						string modsPath = Path.Combine(Data.Settings.DOS2DataDirectory, "Mods");

						if (Directory.Exists(modsPath))
						{
							Log.Here().Activity("Reloading DOS2 project data from mods directory at: {0}.", modsPath);

							foreach (var project in Data.ManagedProjects)
							{
								var modData = Data.ModProjects.Where(p => p.ProjectName == project.ProjectName && p.UUID == project.UUID).FirstOrDefault();
								if (modData != null)
								{
									Log.Here().Activity($"Reloading data for project {project.ProjectName}.");
									modData.ReloadData();
								}
							}
						}
					}
					else
					{
						Log.Here().Error("Loading available projects failed. DOS2 data directory not found at {0}", Data.Settings.DOS2DataDirectory);
					}
				}
				LoadSourceControlData(Data);
			}
		}

		#endregion


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
				string directory = Path.Combine(Path.GetFullPath(MainData.Settings.BackupRootDirectory), modProjectData.ProjectName);
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
				string directory = Path.Combine(Path.GetFullPath(MainData.Settings.GitRootDirectory), modProjectData.ProjectName);
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
				string startPath = Path.Combine(MainData.Settings.DOS2DataDirectory, "Mods");
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
				string startPath = Path.Combine(MainData.Settings.DOS2DataDirectory, "Public");
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
				string startPath = Path.Combine(MainData.Settings.DOS2DataDirectory, "Editor/Mods");
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
				string startPath = Path.Combine(MainData.Settings.DOS2DataDirectory, "Projects");
				string directory = Path.Combine(Path.GetFullPath(startPath), modProjectData.ProjectName);
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
