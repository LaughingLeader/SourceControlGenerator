using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace LL.SCG.Util.HelperUtil
{
	public class RegisteryHelper
	{
		public string GetRegistryKeyValue(RegistryView registryView, string MatchName, string MatchValue, string KeyName, string RegistryKeyLocation, RegistryHive registryHive = RegistryHive.LocalMachine)
		{
			try
			{
				using (var reg = RegistryKey.OpenBaseKey(registryHive, registryView))
				{
					using (Microsoft.Win32.RegistryKey key = reg.OpenSubKey(RegistryKeyLocation))
					{
						foreach (string subkey_name in key.GetSubKeyNames())
						{
							using (RegistryKey subkey = key.OpenSubKey(subkey_name))
							{
								var keyDisplayName = subkey.GetValue(MatchName) as string;
								if (keyDisplayName != null && keyDisplayName == MatchValue)
								{
									string keyValue = subkey.GetValue(KeyName) as string;
									//Log.Here().Activity("Matched {0} to {1} in {2}. {3} = {4}", MatchName, MatchValue, RegistryKeyLocation, KeyName, keyValue);
									return keyValue;
								}
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				Log.Here().Error("Error checking registry key: {0}", e.ToString());
			}

			return "";
		}

		public string GetRegistryKeyValue(RegistryView registryView, string KeyName, string KeyLocation, RegistryHive registryHive = RegistryHive.LocalMachine)
		{
			try
			{
				using (var reg = RegistryKey.OpenBaseKey(registryHive, registryView))
				{
					foreach(var name in reg.GetSubKeyNames())
					{
						Log.Here().Important(name);
					}

					using (Microsoft.Win32.RegistryKey key = reg.OpenSubKey(KeyLocation))
					{
						foreach (string subkey_name in key.GetSubKeyNames())
						{
							//Log.Here().Activity($"Checking subkey {subkey_name}");
							using (RegistryKey subkey = key.OpenSubKey(subkey_name))
							{
								string keyValue = subkey.GetValue(KeyName) as string;
								//Log.Here().Activity("Found {0} in {1}. Value = {2}", KeyName, KeyLocation, keyValue);
								return keyValue;
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				Log.Here().Error($"Error checking registry key ({KeyLocation}\\{KeyName}): {e.ToString()}");
			}
			
			return "";
		}

		public string GetRegistryKeyValue(RegistryView registryView, string SubKeyName, string KeyName, string KeyLocation, RegistryHive registryHive = RegistryHive.LocalMachine)
		{
			try
			{
				using (var reg = RegistryKey.OpenBaseKey(registryHive, registryView))
				{
					using (Microsoft.Win32.RegistryKey key = reg.OpenSubKey(KeyLocation))
					{
						foreach (string subkey_name in key.GetSubKeyNames().Where(k => k == KeyName))
						{
							//Log.Here().Activity($"Checking subkey {subkey_name}");
							using (RegistryKey subkey = key.OpenSubKey(subkey_name))
							{
								string subkeyValue = subkey.GetValue(SubKeyName) as string;
								return subkeyValue;
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				Log.Here().Error($"Error checking registry key ({KeyLocation}\\{KeyName}): {e.ToString()}");
			}

			return "";
		}

		public string GetRegistryKeyValue(string KeyName, string KeyLocation, RegistryHive registryHive = RegistryHive.LocalMachine)
		{
			var firstCheck = GetRegistryKeyValue(RegistryView.Registry64, KeyName, KeyLocation, registryHive);
			if (!String.IsNullOrEmpty(firstCheck))
			{
				return firstCheck;
			}
			else
			{
				return GetRegistryKeyValue(RegistryView.Registry32, KeyName, KeyLocation, registryHive);
			}
		}

		public string GetRegistryKeyValue(string SubKeyName, string KeyName, string KeyLocation, RegistryHive registryHive = RegistryHive.LocalMachine)
		{
			var firstCheck = GetRegistryKeyValue(RegistryView.Registry64, SubKeyName, KeyName, KeyLocation, registryHive);
			if (!String.IsNullOrEmpty(firstCheck))
			{
				return firstCheck;
			}
			else
			{
				return GetRegistryKeyValue(RegistryView.Registry32, SubKeyName, KeyName, KeyLocation, registryHive);
			}
		}

		public bool KeyExists(string Path, RegistryView registryView, RegistryHive registryHive = RegistryHive.LocalMachine)
		{
			//Computer\HKEY_LOCAL_MACHINE\SOFTWARE\GitForWindows
			try
			{
				using (var reg = RegistryKey.OpenBaseKey(registryHive, registryView))
				{
					using (Microsoft.Win32.RegistryKey key = reg.OpenSubKey(Path))
					{
						return key != null;
					}
				}
			}
			catch (Exception e)
			{
				Log.Here().Error("Error checking registry key: {0}", e.ToString());
			}
			return false;
		}

		public bool KeyExists(string Path, RegistryHive registryHive = RegistryHive.LocalMachine)
		{
			var firstCheck = KeyExists(Path, RegistryView.Registry32, registryHive);
			if (firstCheck)
			{
				return firstCheck;
			}
			else
			{
				return KeyExists(Path, RegistryView.Registry64, registryHive);
			}
		}

		public string GetAppInstallPath(RegistryView registryView, string AppDisplayName)
		{
			return GetRegistryKeyValue(registryView, "DisplayName", AppDisplayName, "InstallLocation", @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
		}

		public string GetAppInstallPath(string AppDisplayName)
		{
			var firstCheck = GetRegistryKeyValue(RegistryView.Registry32, "DisplayName", AppDisplayName, "InstallLocation", @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
			if (!String.IsNullOrEmpty(firstCheck))
			{
				return firstCheck;
			}
			else
			{
				return GetRegistryKeyValue(RegistryView.Registry64, "DisplayName", AppDisplayName, "InstallLocation", @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
			}
		}
	}
}
