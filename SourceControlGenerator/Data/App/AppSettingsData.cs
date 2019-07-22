using System;
using System.Collections.Generic;
using Alphaleonis.Win32.Filesystem;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SCG.Core;
using SCG.SCGEnum;
using ReactiveUI;
using System.Runtime.Serialization;

namespace SCG.Data.App
{
	[DataContract]
	public class AppSettingsData : ReactiveObject
	{
		private string lastModule;

		[DataMember]
		public string LastModule
		{
			get { return lastModule; }
			set
			{
				this.RaiseAndSetIfChanged(ref lastModule, value);
			}
		}

		private string lastLogPath;

		[DataMember]
		public string LastLogPath
		{
			get { return lastLogPath; }
			set
			{
				this.RaiseAndSetIfChanged(ref lastLogPath, value);
			}
		}

		private string gitInstallPath;

		[VisibleToView("Git Install Directory", FileBrowseType.Directory)]
		[DataMember]
		public string GitInstallPath
		{
			get { return gitInstallPath; }
			set
			{
				this.RaiseAndSetIfChanged(ref gitInstallPath, value);
			}
		}
#if Debug
		private bool logDisabled = true;
#else
		private bool logDisabled = false;
#endif

		[VisibleToView("Log Enabled")]
		[DataMember]
		public bool LogDisabled
		{
			get => logDisabled;
			set
			{
				this.RaiseAndSetIfChanged(ref logDisabled, value);
				Log.Enabled = !LogDisabled;
			}
		}

	}
}
