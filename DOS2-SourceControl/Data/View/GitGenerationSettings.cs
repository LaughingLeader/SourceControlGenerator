using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LL.DOS2.SourceControl.Data.View
{
	public enum LicenseType
	{
		Custom,
		MIT,
		Apache,
		GNU
	}

	public class GitGenerationSettings
	{
		public LicenseType LicenseType { get; set; }
		public bool GenerateLicense { get; set; }
		public bool GenerateReadme { get; set; }
		public bool GenerateChangelog { get; set; }
		public string Author { get; set; }
	}
}
