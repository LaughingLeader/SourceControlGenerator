using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using LL.DOS2.SourceControl.Data.Command;
using Newtonsoft.Json;

namespace LL.DOS2.SourceControl.Commands
{
	public class SaveFileCommand : SaveFileAsCommand
	{
		public Action<bool> OnSave { get; set; }

		public bool OpenSaveAsOnDefault { get; set; }

		override public void Execute(object parameter)
		{
			if (parameter != null && parameter is SaveFileCommandData data)
			{
				if(OpenSaveAsOnDefault && data.DefaultFilePath == data.FilePath)
				{
					FileCommands.Save.OpenDialogAndSave(App.Current.MainWindow, data.DialogTitle, data.FilePath, data.Content, this.OnSaveAs, data.FileName, data.DefaultFilePath);
				}
				else
				{
					bool success = FileCommands.WriteToFile(data.FilePath, data.Content);
					OnSave?.Invoke(success);
				}
			}
		}

		public SaveFileCommand(Action<bool> onSaveCallback, Action<bool, string> onSaveAsCallback) : base(onSaveAsCallback)
		{
			OnSave = onSaveCallback;
			OpenSaveAsOnDefault = false;
		}
	}
}
