using Alphaleonis.Win32.Filesystem;

using Gameloop.Vdf;
using Gameloop.Vdf.Linq;

using Microsoft.Win32;

using System;
using System.Collections.Generic;
using System.Linq;

namespace SCG.Util
{
	public static class RegistryHelper
	{
		const string REG_App_32 = @"SOFTWARE";
		const string REG_App_64 = @"SOFTWARE\Wow6432Node";		
		const string REG_Steam_32 = @"SOFTWARE\Valve\Steam";
		const string REG_Steam_64 = @"SOFTWARE\Wow6432Node\Valve\Steam";
		const string REG_Steam_32_Apps = @"SOFTWARE\Valve\Steam\Apps";
		const string REG_Steam_64_Apps = @"SOFTWARE\Wow6432Node\Valve\Steam\Apps";
		const string REG_GOG_32_Games = @"SOFTWARE\GOG.com\Games";
		const string REG_GOG_64_Games = @"SOFTWARE\Wow6432Node\GOG.com\Games";

		const string PATH_Steam_WorkshopFolder = @"steamapps/workshop";
		const string PATH_Steam_LibraryFile = @"steamapps/libraryfolders.vdf";

		private static string lastSteamInstallPath = "";
		public static string LastSteamInstallPath
		{
			get
			{
				if (lastSteamInstallPath == "" || !Directory.Exists(lastSteamInstallPath))
				{
					lastSteamInstallPath = GetSteamInstallPath();
				}
				return lastSteamInstallPath;
			}
		}

		private static object GetKey(RegistryKey reg, string subKey, string keyValue)
		{
			try
			{
				RegistryKey key = reg.OpenSubKey(subKey);
				if (key != null)
				{
					return key.GetValue(keyValue);
				}
			}
			catch (Exception e)
			{
				Log.Here().Activity($"Error reading registry subKey ({subKey}): {e.ToString()}");
			}
			return null;
		}

		public static string GetSteamInstallPath()
		{
			RegistryKey reg = Registry.LocalMachine;
			object installPath = GetKey(reg, REG_Steam_64, "InstallPath");
			if (installPath == null)
			{
				installPath = GetKey(reg, REG_Steam_32, "InstallPath");
			}
			if (installPath != null)
			{
				return (string)installPath;
			}
			return "";
		}

		public static string GetGOGInstallPath(string AppId)
		{
			RegistryKey reg = Registry.LocalMachine;
			object installPath = GetKey(reg, REG_GOG_64_Games + "\\" + AppId, "path");
			if (installPath == null)
			{
				installPath = GetKey(reg, REG_GOG_32_Games + "\\" + AppId, "path");
			}
			if (installPath != null)
			{
				return (string)installPath;
			}
			return "";
		}

		public static string GetGameInstallPath(string SteamGameFolder, string GOGGameAppId = "")
		{
			try
			{
				if (LastSteamInstallPath != "")
				{
					string folder = Path.Combine(LastSteamInstallPath, SteamGameFolder);
					if (Directory.Exists(folder))
					{
						Log.Here().Activity($"Found '{SteamGameFolder}' at '{folder}'.");
						return folder;
					}
					else
					{
						Log.Here().Activity($"'{SteamGameFolder}' not found. Looking for Steam libraries.");
						string libraryFile = Path.Combine(LastSteamInstallPath, PATH_Steam_LibraryFile);
						if (File.Exists(libraryFile))
						{
							List<string> libraryFolders = new List<string>();
							try
							{
								var libraryData = VdfConvert.Deserialize(File.ReadAllText(libraryFile));
								foreach (VProperty token in libraryData.Value.Children())
								{
									if (token.Key != "TimeNextStatsReport" && token.Key != "ContentStatsID")
									{
										var path = token.Value.Children().Cast<VProperty>().FirstOrDefault(x => x.Key == "path");
										if (path != null && path.Value is VValue innerValue)
										{
											var p = innerValue.Value<string>();
											if (!String.IsNullOrEmpty(p) && Directory.Exists(p))
											{
												Log.Here().Activity($"Found steam library folder at '{p}'.");
												libraryFolders.Add(p);
											}
										}
									}
								}
							}
							catch (Exception ex)
							{
								Log.Here().Error($"Error parsing steam library file at '{libraryFile}':\n{ex}");
							}

							foreach (var folderPath in libraryFolders)
							{
								string checkFolder = Path.Combine(folderPath, SteamGameFolder);
								if (!String.IsNullOrEmpty(checkFolder) && Directory.Exists(checkFolder))
								{
									Log.Here().Activity($"Found '{SteamGameFolder}' at '{checkFolder}'.");
									return checkFolder;
								}
							}
						}
					}
				}

				if(!String.IsNullOrWhiteSpace(GOGGameAppId))
				{
					string gogGamePath = GetGOGInstallPath(GOGGameAppId);
					if (!String.IsNullOrEmpty(gogGamePath) && Directory.Exists(gogGamePath))
					{
						Log.Here().Activity($"Found '{SteamGameFolder}' (GoG) install at '{gogGamePath}'.");
						return gogGamePath;
					}
				}
			}
			catch (Exception ex)
			{
				Log.Here().Error($"[*ERROR*] Error finding DOS2 path:\n{ex}");
			}

			return "";
		}

		public static string GetSteamWorkshopPath()
		{
			if (LastSteamInstallPath != "")
			{
				string workshopFolder = Path.Combine(LastSteamInstallPath, PATH_Steam_WorkshopFolder);
				Log.Here().Activity($"Looking for workshop folder at '{workshopFolder}'.");
				if (Directory.Exists(workshopFolder))
				{
					return workshopFolder;
				}
			}
			return "";
		}

		public static string GetSteamGameWorkshopPath(string AppId)
		{
			if (LastSteamInstallPath != "")
			{
				string steamWorkshopPath = GetSteamWorkshopPath();
				if (!String.IsNullOrEmpty(steamWorkshopPath))
				{
					string workshopFolder = Path.Combine(steamWorkshopPath, AppId);
					Log.Here().Activity($"Looking for workshop folder for '{AppId}' at '{workshopFolder}'.");
					if (Directory.Exists(workshopFolder))
					{
						return workshopFolder;
					}
				}
			}
			return "";
		}

		public static string GetAppInstallPath(string App)
		{
			RegistryKey reg = Registry.LocalMachine;
			object installPath = GetKey(reg, REG_App_64 + "\\" + App, "InstallPath");
			if (installPath == null)
			{
				installPath = GetKey(reg, REG_App_32 + "\\" + App, "InstallPath");
			}
			if (installPath != null)
			{
				return (string)installPath;
			}
			return "";
		}
	}
}
