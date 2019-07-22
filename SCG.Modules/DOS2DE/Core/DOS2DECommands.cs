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
using SCG.Modules.DOS2DE.Data;
using SCG.Modules.DOS2DE.Data.View;
using SCG.FileGen;
using SCG.Windows;
using Microsoft.Win32;

namespace SCG.Modules.DOS2DE.Core
{
	public static class DOS2DECommands
	{
		public static async Task LoadAll(DOS2DEModuleData Data)
		{
			await new SynchronizationContextRemover();
			await LoadModProjectsAsync(Data);
			await LoadManagedProjectsAsync(Data);
			await LoadSourceControlDataAsync(Data);
			await Task.Run(() => LoadAvailableProjects(Data));
		}

		public static async Task LoadManagedProjectsAsync(DOS2DEModuleData Data, bool ClearExisting = true)
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
					Data.ManagedProjectsData.Projects = Data.ManagedProjectsData.Projects.OrderBy(p => p.Name).ToList();
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
							await existingData.ReloadDataAsync();
						}
					}
				}
			}
		}

		public static void LoadAvailableProjects(DOS2DEModuleData Data, bool ClearExisting = false)
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

		public static string[] IgnoredFolders { get; private set; } = new string[7] { "Origins", "DivinityOrigins_1301db3d-1f54-4e98-9be5-5094030916e4", "Shared", "Arena", "DOS2_Arena", "Game_Master", "GameMaster" };

		public static async Task LoadModProjectsAsync(DOS2DEModuleData Data, bool ClearPrevious = true)
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

			if (Data.Settings != null && !String.IsNullOrEmpty(Data.Settings.DOS2DEDataDirectory))
			{
				if (Directory.Exists(Data.Settings.DOS2DEDataDirectory))
				{
					string projectsPath = Path.Combine(Data.Settings.DOS2DEDataDirectory, "Projects");
					string modsPath = Path.Combine(Data.Settings.DOS2DEDataDirectory, "Mods");

					if (Directory.Exists(modsPath))
					{
						Log.Here().Activity("Loading DOS2 projects from mods directory at: {0}", modsPath);

						//DirectoryInfo modsRoot = new DirectoryInfo(modsPath);
						//var modFolders = modsRoot.GetDirectories().Where(s => !IgnoredFolders.Contains(s.Name));

						DirectoryEnumerationFilters filters = new DirectoryEnumerationFilters()
						{
							InclusionFilter = (f) =>
							{
								return !IgnoredFolders.Contains(f.FileName);
							},
						};

						var modFolders = Directory.EnumerateDirectories(modsPath, DirectoryEnumerationOptions.Folders, filters, PathFormat.LongFullPath);

						if (modFolders != null)
						{
							foreach (string modFolder in modFolders)
							{
								var modFolderName = Path.GetFileName(modFolder);
								Log.Here().Activity("Checking project mod folder: {0}", modFolderName);

								var metaFilePath = Path.Combine(modFolder, "meta.lsx");
								if (File.Exists(metaFilePath))
								{
									Log.Here().Activity("Meta file found for project {0}. Reading file.", modFolderName);

									ModProjectData modProjectData = new ModProjectData();
									await modProjectData.LoadAllDataAsync(metaFilePath, projectsPath);

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
					Log.Here().Error("Loading available projects failed. DOS2 data directory not found at {0}", Data.Settings.DOS2DEDataDirectory);
				}
			}
		}

		public static async Task<bool> LoadSourceControlDataAsync(DOS2DEModuleData Data)
		{
			if (Directory.Exists(Data.Settings.GitRootDirectory))
			{
				int totalSuccess = 0;
				foreach (var project in Data.ModProjects)
				{
					var filePath = Path.Combine(Data.Settings.GitRootDirectory, project.ProjectName, DefaultPaths.SourceControlGeneratorDataFile);
					bool success = false;
					if (File.Exists(filePath))
					{
						var sourceControlData = await FileCommands.Load.LoadSourceControlDataAsync(filePath);
						if (sourceControlData != null)
						{
							project.GitData = sourceControlData;
							totalSuccess += 1;
							success = true;
						}
					}

					project.GitGenerated = success;
				}

				return totalSuccess > 0;
			}
			return false;
		}

		#region Refresh

		public static async Task RefreshAvailableProjects(DOS2DEModuleData Data)
		{
			await new SynchronizationContextRemover();
			await LoadModProjectsAsync(Data, true);
			await Task.Run(() => LoadAvailableProjects(Data, true));
			await LoadManagedProjectsAsync(Data);
		}

		public static async Task RefreshManagedProjects(DOS2DEModuleData Data)
		{
			if (Data.ManagedProjects != null && Data.ManagedProjects.Count > 0)
			{
				if (Data.Settings != null && !String.IsNullOrEmpty(Data.Settings.DOS2DEDataDirectory))
				{
					if (Directory.Exists(Data.Settings.DOS2DEDataDirectory))
					{
						string projectsPath = Path.Combine(Data.Settings.DOS2DEDataDirectory, "Projects");
						string modsPath = Path.Combine(Data.Settings.DOS2DEDataDirectory, "Mods");

						if (Directory.Exists(modsPath))
						{
							Log.Here().Activity("Reloading DOS2 project data from mods directory at: {0}.", modsPath);

							foreach (var project in Data.ManagedProjects)
							{
								var modData = Data.ModProjects.Where(p => p.ProjectName == project.ProjectName && p.UUID == project.UUID).FirstOrDefault();
								if (modData != null)
								{
									Log.Here().Activity($"Reloading data for project {project.ProjectName}.");
									await modData.ReloadDataAsync();
								}
							}
						}
					}
					else
					{
						Log.Here().Error("Loading available projects failed. DOS2 data directory not found at {0}", Data.Settings.DOS2DEDataDirectory);
					}
				}

				//Reload settings like if a git project actually exists
				await LoadSourceControlDataAsync(Data);
			}
		}

		#endregion


		public static bool SaveManagedProjects(DOS2DEModuleData Data)
		{
			Log.Here().Important("Saving Managed Projects data to {0}", Data.Settings.AddedProjectsFile);

			if (Data.ManagedProjectsData != null && Data.ManagedProjectsData.Projects.Count > 0 && Data.Settings != null && FileCommands.IsValidPath(Data.Settings.AddedProjectsFile))
			{
				Data.ManagedProjectsData.Projects.RemoveAll(p => Data.ModProjects.Where(mp => mp.ProjectName == p.Name).FirstOrDefault() == null);
				Data.ManagedProjectsData.Projects = Data.ManagedProjectsData.Projects.OrderBy(p => p.Name).ToList();
				string json = JsonInterface.SerializeObject(Data.ManagedProjectsData);
				return FileCommands.WriteToFile(Data.Settings.AddedProjectsFile, json);
			}

			return false;
		}

		private static DOS2DEModuleData MainData { get; set; }

		public static void SetData(DOS2DEModuleData moduleData)
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
				string startPath = Path.Combine(MainData.Settings.DOS2DEDataDirectory, "Mods");
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
				string startPath = Path.Combine(MainData.Settings.DOS2DEDataDirectory, "Public");
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
				string startPath = Path.Combine(MainData.Settings.DOS2DEDataDirectory, "Editor/Mods");
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
				string startPath = Path.Combine(MainData.Settings.DOS2DEDataDirectory, "Projects");
				string directory = Path.Combine(Path.GetFullPath(startPath), modProjectData.ProjectName);
				if(!Directory.Exists(directory))
				{
					directory = Path.Combine(Path.GetFullPath(startPath), modProjectData.ModuleInfo.Folder); // Imported projects
				}
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
