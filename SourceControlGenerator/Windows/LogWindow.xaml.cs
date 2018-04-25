using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using LL.SCG.Core;
using LL.SCG.Data;
using LL.SCG.Data.View;

namespace LL.SCG.Windows
{
	/// <summary>
	/// Interaction logic for LogWindow.xaml
	/// </summary>
	public partial class LogWindow : HideWindowBase
	{
		public LogWindowViewData Data { get; set; }

		private MainWindow mainWindow;

		public LogWindow(MainWindow mainWindow)
		{
			InitializeComponent();

			this.mainWindow = mainWindow;

			Data = new LogWindowViewData();
			this.DataContext = Data;

			this.IsVisibleChanged += LogWindow_IsVisibleChanged;

			
		}

		public void Init(AppController controller)
		{
			InputBindings.Add(controller.LogMenuData.InputBinding);
			controller.LogMenuData.Header = Data.LogVisibleText;
		}

		private void LogWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			Data.IsVisible = this.IsVisible;
			if (mainWindow.Controller != null && mainWindow.Controller.LogMenuData != null)
			{
				mainWindow.Controller.LogMenuData.Header = Data.LogVisibleText;
			}
		}

		private void ClearButton_Click(object sender, RoutedEventArgs e)
		{
			Data.Clear();
		}

		private void RestoreButton_Click(object sender, RoutedEventArgs e)
		{
			Data.Restore();
		}
	}
}
