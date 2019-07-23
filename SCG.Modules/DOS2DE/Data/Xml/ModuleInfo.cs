using System;
using System.Collections.Generic;
using Alphaleonis.Win32.Filesystem;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;
using SCG.Util;
using ReactiveUI;
using System.Reactive.Concurrency;

namespace SCG.Data.Xml
{
	public class ModuleInfo : ReactiveObject
	{
		private string author;
		public string Author
		{
			get => author;
			set { this.RaiseAndSetIfChanged(ref author, value); }
		}

		private string charactercreationlevelname;
		public string CharacterCreationLevelName
		{
			get => charactercreationlevelname;
			set { this.RaiseAndSetIfChanged(ref charactercreationlevelname, value); }
		}

		private string description;
		public string Description
		{
			get => description;
			set { this.RaiseAndSetIfChanged(ref description, value); }
		}

		private string folder;
		public string Folder
		{
			get => folder;
			set { this.RaiseAndSetIfChanged(ref folder, value); }
		}

		private string gmtemplate;
		public string GMTemplate
		{
			get => gmtemplate;
			set { this.RaiseAndSetIfChanged(ref gmtemplate, value); }
		}

		private string lobbylevelname;
		public string LobbyLevelName
		{
			get => lobbylevelname;
			set { this.RaiseAndSetIfChanged(ref lobbylevelname, value); }
		}

		private string md5;
		public string MD5
		{
			get => md5;
			set { this.RaiseAndSetIfChanged(ref md5, value); }
		}

		private string menulevelname;
		public string MenuLevelName
		{
			get => menulevelname;
			set { this.RaiseAndSetIfChanged(ref menulevelname, value); }
		}

		private string name;
		public string Name
		{
			get => name;
			set { this.RaiseAndSetIfChanged(ref name, value); }
		}

		private string numplayers;
		public string NumPlayers
		{
			get => numplayers;
			set { this.RaiseAndSetIfChanged(ref numplayers, value); }
		}

		private string photobooth;
		public string PhotoBooth
		{
			get => photobooth;
			set { this.RaiseAndSetIfChanged(ref photobooth, value); }
		}

		private string startuplevelname;
		public string StartupLevelName
		{
			get => startuplevelname;
			set { this.RaiseAndSetIfChanged(ref startuplevelname, value); }
		}

		private string tags;
		public string Tags
		{
			get => tags;
			set { this.RaiseAndSetIfChanged(ref tags, value); }
		}

		private string type;
		public string Type
		{
			get => type;
			set { this.RaiseAndSetIfChanged(ref type, value); }
		}

		private string uuid;
		public string UUID
		{
			get => uuid;
			set { this.RaiseAndSetIfChanged(ref uuid, value); }
		}

		private Int32 version;
		public Int32 Version
		{
			get => version;
			set { this.RaiseAndSetIfChanged(ref version, value); }
		}


		public List<String> TargetModes { get; set; } = new List<string>();

		private long timestamp;

		public long Timestamp
		{
			get => timestamp;
			set { this.RaiseAndSetIfChanged(ref timestamp, value); }
		}

		private DateTime modifiedDate;

		public DateTime ModifiedDate
		{
			get => modifiedDate;
			set { this.RaiseAndSetIfChanged(ref modifiedDate, value); }
		}


		public void Set(ModuleInfo moduleInfo)
		{
			PropertyCopier<ModuleInfo, ModuleInfo>.Copy(moduleInfo, this);
		}

		public void LoadFromXml(XDocument modMetaXml, bool isAsync = false)
		{
			bool moduleInfoLoaded = false;
			//try
			//{
			var moduleInfoXml = modMetaXml.XPathSelectElement("save/region/node/children/node[@id='ModuleInfo']");

			if (moduleInfoXml != null)
			{
				PropertyInfo[] propInfo = typeof(ModuleInfo).GetProperties();
				foreach (PropertyInfo property in propInfo)
				{
					if (property.Name == "TargetModes") continue;
					if (property.Name == "Timestamp") continue;
					if (property.Name == "ModifiedDate") continue;

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
					else if(property.PropertyType == typeof(string))
					{
						property.SetValue(this, value);
					}
					//Log.Here().Activity("Set {0} to {1}", property.Name, value);
				}

				Log.Here().Activity("[{0}] Module info parsing complete. Checking target modes.", this.Name);

				if (!isAsync)
				{
					TargetModes.Clear();
				}

				moduleInfoLoaded = true;
			}
			else
			{
				Log.Here().Error("Error selecting path (\"{0}\") from mod meta.lsx file.", @"save / region / node / children / node[@id = 'ModuleInfo']");
			}
			//}
			//catch (Exception ex)
			//{
			//	Log.Here().Error("Error parsing mod meta.lsx: {0}", ex.ToString());
			//}

			/* The timestamp was removed from the xml in a recent version.
			try
			{
				var timeStampXml = modMetaXml.XPathSelectElement("save/header");
				if (timeStampXml != null && timeStampXml.HasAttributes)
				{
					var timeAtt = timeStampXml.Attribute("time");
					if (timeAtt != null)
					{
						var success = long.TryParse(timeAtt.Value, out timestamp);

						if(success)
						{
							ModifiedDate = DateTimeOffset.FromUnixTimeSeconds(timestamp).Date;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Log.Here().Error("Error getting timestamp from meta.lsx: {0}", ex.ToString());
			}
			*/

			if (moduleInfoLoaded)
			{
				try
				{
					var modTargets = modMetaXml.Descendants("node").FirstOrDefault(x => (string)x.Attribute("id") == "TargetModes").Descendants("node").Descendants("attribute");

					if (modTargets != null)
					{
						if (!isAsync)
						{
							TargetModes.AddRange(modTargets.Select(t => t.Attribute("value")).Where(v => !String.IsNullOrEmpty(v?.Value)).Select(x => x.Value));
						}
						else
						{
							RxApp.MainThreadScheduler.Schedule(() =>
							{
								TargetModes.AddRange(modTargets.Select(t => t.Attribute("value")).Where(v => !String.IsNullOrEmpty(v?.Value)).Select(x => x.Value));
							});
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
