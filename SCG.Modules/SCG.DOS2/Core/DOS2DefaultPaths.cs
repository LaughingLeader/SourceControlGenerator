using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SCG.Interfaces;

namespace SCG.Modules.DOS2.Core
{
	public static class DOS2DefaultPaths
	{
		public static string DirectoryLayout(IModuleData Data)
		{
			return DefaultPaths.ModuleSettingsFolder(Data) + @"/DirectoryLayout.txt";
		}
	}
}
