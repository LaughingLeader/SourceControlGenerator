﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;
using ReactiveUI;
using SCG.Util;

namespace SCG.Data.Xml
{
	public class ProjectInfo : ReactiveObject
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

		private long timestamp;

		public long Timestamp
		{
			get { return timestamp; }
			set { timestamp = value; }
		}

		private DateTime creationDate;

		public DateTime CreationDate
		{
			get => creationDate;
			set
			{
				this.RaiseAndSetIfChanged(ref creationDate, value);
				CreationDateString = creationDate.ToString("d");
				Timestamp = ((DateTimeOffset)creationDate).ToUnixTimeSeconds();
			}
		}

		private string creationDateString;

		public string CreationDateString
		{
			get => creationDateString;
			set { this.RaiseAndSetIfChanged(ref creationDateString, value); }
		}

		public void Set(ProjectInfo projectInfo)
		{
			PropertyCopier<ProjectInfo, ProjectInfo>.Copy(projectInfo, this);
		}

		public void LoadFromXml(XDocument projectMetaXml)
		{
			var rootXml = projectMetaXml.XPathSelectElement("save/region/node[@id='root']");

			if(rootXml != null)
			{
				this.Name = XMLHelper.UnescapeXml(XmlDataHelper.GetDOS2AttributeValue(rootXml, "Name"));
				this.Type = XmlDataHelper.GetDOS2AttributeValue(rootXml, "Type");
				this.Module = XmlDataHelper.GetDOS2AttributeValue(rootXml, "Module");
				this.UUID = XmlDataHelper.GetDOS2AttributeValue(rootXml, "UUID");
			}
			else
			{
				Log.Here().Error("Error selecting path (\"{0}\") from project meta.lsx file.", @"save/region/node[@id='root']");
			}

			//<header version="2" time="1523634729" />
			/* This value was removed from the xml in a recent version.
			var timeStampXml = projectMetaXml.XPathSelectElement("save/header");
			if(timeStampXml != null && timeStampXml.HasAttributes)
			{
				var timeAtt = timeStampXml.Attribute("time");
				if(timeAtt != null)
				{
					var success = long.TryParse(timeAtt.Value, out timestamp);
					if(success)
					{
						CreationDate = DateTimeOffset.FromUnixTimeSeconds(timestamp).Date;
					}
				}
			}
			*/
		}
	}
}
