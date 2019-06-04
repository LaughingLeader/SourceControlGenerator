using System;
using System.Collections.Generic;
using Alphaleonis.Win32.Filesystem;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using SCG.Extensions;
using System.Windows;
using SCG.Core;

namespace SCG.Commands
{
	public class OpenFileBrowserCommand : BaseCommand
	{
		public Action<string> SingleCallback { get; set; }

		public Action<IEnumerable<string>> MultiCallback { get; set; }

		public bool BrowserOpen { get; private set; } = false;

		public string StartPath { get; set; }

		public string Title { get; set; }

		public bool UseFolderBrowser { get; set; } = false;

		public bool AllowMultipleFiles { get; set; } = false;

		public FileBrowserFilter[] Filters { get; set; }

		public Window ParentWindow { get; set; }

		public override bool CanExecute(object parameter)
		{
			return !BrowserOpen;
		}

		public override void Execute(object parameter)
		{
			if (String.IsNullOrWhiteSpace(Title)) Title = "Open File...";
			if (String.IsNullOrWhiteSpace(StartPath)) StartPath = Directory.GetCurrentDirectory();
			if (ParentWindow == null) ParentWindow = App.Current.MainWindow;

			var values = (object[])parameter;

			string startPath = FileCommands.IsValidFilePath(StartPath) ? StartPath : Directory.GetCurrentDirectory();

			if(values.ElementAtOrDefault(0) is string path && FileCommands.IsValidFilePath(path))
			{
				startPath = path;
			}

			string title = Title;

			if (values.ElementAtOrDefault(1) is string paramTitle)
			{
				title = paramTitle;
			}

			bool useFolderBrowser = UseFolderBrowser;

			if (values.ElementAtOrDefault(2) is int useFolderMode)
			{
				useFolderBrowser = useFolderMode >= 1;
			}

			BrowserOpen = true;

			if (!useFolderBrowser)
			{
				if(!AllowMultipleFiles || (AllowMultipleFiles && MultiCallback == null))
				{
					FileCommands.Load.OpenFileDialog(ParentWindow, title, startPath, OnFileSelected, Filters);
				}
				else
				{
					FileCommands.Load.OpenMultiFileDialog(ParentWindow, title, startPath, OnMultipleFilesSelected, Filters);
				}
			}
			else
			{
				FileCommands.Load.OpenFolderDialog(ParentWindow, title, startPath, OnFileSelected);
			}

		}

		private void OnMultipleFilesSelected(IEnumerable<string> files)
		{
			BrowserOpen = false;
			MultiCallback?.Invoke(files);
		}

		private void OnFileSelected(string path)
		{
			BrowserOpen = false;
			SingleCallback?.Invoke(path);
		}

		public OpenFileBrowserCommand() { }

		public OpenFileBrowserCommand(Action<string> callback)
		{
			SingleCallback = callback;
			AllowMultipleFiles = false;
		}

		public OpenFileBrowserCommand(Action<IEnumerable<string>> callback)
		{
			MultiCallback = callback;
			AllowMultipleFiles = true;
		}
	}

	public interface IEnumberable<T>
	{
	}
}
