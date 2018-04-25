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

namespace LL.SCG.Windows
{
	/// <summary>
	/// Interaction logic for AboutWindow.xaml
	/// </summary>
	public partial class AboutWindow : Window
	{
		private MainWindow mainWindow;

		private const int GWL_STYLE = -16;
		private const int WS_SYSMENU = 0x80000;
		[System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
		private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
		[System.Runtime.InteropServices.DllImport("user32.dll")]
		private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

		public AboutWindow(MainWindow mainWindow)
		{
			InitializeComponent();

			this.mainWindow = mainWindow;
			Loaded += AboutWindow_Loaded;
		}

		void AboutWindow_Loaded(object sender, RoutedEventArgs e)
		{
			// Code to remove close box from window
			var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
			SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
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
