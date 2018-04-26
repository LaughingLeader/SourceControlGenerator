using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LL.SCG.Commands
{
	/// <summary>
	/// Executes an action, ignoring the parameter.
	/// </summary>
	public class ActionCommand : ICommand
	{
		private Action callback;

		public event EventHandler CanExecuteChanged;

		public ActionCommand(Action callback)
		{
			this.callback = callback;
		}

		public ActionCommand() { }

		public void SetCallback(Action newCallback)
		{
			callback = newCallback;
		}

		public bool CanExecute(object parameter)
		{
			return true;
		}

		public void Execute(object parameter)
		{
			this.callback?.Invoke();
		}
	}
}
