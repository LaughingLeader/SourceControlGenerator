using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LL.SCG.Commands
{
	public class TaskCommand : ICommand
	{
		public event EventHandler CanExecuteChanged;

		public Action<bool> TaskAction { get; set; }

		public Window ParentWindow { get; set; }

		public string TaskTitle { get; set; }

		public string TaskInstructions { get; set; }

		public string TaskContent { get; set; }

		public bool CanExecute(object parameter)
		{
			return true;
		}

		public void Execute(object parameter)
		{
			FileCommands.OpenConfirmationDialog(ParentWindow, TaskTitle, TaskInstructions, TaskContent, TaskAction);
		}

		public TaskCommand(Action<bool> taskAction, Window parentWindow = null, string taskTitle = "", string taskInstructions = "", string taskContent = "")
		{
			TaskAction = taskAction;

			TaskTitle = taskTitle;
			TaskInstructions = taskInstructions;
			TaskContent = taskContent;

			if(parentWindow == null)
			{
				ParentWindow = App.Current.MainWindow;
			}
			else
			{
				ParentWindow = parentWindow;
			}
		}
	}
}
