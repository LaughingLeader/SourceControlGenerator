using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using Newtonsoft.Json;

namespace SCG.Commands
{
	public class SaveFileAsCommand : BaseCommand
	{
		private Action<bool, string> onSaveAs;

		public Action<bool, string> OnSaveAs
		{
			get { return onSaveAs; }
			set
			{
				onSaveAs = value;
				RaiseCanExecuteChanged();
			}
		}

		public override bool CanExecute(object parameter)
		{
			if (parameter != null && FileCommands.Save != null)
			{
				if(parameter is ISaveCommandData data)
				{
					//return FileCommands.IsValidPath(data.FilePath);
					return true;
				}
			}
			return false;
		}

		public override void Execute(object parameter)
		{
			ExecuteSave(parameter);
		}

		public virtual void ExecuteSave(object parameter)
		{
			if (parameter != null && parameter is ISaveCommandData data)
			{
				if (data.TargetWindow == null) data.TargetWindow = App.Current.MainWindow;
				Log.Here().Important("Attempting to save file: {0}", data.FilePath);
				if (String.IsNullOrEmpty(data.SaveAsText)) data.SaveAsText = "Save File As";
				if (data.Content != null)
				{
					FileCommands.Save.OpenDialogAndSave(data.TargetWindow, data.SaveAsText, data.FilePath, data.Content, OnSaveAs, data.DefaultFileName, data.InitialDirectory, data.FileTypes);
				}
			}
		}

		public SaveFileAsCommand() { }
		public SaveFileAsCommand(Action<bool, string> onSaveCallback)
		{
			OnSaveAs = onSaveCallback;
		}
	}
}
