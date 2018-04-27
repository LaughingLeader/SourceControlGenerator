using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LL.SCG.Windows
{
	public class UnclosableWindow : Window
	{
		private const int GWL_STYLE = -16;
		private const int WS_SYSMENU = 0x80000;
		[System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
		private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
		[System.Runtime.InteropServices.DllImport("user32.dll")]
		private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

		const int WM_SYSCOMMAND = 0x0112;
		const int SC_MOVE = 0xF010;

		public bool LockPosition
		{
			get { return (bool)GetValue(LockPositionProperty); }
			set { SetValue(LockPositionProperty, value); }
		}

		// Using a DependencyProperty as the backing store for LockPosition.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty LockPositionProperty =
			DependencyProperty.Register("LockPosition", typeof(bool), typeof(UnclosableWindow), new PropertyMetadata(false));

		public UnclosableWindow()
		{
			Loaded += UnclosableWindow_Loaded;
			SourceInitialized += UnclosableWindow_SourceInitialized;
		}

		void UnclosableWindow_Loaded(object sender, RoutedEventArgs e)
		{
			// Code to remove close box from window
			var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
			SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
		}

		private void UnclosableWindow_SourceInitialized(object sender, EventArgs e)
		{
			if (LockPosition)
			{
				WindowInteropHelper helper = new WindowInteropHelper(this);
				HwndSource source = HwndSource.FromHwnd(helper.Handle);
				source.AddHook(WndProc);
			}
		}

		private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{

			switch (msg)
			{
				case WM_SYSCOMMAND:
					int command = wParam.ToInt32() & 0xfff0;
					if (command == SC_MOVE)
					{
						handled = true;
					}
					break;
				default:
					break;
			}
			return IntPtr.Zero;
		}
	}
}
