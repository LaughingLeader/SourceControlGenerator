using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using LL.DOS2.SourceControl.Controls;
using LL.DOS2.SourceControl.Data;
using LL.DOS2.SourceControl.Data.View;
using LL.DOS2.SourceControl.Windows;
using Microsoft.Win32;
using Newtonsoft.Json;
using Ookii.Dialogs.Wpf;

namespace LL.DOS2.SourceControl.Commands
{
	public class LoadCommands
	{
		private MainAppData Data { get; set; }

		public void SetData(MainAppData data)
		{
			Data = data;
		}

		public void OpenFileDialog(Window ParentWindow, string Title, string FilePath, Action<string> OnFileSelected)
		{
			OpenFileDialog fileDialog = new OpenFileDialog();
			fileDialog.Title = Title;
			fileDialog.InitialDirectory = Directory.GetParent(FilePath).FullName;
			fileDialog.FileName = Path.GetFileName(FilePath);

			Nullable<bool> result = fileDialog.ShowDialog(ParentWindow);
			if (result == true)
			{
				OnFileSelected?.Invoke(fileDialog.FileName);
			}
		}

		public void OpenFolderDialog(Window ParentWindow, string Title, string FilePath, Action<string> OnFolderSelected)
		{
			VistaFolderBrowserDialog folderDialog = new VistaFolderBrowserDialog();
			folderDialog.SelectedPath = FilePath;
			folderDialog.Description = Title;
			folderDialog.UseDescriptionForTitle = true;
			folderDialog.ShowNewFolderButton = true;

			Nullable<bool> result = folderDialog.ShowDialog(ParentWindow);

			if (result == true)
			{
				string path = folderDialog.SelectedPath;
				if (FileCommands.PathIsRelative(path))
				{
					path = folderDialog.SelectedPath.Replace(Directory.GetCurrentDirectory(), "");
				}

				OnFolderSelected?.Invoke(path);
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

				FileCommands.Save.SaveAppSettings();
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

		}

		public void LoadTemplates()
		{
			Data.Templates.Add(new TemplateEditorData()
			{
				ID = DefaultValues.TemplateID_Ignore,
				Name = ".gitignore",
				FileName = ".gitignore.template",
				LabelText = "Default GitIgnore Template",
				TooltipText = TooltipText.GitIgnore,
				DefaultEditorText = Properties.Resources.DefaultGitIgnore,
				DefaultFilePath = DefaultPaths.GitIgnore,
				GetFilePath = () => { return Data.AppSettings.GitIgnoreFile; },
				SetFilePath = (string val) => { Data.AppSettings.GitIgnoreFile = val; }
			});
			
			Data.Templates.Add(new TemplateEditorData()
			{
				ID = DefaultValues.TemplateID_Readme,
				Name = "README.md",
				FileName = "README.md.template",
				LabelText = "Default Readme Template",
				TooltipText = TooltipText.Readme,
				DefaultEditorText = Properties.Resources.DefaultReadme,
				DefaultFilePath = DefaultPaths.ReadmeTemplate,
				GetFilePath = () => { return Data.AppSettings.ReadmeTemplateFile; },
				SetFilePath = (string val) => { Data.AppSettings.ReadmeTemplateFile = val; }
			});
			
			Data.Templates.Add(new TemplateEditorData()
			{
				ID = DefaultValues.TemplateID_Changelog,
				Name = "CHANGELOG.md",
				FileName = "CHANGELOG.md.template",
				LabelText = "Default Changelog Template",
				TooltipText = TooltipText.Changelog,
				DefaultEditorText = Properties.Resources.DefaultChangelog,
				DefaultFilePath = DefaultPaths.ChangelogTemplate,
				GetFilePath = () => { return Data.AppSettings.ChangelogTemplateFile; },
				SetFilePath = (string val) => { Data.AppSettings.ChangelogTemplateFile = val; }
			});
			
			Data.Templates.Add(new TemplateEditorData()
			{
				ID = DefaultValues.TemplateID_License,
				Name = "LICENSE",
				FileName = "LICENSE.template",
				LabelText = "Custom License Template",
				TooltipText = TooltipText.CustomLicense,
				DefaultEditorText = "",
				DefaultFilePath = "",
				GetFilePath = () => { return Data.AppSettings.CustomLicenseFile; },
				SetFilePath = (string val) => { Data.AppSettings.CustomLicenseFile = val; }
			});

			Data.Templates.Add(new TemplateEditorData()
			{
				ID = DefaultValues.TemplateID_Attributes,
				Name = "attributes",
				FileName = "attributes.template",
				LabelText = "Default GitAttributes Template",
				TooltipText = TooltipText.GitAttributes,
				DefaultEditorText = Properties.Resources.DefaultGitAttributes,
				DefaultFilePath = DefaultPaths.GitAttributes,
				GetFilePath = () => { return Data.AppSettings.GitAttributesFile; },
				SetFilePath = (string val) => { Data.AppSettings.GitAttributesFile = val; }
			});

			for (int i = 0 ; i < Data.Templates.Count; i++)
			{
				var template = Data.Templates[i];
				template.Init();
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
							try
							{
								SourceControlData gitProjectData = JsonConvert.DeserializeObject<SourceControlData>(File.ReadAllText(projectFilePath));
								gitProjectData.RepositoryPath = projectFilePath;

								ModProjectData modProject = Data.ModProjects.Where(p => p.Name == gitProjectData.ProjectName).FirstOrDefault();
								if(modProject != null)
								{
									modProject.GitData = gitProjectData;
									Log.Here().Activity("Source control project file found for project {0}.", gitProjectData.ProjectName);
								}
								else
								{
									Log.Here().Error("Source control project file found for project {0}, but mod project data does not exist!", gitProjectData.ProjectName);
								}
							}
							catch(Exception ex)
							{
								Log.Here().Error("Error deserializing source control file at {0}: {1}", projectFilePath, ex.ToString());
							}
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

			if (Data.AppProjects != null)
			{
				//Data.AppProjects.Destroy();
			}

			string projectsAppDataPath = DefaultPaths.ProjectsAppData;

			if(Data.AppSettings != null && File.Exists(Data.AppSettings.ProjectsAppData))
			{
				projectsAppDataPath = Data.AppSettings.ProjectsAppData;
			}

			if(!String.IsNullOrEmpty(projectsAppDataPath) && File.Exists(projectsAppDataPath))
			{
				try
				{
					Data.AppProjects = JsonConvert.DeserializeObject<ManagedProjectsData>(File.ReadAllText(projectsAppDataPath));
				}
				catch(Exception ex)
				{
					Log.Here().Error("Error deserializing managaed projects data at {0}: {1}", projectsAppDataPath, ex.ToString());
				}
			}


			if(Data.AppProjects == null)
			{
				Data.AppProjects = new ManagedProjectsData();
			}
			else
			{
				foreach(var project in Data.AppProjects.ManagedProjects)
				{
					//var modProject = Data.ModProjects.Where(x => x.Name == project.Name && x.ModuleInfo.UUID == project.GUID).FirstOrDefault();
					var modProject = Data.ModProjects.Where(x => x.Name == project.Name).FirstOrDefault();
					if (modProject != null)
					{
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
							Data.AvailableProjects.Add(availableProject);
						}
					}
				}
			}

			//DEBUG
			Data.AvailableProjects.Add(new AvailableProjectViewData()
			{
				Name = "SJjjsjdiasjdiasidiahdisahdihaisdhddddddddddddddddddddddddddddddddddddddddiasdias"
			});

			for(var i = 0; i < 15;i++)
			{
				Data.AvailableProjects.Add(new AvailableProjectViewData()
				{
					Name = "Project_" + i
				});
			}

			/*
			Data.AvailableProjects.Add(new AvailableProjectViewData()
			{
				Name = "TestMod"
			});
			*/
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
					Log.Here().Error("Loading available projects failed. DOS2 data directory not found at {0}", Data.AppSettings.DOS2DataDirectory);
				}
			}
		}

		public void LoadUserKeywords()
		{
			if (Data != null && Data.AppSettings != null)
			{
				
				if(File.Exists(Data.AppSettings.KeywordsFile))
				{
					LoadUserKeywords(Data.AppSettings.KeywordsFile);
				}
				else
				{
					if(File.Exists(DefaultPaths.Keywords))
					{
						LoadUserKeywords(DefaultPaths.Keywords);
					}
					else
					{
						if (Data.UserKeywords == null)
						{
							Data.UserKeywords = new UserKeywordData();
							Data.UserKeywords.ResetToDefault();

							FileCommands.Save.SaveUserKeywords();
						}
					}
				}
			}
		}

		public void LoadUserKeywords(string filePath)
		{
			if (Data != null && !String.IsNullOrEmpty(filePath))
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
						Log.Here().Error("Error deserializing {0}: {1}", filePath, ex.ToString());
					}
				}
			}
			else
			{
				MainWindow.FooterLog("Problem loading keywords file at file path \"{0}\"", filePath);
			}
		}

		public void LoadGitGenerationSettings()
		{
			string filePath = DefaultPaths.GitGenSettings;
			if (Data != null && Data.AppSettings != null && File.Exists(Data.AppSettings.GitGenSettingsFile))
			{
				filePath = Data.AppSettings.GitGenSettingsFile;
			}

			if (File.Exists(filePath))
			{
				try
				{
					Data.GitGenerationSettings = JsonConvert.DeserializeObject<GitGenerationSettings>(File.ReadAllText(filePath));
					Log.Here().Important("Git generation settings file loaded.");
				}
				catch (Exception ex)
				{
					Log.Here().Error("Error deserializing {0}: {1}", filePath, ex.ToString());
				}
			}

			bool settingsNeedSaving = false;

			if(Data.GitGenerationSettings == null)
			{
				Data.GitGenerationSettings = new GitGenerationSettings();
				settingsNeedSaving = true;
			}

			//Rebuild from previous settings, in case a template name has changed, or new templates were added.
			List<TemplateGenerationData> previousSettings = null;
			if (Data.GitGenerationSettings.TemplateSettings != null && Data.GitGenerationSettings.TemplateSettings.Count > 0)
			{
				previousSettings = Data.GitGenerationSettings.TemplateSettings.ToList();
			}

			ObservableCollection<TemplateGenerationData> templateSettings = new ObservableCollection<TemplateGenerationData>();
			foreach (var template in Data.Templates.Where(t => t.Name != "LICENSE"))
			{
				TemplateGenerationData tdata = new TemplateGenerationData()
				{
					ID = template.ID,
					TemplateName = template.Name,
					Enabled = true,
					TooltipText = template.TooltipText
				};

				if (previousSettings != null)
				{
					var previousData = previousSettings.Where(s => s.ID == template.ID).FirstOrDefault();
					if (previousData != null)
					{
						tdata.Enabled = previousData.Enabled;
						settingsNeedSaving = true;
					}
				}

				templateSettings.Add(tdata);
			}

			Data.GitGenerationSettings.TemplateSettings = templateSettings;

			if (settingsNeedSaving) FileCommands.Save.SaveGitGenerationSettings(DefaultPaths.GitGenSettings);
		}

		public void LoadAll()
		{
			LoadAppSettings();
			LoadDirectoryLayout();
			LoadTemplates();
			LoadUserKeywords();
			LoadModProjects();
			LoadGitProjects();
			LoadManagedProjects();
			LoadAvailableProjects();
			LoadGitGenerationSettings();
		}
	}
}
