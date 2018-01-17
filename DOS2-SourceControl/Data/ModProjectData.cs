using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LL.DOS2.SourceControl.Data
{
	/// <summary>
	/// Data gathered from each mod project's meta.lsx file.
	/// </summary>
	public struct ModProjectData
	{
		//Data from meta.lsx
		public string ProjectName { get; set; }
		public string ProjectGUID { get; set; }
		public string Author { get; set; }
		public List<string> TargetModes { get; set; }

		//Interface settings
		public bool Hide { get; set; }
	}
}
