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
	public struct OpenFileBrowserParams
	{
		public string StartPath { get; set; }
		public string Title { get; set; }
		public bool UseFolderBrowser { get; set; }
		public FileBrowserFilter[] Filters { get; set; }
		public Window ParentWindow { get; set; }

		public static OpenFileBrowserParams Default()
		{
			return new OpenFileBrowserParams()
			{
				StartPath = Directory.GetCurrentDirectory(),
				Title = "Open File...",
				ParentWindow = App.Current.MainWindow,
				UseFolderBrowser = false,
				Filters = null
			};
		}

		public OpenFileBrowserParams Copy()
		{
			return new OpenFileBrowserParams()
			{
				StartPath = this.StartPath,
				Title = this.Title,
				ParentWindow = this.ParentWindow,
				UseFolderBrowser = this.UseFolderBrowser,
				Filters = this.Filters
			};
		}
	}

	public class OpenFileBrowserCommand : BaseCommand
	{
		public Action<string> SingleCallback { get; set; }

		public Action<IEnumerable<string>> MultiCallback { get; set; }

		public bool BrowserOpen { get; private set; } = false;

		public OpenFileBrowserParams? DefaultParams { get; set; }

		public override bool CanExecute(object parameter)
		{
			return !BrowserOpen && base.CanExecute(parameter);
		}

		public OpenFileBrowserParams LoadParams(object parameter)
		{
			OpenFileBrowserParams useBrowserParams = DefaultParams != null ? DefaultParams.Value.Copy() : OpenFileBrowserParams.Default();

			if(parameter is  OpenFileBrowserParams browseParams)
			{
				if (FileCommands.IsValidFilePath(browseParams.StartPath)) useBrowserParams.StartPath = browseParams.StartPath;
				if (!string.IsNullOrEmpty(browseParams.Title)) useBrowserParams.Title = browseParams.Title;
				if (browseParams.ParentWindow != null && browseParams.ParentWindow.IsVisible) useBrowserParams.ParentWindow = browseParams.ParentWindow;
				useBrowserParams.UseFolderBrowser = browseParams.UseFolderBrowser;
				if (browseParams.Filters != null) useBrowserParams.Filters = browseParams.Filters;
			}
			
			return useBrowserParams;
		}

		public override void Execute(object parameter)
		{
			OpenFileBrowserParams browserParams = LoadParams(parameter);

			BrowserOpen = true;

			browserParams.StartPath = Path.GetFileName(Path.GetDirectoryName(browserParams.StartPath)) + @"\";

			if (!browserParams.UseFolderBrowser)
			{
				if(MultiCallback == null)
				{
					FileCommands.Load.OpenFileDialog(browserParams.ParentWindow, browserParams.Title, browserParams.StartPath, OnFileSelected, browserParams.Filters);
				}
				else
				{
					FileCommands.Load.OpenMultiFileDialog(browserParams.ParentWindow, browserParams.Title, browserParams.StartPath, OnMultipleFilesSelected, browserParams.Filters);
				}
			}
			else
			{
				FileCommands.Load.OpenFolderDialog(browserParams.ParentWindow, browserParams.Title, browserParams.StartPath, OnFileSelected);
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
		}

		public OpenFileBrowserCommand(Action<IEnumerable<string>> callback)
		{
			MultiCallback = callback;
		}
	}

	public interface IEnumberable<T>
	{
	}
}
