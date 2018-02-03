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

		public bool GenerateGitFiles(AvailableProjectViewData project, GitGenerationSettings generationSettings)
		{
			if(!string.IsNullOrEmpty(Data.AppSettings.GitRootDirectory))
			{
				string gitProjectRootDirectory = Path.Combine(Data.AppSettings.GitRootDirectory, project.Name);
				string sourceControlFile = Path.Combine(gitProjectRootDirectory, DefaultValues.SourceControlDataFileName);
				string readmeFile = Path.Combine(gitProjectRootDirectory, "README.md");
				string changelogFile = Path.Combine(gitProjectRootDirectory, "CHANGELOG.md");
				string licenseFile = Path.Combine(gitProjectRootDirectory, "LICENSE");

				Directory.CreateDirectory(gitProjectRootDirectory);

				SourceControlData sourceControlData = new SourceControlData();
				sourceControlData.ProjectName = project.Name;

				string json = JsonConvert.SerializeObject(sourceControlData, Newtonsoft.Json.Formatting.Indented);
				if(!FileCommands.WriteToFile(sourceControlFile, json))
				{
					Log.Here().Error("[{0}] Failed to write {1}", project.Name, sourceControlFile);
				}

				if(generationSettings.GenerateReadme)
				{
					string readmeText = GitGenerator.GenerateReadmeText(Data.AppSettings, project.Name);
					if (!FileCommands.WriteToFile(readmeFile, readmeText))
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
					if (!FileCommands.WriteToFile(changelogFile, changelogText))
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
					if (!FileCommands.WriteToFile(licenseFile, licenseText))
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
					//Data.ManagedProjects.Add(new ProjectEntryData(modData.ProjectInfo, modData.ModInfo));
					Data.ManagedProjects.Add(modData);
					var availableProject = Data.AvailableProjects.Where(p => p.Name == project.Name).FirstOrDefault();
					if (availableProject != null) Data.AvailableProjects.Remove(availableProject);
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
			FileCommands.Init(Data);
			Start();
		}
	}
}
