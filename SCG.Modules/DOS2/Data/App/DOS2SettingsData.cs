using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SCG.Data;
using SCG.Data.View;
using SCG.Modules.DOS2.Core;
using SCG.SCGEnum;
using SCG.Interfaces;
using Newtonsoft.Json;

namespace SCG.Modules.DOS2.Data.App
{
	public class DOS2SettingsData : ModuleSettingsData
	{
		[JsonIgnore]
		public static string SteamAppID => "435150";

		[JsonIgnore]
		public List<string> DirectoryLayouts { get; set; }

		private string dataDirectory;

		[VisibleToView("DOS2 Data Directory", FileBrowseType.Directory)]
		public string DOS2DataDirectory
		{
			get { return dataDirectory; }
			set
			{
				dataDirectory = value;
				RaisePropertyChanged("DOS2DataDirectory");
			}
		}

		private string directoryLayoutFile;

		[VisibleToView("Directory Layout", FileBrowseType.File)]
		public string DirectoryLayoutFile
		{
			get { return directoryLayoutFile; }
			set
			{
				directoryLayoutFile = value;
				RaisePropertyChanged("DirectoryLayoutFile");
			}
		}

		public bool FindDOS2DataDirectory()
		{
			string dataDirectory = Helpers.Registry.GetAppInstallPath("Divinity: Original Sin 2");
			if (!String.IsNullOrEmpty(dataDirectory))
			{
				dataDirectory = dataDirectory + @"\Data";
				DOS2DataDirectory = dataDirectory;
				return true;
			}
			return false;
		}

		public override void SetToDefault(IModuleData Data)
		{
			base.SetToDefault(Data);

			FindDOS2DataDirectory();

			DirectoryLayoutFile = DOS2DefaultPaths.DirectoryLayout(Data);
		}
	}
}
