using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LL.SCG.Commands
{
	public class OpenFileBrowseCommand : BaseCommand
	{
		private Action<string> onSelected;

		public bool BrowserOpen { get; private set; } = false;

		public override bool CanExecute(object parameter)
		{
			return !BrowserOpen;
		}

		public override void Execute(object parameter)
		{
			var values = (object[])parameter;
			string startPath = (String)values[0];
			string title = values.Count() >= 2 ? (String)values[1] : "Open File...";
			bool folderMode = values.Count() >= 3;

			if (String.IsNullOrEmpty(startPath)) startPath = Directory.GetCurrentDirectory();

			BrowserOpen = true;

			if (!folderMode)
			{
				FileCommands.Load.OpenFileDialog(App.Current.MainWindow, title, startPath, OnFileSelected);
			}
			else
			{
				FileCommands.Load.OpenFolderDialog(App.Current.MainWindow, title, startPath, OnFileSelected);
			}

		}

		private void OnFileSelected(string path)
		{
			BrowserOpen = false;
			onSelected?.Invoke(path);
		}

		public OpenFileBrowseCommand(Action<string> OnFileSelected)
		{
			onSelected = OnFileSelected;
		}
	}
}
