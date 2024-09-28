using SCG.Data.View;
using SCG.Interfaces;
using SCG.BG3.ViewModels;
using SCG.BG3.Views;
using SCG.Windows;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using SCG.BG3.Services;

namespace SCG.BG3.Core;
public class BG3ProjectController : IProjectController
{
	public BG3ModuleData Data { get; }

	public MainAppData MainAppData { get; set; }
	public IModuleData ModuleData => Data;


	private MainViewViewModel? mainViewVM;
	private MainView? mainView;

	static BG3ProjectController()
	{
		SplatRegistrations.RegisterLazySingleton<MainViewViewModel>();
		SplatRegistrations.RegisterLazySingleton<ProjectLoaderService>();
		SplatRegistrations.SetupIOC();
	}

	public BG3ProjectController()
	{
		Data = new BG3ModuleData();
	}

	public UserControl GetProjectView(MainWindow mainWindow)
	{
		if (mainView == null)
		{
			mainViewVM ??= Locator.Current.GetService<MainViewViewModel>();

			mainView = new MainView()
			{
				ViewModel = mainViewVM
			};
		}
		return mainView;
	}

	public void Initialize(MainAppData mainAppData)
	{

	}

	public bool OpenSetup(Action OnSetupFinished)
	{
		return false;
	}

	private async Task LoadProjectsAsync(IScheduler sch, CancellationToken token)
	{
		Log.Here().Important($"DataDirectory: {Data.Settings.DataDirectory}");
		if(!string.IsNullOrEmpty(Data.Settings.DataDirectory) && Directory.Exists(Data.Settings.DataDirectory))
		{
			var projectLoader = Locator.Current.GetService<ProjectLoaderService>();
			Log.Here().Important($"projectLoader: {projectLoader}");
			if (projectLoader != null)
			{
				var projects = await projectLoader.LoadProjectsAsync(Data.Settings.DataDirectory, token);
				var mods = await projectLoader.LoadModulesAsync(Data.Settings.DataDirectory, token);

				foreach(var project in projects.Values)
				{
					if(!string.IsNullOrEmpty(project.Module) && mods.TryGetValue(project.Module, out var mod))
					{
						project.ModMetaFilePath = mod.FilePath;
						project.Mod = mod;
					}
				}

				Log.Here().Activity($"Loaded {projects.Count} projects and {mods.Count} mods.");
			}
		}
	}

	public void Start()
	{
		if (string.IsNullOrEmpty(Data.Settings.DataDirectory)) Data.Settings.DataDirectory = @"C:\BG3\Data";
		RxApp.TaskpoolScheduler.ScheduleAsync(LoadProjectsAsync);
	}

	public void Unload()
	{
		MainAppData.MenuBarData.RemoveAllModuleMenus(Data.ModuleName);
	}
}
