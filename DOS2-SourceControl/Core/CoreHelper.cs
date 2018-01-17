using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LL.DOS2.SourceControl.Core
{
	public static class CoreHelper
	{
		public static bool DOS2IsInstalled()
		{
			string registry_key = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
			using (Microsoft.Win32.RegistryKey key = Registry.LocalMachine.OpenSubKey(registry_key))
			{
				foreach (string subkey_name in key.GetSubKeyNames())
				{
					using (RegistryKey subkey = key.OpenSubKey(subkey_name))
					{
						Console.WriteLine(subkey.GetValue("DisplayName"));
						if (subkey.GetValue("DisplayName").ToString() == "Divinity: Original Sin 2")
						{
							return true;
						}
					}
				}
			}

			return false;
		}

		private static string GetDOS2InstallLocation(RegistryView registryView)
		{
			try
			{

				string registry_key = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
				//@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\";
				using (var reg = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, registryView))
				{
					using (Microsoft.Win32.RegistryKey key = reg.OpenSubKey(registry_key))
					{
						foreach (string subkey_name in key.GetSubKeyNames())
						{
							using (RegistryKey subkey = key.OpenSubKey(subkey_name))
							{
								var keyDisplayName = subkey.GetValue("DisplayName") as string;
								//Log.Here().Activity("Checking registry key: {0}", keyDisplayName);
								if (keyDisplayName != null && keyDisplayName == "Divinity: Original Sin 2")
								{
									string installLocation = subkey.GetValue("InstallLocation") as string;
									Log.Here().Activity("Found DOS2 registry key. Install location: {0}", installLocation);
									if (!String.IsNullOrEmpty(installLocation)) return installLocation;
								}
							}
						}
					}
				}
			}
			catch(Exception e)
			{
				Log.Here().Error("Error getting DOS2 registry key: {0}", e.ToString());
			}

			return null;
		}

		public static string GetDOS2Directory()
		{
			try
			{
				var installLocation = GetDOS2InstallLocation(RegistryView.Registry64);
				if (installLocation == null)
				{
					installLocation = GetDOS2InstallLocation(RegistryView.Registry32);
				}

				if (!String.IsNullOrEmpty(installLocation)) return installLocation;
			}
			catch(Exception e)
			{
				Log.Here().Error("Error checking registry for DOS2: {0}", e.ToString());
			}

			return "";
		}
	}
}