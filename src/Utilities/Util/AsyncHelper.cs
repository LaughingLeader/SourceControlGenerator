using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SCG
{
	public static class AsyncHelper
	{
		public static void PrintContext(string message, [CallerMemberName]string callerName = null)
		{
			var ctx = SynchronizationContext.Current;
			if (ctx != null)
			{

				Console.WriteLine("{0}: {1} 0x{2:X8} TID:{3} TSCHED:0x{4}", callerName, message, ctx.GetHashCode(), Thread.CurrentThread.ManagedThreadId, TaskScheduler.Current);
			}
			else
			{
				Console.WriteLine("{0}: {1} <NO CONTEXT> TID:{2} TSCHED:{3}", callerName, message, Thread.CurrentThread.ManagedThreadId, TaskScheduler.Current);
			}
		}

		//Context debug from https://github.com/negativeeddy/blog-examples/blob/master/ConfigureAwaitBehavior/ExtremeConfigAwaitLibrary/AwaitableExtensions.cs
		public static ConfiguredTaskAwaitable PrintContext(this ConfiguredTaskAwaitable t, [CallerMemberName]string callerName = null, [CallerLineNumber]int line = 0)
		{
			PrintContext(callerName, line);
			return t;
		}

		public static Task PrintContext(this Task t, [CallerMemberName]string callerName = null, [CallerLineNumber]int line = 0)
		{
			PrintContext(callerName, line);
			return t;
		}

		static private void PrintContext([CallerMemberName]string callerName = null, [CallerLineNumber]int line = 0)
		{
			var ctx = SynchronizationContext.Current;
			if (ctx != null)
			{
				Console.WriteLine("{0}:{1:D4} await context will be {2}:", callerName, line, ctx);
				Console.WriteLine("    TSCHED:{0}", TaskScheduler.Current);
			}
			else
			{
				Console.WriteLine("{0}:{1:D4} await context will be <NO CONTEXT>", callerName, line);
				Console.WriteLine("    TSCHED:{0}", TaskScheduler.Current);
			}
		}
	}

	/// <summary>
	/// Alternative to using ConfigureAwait(false) everywhere.
	/// Usage:
	/// Place this above your async calls to remove the context for that block:
	/// <code>
	/// await new SynchronizationContextRemover();
	/// </code>
	/// Sourcces:
	/// https://blogs.msdn.microsoft.com/benwilli/2017/02/09/an-alternative-to-configureawaitfalse-everywhere/
	/// https://github.com/negativeeddy/blog-examples/blob/master/ConfigureAwaitBehavior/ExtremeConfigAwaitLibrary/SynchronizationContextRemover.cs
	/// </summary>
	public struct SynchronizationContextRemover : INotifyCompletion
	{
		public bool IsCompleted
		{
			get { return SynchronizationContext.Current == null; }
		}

		public void OnCompleted(Action continuation)
		{
			var prevContext = SynchronizationContext.Current;
			try
			{
				SynchronizationContext.SetSynchronizationContext(null);
				continuation();
			}
			finally
			{
				SynchronizationContext.SetSynchronizationContext(prevContext);
			}
		}

		public SynchronizationContextRemover GetAwaiter()
		{
			return this;
		}

		public void GetResult()
		{
		}
	}
}
