using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace LL.DOS2.SourceControl.Data.Xml
{
	public class ModuleInfo
	{
		public string Author { get; set; }
		public string CharacterCreationLevelName { get; set; }
		public string Description { get; set; }
		public string Folder { get; set; }
		public string GMTemplate { get; set; }
		public string LobbyLevelName { get; set; }
		public string MD5 { get; set; }
		public string MenuLevelName { get; set; }
		public string Name { get; set; }
		public string NumPlayers { get; set; }
		public string PhotoBooth { get; set; }
		public string StartupLevelName { get; set; }
		public string Tags { get; set; }
		public string Type { get; set; }
		public string UUID { get; set; }
		public Int32 Version { get; set; }

		public List<String> TargetModes { get; set; }

		public void LoadFromXml(XDocument modMetaXml)
		{
			bool moduleInfoLoaded = false;
			try
			{
				var moduleInfoXml = modMetaXml.XPathSelectElement("save/region/node/children/node[@id='ModuleInfo']");

				if (moduleInfoXml != null)
				{
					Log.Here().Activity("Parsing module info xml data.");

					PropertyInfo[] propInfo = typeof(ModuleInfo).GetProperties();
					foreach (PropertyInfo property in propInfo)
					{
						if (property.Name == "TargetModes") continue;
						var value = XmlDataHelper.GetDOS2AttributeValue(moduleInfoXml, property.Name);

						if(property.Name == "Version")
						{
							Int32 version = 0;
							if(!Int32.TryParse(value, out version))
							{
								Log.Here().Error("Error parsing ModuleInfo version string {0} to int.", value);
							}
							else
							{
								property.SetValue(this, version);
							}
						}
						else
						{
							property.SetValue(this, value);
						}
						//Log.Here().Activity("Set {0} to {1}", property.Name, value);
					}

					Log.Here().Activity("[{0}] Module info parsing complete. Checking target modes.", this.Name);

					this.TargetModes = new List<string>();
					moduleInfoLoaded = true;
				}
				else
				{
					Log.Here().Error("Error selecting path (\"{0}\") from mod meta.lsx file.", @"save / region / node / children / node[@id = 'ModuleInfo']");
				}
			}
			catch (Exception ex)
			{
				Log.Here().Error("Error parsing mod meta.lsx: {0}", ex.ToString());
			}

			if (moduleInfoLoaded)
			{
				try
				{
					var modTargets = modMetaXml.Descendants("node").FirstOrDefault(x => (string)x.Attribute("id") == "TargetModes").Descendants("node").Descendants("attribute");

					if (modTargets != null && modTargets.Count() > 0)
					{
						foreach (var target in modTargets)
						{
							var targetVal = target.Attribute("value");
							if (targetVal != null && !String.IsNullOrEmpty(targetVal.Value)) this.TargetModes.Add(targetVal.Value);
						}
					}

					Log.Here().Activity("[{0}] Target mode parsing complete.", this.Name);
				}
				catch (Exception ex)
				{
					Log.Here().Error("[{0}] Error checking target modes: {1}", this.Name, ex.ToString());
				}
			}
		}
	}
}
