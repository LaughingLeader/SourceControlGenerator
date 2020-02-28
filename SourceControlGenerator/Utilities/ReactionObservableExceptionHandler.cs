using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading.Tasks;
using Xceed.Wpf.Toolkit;

namespace SCG.Utilities
{
	public class ReactionObservableExceptionHandler : IObserver<Exception>
	{
		public void OnNext(Exception value)
		{
			//if (Debugger.IsAttached) Debugger.Break();

			var message = $"Exception encountered:\nType: {value.GetType().ToString()}\tMessage: {value.Message}\nSource: {value.Source}\nStackTrace: {value.StackTrace}";
			Debug.WriteLine(message);
			MessageBox.Show(message);
			//RxApp.MainThreadScheduler.Schedule(() => { throw value; });
		}

		public void OnError(Exception value)
		{
			var message = $"Exception encountered:\nType: {value.GetType().ToString()}\tMessage: {value.Message}\nSource: {value.Source}\nStackTrace: {value.StackTrace}";
			Debug.WriteLine(message);
			MessageBox.Show(message);
		}

		public void OnCompleted()
		{
			//if (Debugger.IsAttached) Debugger.Break();
			//RxApp.MainThreadScheduler.Schedule(() => { throw new NotImplementedException(); });
		}
	}
}
