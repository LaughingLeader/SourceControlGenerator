using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LL.SCG.Enum;

namespace LL.SCG.Data.View
{
	public class SettingsEntryData
	{
		public string Name { get; set; }
		public FileBrowseType FileBrowseType { get; set; }
		public SettingsViewPropertyType ViewType { get; set; }
	}
}
