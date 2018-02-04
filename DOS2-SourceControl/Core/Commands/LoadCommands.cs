using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using LL.DOS2.SourceControl.Data;
using LL.DOS2.SourceControl.Data.View;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace LL.DOS2.SourceControl.Core.Commands
{
	public class LoadKeywordsCommand : ICommand
	{
		public event EventHandler CanExecuteChanged;

		public bool CanExecute(object parameter)
		{
			return FileCommands.Load != null;
		}

		public void Execute(object parameter)
		{
			FileCommands.Load.LoadUserKeywords();
		}
	}

	public class LoadCommands
	{
		private MainAppData Data { get; set; }

		public void OpenDialog(Window ParentWindow, string Title, string FilePath, Action<string> OnLoaded)
		{
			OpenFileDialog fileDialog = new OpenFileDialog();
			fileDialog.Title = Title;
			fileDialog.InitialDirectory = Directory.GetParent(FilePath).FullName;
			fileDialog.FileName = Path.GetFileName(FilePath);

			Nullable<bool> result = fileDialog.ShowDialog(ParentWindow);
			if (result == true)
			{
				OnLoaded?.Invoke(fileDialog.FileName);
			}
		}

		public void LoadAppSettings()
		{
			if (File.Exists(DefaultPaths.AppSettings))
			{
				Log.Here().Activity("Loading AppSettings from {0}", DefaultPaths.AppSettings);

				Data.AppSettings = JsonConvert.DeserializeObject<AppSettingsData>(File.ReadAllText(DefaultPaths.AppSettings));
			}
			else
			{
				Log.Here().Activity("AppSettings file at {0} not found. Creating new file.", DefaultPaths.AppSettings);
				Data.AppSettings = new AppSettingsData();
				string dataDirectory = Helpers.DOS2.GetInstallPath();
				if (!String.IsNullOrEmpty(dataDirectory))
				{
					dataDirectory = dataDirectory + @"\Data";
					Data.AppSettings.DOS2DataDirectory = dataDirectory;
				}
			}

			if (String.IsNullOrEmpty(Data.AppSettings.DOS2DataDirectory))
			{
				Log.Here().Important("DOS2 data directory not found. Reverting to default.");
				string dataDirectory = Helpers.DOS2.GetInstallPath();
				if (!String.IsNullOrEmpty(dataDirectory))
				{
					dataDirectory = dataDirectory + @"\Data";
					Data.AppSettings.DOS2DataDirectory = dataDirectory;
				}
			}
			else
			{
				Log.Here().Activity("DOS2 data directory set to {0}", Data.AppSettings.DOS2DataDirectory);
			}


			if (File.Exists(Data.AppSettings.GitIgnoreFile))
			{
				Data.DefaultGitIgnoreText = File.ReadAllText(Data.AppSettings.GitIgnoreFile);
				Log.Here().Important("Loaded .gitignore template file at {0}", Data.AppSettings.GitIgnoreFile);
			}
			else
			{
				if (File.Exists(DefaultPaths.GitIgnore))
				{
					Data.DefaultGitIgnoreText = File.ReadAllText(DefaultPaths.GitIgnore);
				}
				else
				{
					Data.DefaultGitIgnoreText = Properties.Resources.DefaultGitIgnore;
					File.WriteAllText(DefaultPaths.GitIgnore, Data.DefaultGitIgnoreText);
				}
			}

			if (File.Exists(Data.AppSettings.ReadmeTemplateFile))
			{
				Data.DefaultReadmeText = File.ReadAllText(Data.AppSettings.ReadmeTemplateFile);
				Log.Here().Important("Loaded readme template file at {0}", Data.AppSettings.ReadmeTemplateFile);
			}
			else
			{
				if (File.Exists(DefaultPaths.ReadmeTemplate))
				{
					Data.DefaultReadmeText = File.ReadAllText(DefaultPaths.ReadmeTemplate);
				}
				else
				{
					Data.DefaultReadmeText = Properties.Resources.DefaultReadme;
					File.WriteAllText(DefaultPaths.ReadmeTemplate, Data.DefaultReadmeText);
				}
			}

			if (File.Exists(Data.AppSettings.ChangelogTemplateFile))
			{
				Data.DefaultChangelogText = File.ReadAllText(Data.AppSettings.ChangelogTemplateFile);
				Log.Here().Important("Loaded changelog template file at {0}", Data.AppSettings.ChangelogTemplateFile);
			}
			else
			{
				if (File.Exists(DefaultPaths.ChangelogTemplate))
				{
					Data.DefaultChangelogText = File.ReadAllText(DefaultPaths.ChangelogTemplate);
				}
				else
				{
					Data.DefaultChangelogText = Properties.Resources.DefaultChangelog;
					File.WriteAllText(DefaultPaths.ChangelogTemplate, Data.DefaultChangelogText);
				}
			}

			if (File.Exists(Data.AppSettings.CustomLicenseFile))
			{
				Data.CustomLicenseText = File.ReadAllText(Data.AppSettings.CustomLicenseFile);
				Log.Here().Important("Custom license file found and loaded.");
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

			if (Data.AppSettings != null && File.Exists(Data.AppSettings.DirectoryLayoutFile))
			{
				Log.Here().Activity("DirectoryLayout file found at {0}. Reading directory layout.", Data.AppSettings.DirectoryLayoutFile);

				layoutFile = Data.AppSettings.DirectoryLayoutFile;
			}
			else
			{
				Log.Here().Important("DirectoryLayout.txt file not found. Using default settings.");

				if (File.Exists(DefaultPaths.DirectoryLayout))
				{
					Log.Here().Activity("DirectoryLayout.default.txt found. Reading directory layout.");

					layoutFile = DefaultPaths.DirectoryLayout;
				}
				else
				{
					Log.Here().Important("Default DirectoryLayout.default.txt file not found. Using default settings stored in app.");

					layoutFileContents = Properties.Resources.DirectoryLayout;
					FileCommands.WriteToFile(DefaultPaths.DirectoryLayout, layoutFileContents);
				}

				if (Data.AppSettings != null)
				{
					Data.AppSettings.DirectoryLayoutFile = DefaultPaths.DirectoryLayout;
				}
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
								Data.ProjectDirectoryLayouts.Add(line);
								Log.Here().Activity("Added {0} to project directory path patterns.", line);
							}
						}
					}
				}
			}
		}

		public void LoadGitProjects()
		{
			if (Data.GitProjects == null)
			{
				Data.GitProjects = new List<SourceControlData>();
			}
			else
			{
				Data.GitProjects.Clear();
			}

			if (Data.AppSettings != null && !String.IsNullOrEmpty(Data.AppSettings.GitRootDirectory) && Directory.Exists(Data.AppSettings.GitRootDirectory))
			{
				Log.Here().Activity("Scanning git root directory for added projects.");

				var projects = Directory.GetFiles(Data.AppSettings.GitRootDirectory, DefaultValues.SourceControlDataFileName);
				if (projects != null && projects.Length > 0)
				{
					foreach (var projectFilePath in projects)
					{
						if (File.Exists(projectFilePath))
						{
							SourceControlData gitProjectData = JsonConvert.DeserializeObject<SourceControlData>(File.ReadAllText(projectFilePath));
							Data.GitProjects.Add(gitProjectData);
							Log.Here().Activity("Source control project file found for project {0}. Adding to active projects.", gitProjectData.ProjectName);
						}
					}
				}
			}
			else
			{
				Log.Here().Important("No git root directory not found. Skipping.");
			}
		}

		public void LoadManagedProjects()
		{
			if (Data.ManagedProjects == null)
			{
				Data.ManagedProjects = new ObservableCollection<ModProjectData>();
			}
			else
			{
				Data.ManagedProjects.Clear();
			}

			if(Data.GitProjects != null && Data.GitProjects.Count > 0 && Data.ModProjects != null && Data.ModProjects.Count > 0)
			{
				foreach(var gitProject in Data.GitProjects)
				{
					var modProject = Data.ModProjects.Where(x => x.Name == gitProject.ProjectName).FirstOrDefault();
					if (modProject != null)
					{
						//Data.ManagedProjects.Add(new ProjectEntryData(modProject.ProjectInfo, modProject.ModInfo));
						modProject.GitGenerated = true;
						Data.ManagedProjects.Add(modProject);
					}
				}
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

			if (Data.ModProjects != null && Data.ModProjects.Count > 0)
			{
				foreach (var project in Data.ModProjects)
				{
					if (!string.IsNullOrEmpty(project.Name))
					{
						bool projectIsUnmanaged = (Data.ManagedProjects == null || Data.ManagedProjects != null && Data.ManagedProjects.Count <= 0);

						if (projectIsUnmanaged && Data.ManagedProjects != null)
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
							Data.AvailableProjects.Add(availableProject);
						}
					}
				}
			}
		}

		public void LoadModProjects()
		{
			if (Data.ModProjects == null)
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
									Log.Here().Activity("Finished reading meta files for mod: {0}", modProjectData.ModuleInfo.Name);
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

		public void LoadUserKeywords()
		{
			if (Data != null && Data.AppSettings != null)
			{
				LoadUserKeywords(Data.AppSettings.KeywordsFile);
			}
		}

		public void LoadUserKeywords(string filePath)
		{
			if (Data.AppSettings != null && !String.IsNullOrEmpty(filePath))
			{
				if (File.Exists(filePath))
				{
					Log.Here().Important("Deserializing Keywords list from {0}", filePath);
					try
					{
						Data.UserKeywords = JsonConvert.DeserializeObject<UserKeywordData>(File.ReadAllText(filePath));
					}
					catch (Exception ex)
					{
						Log.Here().Error("Error deserializing Keywords.json: {0}", ex.ToString());
					}
				}
			}
			else
			{
				Log.Here().Important("Keywords.json not set or AppSettings is null. Skipping.");
			}

			if (Data.UserKeywords == null)
			{
				Data.UserKeywords = new UserKeywordData()
				{
					DateCustom = "MMMM dd, yyyy",
					Keywords = new ObservableCollection<KeywordData>()
					{
						new KeywordData(),
						new KeywordData(),
						new KeywordData(),
						new KeywordData()
					}
				};
			}
		}

		public void LoadAll()
		{
			LoadAppSettings();
			LoadDirectoryLayout();
			LoadUserKeywords();
			LoadModProjects();
			LoadGitProjects();
			LoadManagedProjects();
			LoadAvailableProjects();
		}

		public LoadCommands(MainAppData AppData)
		{
			this.Data = AppData;
		}
	}
}
