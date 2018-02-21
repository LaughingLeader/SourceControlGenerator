using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using LL.SCG.Util;

namespace LL.SCG.Data.View
{
	public class LogData : PropertyChangedBase
	{
		public DateTime DateTime { get; set; }

		public int Index { get; set; }

		public string Message { get; set; }

		public string Output { get; set; }

		public LogType MessageType { get; set; }

		public Brush BackgroundColor { get; set; }

		public void FormatOutput()
		{
			Output = String.Format("[{0}][{1}]: {2}", DateTime.ToShortTimeString(), Index, Message);

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
			}
		}
	}

	public class CollapsibleLogEntry : LogData
	{
		public List<LogData> Contents { get; set; }
	}
}
