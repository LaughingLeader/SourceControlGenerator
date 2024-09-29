using LazyCache.Splat;

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
			var thread = new Thread(() =>
			{
				Splash.Close(TimeSpan.FromMilliseconds(750));
			});
			RxApp.MainThreadScheduler.Schedule(() =>
			{
				thread.Start();
			});
		}
	}

	public static KeyConverter KeyConverter { get; private set; } = new KeyConverter();
	public static ModifierKeysConverter ModifierKeysConverter { get; private set; } = new ModifierKeysConverter();

	public App()
	{
		var resolver = Locator.CurrentMutable;
		resolver.AddLazyCache();

		SplatRegistrations.RegisterLazySingleton<FileCacheService>();
		SplatRegistrations.RegisterLazySingleton<IEnvironmentService, EnvironmentService>();
		SplatRegistrations.SetupIOC();

		Locator.CurrentMutable.InitializeReactiveUI();

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
