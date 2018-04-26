using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LL.SCG.Commands
{
	/// <summary>
	/// Executes an action, passing in a parameter.
	/// </summary>
	public class ParameterCommand : ICommand
	{
		private Action<object> execute;

		public event EventHandler CanExecuteChanged;

		public ParameterCommand(Action<object> execute)
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
