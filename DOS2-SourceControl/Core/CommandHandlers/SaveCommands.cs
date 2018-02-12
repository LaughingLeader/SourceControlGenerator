using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using LL.DOS2.SourceControl.Data;
using LL.DOS2.SourceControl.Data.View;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace LL.DOS2.SourceControl.Commands
{
	public class SaveCommands
	{
		private MainAppData Data { get; set; }

		public void SetData(MainAppData data)
		{
			Data = data;
		}

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
					if (String.IsNullOrEmpty(DefaultFilePath) || (!String.IsNullOrEmpty(DefaultFilePath) && FilePath != DefaultFilePath))
					{
						//Override the file name with the incoming path, unless that file name matches a default file path.
						//This is to suggest to the user to make a new file instead of overwriting application defaults.

						fileDialog.FileName = Path.GetFileName(FilePath);
					}
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

		public void OpenDialog(Window ParentWindow, string Title, string FilePath, string FileContent, Action<string> SaveAction)
		{
			SaveFileDialog fileDialog = new SaveFileDialog();
			fileDialog.Title = Title;
			fileDialog.InitialDirectory = Directory.GetParent(FilePath).FullName;
			fileDialog.FileName = Path.GetFileName(FilePath);
			fileDialog.OverwritePrompt = true;

			Nullable<bool> result = fileDialog.ShowDialog(ParentWindow);
			if (result == true)
			{
				SaveAction?.Invoke(fileDialog.FileName);
			}
		}

		public void SaveAppSettings()
		{
			Log.Here().Activity("Saving AppSettings to {0}", Path.GetFullPath(DefaultPaths.AppSettings));

			if (Data.AppSettings != null)
			{
				string json = JsonConvert.SerializeObject(Data.AppSettings, Newtonsoft.Json.Formatting.Indented);
				FileCommands.WriteToFile(DefaultPaths.AppSettings, json);
			}
		}

		public bool SaveManagedProjects()
		{
			Log.Here().Important("Saving Managed Projects data to {0}", Data.AppSettings.ProjectsAppData);

			if (Data.AppProjects != null && Data.AppProjects.ManagedProjects.Count > 0 && Data.AppSettings != null && FileCommands.IsValidPath(Data.AppSettings.ProjectsAppData))
			{
				string json = JsonConvert.SerializeObject(Data.AppProjects, Newtonsoft.Json.Formatting.Indented);
				return FileCommands.WriteToFile(Data.AppSettings.ProjectsAppData, json);
			}

			return false;
		}

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

		public void SaveUserKeywords()
		{
			if (Data.UserKeywords != null)
			{
				if (Data.AppSettings != null && !String.IsNullOrEmpty(Data.AppSettings.KeywordsFile))
				{
					Log.Here().Important("Serializing and saving user keywords data.");
					try
					{
						string json = JsonConvert.SerializeObject(Data.UserKeywords, Newtonsoft.Json.Formatting.Indented);
						FileCommands.WriteToFile(Data.AppSettings.KeywordsFile, json);
					}
					catch (Exception ex)
					{
						Log.Here().Error("Error serializing Keywords.json: {0}", ex.ToString());
					}
				}
			}
		}

		public void SaveGitGenerationSettings()
		{
			if (Data != null && Data.AppSettings != null)
			{
				SaveGitGenerationSettings(Data.AppSettings.GitGenSettingsFile);
			}
		}

		public void SaveGitGenerationSettings(string filePath)
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
	}
}
