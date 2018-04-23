using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using LL.SCG.Data;
using LL.SCG.Data.App;
using LL.SCG.Data.View;
using LL.SCG.Interfaces;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace LL.SCG.Commands
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

		public void OpenDialogAndSave(Window ParentWindow, string Title, string FilePath, string FileContent, Action<bool, string> OnSave = null, string FileName = "", string DefaultFilePath="")
		{
			SaveFileDialog fileDialog = new SaveFileDialog();
			fileDialog.Title = Title;

			if (!String.IsNullOrEmpty(FileName)) fileDialog.FileName = FileName;

			if (!String.IsNullOrEmpty(FilePath))
			{
				FileAttributes fileAttributes = File.GetAttributes(FilePath);

				if ((fileAttributes & FileAttributes.Directory) != FileAttributes.Directory)
				{
					/*
					if (String.IsNullOrEmpty(DefaultFilePath) || (!String.IsNullOrEmpty(DefaultFilePath) && FilePath != DefaultFilePath))
					{
						//Override the file name with the incoming path, unless that file name matches a default file path.
						//This is to suggest to the user to make a new file instead of overwriting application defaults.

						fileDialog.FileName = Path.GetFileName(FilePath);
					}
					*/
					fileDialog.InitialDirectory = Directory.GetParent(FilePath).FullName;
				}
				else
				{
					fileDialog.InitialDirectory = Path.GetFullPath(FilePath);
				}
			}
			else
			{
				fileDialog.InitialDirectory = Directory.GetCurrentDirectory();
			}

			
			fileDialog.OverwritePrompt = true;

			Nullable<bool> result = fileDialog.ShowDialog(ParentWindow);
			if (result == true)
			{
				bool success = FileCommands.WriteToFile(fileDialog.FileName, FileContent);
				OnSave?.Invoke(success, fileDialog.FileName);
			}
		}

		public void OpenDialog(Window ParentWindow, string Title, string FilePath, string FileContent, Action<string> SaveAction, string FileName = "")
		{
			string filePath = FilePath;
			string fileName = FileName;
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

			Nullable<bool> result = fileDialog.ShowDialog(ParentWindow);
			if (result == true)
			{
				SaveAction?.Invoke(fileDialog.FileName);
			}
		}

		public void SaveAppSettings(IModuleData Data)
		{
			Log.Here().Activity("Saving AppSettings to {0}", Path.GetFullPath(DefaultPaths.AppSettings(Data)));

			if (Data.ModuleSettings != null)
			{
				SaveTemplates(Data);
				string json = JsonConvert.SerializeObject(Data.ModuleSettings, Newtonsoft.Json.Formatting.Indented);
				FileCommands.WriteToFile(DefaultPaths.AppSettings(Data), json);
			}
		}

		private void SaveTemplates(IModuleData Data)
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
