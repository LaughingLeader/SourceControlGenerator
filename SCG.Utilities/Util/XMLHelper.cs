using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SCG.Util
{
	public static class XMLHelper
	{
		public static string FixWhitespaces(string s)
		{
			string toxml = s;
			if (!string.IsNullOrEmpty(toxml))
			{
				// replace literal values with entities
				toxml = toxml.Replace("< ", "<");
				toxml = toxml.Replace("> ", ">");
				toxml = toxml.Replace(" = ", "=");
			}
			return toxml;
		}

		public static string EscapeXml(string s)
		{
			string toxml = s;
			if (!string.IsNullOrEmpty(toxml))
			{
				// replace literal values with entities
				toxml = toxml.Replace("&", "&amp;");
				toxml = toxml.Replace("'", "&apos;");
				toxml = toxml.Replace("\"", "&quot;");
				toxml = toxml.Replace(">", "&gt;");
				toxml = toxml.Replace("<", "&lt;");
			}
			return toxml;
		}

		public static string EscapeXmlAttributes(string xmlstring)
		{
			if (!string.IsNullOrEmpty(xmlstring))
			{
				xmlstring = FixWhitespaces(xmlstring);
				xmlstring = Regex.Replace(xmlstring, "value=\"(.*?)\"", new MatchEvaluator((m) =>
				{
					return $"value=\"{EscapeXml(m.Groups[1].Value)}\"";
				}));
			}
			return xmlstring;
		}

		public static string UnescapeXml(string str)
		{
			if (!string.IsNullOrEmpty(str))
			{
				str = str.Replace("&amp;", "&");
				str = str.Replace("&apos;", "'");
				str = str.Replace("&quot;", "\"");
				str = str.Replace("&gt;", ">");
				str = str.Replace("&lt;", "<");
				str = str.Replace("<br>", Environment.NewLine);
			}
			return str;
		}
	}
}
