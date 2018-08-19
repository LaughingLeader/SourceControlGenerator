using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace SCG.Data.Xml
{
	public class DependencyInfo
	{
		public string Folder { get; set; }
		public string MD5 { get; set; }
		public string Name { get; set; }
		public string Version { get; set; }
	}
}
