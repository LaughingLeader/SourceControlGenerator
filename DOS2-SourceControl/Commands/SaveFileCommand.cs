using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using Newtonsoft.Json;

namespace LL.DOS2.SourceControl.Commands
{
	public class SaveFileCommandDataConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return new SaveFileCommandData()
			{
				FilePath = values[0].ToString(),
				Content = values[1].ToString()
			};
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class SaveFileCommandData
	{
		public string FilePath { get; set; }
		public string Content { get; set; }
	}

	public class SaveFileCommand : ICommand
	{
		private Action<bool> OnSave;

		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		public bool CanExecute(object parameter)
		{
			if (parameter != null && FileCommands.Save != null && parameter is SaveFileCommandData data)
			{
				return FileCommands.IsValidPath(data.FilePath);
			}
			return false;
		}

		public void Execute(object parameter)
		{
			if (parameter != null && parameter is SaveFileCommandData data)
			{
				bool success = FileCommands.WriteToFile(data.FilePath, data.Content);
				OnSave?.Invoke(success);
			}
		}

		public SaveFileCommand() { }
		public SaveFileCommand(Action<bool> onSaveCallback)
		{
			OnSave = onSaveCallback;
		}
	}
}
