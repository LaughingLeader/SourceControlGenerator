using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Alphaleonis.Win32.Filesystem;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using SCG.Controls;
using SCG.Core;
using SCG.Data;
using SCG.Data.App;
using SCG.Data.View;
using SCG.Interfaces;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;

namespace SCG.Commands
{
	public class SaveCommands
	{
		/*
		private MainAppData Data { get; set; }

		public void SetData(MainAppData data)
		{
			Data = data;
		}
		*/

		public void OpenDialogAndSave(Window ParentWindow, string Title, string FilePath, string FileContent, Action<bool, string> OnSave = null, string DefaultFileName = "", string InitialDirectory="", params FileBrowserFilter[] Filters)
		{
			try
			{
				var fileDialog = new CommonSaveFileDialog();
				fileDialog.Title = Title;

				fileDialog.AlwaysAppendDefaultExtension = false;
				fileDialog.OverwritePrompt = true;

				//Log.Here().Important($"Initial directory: {InitialDirectory} | Default FileName: {DefaultFileName} | FilePath: {FilePath}");

				if (!String.IsNullOrEmpty(DefaultFileName)) fileDialog.DefaultFileName = DefaultFileName;

				//if (FileCommands.IsValidFilePath(InitialDirectory) && !FileCommands.IsValidDirectoryPath(InitialDirectory)) InitialDirectory = Directory.GetParent(InitialDirectory).FullName + @"\";

				if(!String.IsNullOrWhiteSpace(InitialDirectory) && FileCommands.IsValidDirectoryPath(InitialDirectory))
				{
					fileDialog.InitialDirectory = Path.GetFullPath(InitialDirectory);
				}
				else
				{
					if (FileCommands.IsValidPath(FilePath))
					{
						if (FileCommands.IsValidDirectoryPath(FilePath))
						{
							fileDialog.InitialDirectory = Path.GetFullPath(FilePath);
						}
						else
						{
							fileDialog.InitialDirectory = Directory.GetParent(FilePath).FullName;
						}
					}
					else
					{
						fileDialog.InitialDirectory = Directory.GetCurrentDirectory();
					}
				}

				//Log.Here().Important($"Initial directory set to {fileDialog.InitialDirectory}");

				if (Filters != null)
				{
					if (Filters.Length <= 0)
					{
						fileDialog.Filters.Add(new CommonFileDialogFilter(CommonFileFilters.All.Name, CommonFileFilters.All.Values));
					}
					else
					{
						foreach (var filter in Filters)
						{
							fileDialog.Filters.Add(new CommonFileDialogFilter(filter.Name, filter.Values));
						}
					}
				}
				else
				{
					fileDialog.Filters.Add(new CommonFileDialogFilter(CommonFileFilters.All.Name, CommonFileFilters.All.Values));
				}

				var result = fileDialog.ShowDialog(ParentWindow);

				if (result == CommonFileDialogResult.Ok)
				{
					bool success = FileCommands.WriteToFile(fileDialog.FileName, FileContent);
					OnSave?.Invoke(success, fileDialog.FileName);
				}
			}
			catch(Exception ex)
			{
				Log.Here().Error($"Error opening dialog window: {ex.ToString()}");
			}
		}

		public void OpenSaveDialog(Window ParentWindow, string Title, Action<FileDialogResult, string> OnClose, string DefaultFileName = "", string InitialDirectory = "", params FileBrowserFilter[] Filters)
		{
			try
			{
				var fileDialog = new CommonSaveFileDialog();
				fileDialog.Title = Title;

				fileDialog.AlwaysAppendDefaultExtension = false;
				fileDialog.OverwritePrompt = true;

				//Log.Here().Important($"Initial directory: {InitialDirectory} | Default FileName: {DefaultFileName} | FilePath: {FilePath}");

				if (!String.IsNullOrEmpty(DefaultFileName)) fileDialog.DefaultFileName = DefaultFileName;

				//if (FileCommands.IsValidFilePath(InitialDirectory) && !FileCommands.IsValidDirectoryPath(InitialDirectory)) InitialDirectory = Directory.GetParent(InitialDirectory).FullName + @"\";

				if (!String.IsNullOrWhiteSpace(InitialDirectory) && Directory.Exists(InitialDirectory))
				{
					fileDialog.InitialDirectory = Path.GetFullPath(InitialDirectory);
				}
				else
				{
					fileDialog.InitialDirectory = Directory.GetCurrentDirectory();
				}

				//Log.Here().Important($"Initial directory set to {fileDialog.InitialDirectory}");

				if (Filters != null)
				{
					if (Filters.Length <= 0)
					{
						fileDialog.Filters.Add(new CommonFileDialogFilter(CommonFileFilters.All.Name, CommonFileFilters.All.Values));
					}
					else
					{
						foreach (var filter in Filters)
						{
							fileDialog.Filters.Add(new CommonFileDialogFilter(filter.Name, filter.Values));
						}
					}
				}
				else
				{
					fileDialog.Filters.Add(new CommonFileDialogFilter(CommonFileFilters.All.Name, CommonFileFilters.All.Values));
				}

				var result = fileDialog.ShowDialog(ParentWindow);
				FileDialogResult fileDialogResult = FileDialogResult.Ok;
				if (result == CommonFileDialogResult.Cancel) fileDialogResult = FileDialogResult.Cancel;
				if (result == CommonFileDialogResult.None) fileDialogResult = FileDialogResult.None;

				if(result == CommonFileDialogResult.Ok)
				{
					Directory.SetCurrentDirectory(fileDialog.FileName);
				}

				OnClose.Invoke(fileDialogResult, fileDialog.FileName);
			}
			catch (Exception ex)
			{
				Log.Here().Error($"Error opening dialog window: {ex.ToString()}");
			}
		}

		public void OpenDialog_Old(Window ParentWindow, string Title, string FilePath, Action<string> SaveAction, string DefaultFileName = "", string filter = "All files (*.*)|*.*")
		{
			string filePath = FilePath;
			string fileName = DefaultFileName;
			if (String.IsNullOrWhiteSpace(filePath))
			{
				filePath = AppDomain.CurrentDomain.BaseDirectory;
			}

			if (String.IsNullOrWhiteSpace(fileName) && !String.IsNullOrWhiteSpace(FilePath))
			{
				filePath = Path.GetFileName(filePath);
			}

			SaveFileDialog fileDialog = new SaveFileDialog();
			fileDialog.Title = Title;
			fileDialog.InitialDirectory = Directory.GetParent(filePath).FullName;
			fileDialog.FileName = fileName;
			fileDialog.OverwritePrompt = true;
			fileDialog.Filter = filter;

			Nullable<bool> result = fileDialog.ShowDialog(ParentWindow);
			if (result == true)
			{
				SaveAction?.Invoke(fileDialog.FileName);
			}
		}

		public void SaveModuleSettings(IModuleData Data)
		{
			Log.Here().Activity("Saving module settings to {0}", Path.GetFullPath(DefaultPaths.ModuleSettingsFile(Data)));

			if (Data.ModuleSettings != null)
			{
				SaveTemplates(Data);
				string json = JsonConvert.SerializeObject(Data.ModuleSettings, Newtonsoft.Json.Formatting.Indented);
				FileCommands.WriteToFile(DefaultPaths.ModuleSettingsFile(Data), json);
			}
		}

		private void SaveTemplates(IModuleData Data)
		{
			if (Data.ModuleSettings != null && Data.Templates != null)
			{
				if (Data.ModuleSettings.TemplateFiles != null)
				{
					Data.ModuleSettings.TemplateFiles.Clear();
				}
				else
				{
					Data.ModuleSettings.TemplateFiles = new ObservableCollection<TemplateFileData>();
				}

				foreach (var templateData in Data.Templates)
				{
					Data.ModuleSettings.TemplateFiles.Add(new TemplateFileData()
					{
						ID = templateData.ID,
						FilePath = templateData.FilePath
					});
				}
			}
		}

		/*
		public void SaveGitIgnore(string content)
		{
			if (Data.AppSettings != null)
			{
				Log.Here().Activity("Saving .gitignore.default to {0}", Data.AppSettings.GitIgnoreFile);

				if (FileCommands.IsValidPath(Data.AppSettings.GitIgnoreFile))
				{
					FileCommands.WriteToFile(Data.AppSettings.GitIgnoreFile, content);
				}
				else
				{
					Log.Here().Error("Invalid path for default .gitignore file: {0}. Using default path: {1}", Data.AppSettings.GitIgnoreFile, DefaultPaths.GitIgnore);
					FileCommands.WriteToFile(DefaultPaths.GitIgnore, content);
				}
			}
		}
		*/

		public bool SaveUserKeywords(IModuleData Data)
		{
			if (Data.UserKeywords != null)
			{
				if (Data.ModuleSettings != null && !String.IsNullOrEmpty(Data.ModuleSettings.UserKeywordsFile))
				{
					Log.Here().Important("Serializing and saving user keywords data.");
					try
					{
						string json = JsonConvert.SerializeObject(Data.UserKeywords, Newtonsoft.Json.Formatting.Indented);
						return FileCommands.WriteToFile(Data.ModuleSettings.UserKeywordsFile, json);
					}
					catch (Exception ex)
					{
						Log.Here().Error("Error serializing Keywords.json: {0}", ex.ToString());
					}
				}
			}

			return false;
		}

		public void SaveGitGenerationSettings(IModuleData Data)
		{
			if (Data != null && Data.ModuleSettings != null)
			{
				SaveGitGenerationSettings(Data, Data.ModuleSettings.GitGenSettingsFile);
			}
		}

		public void SaveGitGenerationSettings(IModuleData Data, string filePath)
		{
			if (Data.GitGenerationSettings != null)
			{
				if (!String.IsNullOrEmpty(filePath))
				{
					Log.Here().Important("Serializing and saving git generation settings data.");
					try
					{
						string json = JsonConvert.SerializeObject(Data.GitGenerationSettings, Newtonsoft.Json.Formatting.Indented);
						FileCommands.WriteToFile(filePath, json);
					}
					catch (Exception ex)
					{
						Log.Here().Error("Error serializing {0}: {1}", filePath, ex.ToString());
					}
				}
			}
		}

		public bool SaveSourceControlData(IProjectData data, string folderPath)
		{
			SourceControlData sourceControlData = new SourceControlData()
			{
				ProjectName = data.ProjectName,
				ProjectUUID = data.UUID
			};
			return SaveSourceControlData(sourceControlData, folderPath);
		}

		public bool SaveSourceControlData(SourceControlData data, string folderPath)
		{
			if (!String.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath))
			{
				Log.Here().Important("Serializing and saving source control data.");
				var filePath = Path.Combine(folderPath, DefaultPaths.SourceControlGeneratorDataFile);
				try
				{
					string json = JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);
					return FileCommands.WriteToFile(filePath, json);
				}
				catch (Exception ex)
				{
					Log.Here().Error("Error serializing source control data at {0}: {1}", filePath, ex.ToString());
				}
			}
			return false;
		}
	}
}
