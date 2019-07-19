using System.Runtime.CompilerServices;
using Alphaleonis.Win32.Filesystem;
using System.Diagnostics;
using SCG.Util;
using System;
using System.ComponentModel;

namespace SCG
{
	public delegate void LogDelegate(string Message, params object[] Vars);
	public delegate void OnLog(string Message);
	public delegate void OnSpecificLog(string Message, LogType logType);

	public enum LogType
	{
		Activity,
		Important,
		Warning,
		Error
	}

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

		private void _logHere(LogType logType, string Message, params object[] Vars)
		{
			//Path.GetFileName(_filePath)
			if (Vars.Length > 0) Message = String.Format(Message, Vars);
			if (Log.traceCaller)
			{
				string FinalMessage = !string.IsNullOrEmpty(messageFormat) ? String.Format("{0}:{1}({2}): {3}", _filename, _memberName, _lineNumber, Message) : Message;
#if DEBUG
				//For VS2017 Community
				//Trace.WriteLine(FinalMessage);
				Debug.WriteLine(FinalMessage);
#endif
				Log.AllCallback?.Invoke(FinalMessage, logType);
				if (logType == LogType.Activity) Log.ActivityCallback?.Invoke(FinalMessage);
				if (logType == LogType.Important) Log.ImportantCallback?.Invoke(FinalMessage);
				if (logType == LogType.Error) Log.ErrorCallback?.Invoke(FinalMessage);
			}
			else
			{
#if DEBUG
				Debug.WriteLine(Message);
#endif
				Log.AllCallback?.Invoke(Message, logType);
				if (logType == LogType.Activity) Log.ActivityCallback?.Invoke(Message);
				if (logType == LogType.Important) Log.ImportantCallback?.Invoke(Message);
				if (logType == LogType.Error) Log.ErrorCallback?.Invoke(Message);
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
				_logHere(LogType.Activity, Message, Vars);
			}
		}

		/// <summary>
		/// Writes a message to the console, bypassing traceActivity.
		/// </summary>
		/// <param name="Message">The message to log.</param>
		public void Important(String Message, params object[] Vars)
		{
			_logHere(LogType.Important, Message, Vars);
		}

		/// <summary>
		/// Writes an warning message to the console, if traceActivity or traceErrors is true.
		/// </summary>
		/// <param name="Message">The message to log.</param>
		public void Warning(String Message, params object[] Vars)
		{
			if (Log.traceActivity || Log.traceErrors) _logHere(LogType.Warning, Message, Vars);
		}

		/// <summary>
		/// Writes an error message to the console, if traceActivity or traceErrors is true.
		/// </summary>
		/// <param name="Message">The message to log.</param>
		public void Error(String Message, params object[] Vars)
		{
			if (Log.traceActivity || Log.traceErrors) _logHere(LogType.Error, Message, Vars);
		}
	}
	public static class Log
	{
		public static OnSpecificLog AllCallback;
		public static OnLog ActivityCallback;
		public static OnLog ImportantCallback;
		public static OnLog ErrorCallback;

		public static bool debugMode = true;
		public static bool traceActivity = true;
		public static bool traceErrors = true;
		public static bool traceCaller = true;

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