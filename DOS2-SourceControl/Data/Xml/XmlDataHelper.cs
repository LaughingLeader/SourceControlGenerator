using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LL.DOS2.SourceControl.Data.Xml
{
	public static class XmlDataHelper
	{
		public static string GetAttributeValue(XElement xmlData, string AttributeID)
		{
			var val = xmlData.Descendants("attribute").FirstOrDefault(x => (string)x.Attribute("id") == AttributeID).Attribute("value").Value;
			if (!String.IsNullOrEmpty(val))
			{
				return val;
			}
			return "";
		}
	}
}
