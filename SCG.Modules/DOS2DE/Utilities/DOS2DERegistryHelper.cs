using Alphaleonis.Win32.Filesystem;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SCG.Util;

namespace SCG.Modules.DOS2DE.Utilities
{
	public static class DOS2DERegistryHelper
	{
		const string REG_Steam_AppId = "435150";
		const string REG_GOG_AppId = @"1584823040";
		const string PATH_Steam_DivinityOriginalSin2 = @"Divinity Original Sin 2";
		const string PATH_Steam_DivinityOriginalSin2_WorkshopFolder = @"435150";

		private static string lastDivinityOriginalSin2Path = "";
		private static string LastDivinityOriginalSin2Path => lastDivinityOriginalSin2Path;

		public static string GetDOS2WorkshopPath()
		{
			return RegistryHelper.GetSteamGameWorkshopPath(PATH_Steam_DivinityOriginalSin2_WorkshopFolder);
		}

		public static string GetDOS2InstallPath()
		{
			return RegistryHelper.GetGameInstallPath(PATH_Steam_DivinityOriginalSin2, REG_GOG_AppId);
		}
	}
}
