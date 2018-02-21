using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using LL.SCG.Data.Command;
using Newtonsoft.Json;

namespace LL.SCG.Commands
{
	public class SaveFileAsCommandDataConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (values != null)
			{
				var commandData = new SaveFileCommandData();

				if (values.Length >= 0) commandData.Content = values[0]?.ToString();
				if (values.Length > 1) commandData.DialogTitle = values[1]?.ToString();
				if (values.Length >= 2) commandData.FileName = values[2]?.ToString();
				if (values.Length >= 3) commandData.FilePath = values[3]?.ToString();
				if (values.Length >= 4) commandData.DefaultFilePath = values[4]?.ToString();

				return commandData;
			}
			
			return null;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class SaveFileAsCommand : ICommand
	{
		public Action<bool, string> OnSaveAs { get; set; }

		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		public bool CanExecute(object parameter)
		{
			if(parameter != null && FileCommands.Save != null)
			{
				if(parameter is SaveFileCommandData data)
				{
					//return FileCommands.IsValidPath(data.FilePath);
					return true;
				}
			}
			return false;
		}

		public virtual void Execute(object parameter)
		{
			if (parameter != null && parameter is SaveFileCommandData data)
			{
				Log.Here().Important("Attempting to save file: {0}", data.FilePath);
				if (String.IsNullOrEmpty(data.DialogTitle)) data.DialogTitle = "Save File As";
				if (data.Content != null)
				{
					FileCommands.Save.OpenDialogAndSave(App.Current.MainWindow, data.DialogTitle, data.FilePath, data.Content, OnSaveAs, data.FileName, data.DefaultFilePath);
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
