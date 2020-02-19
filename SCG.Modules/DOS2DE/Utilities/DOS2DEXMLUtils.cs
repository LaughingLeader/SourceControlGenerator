using Alphaleonis.Win32.Filesystem;
using LSLib.LS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SCG.Modules.DOS2DE.Utilities
{
	public static class DOS2DEXMLUtils
	{
		public static readonly string ProjectMetaTemplate = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>\n<save>\n\t<header version=\"2\" />\n\t<version major=\"3\" minor=\"6\" revision=\"1\" build=\"0\" />\n\t<region id=\"MetaData\">\n\t\t<node id=\"root\">\n\t\t\t<attribute id=\"Module\" value=\"{0}\" type=\"23\" />\n\t\t\t<attribute id=\"Name\" value=\"{1}\" type=\"23\" />\n\t\t\t<attribute id=\"Type\" value=\"{2}\" type=\"23\" />\n\t\t\t<attribute id=\"UUID\" value=\"{3}\" type=\"23\" />\n\t\t</node>\n\t</region>\n</save>";
		public static string CreateProjectMetaString(string moduleUUID, string name, string modType, string projectUUID = "")
		{
			if(String.IsNullOrWhiteSpace(projectUUID))
			{
				projectUUID = System.Guid.NewGuid().ToString();
			}

			return String.Format(ProjectMetaTemplate, moduleUUID, name, modType, projectUUID);
		}


		private static Regex modMetaPattern = new Regex("^Mods/([^/]+)/meta.lsx", RegexOptions.IgnoreCase);
		public static bool IsModMetaFile(string pakName, AbstractFileInfo f)
		{
			if (Path.GetFileName(f.Name).Equals("meta.lsx", StringComparison.OrdinalIgnoreCase))
			{
				return modMetaPattern.IsMatch(f.Name);
			}
			return false;
		}

		public static string EscapeXml(string s)
		{
			string toxml = s;
			if (!string.IsNullOrEmpty(toxml))
			{
				// replace literal values with entities
				//toxml = toxml.Replace("&", "&amp;");
				//toxml = toxml.Replace("'", "&apos;");
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
