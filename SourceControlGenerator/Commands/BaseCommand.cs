using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SCG.Commands
{
	public class BaseCommand : ICommand
	{
		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		public virtual void RaiseCanExecuteChanged()
		{
			CommandManager.InvalidateRequerySuggested();
		}

		public bool Enabled { get; set; } = true;

		public virtual bool CanExecute(object parameter)
		{
			if(parameter is bool changeEnabled)
			{
				Enabled = changeEnabled;
			}
			return Enabled;
		}

		public virtual void Execute(object parameter) { }
	}
}
