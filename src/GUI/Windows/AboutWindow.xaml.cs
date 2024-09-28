using SCG.Core;

using System.Windows;

namespace SCG.Windows;

/// <summary>
/// Interaction logic for AboutWindow.xaml
/// </summary>
public partial class AboutWindow : UnclosableWindow, IToolWindow
{
	private readonly MainWindow mainWindow;

	public AboutWindow(MainWindow mainWindow)
	{
		InitializeComponent();

		this.mainWindow = mainWindow;
	}

	public void Init(AppController controller)
	{
		var aboutMenu = controller.Data.MenuBarData.FindByID(MenuID.About);
		if (aboutMenu != null)
		{
			aboutMenu.RegisterInputBinding(this.InputBindings);
		}
	}

	private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
	{
		Log.Here().Activity($"Opening url {e.Uri}");
		Helpers.Web.OpenUri(e.Uri.ToString());
	}

	private void CloseButton_Click(object sender, RoutedEventArgs e)
	{
		this.Hide();
	}
}
