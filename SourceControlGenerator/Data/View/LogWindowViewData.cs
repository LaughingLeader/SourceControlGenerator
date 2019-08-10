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
		public SourceList<LogData> Logs { get; set; } = new SourceList<LogData>();

		private ReadOnlyObservableCollection<LogData> visibleLogs;
		public ReadOnlyObservableCollection<LogData> VisibleLogs => visibleLogs;

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

		public void Add(LogData log)
		{
			RxApp.MainThreadScheduler.Schedule(() =>
			{
				Logs.Add(log);
				this.RaisePropertyChanged("Logs");
				this.RaisePropertyChanged("CanClear");

				if (LastLogs != null)
				{
					LastLogs = null;
					this.RaisePropertyChanged("CanRestore");
				}
			});
		}

		public void Clear()
		{
			if (Logs.Count > 0)
			{
				LastLogs = new List<LogData>(Logs.Items);
				Logs.Clear();

				this.RaisePropertyChanged("Logs");
				this.RaisePropertyChanged("CanClear");
				this.RaisePropertyChanged("CanRestore");
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

				this.RaisePropertyChanged("Logs");
				this.RaisePropertyChanged("CanClear");
				this.RaisePropertyChanged("CanRestore");
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

				if(searchText != lastVal)
				{
					this.RaisePropertyChanged("Logs");
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
				this.RaiseAndSetIfChanged(ref filterActivity, value);
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
					if(log.IsVisible != showType)
					{
						log.IsVisible = showType;
						change = true;
					}
				}

				if(raiseLogsChanged && change) this.RaisePropertyChanged("Logs");
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

			foreach(var log in Logs.Items)
			{
				log.IsVisible = log.MessageType == logType;
			}

			this.RaisePropertyChanged("FilterActivity");
			this.RaisePropertyChanged("FilterImportant");
			this.RaisePropertyChanged("FilterWarnings");
			this.RaisePropertyChanged("FilterErrors");
			this.RaisePropertyChanged("Logs");
		}

		public void ToggleAllFilters(bool toValue)
		{
			filterActivity = filterImportant = filterWarnings = filterErrors = toValue;

			foreach (var log in Logs.Items)
			{
				log.IsVisible = toValue;
			}

			this.RaisePropertyChanged("FilterActivity");
			this.RaisePropertyChanged("FilterImportant");
			this.RaisePropertyChanged("FilterWarnings");
			this.RaisePropertyChanged("FilterErrors");
			this.RaisePropertyChanged("Logs");
		}

		private bool CanDisplayLog(LogData logData)
		{
			if(logData.IsVisible)
			{
				if(String.IsNullOrWhiteSpace(SearchText))
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

		public LogWindowViewData()
		{
			//Logs.ToObservableChangeSet().Filter(x => x.IsVisible || (!String.IsNullOrWhiteSpace(searchText) && x.Message.CaseInsensitiveContains(searchText))).AsObservableList();
			Logs.Connect().Filter(x => CanDisplayLog(x)).Bind(out visibleLogs).Subscribe((x) =>
			{
				//Console.WriteLine($"Log added: {x.First().Item.Current?.Message}");
			});
			//this.WhenAnyValue(x => x, x => x.Logs, x => x.FilterActivity, x => x.FilterErrors, x => x.FilterImportant, x => x.SearchText).
		}
	}
}
