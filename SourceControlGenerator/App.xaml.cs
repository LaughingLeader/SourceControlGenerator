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
using LL.SCG.ThemeSystem;
using LL.SCG.Util;

namespace LL.SCG
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);
		}

		public App()
		{
			ThemeController.Init(this);
			LL.SCG.Helpers.Init();
			FileCommands.Init();
		}

		private void Application_Startup(object sender, StartupEventArgs e)
		{
			
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
