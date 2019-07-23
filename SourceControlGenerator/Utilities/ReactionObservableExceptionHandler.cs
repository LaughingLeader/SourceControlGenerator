using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading.Tasks;

namespace SCG.Utilities
{
	public class ReactionObservableExceptionHandler : IObserver<Exception>
	{
		public void OnNext(Exception value)
		{
			//if (Debugger.IsAttached) Debugger.Break();

			Log.Here().Important($"Next: [{value.GetType().ToString()}] {value.Message}");

			RxApp.MainThreadScheduler.Schedule(() => { throw value; });
		}

		public void OnError(Exception error)
		{
			//if (Debugger.IsAttached) Debugger.Break();

			Log.Here().Error($"Error: [{error.GetType().ToString()}] {error.Message}");

			RxApp.MainThreadScheduler.Schedule(() => { throw error; });
		}

		public void OnCompleted()
		{
			//if (Debugger.IsAttached) Debugger.Break();
			RxApp.MainThreadScheduler.Schedule(() => { throw new NotImplementedException(); });
		}
	}
}
