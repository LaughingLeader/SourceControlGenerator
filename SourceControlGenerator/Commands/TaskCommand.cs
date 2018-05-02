using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LL.SCG.Commands
{
	public class TaskCommand : BaseCommand
	{
		public Action<bool> TaskAction { get; set; }

		public Window ParentWindow { get; set; }

		public string TaskTitle { get; set; }

		public string TaskInstructions { get; set; }

		public string TaskContent { get; set; }

		public bool TaskOpen { get; private set; } = false;

		public override bool CanExecute(object parameter)
		{
			return !TaskOpen;
		}

		public override void Execute(object parameter)
		{
			TaskOpen = true;
			FileCommands.OpenConfirmationDialog(ParentWindow, TaskTitle, TaskInstructions, TaskContent, OnTaskDone);
			RaiseCanExecuteChanged();
		}

		private void OnTaskDone(bool param)
		{
			TaskOpen = false;
			TaskAction?.Invoke(param);
			RaiseCanExecuteChanged();
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
