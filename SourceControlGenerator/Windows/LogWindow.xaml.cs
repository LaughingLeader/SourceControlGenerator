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
using ReactiveUI;
using SCG.Core;
using SCG.Data;
using SCG.Data.View;
using DynamicData;

namespace SCG.Windows
{
	public class LogWindowDebugViewData : LogWindowViewData
	{
		public LogWindowDebugViewData() : base()
		{
			Logs.Add(new LogData { IsVisible = true, Message = "Test", MessageType = LogType.Warning });
			Logs.Add(new LogData { IsVisible = true, Message = "Test", MessageType = LogType.Important });
		}
	}
	/// <summary>
	/// Interaction logic for LogWindow.xaml
	/// </summary>
	public partial class LogWindow : HideWindowBase, IToolWindow, IViewFor<LogWindowViewData>
	{
		public LogWindowViewData ViewModel { get; set; }
		object IViewFor.ViewModel { get; set; }

		private MainWindow mainWindow;

		public LogWindow(MainWindow mainWindow)
		{
			InitializeComponent();

			this.mainWindow = mainWindow;

			ViewModel = new LogWindowViewData();
			this.DataContext = ViewModel;

			this.IsVisibleChanged += LogWindow_IsVisibleChanged;

			this.OneWayBind(this.ViewModel, vm => vm.VisibleLogs, view => view.LogsItemsControl.ItemsSource);

			this.WhenActivated((disposables) =>
			{
				//Console.WriteLine($"Logs: {String.Join(",", ViewModel.VisibleLogs.Select(x => x.Message))}");
			});
		}

		public void Init(AppController controller)
		{
			controller.LogMenuData.RegisterInputBinding(this.InputBindings);
			controller.LogMenuData.Header = ViewModel.LogVisibleText;
		}

		private void LogWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			ViewModel.IsVisible = this.IsVisible;
			if (mainWindow.Controller != null && mainWindow.Controller.LogMenuData != null)
			{
				mainWindow.Controller.LogMenuData.Header = ViewModel.LogVisibleText;
			}

			if(this.IsVisible)
			{
				ViewModel.OnOpened();
			}
			else
			{
				ViewModel.OnClosed();
			}
		}

		private void CopyButton_Click(object sender, RoutedEventArgs e)
		{
			if(ViewModel.Logs.Count > 0)
			{
				string logContent = "";
				foreach (var data in mainWindow.LogWindow.ViewModel.Logs.Items)
				{
					logContent += data.Output + Environment.NewLine;
				}
				Clipboard.SetText(logContent);
			}
		}

		private void ClearButton_Click(object sender, RoutedEventArgs e)
		{
			ViewModel.Clear();
		}

		private void RestoreButton_Click(object sender, RoutedEventArgs e)
		{
			ViewModel.Restore();
		}

		private void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			mainWindow.Controller.MenuAction_SaveLog();
		}

		private void SearchClearButton_Click(object sender, RoutedEventArgs e)
		{
			ViewModel.SearchText = "";
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
					if (ViewModel.FilterIsSolelyVisible(nextFilter.Value))
					{
						ViewModel.ToggleAllFilters(true);
					}
					else
					{
						ViewModel.OnlyShowFilter(nextFilter.Value);
					}
				}
			}
		}
	}
}
