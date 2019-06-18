using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using Alphaleonis.Win32.Filesystem;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using SCG.Core;
using SCG.Data;
using SCG.Data.View;
using SCG.Modules;
using SCG.ThemeSystem;
using SCG.Util;

namespace SCG
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

		public static KeyConverter KeyConverter { get; private set; } = new KeyConverter();
		public static ModifierKeysConverter ModifierKeysConverter { get; private set; } = new ModifierKeysConverter();

		public App()
		{
			ThemeController.Init(this);
			SCG.Helpers.Init();
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
