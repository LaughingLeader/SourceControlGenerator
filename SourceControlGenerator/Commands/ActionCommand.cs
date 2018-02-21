using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LL.SCG.Commands
{
	public class ActionCommand : ICommand
	{
		private Action<object> execute;

		public event EventHandler CanExecuteChanged;

		public ActionCommand(Action<object> execute)
		{
			this.execute = execute;
		}

		public bool CanExecute(object parameter)
		{
			return true;
		}

		public void Execute(object parameter)
		{
			this.execute?.Invoke(parameter);
		}
	}
}
