using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using SCG.Collections;
using SCG.Data;
using SCG.Data.View;
using SCG.Windows;

namespace SCG.Interfaces
{
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
}
