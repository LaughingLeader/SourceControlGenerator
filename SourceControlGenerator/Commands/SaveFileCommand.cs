using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using Newtonsoft.Json;

namespace SCG.Commands
{
	public class SaveFileCommand : SaveFileAsCommand
	{
		public Action<bool> OnSave { get; set; }

		public bool OpenSaveAsOnDefault { get; set; }

		public override void ExecuteSave(object parameter)
		{
			if (parameter != null && parameter is ISaveCommandData data)
			{
				if(OpenSaveAsOnDefault && data.DefaultFilePath == data.FilePath)
				{
					FileCommands.Save.OpenDialogAndSave(App.Current.MainWindow, data.SaveAsText, data.FilePath, data.Content, this.OnSaveAs, data.Filename, data.DefaultFilePath);
				}
				else
				{
					bool success = FileCommands.WriteToFile(data.FilePath, data.Content);
					OnSave?.Invoke(success);
				}
			}
			else
			{
				Log.Here().Error("Parameter is not SaveFileCommandData!");
			}
		}

		public SaveFileCommand(Action<bool> onSaveCallback, Action<bool, string> onSaveAsCallback) : base(onSaveAsCallback)
		{
			OnSave = onSaveCallback;
			OpenSaveAsOnDefault = false;
		}
	}
}
