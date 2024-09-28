using SCG.Core;
using SCG.Windows;

using System.Windows;

namespace SCG.Modules.DOS2DE.Windows;

/// <summary>
/// Interaction logic for SetupWindow.xaml
/// </summary>
public partial class SetupWindow : UnclosableWindow
{
	private readonly DOS2DEProjectController controller;
	private readonly Action onConfirmed;

	public SetupWindow(DOS2DEProjectController projectController, Action OnConfirmed)
	{
		InitializeComponent();

		controller = projectController;
		onConfirmed = OnConfirmed;

		DataContext = controller.Data;

		Loaded += SetupWindow_Loaded;
	}

	private void SetupWindow_Loaded(object sender, RoutedEventArgs e)
	{
		if (Owner != null && Owner is MainWindow mainWindow)
		{
			mainWindow.LocationChanged += UpdatePositionWithMainWindow;
			mainWindow.SizeChanged += UpdatePositionWithMainWindow;
		}
	}

	private void UpdatePositionWithMainWindow(object sender, EventArgs e)
	{
		if (sender is MainWindow mainWindow)
		{
			var top = mainWindow.Top + ((mainWindow.Height - this.ActualHeight) / 2);
			var left = mainWindow.Left + ((mainWindow.Width - this.ActualWidth) / 2);

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

		controller.RefreshAllProjects_Start();
	}
}
