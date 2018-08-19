using System;
using System.Collections.Generic;
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
using SCG.Core;
using SCG.Windows;

namespace SCG.Modules.DOS2.Windows
{
	/// <summary>
	/// Interaction logic for SetupWindow.xaml
	/// </summary>
	public partial class SetupWindow : UnclosableWindow
	{
		private DOS2ProjectController controller;
		private Action onConfirmed;

		public SetupWindow(DOS2ProjectController projectController, Action OnConfirmed)
		{
			InitializeComponent();

			controller = projectController;
			onConfirmed = OnConfirmed;

			DataContext = controller.Data;

			Loaded += SetupWindow_Loaded;
		}

		private void SetupWindow_Loaded(object sender, RoutedEventArgs e)
		{
			if(Owner != null && Owner is MainWindow mainWindow)
			{
				mainWindow.LocationChanged += UpdatePositionWithMainWindow;
				mainWindow.SizeChanged += UpdatePositionWithMainWindow;
			}
		}

		private void UpdatePositionWithMainWindow(object sender, EventArgs e)
		{
			if(sender is MainWindow mainWindow)
			{
				double top = mainWindow.Top + ((mainWindow.Height - this.ActualHeight) / 2);
				double left = mainWindow.Left + ((mainWindow.Width - this.ActualWidth) / 2);

				this.Top = top < 0 ? 0 : top;
				this.Left = left < 0 ? 0 : left;
			}
		}

		private void ConfirmButton_Click(object sender, RoutedEventArgs e)
		{
			if (Owner != null && Owner is MainWindow mainWindow)
			{
				mainWindow.LocationChanged -= UpdatePositionWithMainWindow;
				mainWindow.SizeChanged -= UpdatePositionWithMainWindow;
			}

			onConfirmed?.Invoke();
			Close();

			controller.RefreshAllProjects();
		}
	}
}
