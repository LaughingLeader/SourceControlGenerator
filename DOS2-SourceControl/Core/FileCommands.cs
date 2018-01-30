using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ookii.Dialogs.Wpf;
using LL.DOS2.SourceControl.Data;
using LL.DOS2.SourceControl.Core.Commands;
using System.Windows;

namespace LL.DOS2.SourceControl.Core
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

				Log.Here().Activity("Created file: {0}", filePath);
				return true;
			}
			catch (Exception e)
			{
				Log.Here().Error("Error creating file at {0} - {1}", filePath, e.ToString());
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

		public static bool IsPathValid(String pathString)
		{
			Uri pathUri;
			Boolean isValidUri = Uri.TryCreate(pathString, UriKind.Absolute, out pathUri);
			return isValidUri && pathUri != null && pathUri.IsLoopback;
		}

		public static void Init(MainAppData AppData)
		{
			loadCommands = new LoadCommands(AppData);
			saveCommands = new SaveCommands(AppData);
		}
	}
}
