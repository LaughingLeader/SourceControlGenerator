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
	public class ModProjectData : PropertyChangedBase
	{
		public ProjectInfo ProjectInfo { get; set; }
		public ModuleInfo ModuleInfo { get; set; }
		public List<DependencyInfo> Dependencies { get; set; }

		public string Name
		{
			get => ModuleInfo.Name;
		}

		private string tooltip;

		public string Tooltip
		{
			get { return tooltip; }
			set
			{
				tooltip = value;
				RaisePropertyChanged("Tooltip");
			}
		}


		private bool gitGenerated = false;

		public bool GitGenerated
		{
			get { return gitGenerated; }
			set
			{
				gitGenerated = value;
				RaisePropertyChanged("GitGenerated");
			}
		}


		public string Version
		{
			get
			{
				//(major version << 28) | (minor version << 24) | (revision << 16) | (build << 0)
				if(ModuleInfo != null)
				{
					return ModuleInfo.Version;
				}
				return "";
			}
		}



		public ModProjectData(FileInfo ModMetaFile, string ProjectsFolderPath)
		{
			this.ModuleInfo = new ModuleInfo();
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
					this.ModuleInfo.LoadFromXml(modMetaXml);

					try
					{
						var dependencyInfoXml = modMetaXml.XPathSelectElement("save/region/node/children/node[@id='Dependencies']");
						if(dependencyInfoXml.HasElements)
						{
							foreach (var node in dependencyInfoXml.Element("children").Elements())
							{
								DependencyInfo dependencyInfo = new DependencyInfo()
								{
									Folder = XmlDataHelper.GetAttributeValue(node, "Folder"),
									MD5 = XmlDataHelper.GetAttributeValue(node, "MD5"),
									Name = XmlDataHelper.GetAttributeValue(node, "Name"),
									Version = XmlDataHelper.GetAttributeValue(node, "Version")
								};
								Dependencies.Add(dependencyInfo);
								Log.Here().Activity("[{0}] Dependency ({1}) added.", this.ModuleInfo.Name, dependencyInfo.Name);
							}
						}
						else
						{
							Log.Here().Activity("[{0}] No dependencies found.", this.ModuleInfo.Name);
						}
						
					}
					catch (Exception ex)
					{
						Log.Here().Error("Error parsing mod dependencies: {0}", ex.ToString());
					}

					Log.Here().Important("[{0}] All mod data loaded.", this.ModuleInfo.Name);
				}
				else
				{
					Log.Here().Error("Error loading mod meta.lsx: modMetaXml is null. Is this an xml file?");
				}
			}

			try
			{
				string projectMetaFilePath = Path.Combine(ProjectsFolderPath, ModuleInfo.Name, "meta.lsx");

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
				Log.Here().Error("Error loading project meta.lsx for {0}: {1}", this.ModuleInfo.Name, ex.ToString());
			}

			if(ModuleInfo != null)
			{
				var tooltipText = "";
				if(!String.IsNullOrEmpty(ModuleInfo.Author))
				{
					tooltipText = "created by " + ModuleInfo.Author;
					if (ModuleInfo.TargetModes != null && ModuleInfo.TargetModes.Count > 0)
					{
						tooltipText = tooltipText+ " [" + String.Join(", ", ModuleInfo.TargetModes.ToArray()) + "]";
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
