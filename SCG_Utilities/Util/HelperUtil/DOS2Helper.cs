using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace LL.SCG.Util.HelperUtil
{
	public class DOS2Helper
	{
		public string GetInstallPath()
		{
			string installLocation = Helpers.Registry.GetAppInstallPath(RegistryView.Registry64, "Divinity: Original Sin 2");
			if (String.IsNullOrEmpty(installLocation))
			{
				installLocation = Helpers.Registry.GetAppInstallPath(RegistryView.Registry32, "Divinity: Original Sin 2");
			}

			if (!String.IsNullOrEmpty(installLocation)) return installLocation;

			return "";
		}
	}
}
