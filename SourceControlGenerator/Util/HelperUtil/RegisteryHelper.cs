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
		public string GetRegistryKeyValue(RegistryView registryView, string MatchName, string MatchValue, string KeyName, string RegistryKeyLocation)
		{
			try
			{
				using (var reg = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, registryView))
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
									Log.Here().Activity("Matched {0} to {1} in {2}. {3} = {4}", MatchName, MatchValue, RegistryKeyLocation, KeyName, keyValue);
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

		public string GetRegistryKeyValue(RegistryView registryView, string KeyName, string KeyLocation)
		{
			try
			{
				using (var reg = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, registryView))
				{
					using (Microsoft.Win32.RegistryKey key = reg.OpenSubKey(KeyLocation))
					{
						foreach (string subkey_name in key.GetSubKeyNames())
						{
							using (RegistryKey subkey = key.OpenSubKey(subkey_name))
							{
								string keyValue = subkey.GetValue(KeyName) as string;
								Log.Here().Activity("Found {0} in {1}. Value = {2}", KeyName, KeyLocation, keyValue);
								return keyValue;
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
