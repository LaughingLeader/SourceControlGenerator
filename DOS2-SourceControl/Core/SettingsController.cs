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
using LL.DOS2.SourceControl.Windows;

namespace LL.DOS2.SourceControl.Core
{
   public class SettingsController
	{

		public MainAppData Data { get; set; }

		private MainWindow mainWindow;

		public bool GenerateGitFiles(ModProjectData project, GitGenerationSettings generationSettings)
		{
			if(!string.IsNullOrEmpty(Data.AppSettings.GitRootDirectory))
			{
				string gitProjectRootDirectory = Path.Combine(Data.AppSettings.GitRootDirectory, project.Name);

				var rootRepoDirectory = Directory.CreateDirectory(gitProjectRootDirectory);

				foreach (var templateSetting in generationSettings.TemplateSettings)
				{
					var templateData = Data.Templates.Where(t => t.Name == templateSetting.TemplateName).FirstOrDefault();
					if(templateData != null)
					{
						if (templateSetting.Enabled)
						{
							string outputFIlePath = Path.Combine(gitProjectRootDirectory, templateData.ExportPath);
							string outputText = GitGenerator.ReplaceKeywords(templateData.EditorText, project, Data);
							if (!FileCommands.WriteToFile(outputFIlePath, outputText))
							{
								Log.Here().Error("[{0}] Failed to create template file at {1}", project.Name, templateData.ExportPath);
							}
						}
						else
						{
							Log.Here().Activity("[{0}] Skipping {1}", project.Name, templateSetting.TemplateName);
						}
					}
				}

				if(generationSettings.SelectedLicense != LicenseType.None)
				{
					string outputText = "";
					if(generationSettings.SelectedLicense == LicenseType.Custom)
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

					if(!String.IsNullOrEmpty(outputText))
					{
						outputText = GitGenerator.ReplaceKeywords(outputText, project, Data);
					}

					string licenseFile = Path.Combine(gitProjectRootDirectory, "LICENSE");

					if (!FileCommands.WriteToFile(licenseFile, outputText))
					{
						Log.Here().Error("[{0}] Failed to write license template file at {1}", project.Name, licenseFile);
					}
				}

				if(GitGenerator.CreateJunctions(project, Data))
				{
					Log.Here().Activity("[{0}] Successfully created junctions.", project.Name);
				}
				else
				{
					Log.Here().Error("[{0}] Problem creating junctions.", project.Name);
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

		public bool GenerateBackupFolder(ModProjectData project)
		{
			string projectBackupDirectory = Path.Combine(Data.AppSettings.BackupRootDirectory, project.Name);
			try
			{
				Directory.CreateDirectory(projectBackupDirectory);
				return true;
			}
			catch(Exception ex)
			{
				MainWindow.FooterError("Error creating backup directory for {0}: {1}", project.Name, ex.Message);
			}

			return false;
		}

		public bool BackupGitProject(ModProjectData project)
		{
			if (!string.IsNullOrEmpty(Data.AppSettings.GitRootDirectory))
			{
				string gitProjectRootDirectory = Path.Combine(Data.AppSettings.GitRootDirectory, project.Name);
				if (Directory.Exists(gitProjectRootDirectory))
				{
					
				}
			}
			return false;
		}

		public void AddProjectsToManaged(List<AvailableProjectViewData> selectedItems)
		{
			bool bSaveData = false;

			foreach(var project in selectedItems)
			{
				var modData = Data.ModProjects.Where(p => p.Name == project.Name).FirstOrDefault();
				if(modData != null)
				{
					//Data.ManagedProjects.Add(new ProjectEntryData(modData.ProjectInfo, modData.ModInfo));
					Data.ManagedProjects.Add(modData);
					var availableProject = Data.AvailableProjects.Where(p => p.Name == project.Name).FirstOrDefault();
					if (availableProject != null) Data.AvailableProjects.Remove(availableProject);

					if(Data.AppProjects != null)
					{
						if(Data.AppProjects.ManagedProjects.Any(p => p.Name == modData.Name))
						{
							if(modData.ProjectAppData == null)
							{
								ProjectAppData data = Data.AppProjects.ManagedProjects.Where(p => p.Name == modData.Name && p.GUID == modData.ModuleInfo.UUID).FirstOrDefault();
								if (data != null)
								{
									modData.ProjectAppData = data;
								}
							}
						}
						else
						{
							ProjectAppData data = new ProjectAppData()
							{
								Name = modData.Name,
								GUID = modData.ModuleInfo.UUID,
								LastBackup = null
							};
							Data.AppProjects.ManagedProjects.Add(data);
							modData.ProjectAppData = data;

							bSaveData = true;
						}
						
					}
				}
			}

			if(bSaveData)
			{
				if (FileCommands.Save.SaveManagedProjects())
				{
					MainWindow.FooterLog("Saved Managed Projects data to {0}.", Data.AppSettings.ProjectsAppData);
				}
				else
				{
					MainWindow.FooterError("Error saving Managed Projects data to {0}.", Data.AppSettings.ProjectsAppData);
				}
			}
		}

		public void Start()
		{
			Log.Here().Important("Starting application.");
			FileCommands.Load.LoadAll();
		}

		public SettingsController(MainWindow MainAppWindow)
		{
			mainWindow = MainAppWindow;
			Data = new MainAppData();
			FileCommands.SetData(Data);
			Start();
		}
	}
}
