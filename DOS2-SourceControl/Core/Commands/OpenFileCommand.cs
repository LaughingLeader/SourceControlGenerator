using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LL.DOS2.SourceControl.Core.Commands
{
	public class OpenFileBrowserCommand : ICommand
	{
		public event EventHandler CanExecuteChanged;

		private Action<string> onLoad;

		public bool CanExecute(object parameter)
		{
			if (parameter != null && FileCommands.Load != null)
			{
				string filePath = (String)parameter;
				return FileCommands.IsValidPath(filePath);
			}

			return false;
		}

		public void Execute(object parameter)
		{
			string filePath = (String)parameter;

			if (!String.IsNullOrEmpty(filePath) && File.Exists(filePath))
			{
				Log.Here().Important("Attempting to open file: {0}", filePath);
				try
				{
					onLoad?.Invoke(File.ReadAllText(filePath));
				}
				catch(Exception ex)
				{
					Log.Here().Error("Error opening file {0}: {1}", filePath, ex.ToString());
				}
			}
		}

		public OpenFileBrowserCommand(Action<string> OnLoad)
		{
			onLoad = OnLoad;
		}
	}
}
