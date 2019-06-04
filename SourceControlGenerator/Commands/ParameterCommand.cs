using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SCG.Commands
{
	/// <summary>
	/// Executes an action, passing in a parameter.
	/// </summary>
	public class ParameterCommand : BaseCommand
	{
		private Action<object> execute;

		public ParameterCommand(Action<object> execute)
		{
			this.execute = execute;
		}

		public void SetExecuteAction(Action<object> executeAction)
		{
			execute = executeAction;
			RaiseCanExecuteChanged();
		}

		public override bool CanExecute(object parameter)
		{
			return execute != null && base.CanExecute(parameter);
		}

		public override void Execute(object parameter)
		{
			this.execute?.Invoke(parameter);
		}
	}
}
