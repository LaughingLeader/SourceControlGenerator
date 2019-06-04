using System;
using System.Collections.Generic;
using Alphaleonis.Win32.Filesystem;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using SCG.Extensions;
using System.Windows;

namespace SCG.Commands
{
	public class OpenFileBrowserCommand : BaseCommand
	{
		private Action<string> onSelected;

		public bool BrowserOpen { get; private set; } = false;

		public string StartPath { get; set; }

		public string Title { get; set; }

		public bool UseFolderBrowser { get; set; } = false;

		public Window ParentWindow { get; set; }

		public override bool CanExecute(object parameter)
		{
			return !BrowserOpen;
		}

		public override void Execute(object parameter)
		{
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
				FileCommands.Load.OpenFileDialog(ParentWindow, title, startPath, OnFileSelected);
			}
			else
			{
				FileCommands.Load.OpenFolderDialog(ParentWindow, title, startPath, OnFileSelected);
			}

		}

		private void OnFileSelected(string path)
		{
			BrowserOpen = false;
			onSelected?.Invoke(path);
		}

		public OpenFileBrowserCommand(Action<string> OnFileSelected)
		{
			onSelected = OnFileSelected;

			Title = "Open File...";
			StartPath = Directory.GetCurrentDirectory();

			ParentWindow = App.Current.MainWindow;
		}
	}
}
