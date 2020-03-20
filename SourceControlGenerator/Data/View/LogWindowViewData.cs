using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using SCG.Util;
using SCG.Windows;

namespace SCG.Data.View
{
	public class LogWindowViewData : ReactiveObject
	{
		public SourceList<LogData> Logs { get; set; }

		public ObservableCollectionExtended<LogData> VisibleLogs {get; private set;}

		public List<LogData> LastLogs { get; set; }

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
				this.RaiseAndSetIfChanged(ref isVisible, value);
				this.RaisePropertyChanged("LogVisibleText");
			}
		}

		public string LogVisibleText
		{
			get => IsVisible ? "Close Log Window" : "Open Log Window";
		}

		private void UpdateLogs()
		{
			RxApp.MainThreadScheduler.Schedule(_ =>
			{
				VisibleLogs.Clear();
				if (Logs.Count > 0) VisibleLogs.AddRange(Logs.Items.Where(x => CanDisplayLog(x)).OrderBy(x => x.Index));
			});
		}

		public void Add(LogData log)
		{
			Logs.Add(log);

			if(IsVisible)
			{
				this.RaisePropertyChanged("Logs");
				this.RaisePropertyChanged("CanClear");

				if (LastLogs != null)
				{
					LastLogs = null;
					this.RaisePropertyChanged("CanRestore");
				}
			}
		}

		public void AddRange(IEnumerable<LogData> logs)
		{
			Logs.AddRange(logs);

			if (IsVisible)
			{
				this.RaisePropertyChanged("CanClear");

				if (LastLogs != null)
				{
					LastLogs = null;
					this.RaisePropertyChanged("CanRestore");
				}

				UpdateLogs();
			}
		}

		public void Clear()
		{
			if (Logs.Count > 0)
			{
				LastLogs = new List<LogData>(Logs.Items);
				Logs.Clear();

				if (IsVisible)
				{
					UpdateLogs();
					this.RaisePropertyChanged("CanClear");
					this.RaisePropertyChanged("CanRestore");
				}
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
				if (IsVisible)
				{
					UpdateLogs();
					this.RaisePropertyChanged("CanClear");
					this.RaisePropertyChanged("CanRestore");
				}
			}
		}

		private string searchText;

		public string SearchText
		{
			get { return searchText; }
			set
			{
				var lastVal = searchText;
				this.RaiseAndSetIfChanged(ref searchText, value);

				if (!String.IsNullOrWhiteSpace(value))
				{
					foreach (var log in Logs.Items)
					{
						log.IsVisible = CanDisplayLog(log);
					}
				}

				if (searchText != lastVal)
				{
					UpdateLogs();
				}
			}
		}

		private bool autoRaiseLogsChanged = true;

		private HashSet<LogType> visibleFilters = new HashSet<LogType>()
		{
			LogType.Activity,
			LogType.Important,
			LogType.Warning,
			LogType.Error
		};

		private bool filterActivity = true;

		public bool FilterActivity
		{
			get { return filterActivity; }
			set
			{
				this.RaiseAndSetIfChanged(ref filterActivity, value);
				if(value)
				{
					visibleFilters.Add(LogType.Activity);
				}
				else if(visibleFilters.Contains(LogType.Activity))
				{
					visibleFilters.Remove(LogType.Activity);
				}
				FilterChanged(LogType.Activity, filterActivity, autoRaiseLogsChanged);
			}
		}

		private bool filterImportant = true;

		public bool FilterImportant
		{
			get { return filterImportant; }
			set
			{
				this.RaiseAndSetIfChanged(ref filterImportant, value);
				if (value)
				{
					visibleFilters.Add(LogType.Important);
				}
				else if (visibleFilters.Contains(LogType.Important))
				{
					visibleFilters.Remove(LogType.Important);
				}
				FilterChanged(LogType.Important, filterImportant, autoRaiseLogsChanged);
			}
		}

		private bool filterWarnings = true;

		public bool FilterWarnings
		{
			get { return filterWarnings; }
			set
			{
				this.RaiseAndSetIfChanged(ref filterWarnings, value);
				if (value)
				{
					visibleFilters.Add(LogType.Warning);
				}
				else if (visibleFilters.Contains(LogType.Warning))
				{
					visibleFilters.Remove(LogType.Warning);
				}
				FilterChanged(LogType.Warning, filterWarnings, autoRaiseLogsChanged);
			}
		}

		private bool filterErrors = true;

		public bool FilterErrors
		{
			get { return filterErrors; }
			set
			{
				this.RaiseAndSetIfChanged(ref filterErrors, value);
				if (value)
				{
					visibleFilters.Add(LogType.Error);
				}
				else if (visibleFilters.Contains(LogType.Error))
				{
					visibleFilters.Remove(LogType.Error);
				}
				FilterChanged(LogType.Error, filterErrors, autoRaiseLogsChanged);
			}
		}

		public void FilterChanged(LogType logType, bool showType, bool raiseLogsChanged = true)
		{
			var logs = Logs.Items.Where(ld => ld.MessageType == logType);
			if(logs != null)
			{
				var change = false;
				foreach(var log in logs)
				{
					log.IsVisible = CanDisplayLog(log);
				}

				UpdateLogs();
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
			visibleFilters.Clear();
			visibleFilters.Add(logType);
			if (logType == LogType.Activity)
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

			foreach(var log in Logs.Items)
			{
				log.IsVisible = CanDisplayLog(log);
			}

			this.RaisePropertyChanged("FilterActivity");
			this.RaisePropertyChanged("FilterImportant");
			this.RaisePropertyChanged("FilterWarnings");
			this.RaisePropertyChanged("FilterErrors");
			UpdateLogs();
		}

		public void ToggleAllFilters(bool toValue)
		{
			filterActivity = filterImportant = filterWarnings = filterErrors = toValue;

			visibleFilters.Clear();
			if (toValue == true)
			{
				visibleFilters.Add(LogType.Activity);
				visibleFilters.Add(LogType.Important);
				visibleFilters.Add(LogType.Warning);
				visibleFilters.Add(LogType.Error);
			}

			foreach (var log in Logs.Items)
			{
				log.IsVisible = CanDisplayLog(log);
			}

			this.RaisePropertyChanged("FilterActivity");
			this.RaisePropertyChanged("FilterImportant");
			this.RaisePropertyChanged("FilterWarnings");
			this.RaisePropertyChanged("FilterErrors");
			UpdateLogs();
		}

		private bool CanDisplayLog(LogData logData)
		{
			if(visibleFilters.Contains(logData.MessageType))
			{
				if (String.IsNullOrWhiteSpace(SearchText))
				{
					return true;
				}
				else
				{
					return logData.Message.CaseInsensitiveContains(searchText);
				}
			}
			return false;
		}

		public void OnOpened()
		{
			UpdateLogs();
		}

		public void OnClosed()
		{

		}

		public LogWindowViewData()
		{
			Logs = new SourceList<LogData>();
			VisibleLogs = new ObservableCollectionExtended<LogData>();
		}
	}
}
