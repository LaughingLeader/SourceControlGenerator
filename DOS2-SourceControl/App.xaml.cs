using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using LL.DOS2.SourceControl.Core;
using LL.DOS2.SourceControl.Data;
using LL.DOS2.SourceControl.Data.View;
using LL.DOS2.SourceControl.Util;

namespace LL.DOS2.SourceControl
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public static ObservableCollection<LogData> LogEntries { get; set; }
		private int logIndex = 0;

		public App()
		{
			LL.DOS2.SourceControl.Helpers.Init();

			LogEntries = new ObservableCollection<LogData>();
			Log.AllCallback = AddLogMessage;
		}

		public void AddLogMessage(string LogMessage, LogType logType)
		{
			var log = new LogData()
			{
				Index = logIndex++,
				DateTime = DateTime.Now,
				Message = LogMessage,
				MessageType = logType
			};
			log.FormatOutput();
			Dispatcher.BeginInvoke((Action)(() => LogEntries.Add(log)));
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);
		}
	}
}
