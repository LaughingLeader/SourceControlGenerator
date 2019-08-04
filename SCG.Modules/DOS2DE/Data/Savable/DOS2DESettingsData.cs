﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SCG.Data;
using SCG.Data.View;
using SCG.Modules.DOS2DE.Core;
using SCG.SCGEnum;
using SCG.Interfaces;
using Newtonsoft.Json;
using Alphaleonis.Win32.Filesystem;
using System.Runtime.Serialization;
using ReactiveUI;

namespace SCG.Modules.DOS2DE.Data
{
	[DataContract]
	public class DOS2DESettingsData : ModuleSettingsData
	{
		[JsonIgnore]
		public static string SteamAppID => "435150";

		[JsonIgnore]
		public List<string> DirectoryLayouts { get; set; }

		private string dataDirectory;

		[VisibleToView("DOS2DE Data Directory", FileBrowseType.Directory)]
		[DataMember]
		public string DOS2DEDataDirectory
		{
			get { return dataDirectory; }
			set
			{
				this.RaiseAndSetIfChanged(ref dataDirectory, value);
			}
		}

		private string directoryLayoutFile;

		[VisibleToView("Directory Layout", FileBrowseType.File)]
		[DataMember]
		public string DirectoryLayoutFile
		{
			get { return directoryLayoutFile; }
			set
			{
				this.RaiseAndSetIfChanged(ref directoryLayoutFile, value);
			}
		}

		public bool FindDOS2DataDirectory()
		{
			string dataDirectory = Helpers.Registry.GetAppInstallPath("Divinity: Original Sin 2");
			if (!String.IsNullOrEmpty(dataDirectory))
			{
				dataDirectory = Path.Combine(dataDirectory, DOS2DEDefaultPaths.DataDirectory_DefEd);
				if(Directory.Exists(dataDirectory))
				{
					DOS2DEDataDirectory = dataDirectory;
					return true;
				}
			}
			return false;
		}

		public override void SetToDefault(IModuleData Data)
		{
			base.SetToDefault(Data);

			FindDOS2DataDirectory();

			DirectoryLayoutFile = DOS2DEDefaultPaths.DirectoryLayout(Data);
		}
	}
}