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
		private ProjectAppData projectAppData;

		public ProjectAppData ProjectAppData
		{
			get { return projectAppData; }
			set
			{
				projectAppData = value;
				RaisePropertyChanged("ProjectAppData");
			}
		}

		private ProjectInfo projectInfo;

		public ProjectInfo ProjectInfo
		{
			get { return projectInfo; }
			set
			{
				projectInfo = value;
				RaisePropertyChanged("ProjectInfo");
			}
		}

		private ModuleInfo moduleInfo;

		public ModuleInfo ModuleInfo
		{
			get { return moduleInfo; }
			set
			{
				moduleInfo = value;
				RaisePropertyChanged("ModuleInfo");
				RaisePropertyChanged("Dependencies");
				RaisePropertyChanged("Name");
			}
		}

		private SourceControlData gitData;

		public SourceControlData GitData
		{
			get { return gitData; }
			set
			{
				gitData = value;
				RaisePropertyChanged("GitGenerated");
			}
		}

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

		public bool GitGenerated
		{
			get
			{
				return GitData != null;
			}
		}


		private string version;

		public string Version
		{
			get { return version; }
			set
			{
				version = value;
				RaisePropertyChanged("Version");
			}
		}

		public DateTime? LastBackup
		{
			get
			{
				if (ProjectAppData != null) return ProjectAppData.LastBackup;
				return null;
			}
			set
			{
				if (ProjectAppData != null)
				{
					ProjectAppData.LastBackup = value;
					RaisePropertyChanged("LastBackup");
					RaisePropertyChanged("LastBackupText");
				}
			}
		}

		public string LastBackupText
		{
			get
			{
				if(LastBackup != null)
				{
					return LastBackup?.ToShortDateString();
				}
				return "";
			}
		}

		private bool selected;

		public bool Selected
		{
			get { return selected; }
			set
			{
				selected = value;
				RaisePropertyChanged("Selected");
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

				//(major version << 28) | (minor version << 24) | (revision << 16) | (build << 0)
				var major = (ModuleInfo.Version >> 28);
				var minor = (ModuleInfo.Version >> 24) & 0x0F;
				var revision = (ModuleInfo.Version >> 16) & 0xFF;
				var build = (ModuleInfo.Version & 0xFFFF);
				//var version = ((ModuleInfo.Version << 28) | (ModuleInfo.Version << 24) | (ModuleInfo.Version << 16) | (ModuleInfo.Version << 0)).ToString("X");
				//Log.Here().Important("[{5}] Bitshift test: {6} = {4} = {0}.{1}.{2}.{3}", major, minor, revision, build, version, ModuleInfo.Name, ModuleInfo.Version);
				Version = String.Format("{0}.{1}.{2}.{3}", major, minor, revision, build);
			}
		}
	}
}
