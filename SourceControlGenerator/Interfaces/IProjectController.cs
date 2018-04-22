using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using LL.SCG.Collections;
using LL.SCG.Data;
using LL.SCG.Data.View;
using LL.SCG.Windows;

namespace LL.SCG.Interfaces
{
	public interface IProjectController
	{
		MainAppData MainAppData { get; set; }

		void AddProjects(List<AvailableProjectViewData> selectedItems);
		void Initialize(MainAppData mainAppData);
		void Start();
		void Unload();

		UserControl GetProjectView(MainWindow mainWindow);
		IModuleData ModuleData { get; }
	}
}
