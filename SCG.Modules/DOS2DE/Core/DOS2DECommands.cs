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
using ReactiveUI;
using System.Reactive.Concurrency;
using DynamicData;
using Newtonsoft.Json;
using System.Reactive.Linq;
using System.Windows.Threading;
using System.Reactive.Disposables;
using System.Reactive;
using System.Threading;

namespace SCG.Modules.DOS2DE.Core
{
	public static class DOS2DECommands
	{
		#region Sync
		public static List<ModProjectData> LoadAll(DOS2DEModuleData Data, bool clearExisting = false)
		{
			var newMods = LoadModProjects(Data, clearExisting);
			LoadManagedProjects(Data, newMods, clearExisting);
			LoadSourceControlData(Data, newMods);
			return newMods;
		}

		public static List<ModProjectData> LoadModProjects(DOS2DEModuleData Data, bool clearExisting = false)
		{
			if (clearExisting)
			{
				Data.ModProjects.Clear();
				Log.Here().Important("Cleared mod project data.");
			}

			List<ModProjectData> newItems = new List<ModProjectData>();

			if (Data.Settings != null && !String.IsNullOrEmpty(Data.Settings.DOS2DEDataDirectory))
			{
				if (Directory.Exists(Data.Settings.DOS2DEDataDirectory))
				{
					string projectsPath = Path.Combine(Data.Settings.DOS2DEDataDirectory, "Projects");
					string modsPath = Path.Combine(Data.Settings.DOS2DEDataDirectory, "Mods");

					if (Directory.Exists(modsPath))
					{
						Log.Here().Activity("Loading DOS2 projects from mods directory at: {0}", modsPath);

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
								//Log.Here().Activity("Checking project mod folder: {0}", modFolderName);

								var metaFilePath = Path.Combine(modFolder, "meta.lsx");
								if (File.Exists(metaFilePath))
								{
									//Log.Here().Activity("Meta file found for project {0}. Reading file.", modFolderName);

									ModProjectData modProjectData = new ModProjectData();
									modProjectData.LoadAllData(metaFilePath, projectsPath);

									//Log.Here().Activity("Finished reading meta files for mod: {0}", modProjectData.ModuleInfo.Name);

									if (!clearExisting)
									{
										var previous = Data.ModProjects.Items.FirstOrDefault(p => p.FolderName == modProjectData.FolderName);
										if (previous != null)
										{
											if (previous.DataIsNewer(modProjectData))
											{
												previous.UpdateData(modProjectData);
											}
										}
										else
										{
											newItems.Add(modProjectData);
										}
									}
									else
									{
										newItems.Add(modProjectData);
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

			return newItems;
		}

		public static void LoadManagedProjects(DOS2DEModuleData Data, List<ModProjectData> modProjects, bool clearExisting = false)
		{
			if (clearExisting)
			{
				foreach (var mod in Data.ModProjects.Items)
				{
					mod.IsManaged = false;
				}
			}

			string projectsAppDataPath = DefaultPaths.ModuleAddedProjectsFile(Data);

			if (Data.Settings != null && File.Exists(Data.Settings.AddedProjectsFile))
			{
				projectsAppDataPath = Data.Settings.AddedProjectsFile;
			}

			if (!String.IsNullOrEmpty(projectsAppDataPath) && File.Exists(projectsAppDataPath))
			{
				try
				{
					string contents = FileCommands.ReadFile(projectsAppDataPath);
					var settings = new JsonSerializerSettings
					{
						NullValueHandling = NullValueHandling.Ignore,
						MissingMemberHandling = MissingMemberHandling.Error
					};

					Data.ManagedProjectsData = JsonConvert.DeserializeObject<ManagedProjectsData>(contents, settings);
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
				if (Data.ManagedProjectsData.SortedProjects != null)
				{
					foreach (var p in Data.ManagedProjectsData.SortedProjects)
					{
						Data.ManagedProjectsData.SavedProjects.AddOrUpdate(p);
					}
				}

				foreach (var m in Data.ManagedProjectsData.SavedProjects.Items)
				{
					var modData = modProjects.FirstOrDefault(p => p.UUID == m.UUID);
					if (modData != null)
					{
						modData.IsManaged = true;

						if (clearExisting)
						{
							RxApp.MainThreadScheduler.ScheduleAsync(async (s, t) =>
							{
								await modData.ReloadDataAsync();

								if (!String.IsNullOrWhiteSpace(m.LastBackupUTC))
								{
									DateTime lastBackup;

									var success = DateTime.TryParse(m.LastBackupUTC, out lastBackup);
									if (success)
									{
										//Log.Here().Activity($"Successully parsed {modProject.LastBackup} to DateTime.");
										modData.LastBackup = lastBackup.ToLocalTime();
									}
									else
									{
										Log.Here().Error($"Could not convert {m.LastBackupUTC} to DateTime.");
									}
								}
								await s.Yield();
								return Disposable.Empty;
							});	
						}
					}
				};
			}
		}

		public static bool LoadSourceControlData(DOS2DEModuleData Data, IEnumerable<ModProjectData> modProjects)
		{
			if (Directory.Exists(Data.Settings.GitRootDirectory))
			{
				int totalSuccess = 0;
				foreach (var project in modProjects)
				{
					var filePath = Path.Combine(Data.Settings.GitRootDirectory, project.ProjectName, DefaultPaths.SourceControlGeneratorDataFile);
					bool success = false;
					if (File.Exists(filePath))
					{
						var sourceControlData = FileCommands.Load.LoadSourceControlData(filePath);
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
		#endregion
		#region Async
		public static async Task<List<ModProjectData>> LoadAllAsync(DOS2DEModuleData Data, bool clearExisting = false, bool continueOnCaptureContext = true)
		{
			//await new SynchronizationContextRemover();
			var newMods = await LoadModProjectsAsync(Data, clearExisting);
			await LoadManagedProjectsAsync(Data, newMods, clearExisting);
			await LoadSourceControlDataAsync(Data, newMods);
			return newMods;
		}

		public static string[] IgnoredFolders { get; private set; } = new string[7] { "Origins", "DivinityOrigins_1301db3d-1f54-4e98-9be5-5094030916e4", "Shared", "Arena", "DOS2_Arena", "Game_Master", "GameMaster" };

		public static async Task<List<ModProjectData>> LoadModProjectsAsync(DOS2DEModuleData Data, bool clearExisting = false)
		{
			if (clearExisting)
			{
				Log.Here().Important("Clearing mod projects");
				if (Thread.CurrentThread.IsBackground)
				{
					await Observable.Start(() =>
					{
						Data.ModProjects.Clear();
						return Unit.Default;
					}, RxApp.MainThreadScheduler);
				}
				else
				{
					Data.ModProjects.Clear();
				}
			}

			List<ModProjectData> newItems = new List<ModProjectData>();

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

						//var modFolders = await Observable.Start(() =>
						//{
						//	return Directory.EnumerateDirectories(modsPath, DirectoryEnumerationOptions.Folders, filters, PathFormat.LongFullPath);
						//}, RxApp.TaskpoolScheduler);

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

									if(!clearExisting)
									{
										var previous = Data.ModProjects.Items.FirstOrDefault(p => p.FolderName == modProjectData.FolderName);
										if(previous != null)
										{
											if(previous.DataIsNewer(modProjectData))
											{
												if (Thread.CurrentThread.IsBackground)
												{
													await Observable.Start(() =>
													{
														previous.UpdateData(modProjectData);
														return Unit.Default;
													}, RxApp.MainThreadScheduler);
												}
												else
												{
													previous.UpdateData(modProjectData);
												}
											}
										}
										else
										{
											newItems.Add(modProjectData);
										}
									}
									else
									{
										newItems.Add(modProjectData);
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

			return newItems;
		}

		public static async Task<Unit> LoadManagedProjectsAsync(DOS2DEModuleData Data, List<ModProjectData> modProjects, bool clearExisting = false)
		{
			if (clearExisting)
			{
				foreach (var mod in Data.ModProjects.Items)
				{
					mod.IsManaged = false;
				}
			}

			string projectsAppDataPath = DefaultPaths.ModuleAddedProjectsFile(Data);

			if (Data.Settings != null && File.Exists(Data.Settings.AddedProjectsFile))
			{
				projectsAppDataPath = Data.Settings.AddedProjectsFile;
			}

			if (!String.IsNullOrEmpty(projectsAppDataPath) && File.Exists(projectsAppDataPath))
			{
				try
				{
					string contents = await FileCommands.ReadFileAsync(projectsAppDataPath);
					var settings = new JsonSerializerSettings
					{
						NullValueHandling = NullValueHandling.Ignore,
						MissingMemberHandling = MissingMemberHandling.Error
					};

					if (Thread.CurrentThread.IsBackground)
					{
						await Observable.Start(() =>
						{
							Data.ManagedProjectsData = JsonConvert.DeserializeObject<ManagedProjectsData>(contents, settings);
							return Unit.Default;
						}, RxApp.MainThreadScheduler);
					}
					else
					{
						Data.ManagedProjectsData = JsonConvert.DeserializeObject<ManagedProjectsData>(contents, settings);
					}
				}
				catch (Exception ex)
				{
					Log.Here().Error("Error deserializing managed projects data at {0}: {1}", projectsAppDataPath, ex.ToString());
				}
			}

			Debug.WriteLine("Loading managed project data");

			if (Data.ManagedProjectsData == null)
			{
				if (Thread.CurrentThread.IsBackground)
				{
					await Observable.Start(() =>
					{
						Data.ManagedProjectsData = new ManagedProjectsData();
						return Unit.Default;
					}, RxApp.MainThreadScheduler);
				}
				else
				{
					Data.ManagedProjectsData = new ManagedProjectsData();
				}
			}
			else
			{
				if (Data.ManagedProjectsData.SortedProjects != null)
				{
					if (Thread.CurrentThread.IsBackground)
					{
						await Observable.Start(() =>
						{
							foreach (var p in Data.ManagedProjectsData.SortedProjects)
							{
								Data.ManagedProjectsData.SavedProjects.AddOrUpdate(p);
							}
							return Unit.Default;
						}, RxApp.MainThreadScheduler);
					}
					else
					{
						foreach (var p in Data.ManagedProjectsData.SortedProjects)
						{
							Data.ManagedProjectsData.SavedProjects.AddOrUpdate(p);
						}
					}
				}

				foreach (var m in Data.ManagedProjectsData.SavedProjects.Items)
				{
					var modData = modProjects.FirstOrDefault(p => p.UUID == m.UUID);
					if (modData != null)
					{
						modData.IsManaged = true;

						if (clearExisting)
						{
							await modData.ReloadDataAsync();

							if (!String.IsNullOrWhiteSpace(m.LastBackupUTC))
							{
								DateTime lastBackup;

								var success = DateTime.TryParse(m.LastBackupUTC, out lastBackup);
								if (success)
								{
									//Log.Here().Activity($"Successully parsed {modProject.LastBackup} to DateTime.");
									modData.LastBackup = lastBackup.ToLocalTime();
								}
								else
								{
									Log.Here().Error($"Could not convert {m.LastBackupUTC} to DateTime.");
								}
							}
						}
					}
				};
			}

			return Unit.Default;
		}

		public static async Task<bool> LoadSourceControlDataAsync(DOS2DEModuleData Data, IEnumerable<ModProjectData> modProjects)
		{
			if (Directory.Exists(Data.Settings.GitRootDirectory))
			{
				int totalSuccess = 0;
				foreach (var project in modProjects)
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
		#endregion

		#region Refresh

		public static async Task<bool> RefreshAvailableProjects(DOS2DEModuleData Data)
		{
			//await new SynchronizationContextRemover();
			var newMods = await LoadModProjectsAsync(Data, true);
			if (Thread.CurrentThread.IsBackground)
			{
				await Observable.Start(() =>
				{
					Data.ModProjects.AddRange(newMods);
					return Unit.Default;
				}, RxApp.MainThreadScheduler);
			}
			else
			{
				Data.ModProjects.AddRange(newMods);
			}
			
			await LoadManagedProjectsAsync(Data, newMods, true);
			return true;
		}

		public static async Task<bool> RefreshManagedProjects(DOS2DEModuleData Data)
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
								var modData = Data.ModProjects.Items.FirstOrDefault(p => p.ProjectName == project.ProjectName && p.UUID == project.UUID);
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
				await LoadSourceControlDataAsync(Data, Data.ModProjects.Items);
			}

			return true;
		}

		#endregion


		public static bool SaveManagedProjects(DOS2DEModuleData Data)
		{
			Log.Here().Important("Saving Managed Projects data to {0}", Data.Settings.AddedProjectsFile);

			if (Data.ManagedProjectsData != null && Data.ManagedProjectsData.SavedProjects.Count > 0 && Data.Settings != null && FileCommands.IsValidPath(Data.Settings.AddedProjectsFile))
			{
				//Data.ManagedProjectsData.Projects.Clear();
				//Data.ManagedProjectsData.Projects.AddRange(Data.ManagedProjects.Select(m => m.ProjectAppData).ToList());
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
