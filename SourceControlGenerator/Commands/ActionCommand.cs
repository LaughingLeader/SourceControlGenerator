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
	public class ActionCommand : BaseCommand
	{
		private Action callback;

		public ActionCommand(Action callback)
		{
			this.callback = callback;
		}

		public ActionCommand() { }

		public void SetCallback(Action newCallback)
		{
			callback = newCallback;
			RaiseCanExecuteChanged();
		}

		public override bool CanExecute(object parameter)
		{
			return callback != null;
		}

		public override void Execute(object parameter)
		{
			this.callback?.Invoke();
		}
	}
}
