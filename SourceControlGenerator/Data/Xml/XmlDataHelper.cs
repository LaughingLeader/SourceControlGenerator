using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LL.SCG.Data.Xml
{
	public static class XmlDataHelper
	{
		/// <summary>
		/// Get an attribute value a DOS2-specific xml element.
		/// </summary>
		/// <param name="xmlData"></param>
		/// <param name="AttributeID"></param>
		/// <returns></returns>
		public static string GetDOS2AttributeValue(XElement xmlData, string AttributeID)
		{
			var val = xmlData.Descendants("attribute").FirstOrDefault(x => (string)x.Attribute("id") == AttributeID)?.Attribute("value").Value;
			if (!String.IsNullOrEmpty(val))
			{
				return val;
			}
			return "";
		}

		public static XElement GetDescendantByAttributeValue(XElement xmlData, string descendantName, string descendantIDName, string descendantIDValue)
		{
			var val = xmlData.Descendants(descendantName).FirstOrDefault(x => (string)x.Attribute(descendantIDName) == descendantIDValue);
			return val;
		}

		public static string GetDescendantValue(XElement xmlData, string descendantName, string descendantIDName, string descendantIDValue, string defaultValue)
		{
			var val = xmlData.Descendants(descendantName).FirstOrDefault(x => (string)x.Attribute(descendantIDName) == descendantIDValue);
			if(val != null)
			{
				return val.Value;
			}

			return defaultValue;
		}

		public static int GetAttributeAsInt(XElement data, string attributeName, int defaultValue)
		{
			if (data.HasAttributes)
			{
				var att = data.Attribute(attributeName);
				if(att.Value != null)
				{
					if (int.TryParse(att.Value, out int val))
					{
						return val;
					}
				}
			}

			return defaultValue;
		}

		public static bool GetAttributeAsBool(XElement data, string attributeName, bool defaultValue)
		{
			if (data.HasAttributes)
			{
				var att = data.Attribute(attributeName);
				if (att.Value != null)
				{
					if (bool.TryParse(att.Value, out bool val))
					{
						return val;
					}
				}
			}

			return defaultValue;
		}

		public static string GetAttributeAsString(XElement data, string attributeName, string defaultValue)
		{
			if (data.HasAttributes)
			{
				var att = data.Attribute(attributeName);
				if (att.Value != null)
				{
					return att.Value;
				}
			}

			return defaultValue;
		}
	}
}
