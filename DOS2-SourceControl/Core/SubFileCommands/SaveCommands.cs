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

namespace LL.DOS2.SourceControl.Core.Commands
{
	public class SaveCommands
	{
		private MainAppData Data { get; set; }

		public void SetData(MainAppData data)
		{
			Data = data;
		}

		public void OpenDialogAndSave(Window ParentWindow, string Title, string FilePath, string FileContent, Action<bool, string> OnSave = null)
		{
			SaveFileDialog fileDialog = new SaveFileDialog();
			fileDialog.Title = Title;
			fileDialog.InitialDirectory = Directory.GetParent(FilePath).FullName;
			fileDialog.FileName = Path.GetFileName(FilePath);
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
						string json = JsonConvert.SerializeObject(Data.UserKeywords);
						FileCommands.WriteToFile(Data.AppSettings.KeywordsFile, json);
					}
					catch (Exception ex)
					{
						Log.Here().Error("Error serializing Keywords.json: {0}", ex.ToString());
					}
				}
			}
		}
	}
}
