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
using LL.SCG.Data;
using LL.SCG.Data.View;

namespace LL.SCG.Windows
{
	public class LogWindowViewData : PropertyChangedBase
	{
		public ObservableCollection<LogData> Logs { get; set; }

		public ObservableCollection<LogData> LastLogs { get; set; }

		public bool CanRestore => LastLogs != null;

		public bool CanClear
		{
			get
			{
				return Logs != null ? Logs.Count > 0 : false;
			}
		}

	}

	/// <summary>
	/// Interaction logic for LogWindow.xaml
	/// </summary>
	public partial class LogWindow : CustomWindow
	{
		public LogWindowViewData Data { get; set; }

		private MainWindow mainWindow;

		public LogWindow(MainWindow mainWindow)
		{
			InitializeComponent();

			this.mainWindow = mainWindow;

			Data = new LogWindowViewData();
			Data.Logs = App.LogEntries;
			this.DataContext = Data;
		}

		
		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			this.Hide();
			mainWindow.RaisePropertyChanged("LogVisibleText");
		}

		private void ClearButton_Click(object sender, RoutedEventArgs e)
		{
			if(Data.Logs.Count > 0)
			{
				Data.LastLogs = new ObservableCollection<LogData>(Data.Logs);
				Data.Logs.Clear();

				Data.RaisePropertyChanged("Logs");
				Data.RaisePropertyChanged("CanClear");
				Data.RaisePropertyChanged("CanRestore");
			}
		}

		private void RestoreButton_Click(object sender, RoutedEventArgs e)
		{
			if(Data.LastLogs != null)
			{
				Data.Logs = new ObservableCollection<LogData>(Data.LastLogs);
				Data.LastLogs = null;

				Data.RaisePropertyChanged("Logs");
				Data.RaisePropertyChanged("CanClear");
				Data.RaisePropertyChanged("CanRestore");
			}
		}
	}
}
