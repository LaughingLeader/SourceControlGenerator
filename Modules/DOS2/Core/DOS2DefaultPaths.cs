using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LL.SCG.Interfaces;

namespace LL.SCG.DOS2.Core
{
	public static class DOS2DefaultPaths
	{
		public static string DirectoryLayout(IModuleData Data)
		{
			return DefaultPaths.DataFolder + @"Settings/" + Data.ModuleFolderName + @"/DirectoryLayout.txt";
		}
	}
}
