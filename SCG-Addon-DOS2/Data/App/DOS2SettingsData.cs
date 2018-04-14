using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LL.SCG.Data;
using LL.SCG.Data.View;
using LL.SCG.Enum;
using LL.SCG.Interfaces;

namespace LL.SCG.DOS2.Data.App
{
	public class DOS2SettingsData : ModuleSettingsData
	{
		public static string SteamAppID => "435150";

		public List<string> DirectoryLayouts { get; set; }

		private string dataDirectory;

		[VisibleToView("Data Directory", FileBrowseType.Directory)]
		public string DataDirectory
		{
			get { return dataDirectory; }
			set
			{
				dataDirectory = value;
				RaisePropertyChanged("DataDirectory");
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
	}
}
