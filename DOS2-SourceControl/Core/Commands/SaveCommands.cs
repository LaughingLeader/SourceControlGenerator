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
using Microsoft.Win32;
using Newtonsoft.Json;

namespace LL.DOS2.SourceControl.Core.Commands
{
	public class SaveDataConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			return values.Clone();
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class SaveToFileCommand : ICommand
	{
		public event EventHandler CanExecuteChanged;

		public bool CanExecute(object parameter)
		{
			return true;
		}

		public void Execute(object parameter)
		{
			var values = (object[])parameter;
			string filePath = (String)values[0];

			if(!String.IsNullOrEmpty(filePath))
			{
				Log.Here().Important("Attempting to save file: {0}", filePath);
				if (values.Count() == 3)
				{
					string jsonMode = (String)values[2];
					if (!string.IsNullOrEmpty(jsonMode) && values[1] != null)
					{
						string json = JsonConvert.SerializeObject(values[1], Newtonsoft.Json.Formatting.Indented);
						FileCommands.WriteToFile(filePath, json);
					}
				}
				else
				{
					string fileContents = (String)values[1];
					if(fileContents != null)
					{
						FileCommands.WriteToFile(filePath, fileContents);
					}
				}
			}
		}
	}

	public class SaveCommands
	{
		private MainAppData Data { get; set; }

		public SaveToFileCommand SaveCommand { get; set; }

		public void OpenDialog(Window ParentWindow, string Title, string FilePath, string FileContent)
		{
			SaveFileDialog fileDialog = new SaveFileDialog();
			fileDialog.Title = Title;
			fileDialog.InitialDirectory = Directory.GetParent(FilePath).FullName;
			fileDialog.FileName = Path.GetFileName(FilePath);
			fileDialog.OverwritePrompt = true;

			Nullable<bool> result = fileDialog.ShowDialog(ParentWindow);
			if (result == true)
			{
				FileCommands.WriteToFile(fileDialog.FileName, FileContent);
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

		public void SaveGitIgnore()
		{
			if (Data.AppSettings != null)
			{
				Log.Here().Activity("Saving .gitignore.default to {0}", Data.AppSettings.GitIgnoreFile);

				if (FileCommands.IsPathValid(Data.AppSettings.GitIgnoreFile))
				{
					FileCommands.WriteToFile(Data.AppSettings.GitIgnoreFile, Data.DefaultGitIgnoreText);
				}
				else
				{
					Log.Here().Error("Invalid path for default .gitignore file: {0}. Using default path: {1}", Data.AppSettings.GitIgnoreFile, DefaultPaths.GitIgnore);
					FileCommands.WriteToFile(DefaultPaths.GitIgnore, Data.DefaultGitIgnoreText);
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

		public SaveCommands(MainAppData AppData)
		{
			Data = AppData;
			SaveCommand = new SaveToFileCommand();
		}
	}
}
