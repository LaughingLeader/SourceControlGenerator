using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using LL.SCG.Windows;

namespace LL.SCG.Data.View
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

		private bool isVisible;

		public bool IsVisible
		{
			get { return isVisible; }
			set
			{
				isVisible = value;
				RaisePropertyChanged("IsVisible");
				RaisePropertyChanged("LogVisibleText");
			}
		}

		public string LogVisibleText
		{
			get => IsVisible ? "Close Log Window" : "Open Log Window";
		}

		public void Add(LogData log)
		{
			Logs.Add(log);
			RaisePropertyChanged("Logs");
			RaisePropertyChanged("CanClear");

			if(LastLogs != null)
			{
				LastLogs = null;
				RaisePropertyChanged("CanRestore");
			}
		}

		public void Clear()
		{
			if (Logs.Count > 0)
			{
				LastLogs = new ObservableCollection<LogData>(Logs);
				Logs.Clear();

				RaisePropertyChanged("Logs");
				RaisePropertyChanged("CanClear");
				RaisePropertyChanged("CanRestore");
			}
		}

		public void Restore()
		{
			if (LastLogs != null)
			{
				foreach(var log in LastLogs)
				{
					Logs.Add(log);
				}

				LastLogs = null;

				RaisePropertyChanged("Logs");
				RaisePropertyChanged("CanClear");
				RaisePropertyChanged("CanRestore");
			}
		}

		private object logsLock = new object();

		public LogWindowViewData()
		{
			Logs = new ObservableCollection<LogData>();
			BindingOperations.EnableCollectionSynchronization(Logs, logsLock);
		}
	}
}
