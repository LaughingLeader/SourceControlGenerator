using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;
using LL.DOS2.SourceControl.Data.Xml;

namespace LL.DOS2.SourceControl.Data
{
	public class ModProjectData
	{
		public ProjectInfo ProjectInfo { get; set; }
		public ModuleInfo ModInfo { get; set; }
		public List<DependencyInfo> Dependencies { get; set; }

		public string Name
		{
			get => ModInfo.Name;
		}

		public string Tooltip { get; set; }

		public ModProjectData(FileInfo ModMetaFile, string ProjectsFolderPath)
		{
			this.ModInfo = new ModuleInfo();
			this.ProjectInfo = new ProjectInfo();
			this.Dependencies = new List<DependencyInfo>();		

			if (ModMetaFile != null)
			{
				XDocument modMetaXml = null;
				try
				{
					modMetaXml = XDocument.Load(ModMetaFile.OpenRead());
				}
				catch (Exception ex)
				{
					Log.Here().Error("Error loading mod meta.lsx: {0}", ex.ToString());

				}

				if (modMetaXml != null)
				{
					this.ModInfo.LoadFromXml(modMetaXml);

					try
					{
						var dependencyInfoXml = modMetaXml.XPathSelectElement("save/region/node/children/node[@id='Dependencies']/children");
						foreach (var node in dependencyInfoXml.Elements())
						{
							DependencyInfo dependencyInfo = new DependencyInfo()
							{
								Folder = XmlDataHelper.GetAttributeValue(node, "Folder"),
								MD5 = XmlDataHelper.GetAttributeValue(node, "MD5"),
								Name = XmlDataHelper.GetAttributeValue(node, "Name"),
								Version = XmlDataHelper.GetAttributeValue(node, "Version")
							};
							Dependencies.Add(dependencyInfo);
							Log.Here().Activity("[{0}] Dependency ({1}) added.", this.ModInfo.Name, dependencyInfo.Name);
						}
					}
					catch (Exception ex)
					{
						Log.Here().Error("Error parsing mod dependencies: {0}", ex.ToString());
					}
				}
				else
				{
					
				}
			}

			try
			{
				string projectMetaFilePath = Path.Combine(ProjectsFolderPath, ModInfo.Name, "meta.lsx");

				Log.Here().Activity("Attempting to load project meta.lsx at {0}", projectMetaFilePath);

				FileInfo projectMetaFile = new FileInfo(projectMetaFilePath);

				if (projectMetaFile != null)
				{
					var projectMetaXml = XDocument.Load(projectMetaFile.OpenRead());
					this.ProjectInfo.LoadFromXml(projectMetaXml);
				}
			}
			catch (Exception ex)
			{
				Log.Here().Error("Error loading project meta.lsx for {0}: {1}", this.ModInfo.Name, ex.ToString());
			}

			if(ModInfo != null)
			{
				var tooltipText = "";
				if(!String.IsNullOrEmpty(ModInfo.Author))
				{
					tooltipText = "created by " + ModInfo.Author;
					if (ModInfo.TargetModes != null && ModInfo.TargetModes.Count > 0)
					{
						tooltipText = tooltipText+ " [" + String.Join(", ", ModInfo.TargetModes.ToArray()) + "]";
					}
				}

				if (ProjectInfo != null && !String.IsNullOrEmpty(ProjectInfo.Type))
				{
					tooltipText = ProjectInfo.Type + " mod " + tooltipText;
				}
				else
				{
					tooltipText = "Mod " + tooltipText;
				}

				Tooltip = tooltipText;
			}
		}
	}
}
