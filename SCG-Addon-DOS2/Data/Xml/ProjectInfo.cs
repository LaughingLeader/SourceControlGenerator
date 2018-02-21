using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace LL.SCG.Data.Xml
{
	public class ProjectInfo
	{
		public string Name { get; set; }

		/// <summary>
		/// The mod type (Addon, Adventure)
		/// </summary>
		public string Type { get; set; }

		/// <summary>
		/// The actual UUID used by the mod in the ModuleInfo.
		/// </summary>
		public string Module { get; set; }

		/// <summary>
		/// The project's editor-based UUID.
		/// </summary>
		public string UUID { get; set; }

		public void LoadFromXml(XDocument projectMetaXml)
		{
			var rootXml = projectMetaXml.XPathSelectElement("save/region/node[@id='root']");

			if(rootXml != null)
			{
				this.Name = XmlDataHelper.GetDOS2AttributeValue(rootXml, "Name");
				this.Type = XmlDataHelper.GetDOS2AttributeValue(rootXml, "Type");
				this.Module = XmlDataHelper.GetDOS2AttributeValue(rootXml, "Module");
				this.UUID = XmlDataHelper.GetDOS2AttributeValue(rootXml, "UUID");
			}
			else
			{
				Log.Here().Error("Error selecting path (\"{0}\") from project meta.lsx file.", @"save/region/node[@id='root']");
			}
		}
	}
}
