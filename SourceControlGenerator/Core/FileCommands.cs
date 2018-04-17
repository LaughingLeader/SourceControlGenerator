using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ookii.Dialogs.Wpf;
using LL.SCG.Data;
using LL.SCG.Commands;
using System.Windows;
using System.Windows.Input;
using System.Text.RegularExpressions;
using LL.SCG.Data.View;
using System.Windows.Media.Imaging;

namespace LL.SCG
{
	public static class FileCommands
	{
		private static LoadCommands loadCommands;
		private static SaveCommands saveCommands;

		public static LoadCommands Load => loadCommands;
		public static SaveCommands Save => saveCommands;

		public static bool WriteToFile(string filePath, string Contents)
		{
			try
			{
				Directory.CreateDirectory(Path.GetDirectoryName(filePath));

				FileInfo file = new FileInfo(filePath);
				File.WriteAllText(filePath, Contents);

				Log.Here().Activity("Saved file: {0}", filePath);
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

		public static void OpenConfirmationDialog(Window ParentWindow, string WindowTitle, string MainInstruction, string Content, Action OnConfirmed)
		{
			if(TaskDialog.OSSupportsTaskDialogs)
			{
				using (TaskDialog dialog = new TaskDialog())
				{
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
						OnConfirmed?.Invoke();
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

			Regex driveCheck = new Regex(@"^[a-zA-Z]:\\$");

			if (!driveCheck.IsMatch(path.Substring(0, 3)))
			{
				return false;
			}

			var x1 = (path.Substring(3, path.Length - 3));
			string strTheseAreInvalidFileNameChars = new string(Path.GetInvalidPathChars());
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

			return true;

			//return directoryRegex.IsMatch(path);
		}

		public static bool PathIsRelative(string path)
		{
			try
			{
				DirectoryInfo appDir = new DirectoryInfo(Directory.GetCurrentDirectory());
				FileInfo file = new FileInfo(path);

				if (file.FullName.Contains(appDir.FullName)) return true;
			}
			catch (Exception ex)
			{
				Log.Here().Error("Error in relative path check: {0}", ex.ToString());
			}

			return false;
		}

		public static bool IsValidImage(string filename)
		{
			if (String.IsNullOrWhiteSpace(filename) || !File.Exists(filename)) return false;

			return Helpers.Image.CheckImageType(filename) != Util.HelperUtil.ImageType.None;
		}

		public static void Init()
		{
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
