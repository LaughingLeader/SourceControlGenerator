using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace SCG;

internal class Program
{
	private static SplashScreen? _splash;
	private static string? _appDir;
	private static string? _libDir;

	private static Assembly? AssemblyResolve(object? sender, ResolveEventArgs args)
	{
		if (args.Name.EndsWith(".resources.dll")) return null;
		var name = new AssemblyName(args.Name).Name + ".dll";
		var libPath = Path.Join(_libDir, name);
		if (File.Exists(libPath)) return Assembly.LoadFile(libPath);
		return null;
	}

	[STAThread]
	static void Main(string[] args)
	{
		_appDir = AppDomain.CurrentDomain.BaseDirectory;
		_libDir = Path.Join(_appDir, "_Lib");
		if (!Directory.Exists(_libDir)) _libDir = _appDir;

		AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;

		_splash = new SplashScreen("Resources/Images/SplashScreen.png");
		_splash.Show(false, true);

		var app = new App
		{
			Splash = _splash
		};
		app.InitializeComponent();
		app.Run();
	}
}
