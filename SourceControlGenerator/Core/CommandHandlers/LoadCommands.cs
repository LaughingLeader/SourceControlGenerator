using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using LL.SCG.Controls;
using LL.SCG.Data;
using LL.SCG.Data.View;
using LL.SCG.Interfaces;
using LL.SCG.Windows;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;


namespace LL.SCG.Commands
{
	public class LoadCommands
	{
		/*
		private MainAppData Data { get; set; }

		public void SetData(MainAppData data)
		{
			//Data = data;
		}
		*/

		private bool SaveAppSettings = false;

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

		/*
		public void OpenOokiiFolderDialog(Window ParentWindow, string Title, string FilePath, Action<string> OnFolderSelected)
		{
			VistaFolderBrowserDialog folderDialog = new VistaFolderBrowserDialog();
			folderDialog.SelectedPath = Path.GetFullPath(FilePath);
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
		*/

		public void OpenFolderDialog(Window ParentWindow, string Title, string FilePath, Action<string> OnFolderSelected, bool RetainRelativity = true)
		{
			var openFolder = new CommonOpenFileDialog();
			openFolder.AllowNonFileSystemItems = true;
			openFolder.Multiselect = false;
			openFolder.IsFolderPicker = true;
			openFolder.Title = Title;
			openFolder.DefaultFileName = "";
			openFolder.InitialDirectory = Path.GetFullPath(FilePath);
			openFolder.DefaultDirectory = Path.GetFullPath(Assembly.GetExecutingAssembly().Location);

			var result = openFolder.ShowDialog(ParentWindow);

			if (result == CommonFileDialogResult.Ok)
			{
				string path = Path.GetFullPath(openFolder.FileNames.First());
				if (RetainRelativity && FileCommands.PathIsRelative(path))
				{
					path = path.Replace(Directory.GetCurrentDirectory(), "");
				}

				OnFolderSelected?.Invoke(path);
			}
		}

		public void SelectFoldersDialog(Window ParentWindow, string Title, string FilePath, Action<List<string>> OnFoldersSelected)
		{
			var openFolder = new CommonOpenFileDialog();
			openFolder.AllowNonFileSystemItems = true;
			openFolder.Multiselect = true;
			openFolder.IsFolderPicker = true;
			openFolder.Title = Title;
			openFolder.DefaultFileName = "";
			openFolder.InitialDirectory = Path.GetFullPath(FilePath);
			openFolder.DefaultDirectory = Path.GetFullPath(Assembly.GetExecutingAssembly().Location);

			var result = openFolder.ShowDialog(ParentWindow);

			if (result == CommonFileDialogResult.Ok)
			{
				var folders = openFolder.FileNames.ToList();
				for (int i = 0; i < folders.Count; i++)
				{
					var path = folders[i];
					if (FileCommands.PathIsRelative(path))
					{
						path = path.Replace(Directory.GetCurrentDirectory(), "");
					}
				}

				OnFoldersSelected?.Invoke(folders);
			}
		}

		public void LoadAppSettings(IModuleData Data)
		{
			if (File.Exists(DefaultPaths.AppSettings(Data)))
			{
				Log.Here().Activity("Loading settings from {0}", DefaultPaths.AppSettings(Data));
				Data.LoadSettings();
			}
			else
			{
				Log.Here().Warning("settings file at {0} not found. Creating new file.", DefaultPaths.AppSettings(Data));
				Data.InitializeSettings();
				SaveAppSettings = true;
			}
		}

		public void LoadTemplates(IModuleData Data)
		{
			string templateFilePath = DefaultPaths.TemplateSettings(Data);
			if(File.Exists(Data.ModuleSettings.TemplateSettingsFile))
			{
				templateFilePath = Data.ModuleSettings.TemplateSettingsFile;
			}
			else if(!File.Exists(templateFilePath))
			{
				FileCommands.WriteToFile(templateFilePath, Properties.Resources.Templates);
			}

			XDocument templateXml = null;
			try
			{
				templateXml = XDocument.Load(templateFilePath);

				foreach(var template in templateXml.Descendants("Template"))
				{
					TemplateEditorData templateData = TemplateEditorData.LoadFromXml(Data, template);
					Data.Templates.Add(templateData);
				}
			}
			catch (Exception ex)
			{
				Log.Here().Error(Message: $"Error loading template file {templateFilePath}: {ex.ToString()}");
			}

			if(Data.ModuleSettings.TemplateFiles != null && Data.ModuleSettings.TemplateFiles.Count > 0)
			{
				foreach(var templateFile in Data.ModuleSettings.TemplateFiles)
				{
					var data = Data.Templates.FirstOrDefault(t => t.ID == templateFile.ID);
					if (data != null)
					{
						data.FilePath = templateFile.FilePath;

						
					}
				}
			}

			for (int i = 0; i < Data.Templates.Count; i++)
			{
				var template = Data.Templates[i];
				template.Init(Data);
			}
		}

		public void LoadUserKeywords(IModuleData Data)
		{
			if (Data != null && Data.ModuleSettings != null)
			{
				if(File.Exists(Data.ModuleSettings.UserKeywordsFile))
				{
					LoadUserKeywords(Data, Data.ModuleSettings.UserKeywordsFile);
				}
				else
				{
					if(File.Exists(DefaultPaths.Keywords(Data)))
					{
						LoadUserKeywords(Data, DefaultPaths.Keywords(Data));
					}
					else
					{
						if (Data.UserKeywords == null)
						{
							Data.UserKeywords = new UserKeywordData();
							Data.UserKeywords.ResetToDefault();

							FileCommands.Save.SaveUserKeywords(Data);
						}
					}
				}
			}
		}

		public void LoadUserKeywords(IModuleData Data, string filePath)
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

		public void LoadGitGenerationSettings(IModuleData Data)
		{
			string filePath = DefaultPaths.GitGenSettings(Data);
			if (Data != null && Data.ModuleSettings != null && File.Exists(Data.ModuleSettings.GitGenSettingsFile))
			{
				filePath = Data.ModuleSettings.GitGenSettingsFile;
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
			foreach (var template in Data.Templates.Where(t => t.ID.ToLower() != "license"))
			{
				TemplateGenerationData tdata = new TemplateGenerationData()
				{
					ID = template.ID,
					TemplateName = template.Name,
					Enabled = true,
					TooltipText = template.ToolTipText
				};

				if (previousSettings != null)
				{
					var previousData = previousSettings.FirstOrDefault(s => s.ID == template.ID);
					if (previousData != null)
					{
						tdata.Enabled = previousData.Enabled;
						settingsNeedSaving = true;
					}
				}

				templateSettings.Add(tdata);
			}

			Data.GitGenerationSettings.TemplateSettings = templateSettings;

			if (settingsNeedSaving) FileCommands.Save.SaveGitGenerationSettings(Data, DefaultPaths.GitGenSettings(Data));
		}

		public SourceControlData LoadSourceControlData(string filePath)
		{
			if (!String.IsNullOrEmpty(filePath) && File.Exists(filePath))
			{
				Log.Here().Important("Deserializing source control data.");

				try
				{
					SourceControlData data = JsonConvert.DeserializeObject<SourceControlData>(File.ReadAllText(filePath));
					data.RepositoryPath = Directory.GetDirectoryRoot(filePath);
					return data;
				}
				catch (Exception ex)
				{
					Log.Here().Error("Error deserializing {0}: {1}", filePath, ex.ToString());
				}
			}
			return null;
		}

		public void LoadAll(IModuleData Data)
		{
			LoadAppSettings(Data);
			LoadTemplates(Data);
			LoadUserKeywords(Data);
			LoadGitGenerationSettings(Data);

			if(SaveAppSettings)
			{
				FileCommands.Save.SaveAppSettings(Data);
				SaveAppSettings = false;
			}
		}
	}
}
