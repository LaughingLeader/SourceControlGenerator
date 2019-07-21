using System;
using System.Collections.Generic;
using Alphaleonis.Win32.Filesystem;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SCG.Core;
using SCG.SCGEnum;

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
				Update(ref lastModule, value);
			}
		}

		private string lastLogPath;

		public string LastLogPath
		{
			get { return lastLogPath; }
			set
			{
				Update(ref lastLogPath, value);
			}
		}

		private string gitInstallPath;

		[VisibleToView("Git Install Directory", FileBrowseType.Directory)]
		public string GitInstallPath
		{
			get { return gitInstallPath; }
			set
			{
				Update(ref gitInstallPath, value);
			}
		}
#if Debug
		private bool logDisabled = true;
#else
		private bool logDisabled = false;
#endif

		[VisibleToView("Log Enabled")]
		public bool LogDisabled
		{
			get => logDisabled;
			set
			{
				Update(ref logDisabled, value);
				Log.Enabled = !LogDisabled;
			}
		}

	}
}
