using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using Newtonsoft.Json;

namespace LL.DOS2.SourceControl.Core.Commands
{
	public class SaveFileAsCommandDataConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return new SaveFileAsCommandData()
			{
				FilePath = values[0].ToString(),
				Content = values[1].ToString(),
				DialogTitle = values[2].ToString()
			};
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class SaveFileAsCommandData
	{
		public string FilePath { get; set; }
		public string Content { get; set; }
		public string DialogTitle { get; set; }
	}

	public class SaveFileAsCommand : ICommand
	{
		private Action<bool, string> OnSave;

		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		public bool CanExecute(object parameter)
		{
			if(parameter != null && FileCommands.Save != null)
			{
				SaveFileAsCommandData data = (SaveFileAsCommandData)parameter;
				if(data != null)
				{
					return FileCommands.IsValidPath(data.FilePath);
				}
			}
			return false;
		}

		public void Execute(object parameter)
		{
			SaveFileAsCommandData data = (SaveFileAsCommandData)parameter;

			if (data != null)
			{
				Log.Here().Important("Attempting to save file: {0}", data.FilePath);
				if (String.IsNullOrEmpty(data.DialogTitle)) data.DialogTitle = "Save File As";
				if (!String.IsNullOrEmpty(data.Content))
				{
					FileCommands.Save.OpenDialogAndSave(App.Current.MainWindow, data.DialogTitle, data.FilePath, data.Content, OnSave);
				}
			}
		}

		public SaveFileAsCommand() { }
		public SaveFileAsCommand(Action<bool, string> onSaveCallback)
		{
			OnSave = onSaveCallback;
		}
	}
}
