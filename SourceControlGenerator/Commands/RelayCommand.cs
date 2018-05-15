using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SCG.Commands
{

	public class RelayCommand : BaseCommand
	{
		private Action<object> execute;
		private Func<object, bool> canExecute;

		public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
		{
			this.execute = execute;
			this.canExecute = canExecute;
		}

		public override bool CanExecute(object parameter)
		{
			return this.canExecute == null || this.canExecute(parameter);
		}

		public override void Execute(object parameter)
		{
			this.execute?.Invoke(parameter);
		}
	}
}
