using Microsoft.Win32;

using Newtonsoft.Json;

using SCG.Core;
using SCG.Data;
using SCG.Data.App;
using SCG.Interfaces;

using System.Windows;

namespace SCG.Commands;

public class SaveCommands
{
	/*
	private MainAppData Data { get; set; }

	public void SetData(MainAppData data)
	{
		Data = data;
	}
	*/

	public void OpenDialogAndSave(Window ParentWindow, string Title, string FilePath, string FileContent, Action<bool, string> OnSave = null, string DefaultFileName = "", string InitialDirectory = "", params FileBrowserFilter[] Filters)
	{
		try
		{
			var fileDialog = new SaveFileDialog
			{
				Title = Title,

				AddExtension = false,
				OverwritePrompt = true
			};

			//Log.Here().Important($"Initial directory: {InitialDirectory} | Default FileName: {DefaultFileName} | FilePath: {FilePath}");

			if (!String.IsNullOrEmpty(DefaultFileName)) fileDialog.FileName = DefaultFileName;

			//if (FileCommands.IsValidFilePath(InitialDirectory) && !FileCommands.IsValidDirectoryPath(InitialDirectory)) InitialDirectory = Directory.GetParent(InitialDirectory).FullName + @"\";

			if (!String.IsNullOrWhiteSpace(InitialDirectory) && FileCommands.IsValidDirectoryPath(InitialDirectory))
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

			if (Filters?.Length > 0)
			{
				fileDialog.Filter = string.Join("|", Filters.Select(x => $"{x.Name} ({x.Values})"));
			}
			else
			{
				fileDialog.Filter = "All files (*.*)|*.*";
			}

			var result = fileDialog.ShowDialog(ParentWindow);

			if (result == true)
			{
				var success = FileCommands.WriteToFile(fileDialog.FileName, FileContent);
				OnSave?.Invoke(success, fileDialog.FileName);
			}
		}
		catch (Exception ex)
		{
			Log.Here().Error($"Error opening dialog window: {ex.ToString()}");
		}
	}

	public void OpenSaveDialog(Window ParentWindow, string Title, Action<FileDialogResult, string> OnClose, string DefaultFileName = "", string InitialDirectory = "", params FileBrowserFilter[] Filters)
	{
		try
		{
			var fileDialog = new SaveFileDialog
			{
				Title = Title,

				AddExtension = false,
				OverwritePrompt = true
			};

			//Log.Here().Important($"Initial directory: {InitialDirectory} | Default FileName: {DefaultFileName} | FilePath: {FilePath}");

			if (!String.IsNullOrEmpty(DefaultFileName)) fileDialog.FileName = DefaultFileName;

			//if (FileCommands.IsValidFilePath(InitialDirectory) && !FileCommands.IsValidDirectoryPath(InitialDirectory)) InitialDirectory = Directory.GetParent(InitialDirectory).FullName + @"\";

			if (!String.IsNullOrWhiteSpace(InitialDirectory))
			{
				if (Directory.Exists(InitialDirectory))
				{
					fileDialog.InitialDirectory = InitialDirectory;
				}
				else
				{
					InitialDirectory = Path.GetDirectoryName(InitialDirectory);
				}
			}
			else
			{
				fileDialog.InitialDirectory = Directory.GetCurrentDirectory();
			}

			//Log.Here().Important($"Initial directory set to {fileDialog.InitialDirectory}");

			if (Filters?.Length > 0)
			{
				fileDialog.Filter = string.Join("|", Filters.Select(x => $"{x.Name} ({x.Values})"));
			}
			else
			{
				fileDialog.Filter = "All files (*.*)|*.*";
			}

			var result = fileDialog.ShowDialog(ParentWindow);
			var fileDialogResult = FileDialogResult.Ok;
			if (result == false) fileDialogResult = FileDialogResult.Cancel;
			if (result == true) fileDialogResult = FileDialogResult.None;

			//if(result == CommonFileDialogResult.Ok)
			//{
			//	Directory.SetCurrentDirectory(Path.GetDirectoryName(fileDialog.FileName));
			//}

			OnClose.Invoke(fileDialogResult, fileDialog.FileName);
		}
		catch (Exception ex)
		{
			if (ex is InvalidOperationException)
			{

			}
			else
			{
				Log.Here().Error($"Error opening dialog window: {ex.ToString()}");
			}
		}
	}

	public void OpenDialog_Old(Window ParentWindow, string Title, string FilePath, Action<string> SaveAction, string DefaultFileName = "", string filter = "All files (*.*)|*.*")
	{
		var filePath = FilePath;
		var fileName = DefaultFileName;
		if (String.IsNullOrWhiteSpace(filePath))
		{
			filePath = AppDomain.CurrentDomain.BaseDirectory;
		}

		if (String.IsNullOrWhiteSpace(fileName) && !String.IsNullOrWhiteSpace(FilePath))
		{
			filePath = Path.GetFileName(filePath);
		}

		var fileDialog = new SaveFileDialog
		{
			Title = Title,
			InitialDirectory = Directory.GetParent(filePath).FullName,
			FileName = fileName,
			OverwritePrompt = true,
			Filter = filter
		};

		var result = fileDialog.ShowDialog(ParentWindow);
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
			var json = JsonConvert.SerializeObject(Data.ModuleSettings, Newtonsoft.Json.Formatting.Indented);
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
				Data.ModuleSettings.TemplateFiles = [];
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
					var json = JsonConvert.SerializeObject(Data.UserKeywords, Newtonsoft.Json.Formatting.Indented);
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
					var json = JsonConvert.SerializeObject(Data.GitGenerationSettings, Newtonsoft.Json.Formatting.Indented);
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
		var sourceControlData = new SourceControlData()
		{
			ProjectName = data.ProjectName,
			ProjectUUID = data.UUID,
			RepositoryPath = folderPath,
			SourceFile = Path.Combine(folderPath, DefaultPaths.SourceControlGeneratorDataFile)
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
				var json = JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);
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
