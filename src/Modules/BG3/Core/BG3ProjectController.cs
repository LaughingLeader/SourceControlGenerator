using SCG.Data.View;
using SCG.Interfaces;
using SCG.Modules.BG3.ViewModels;
using SCG.Modules.BG3.Views;
using SCG.Windows;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SCG.Modules.BG3.Core;
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

	public void Start()
	{

	}

	public void Unload()
	{
		MainAppData.MenuBarData.RemoveAllModuleMenus(Data.ModuleName);
	}
}
