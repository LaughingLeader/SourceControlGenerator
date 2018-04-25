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
	public class CustomWindow : Window
	{
		private const int GWL_STYLE = -16;
		private const int WS_SYSMENU = 0x80000;
		[System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
		private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
		[System.Runtime.InteropServices.DllImport("user32.dll")]
		private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

		public CustomWindow()
		{
			Loaded += CustomWindow_Loaded;
		}

		void CustomWindow_Loaded(object sender, RoutedEventArgs e)
		{
			// Code to remove close box from window
			//var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
			//SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
		}

		#region Click events
		protected void MinimizeClick(object sender, RoutedEventArgs e)
		{
			WindowState = WindowState.Minimized;
		}

		protected void RestoreClick(object sender, RoutedEventArgs e)
		{
			WindowState = (WindowState == WindowState.Normal) ? WindowState.Maximized : WindowState.Normal;
		}

		protected void CloseClick(object sender, RoutedEventArgs e)
		{
			Hide();
		}
		#endregion

		public override void OnApplyTemplate()
		{
			Button minimizeButton = GetTemplateChild("minimizeButton") as Button;
			if (minimizeButton != null)
				minimizeButton.Click += MinimizeClick;

			Button restoreButton = GetTemplateChild("restoreButton") as Button;
			if (restoreButton != null)
				restoreButton.Click += RestoreClick;

			Button closeButton = GetTemplateChild("closeButton") as Button;
			if (closeButton != null)
				closeButton.Click += CloseClick;

			base.OnApplyTemplate();
		}

		private void moveRectangle_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (Mouse.LeftButton == MouseButtonState.Pressed)
				DragMove();
		}
	}
}
