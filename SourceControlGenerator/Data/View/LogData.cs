﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using ReactiveUI;
using SCG.Util;

namespace SCG.Data.View
{
	public class LogData : ReactiveObject
	{
		public DateTime DateTime { get; set; }

		public int Index { get; set; }

		public string Message { get; set; }

		public string Output { get; set; }

		public LogType MessageType { get; set; }

		public Brush BackgroundColor { get; set; }

		private bool isVisible = true;

		public bool IsVisible
		{
			get { return isVisible; }
			set
			{
				this.RaiseAndSetIfChanged(ref isVisible, value);
			}
		}

		public void FormatOutput()
		{
			//Output = String.Format("[{0}][{1}]: {2}", DateTime.ToLongTimeString(), Index.ToString().PadLeft(4, '0'), Message);
			Output = String.Format("[{0}]: {1}", Index.ToString().PadLeft(4, '0'), Message);

			switch (MessageType)
			{
				case LogType.Activity:
					BackgroundColor = SystemColors.WindowBrush;
					break;
				case LogType.Important:
					BackgroundColor = new SolidColorBrush(Colors.Azure);
					break;
				case LogType.Error:
					BackgroundColor = new SolidColorBrush(Colors.Salmon);
					break;
				case LogType.Warning:
					BackgroundColor = new SolidColorBrush(Colors.Khaki);
					break;
			}
		}
	}

	public class CollapsibleLogEntry : LogData
	{
		public List<LogData> Contents { get; set; }
	}
}
