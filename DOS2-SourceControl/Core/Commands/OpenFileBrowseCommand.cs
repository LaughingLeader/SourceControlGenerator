using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LL.DOS2.SourceControl.Core.Commands
{
	public class OpenFileBrowseCommand : ICommand
	{
		public event EventHandler CanExecuteChanged;

		private Action<string> onSelected;

		public bool CanExecute(object parameter)
		{
			return true;
		}

		public void Execute(object parameter)
		{
			var values = (object[])parameter;
			string startPath = (String)values[0];
			string title = values.Count() >= 2 ? (String)values[1] : "Open File...";
			bool folderMode = values.Count() >= 3;

			if (String.IsNullOrEmpty(startPath)) startPath = Directory.GetCurrentDirectory();

			if (!folderMode)
			{
				FileCommands.Load.OpenFileDialog(App.Current.MainWindow, title, startPath, onSelected);
			}
			else
			{
				FileCommands.Load.OpenFolderDialog(App.Current.MainWindow, title, startPath, onSelected);
			}

		}

		public OpenFileBrowseCommand(Action<string> OnFileSelected)
		{
			onSelected = OnFileSelected;
		}
	}
}
