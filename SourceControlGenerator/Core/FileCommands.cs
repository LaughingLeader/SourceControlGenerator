using System;
using System.Collections.Generic;
using Alphaleonis.Win32.Filesystem;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SCG.Data;
using SCG.Commands;
using System.Windows;
using System.Windows.Input;
using System.Text.RegularExpressions;
using SCG.Data.View;
using System.Windows.Media.Imaging;
using Ookii.Dialogs.Wpf;
using SCG.Interfaces;
using SCG.Core;
using System.Diagnostics;

namespace SCG
{
	public static class FileCommands
	{
		private static LoadCommands loadCommands;
		private static SaveCommands saveCommands;

		public static LoadCommands Load => loadCommands;
		public static SaveCommands Save => saveCommands;

		public static string AppDirectory = "";

		public static string EnsureExtension(string filePath, string extension)
		{
			if(!FileExtensionFound(filePath, extension))
			{
				return Path.ChangeExtension(filePath, extension);
			}
			return filePath;
		}
		public static string ReadFile(string filePath)
		{
			var contents = "";
			try
			{
				if (File.Exists(filePath))
				{
					contents = File.ReadAllText(filePath);
				}
				else
				{
					Log.Here().Warning("File \"{0}\" does not exist.", filePath);
				}
			}
			catch (Exception e)
			{
				Log.Here().Error("Error reading file at {0} - {1}", filePath, e.ToString());
			}
			return contents;
		}

		public static bool RenameFile(string filePath, string nextFilePath)
		{
			try
			{
				if (File.Exists(filePath))
				{
					var result = File.Copy(filePath, nextFilePath, CopyOptions.FailIfExists);
					if(!result.IsCanceled && result.IsFile)
					{
						Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(filePath, 
							Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
						return true;
					}
				}
				else
				{
					Log.Here().Warning("File \"{0}\" does not exist.", filePath);
				}
			}
			catch (Exception e)
			{
				Log.Here().Error("Error reading file at {0} - {1}", filePath, e.ToString());
			}
			return false;
		}

		public static async Task<string> ReadFileAsync(string filePath)
		{
			using (System.IO.FileStream stream = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.None, 4096, true))
			{
				using (System.IO.StreamReader sr = new System.IO.StreamReader(stream))
				{
					string contents = await sr.ReadToEndAsync();
					return contents;
				}
			}
		}

		public static bool WriteToFile(string filePath, string contents, bool supressLogMessage = true)
		{
			try
			{
				Directory.CreateDirectory(Path.GetDirectoryName(filePath));

				using (System.IO.FileStream stream = new System.IO.FileStream(filePath, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.None, 4096, false))
				{
					using (System.IO.StreamWriter sw = new System.IO.StreamWriter(stream))
					{
						sw.WriteLine(contents);
						if (!supressLogMessage) Log.Here().Activity("Saved file: {0}", filePath);
					}
				}

				return true;
			}
			catch (Exception e)
			{
				Log.Here().Error("Error saving file at {0} - {1}", filePath, e.ToString());
				return false;
			}
		}

		public static async Task<bool> WriteToFileAsync(string filePath, string contents, bool supressLogMessage = true)
		{
			try
			{
				Directory.CreateDirectory(Path.GetDirectoryName(filePath));

				using (System.IO.FileStream stream = new System.IO.FileStream(filePath, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.None, 4096, true))
				{
					using (System.IO.StreamWriter sw = new System.IO.StreamWriter(stream))
					{
						await sw.WriteLineAsync(contents);
						if (!supressLogMessage) Log.Here().Activity("Saved file: {0}", filePath);
					}
				}
				
				return true;
			}
			catch (Exception e)
			{
				Log.Here().Error("Error saving file at {0} - {1}", filePath, e.ToString());
				return false;
			}
		}

		public static bool CreateFile(string filePath)
		{
			try
			{
				Directory.CreateDirectory(Path.GetDirectoryName(filePath));

				FileInfo file = new FileInfo(filePath);
				file.Create();
				Log.Here().Activity("Created file: {0}", filePath);
				return true;
			}
			catch (Exception e)
			{
				Log.Here().Error("Error creating file at {0} - {1}", filePath, e.ToString());
				return false;
			}
		}

		public static void OpenConfirmationDialog(Window ParentWindow, string WindowTitle, string MainInstruction, string Content, Action<bool> TaskAction)
		{
			if(TaskDialog.OSSupportsTaskDialogs)
			{
				using (TaskDialog dialog = new TaskDialog())
				{
					dialog.CenterParent = true;
					dialog.WindowTitle = WindowTitle;
					dialog.MainInstruction = MainInstruction;
					dialog.Content = Content;
					//dialog.ExpandedInformation = "";
					//dialog.Footer = "Task Dialogs support footers and can even include <a href=\"http://www.ookii.org\">hyperlinks</a>.";
					dialog.FooterIcon = TaskDialogIcon.Information;
					//dialog.EnableHyperlinks = true;
					TaskDialogButton okButton = new TaskDialogButton(ButtonType.Ok);
					TaskDialogButton cancelButton = new TaskDialogButton(ButtonType.Cancel);
					dialog.Buttons.Add(okButton);
					dialog.Buttons.Add(cancelButton);
					//dialog.HyperlinkClicked += new EventHandler<HyperlinkClickedEventArgs>(TaskDialog_HyperLinkClicked);
					TaskDialogButton button = dialog.ShowDialog(ParentWindow);
					if (button == okButton)
					{
						TaskAction?.Invoke(true);
					}
					else
					{
						TaskAction?.Invoke(false);
					}
				}
			}
		}

		public static bool ModuleSettingsExist
		{
			get
			{
				return AppController.Main.CurrentModule != null && AppController.Main.CurrentModule.ModuleData != null && AppController.Main.CurrentModule.ModuleData.ModuleSettings != null;
			}
		}

		public static void OpenBackupFolder(IProjectData projectData)
		{
			if (ModuleSettingsExist)
			{
				string directory = Path.Combine(Path.GetFullPath(AppController.Main.CurrentModule.ModuleData.ModuleSettings.BackupRootDirectory), projectData.ProjectName);
				if (!Directory.Exists(directory))
				{
					Directory.CreateDirectory(directory);
				}

				Process.Start(directory);
			}
		}

		public static void OpenGitFolder(IProjectData projectData)
		{
			if (ModuleSettingsExist)
			{
				string directory = Path.Combine(Path.GetFullPath(AppController.Main.CurrentModule.ModuleData.ModuleSettings.GitRootDirectory), projectData.ProjectName);
				if (Directory.Exists(directory))
				{
					Process.Start(directory);
				}
				else
				{
					Process.Start(Path.GetFullPath(AppController.Main.CurrentModule.ModuleData.ModuleSettings.GitRootDirectory));
				}
			}
		}

		/*
		public static bool IsPathValid(String pathString)
		{
			Uri pathUri;
			Boolean isValidUri = Uri.TryCreate(pathString, UriKind.Absolute, out pathUri);
			return isValidUri && pathUri != null && pathUri.IsLoopback;
		}
		*/

		public static bool IsValidPath(string path)
		{
			if (String.IsNullOrWhiteSpace(path)) return false;

			if (PathIsRelative(path)) return true;

			if (path.Length < 3) return false;

			string validLongPathPrefix = @"\\?\";

			path = path.Replace(validLongPathPrefix, "");

			Regex driveCheck = new Regex(@"^[a-zA-Z]:\\$");

			if (!driveCheck.IsMatch(path.Substring(0, 3)))
			{
				return false;
			}

			var x1 = (path.Substring(3, path.Length - 3));
			string strTheseAreInvalidFileNameChars = new string(Path.GetInvalidPathChars());;

			strTheseAreInvalidFileNameChars += @":?*";
			Regex containsABadCharacter = new Regex("[" + Regex.Escape(strTheseAreInvalidFileNameChars) + "]");

			if (containsABadCharacter.IsMatch(path.Substring(3, path.Length - 3)))
			{
				return false;
			}

			var driveLetterWithColonAndSlash = Path.GetPathRoot(path);

			if (!DriveInfo.GetDrives().Any(x => x.Name == driveLetterWithColonAndSlash))
			{
				return false;
			}

			return true;
		}

		public static bool IsValidFilePath(string path)
		{
			if (String.IsNullOrWhiteSpace(path)) return false;

			//check the path:
			if (Path.GetInvalidPathChars().Any(x => path.Contains(x)))
				return false;

			//check the filename (if one can be isolated out):
			string fileName = Path.GetFileName(path);
			if (Path.GetInvalidFileNameChars().Any(x => fileName.Contains(x)))
				return false;

			return true;
		}

		//private static Regex directoryRegex = new Regex("^([a-zA-Z]:)?(\\\\[^<>:\"/\\\\|?*]+)+\\\\?$");

		public static bool IsValidDirectoryPath(string path)
		{
			if (String.IsNullOrWhiteSpace(path)) return false;

			//check the path:
			if (Path.GetInvalidPathChars().Any(x => path.Contains(x)))
				return false;

			if(path[path.Length - 1] != '\\') return false;

			return true;

			//return directoryRegex.IsMatch(path);
		}

		public static bool PathIsRelative(string path)
		{
			Uri result;
			return Uri.TryCreate(path, UriKind.Relative, out result);
		}

		public static bool IsValidImage(string filename)
		{
			if (String.IsNullOrWhiteSpace(filename) || !File.Exists(filename)) return false;

			return Helpers.Image.CheckImageType(filename) != Util.HelperUtil.ImageType.None;
		}

		public static bool FileExtensionFound(string fPath, params string[] extensions)
		{
			if (extensions.Length > 1)
			{
				Array.Sort(extensions, StringComparer.OrdinalIgnoreCase);
				int result = Array.BinarySearch(extensions, Path.GetExtension(fPath), StringComparer.OrdinalIgnoreCase);
				//Log.Here().Activity($"Binary search: {fPath} [{string.Join(",", extensions)}] = {result}");
				return result > -1;
			}
			else if (extensions.Length == 1)
			{
				return extensions[0].Equals(Path.GetExtension(fPath), StringComparison.OrdinalIgnoreCase);
			}
			return false;
		}

		public static bool FileExists(string path)
		{
			if(PathIsRelative(path))
			{
				return File.Exists(Path.ResolveRelativePath(AppDirectory, path));
			}
			else
			{
				return File.Exists(path);
			}
		}

		public static bool DirectoryExists(string path)
		{
			if (PathIsRelative(path))
			{
				return Directory.Exists(Path.ResolveRelativePath(AppDirectory, path));
			}
			else
			{
				return Directory.Exists(path);
			}
		}

		public static void Init()
		{
			AppDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			loadCommands = new LoadCommands();
			saveCommands = new SaveCommands();
		}

		/*
		public static void SetData(MainAppData AppData)
		{
			loadCommands.SetData(AppData);
			saveCommands.SetData(AppData);
		}
		*/
	}
}
