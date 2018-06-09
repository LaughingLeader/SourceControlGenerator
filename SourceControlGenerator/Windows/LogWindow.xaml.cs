using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
using System.Xaml;
using SCG.Core;
using SCG.Data;
using SCG.Data.View;

namespace SCG.Windows
{
	/// <summary>
	/// Interaction logic for LogWindow.xaml
	/// </summary>
	public partial class LogWindow : HideWindowBase, IToolWindow
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

		private void CopyButton_Click(object sender, RoutedEventArgs e)
		{
			if(Data.Logs.Count > 0)
			{
				string logContent = "";
				foreach (var data in mainWindow.LogWindow.Data.Logs)
				{
					logContent += data.Output + Environment.NewLine;
				}
				Clipboard.SetText(logContent);
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

		private void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			mainWindow.Controller.MenuAction_SaveLog();
		}

		private void SearchClearButton_Click(object sender, RoutedEventArgs e)
		{
			Data.SearchText = "";
		}

		private void FilterCheck_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			if(sender is CheckBox checkBox)
			{
				LogType? nextFilter = null;
				if (checkBox.Name == "ActivityCheck")
				{
					nextFilter = LogType.Activity;
				}
				else if (checkBox.Name == "ImportantCheck")
				{
					nextFilter = LogType.Important;
				}
				else if (checkBox.Name == "WarningCheck")
				{
					nextFilter = LogType.Warning;
				}
				else if (checkBox.Name == "ErrorCheck")
				{
					nextFilter = LogType.Error;
				}


				if (nextFilter != null)
				{
					//Log.Here().Activity($"Right clicked {nextFilter.ToString()} filter. Solely visible: {Data.FilterIsSolelyVisible(nextFilter.Value)}");
					if (Data.FilterIsSolelyVisible(nextFilter.Value))
					{
						Data.ToggleAllFilters(true);
					}
					else
					{
						Data.OnlyShowFilter(nextFilter.Value);
					}
				}
			}
		}
	}
}
