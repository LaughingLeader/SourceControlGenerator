using SCG.Data.View;
using SCG.Windows;

using System.Windows.Controls;

namespace SCG.Interfaces;

public interface IProjectController
{
	MainAppData MainAppData { get; set; }

	//void AddProjects(List<AvailableProjectViewData> selectedItems);

	void OpenSetup(Action OnSetupFinished);

	void Initialize(MainAppData mainAppData);
	void Start();
	void Unload();

	UserControl GetProjectView(MainWindow mainWindow);
	IModuleData ModuleData { get; }
}
