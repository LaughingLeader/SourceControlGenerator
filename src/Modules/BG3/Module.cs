using SCG.Core;
using SCG.Interfaces;
using SCG.BG3.Core;

namespace SCG;

public class Module : IModuleMain
{
	public BG3ProjectController Controller { get; private set; }

	public static string SteamAppID => "1086940";

	public void Init()
	{
		Controller = new();
		AppController.RegisterController("Baldur's Gate 3", Controller, "pack://application:,,,/SourceControlGenerator;component/Resources/Logos/BG3.png", "");
	}

	public Module() { }
}