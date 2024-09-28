using SCG.ThemeSystem;
using SCG.Utilities;

using System.Windows;
using System.Windows.Input;

namespace SCG;

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
		RxApp.DefaultExceptionHandler = new ReactionObservableExceptionHandler();
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
