using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using LL.SCG.Core;
using LL.SCG.Data;
using LL.SCG.Data.View;
using LL.SCG.Modules;
using LL.SCG.Util;

namespace LL.SCG
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public static ObservableCollection<LogData> LogEntries { get; set; }
		private int logIndex = 0;

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

		public App()
		{
			LL.SCG.Helpers.Init();
			FileCommands.Init();

			LogEntries = new ObservableCollection<LogData>();
			Log.AllCallback = AddLogMessage;
		}

		/*
		private static void OnAddonLoaded(object sender, AssemblyLoadEventArgs args)
		{
			Log.Here().Important("Loading module addon: " + args.LoadedAssembly.FullName);

			Loader.Call(AppDomain.CurrentDomain, args.LoadedAssembly.FullName, "AddonModule", "Init");
		}
		*/
	}
}
