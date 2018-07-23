using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SCG.Core;
using SCG.Enum;

namespace SCG.Data.App
{
	public class AppSettingsData : PropertyChangedBase
	{
		private string lastModule;

		public string LastModule
		{
			get { return lastModule; }
			set
			{
				lastModule = value;
				RaisePropertyChanged("LastModule");
			}
		}

		private string lastLogPath;

		public string LastLogPath
		{
			get { return lastLogPath; }
			set
			{
				lastLogPath = value;
				RaisePropertyChanged("LastLogPath");
			}
		}

		private string gitInstallPath;

		[VisibleToView("Git Install Directory", FileBrowseType.Directory)]
		public string GitInstallPath
		{
			get { return gitInstallPath; }
			set
			{
				gitInstallPath = value;
				AppController.Main.GitDetected = FileCommands.IsValidPath(gitInstallPath);
				RaisePropertyChanged("GitInstallPath");
			}
		}
	}
}
