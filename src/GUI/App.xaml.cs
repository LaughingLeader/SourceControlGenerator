using SCG.Services;
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
	public SplashScreen? Splash { get; set; }

	protected override void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);

		if (Splash != null)
		{
			var splashFade = new Thread(() =>
			{
				Splash.Close(TimeSpan.FromSeconds(1));
			});
		}
	}

	public static KeyConverter KeyConverter { get; private set; } = new KeyConverter();
	public static ModifierKeysConverter ModifierKeysConverter { get; private set; } = new ModifierKeysConverter();

	public App()
	{
		SplatRegistrations.RegisterLazySingleton<FileCacheService>();

		RxApp.DefaultExceptionHandler = new ReactionObservableExceptionHandler();
		ThemeController.Init(this);
		SCG.Helpers.Init();
		FileCommands.Init();
	}

	/*
	private static void OnAddonLoaded(object sender, AssemblyLoadEventArgs args)
	{
		Log.Here().Important("Loading module addon: " + args.LoadedAssembly.FullName);

		Loader.Call(AppDomain.CurrentDomain, args.LoadedAssembly.FullName, "AddonModule", "Init");
	}
	*/
}
