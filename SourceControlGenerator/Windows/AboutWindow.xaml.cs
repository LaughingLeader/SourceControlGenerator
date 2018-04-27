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
using LL.SCG.Core;

namespace LL.SCG.Windows
{
	/// <summary>
	/// Interaction logic for AboutWindow.xaml
	/// </summary>
	public partial class AboutWindow : UnclosableWindow
	{
		private MainWindow mainWindow;

		public AboutWindow(MainWindow mainWindow)
		{
			InitializeComponent();

			this.mainWindow = mainWindow;
		}

		public void Init(AppController controller)
		{
			var aboutMenu = controller.Data.MenuBarData.FindByID(MenuID.About);
			if(aboutMenu != null && aboutMenu.InputBinding != null)
			{
				InputBindings.Add(aboutMenu.InputBinding);
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
}
