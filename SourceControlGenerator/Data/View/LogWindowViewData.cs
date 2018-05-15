using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using SCG.Util;
using SCG.Windows;

namespace SCG.Data.View
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

			if (LastLogs != null)
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
				foreach (var log in LastLogs)
				{
					Logs.Add(log);
				}

				LastLogs = null;

				RaisePropertyChanged("Logs");
				RaisePropertyChanged("CanClear");
				RaisePropertyChanged("CanRestore");
			}
		}

		private string searchText;

		public string SearchText
		{
			get { return searchText; }
			set
			{
				var lastVal = searchText;
				searchText = value;
				RaisePropertyChanged("SearchText");

				if(searchText != lastVal)
				{
					RaisePropertyChanged("Logs");
				}
			}
		}

		private bool autoRaiseLogsChanged = true;

		private bool filterActivity = true;

		public bool FilterActivity
		{
			get { return filterActivity; }
			set
			{
				filterActivity = value;
				RaisePropertyChanged("FilterActivity");
				FilterChanged(LogType.Activity, filterActivity, autoRaiseLogsChanged);
			}
		}

		private bool filterImportant = true;

		public bool FilterImportant
		{
			get { return filterImportant; }
			set
			{
				filterImportant = value;
				RaisePropertyChanged("FilterImportant");
				FilterChanged(LogType.Important, filterImportant, autoRaiseLogsChanged);
			}
		}

		private bool filterWarnings = true;

		public bool FilterWarnings
		{
			get { return filterWarnings; }
			set
			{
				filterWarnings = value;
				RaisePropertyChanged("FilterWarnings");
				FilterChanged(LogType.Warning, filterWarnings, autoRaiseLogsChanged);
			}
		}

		private bool filterErrors = true;

		public bool FilterErrors
		{
			get { return filterErrors; }
			set
			{
				filterErrors = value;
				RaisePropertyChanged("FilterErrors");
				FilterChanged(LogType.Error, filterErrors, autoRaiseLogsChanged);
			}
		}

		public void FilterChanged(LogType logType, bool showType, bool raiseLogsChanged = true)
		{
			var logs = Logs.Where(ld => ld.MessageType == logType);
			if(logs != null)
			{
				var change = false;
				foreach(var log in logs)
				{
					if(log.IsVisible != showType)
					{
						log.IsVisible = showType;
						change = true;
					}
				}

				if(raiseLogsChanged && change) RaisePropertyChanged("Logs");
			}
		}

		public bool FilterIsSolelyVisible(LogType logType)
		{
			switch (logType)
			{
				case LogType.Activity:
					return filterActivity == true && (!filterImportant && !filterWarnings && !filterErrors);
				case LogType.Important:
					return filterImportant == true && (!filterActivity && !filterWarnings && !filterErrors);
				case LogType.Warning:
					return filterWarnings == true && (!filterActivity && !filterImportant && !filterErrors);
				case LogType.Error:
					return filterErrors == true && (!filterActivity && !filterImportant && !filterWarnings);
				default:
					return false;
			}
		}

		public void OnlyShowFilter(LogType logType)
		{
			if(logType == LogType.Activity)
			{
				filterActivity = true;
				filterImportant = filterWarnings = filterErrors = false;
			}
			else if(logType == LogType.Important)
			{
				filterImportant = true;
				filterActivity = filterWarnings = filterErrors = false;
			}
			else if (logType == LogType.Warning)
			{
				filterWarnings = true;
				filterActivity = filterImportant = filterErrors = false;
			}
			else if (logType == LogType.Error)
			{
				filterErrors = true;
				filterActivity = filterImportant = filterWarnings = false;
			}

			foreach(var log in Logs)
			{
				log.IsVisible = log.MessageType == logType;
			}

			RaisePropertyChanged("FilterActivity");
			RaisePropertyChanged("FilterImportant");
			RaisePropertyChanged("FilterWarnings");
			RaisePropertyChanged("FilterErrors");
			RaisePropertyChanged("Logs");
		}

		public void ToggleAllFilters(bool toValue)
		{
			filterActivity = filterImportant = filterWarnings = filterErrors = toValue;

			foreach (var log in Logs)
			{
				log.IsVisible = toValue;
			}

			RaisePropertyChanged("FilterActivity");
			RaisePropertyChanged("FilterImportant");
			RaisePropertyChanged("FilterWarnings");
			RaisePropertyChanged("FilterErrors");
			RaisePropertyChanged("Logs");
		}

		private object logsLock = new object();

		public LogWindowViewData()
		{
			Logs = new ObservableCollection<LogData>();
			BindingOperations.EnableCollectionSynchronization(Logs, logsLock);
		}
	}
}
