using System.Runtime.CompilerServices;
using System.IO;
using System.Diagnostics;

namespace System
{
	public delegate void LogDelegate(string Message, params object[] Vars);
	public delegate void OnLog(string Message);

	public class LogContext
	{
		private readonly string _memberName;
		private readonly string _filePath;
		private readonly int _lineNumber;
		private readonly string _filename;

		private string messageFormat
		{
			get
			{
				if (!string.IsNullOrEmpty(_filename))
				{
					return "{0}:{1}({2}): {3}";
				}
				else
				{
					return "";
				}
			}
		}

		public LogContext(string MemberName = "", string FilePath = "", int LineNumber = -1)
		{
			_memberName = MemberName;
			_filePath = FilePath;
			_lineNumber = LineNumber;

			if (!string.IsNullOrEmpty(_filePath))
			{
				_filename = Path.GetFileName(_filePath);
			}
		}

		private void _logHere(string Message, params object[] Vars)
		{
			//Path.GetFileName(_filePath)
			Message = String.Format(Message, Vars);
			if (Log.traceCaller)
			{
				string FinalMessage = !string.IsNullOrEmpty(messageFormat) ? String.Format("{0}:{1}({2}): {3}", _filename, _memberName, _lineNumber, Message) : Message;
#if DEBUG
				//For VS2017 Community
			   Trace.WriteLine(FinalMessage);
#endif
				if (Log.logCallback != null) Log.logCallback(FinalMessage);
			}
			else
			{
#if DEBUG
				Trace.WriteLine(Message);
#endif
				if (Log.logCallback != null) Log.logCallback(Message);
			}
		}

		/// <summary>
		/// Writes a message to the console. Will skip if traceActivity is false.
		/// </summary>
		/// <param name="Message">The message to log.</param>
		public void Activity(String Message, params object[] Vars)
		{
			if (Log.traceActivity)
			{
				_logHere(Message, Vars);
			}
		}

		/// <summary>
		/// Writes a message to the console, bypassing traceActivity.
		/// </summary>
		/// <param name="Message">The message to log.</param>
		public void Important(String Message, params object[] Vars)
		{
			_logHere(Message, Vars);
		}

		/// <summary>
		/// Writes an error message to the console, if traceActivity or traceErrors is true.
		/// </summary>
		/// <param name="Message">The message to log.</param>
		public void Error(String Message, params object[] Vars)
		{
			if (Log.traceActivity || Log.traceErrors) _logHere(Message, Vars);
		}
	}
	public static class Log
	{
		public static OnLog logCallback;
		public static bool debugMode = true;
		public static bool traceActivity = true;
		public static bool traceErrors = true;
#if DEBUG
		public static bool traceCaller = true;
#else
		public static bool traceCaller = false;
#endif

		public static LogContext Here([CallerMemberName] string MemberName = "", [CallerFilePath] string FilePath = "", [CallerLineNumber] int LineNumber = 0)
		{
			return new LogContext(MemberName, FilePath, LineNumber);
		}

		public static LogContext Blank()
		{
			return new LogContext();
		}
	}
}