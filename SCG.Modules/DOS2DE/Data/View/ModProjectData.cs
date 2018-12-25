using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;
using SCG.Commands;
using SCG.Data.Xml;
using SCG.Modules.DOS2DE.Core;
using SCG.Interfaces;
using SCG.Data;
using SCG.Data.App;

namespace SCG.Modules.DOS2DE.Data.View
{
	public class ModProjectData : PropertyChangedBase, IProjectData
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
				GitGenerated = gitData != null;
			}
		}

		public List<DependencyInfo> Dependencies { get; set; }

		public string DisplayName
		{
			get => ModuleInfo.Name;
			set
			{
				ModuleInfo.Name = value;
			}
		}

		public string UUID
		{
			get => ModuleInfo.UUID;
			set
			{
				ModuleInfo.UUID = value;
			}
		}

		public string ProjectName { get; set; }

		/// <summary>
		/// The directory name that holds the project meta.lsx. May be either the same as the mod folder name, or just the mod name itself, depending on if it's an imported project.
		/// </summary>
		public string ProjectFolder { get; set; }

		/// <summary>
		/// For imported projects, the UUID used in the folder name may be different from the actual UUID.
		/// </summary>
		public string FolderUUID { get; set; }

		/// <summary>
		/// The project's mod folder name. Also used for the repo.
		/// </summary>
		public string FolderName
		{
			get
			{
				if(this.ModuleInfo != null)
				{
					return ModuleInfo.Folder;
				}
				return ProjectName;
			}
		}

		private string shortDescription;

		public string ShortDescription
		{
			get
			{
				if(shortDescription == null)
				{
					shortDescription = ModuleInfo.Description.Truncate(30, "...");
					RaisePropertyChanged("ShortDescription");
				}
				return shortDescription;
			}
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

		private DateTime? lastBackup = null;

		public DateTime? LastBackup
		{
			get
			{
				return lastBackup;
			}
			set
			{
				lastBackup = value;
				RaisePropertyChanged("LastBackup");
				RaisePropertyChanged("LastBackupText");
			}
		}

		public string LastBackupText
		{
			get
			{
				if(LastBackup != null)
				{
					return LastBackup.ToString();
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

		private string thumbnailPath;

		public string ThumbnailPath
		{
			get { return thumbnailPath; }
			set
			{
				thumbnailPath = value;
				RaisePropertyChanged("ThumbnailPath");
			}
		}


		private Visibility thumbnailExists = Visibility.Collapsed;

		public Visibility ThumbnailExists
		{
			get { return thumbnailExists; }
			set
			{
				thumbnailExists = value;
				RaisePropertyChanged("ThumbnailExists");
			}
		}

		/*
		private CachedImageSource cachedImageSource;

		public CachedImageSource ThumbnailSource
		{
			get { return cachedImageSource; }
			set
			{
				cachedImageSource = value;
				RaisePropertyChanged("ThumbnailSource");
			}
		}
		*/

		private string modMetaFilePath;

		public string ModMetaFilePath
		{
			get { return modMetaFilePath; }
			set
			{
				modMetaFilePath = value;
				RaisePropertyChanged("ModMetaFilePath");
			}
		}

		private string projectMetaFilePath;

		public string ProjectMetaFilePath
		{
			get { return projectMetaFilePath; }
			set
			{
				projectMetaFilePath = value;
				RaisePropertyChanged("ProjectMetaFilePath");
			}
		}


		public ICommand OpenBackupFolder { get; private set; }

		public ICommand OpenGitFolder { get; private set; }

		public ICommand OpenModsFolder { get; private set; }

		public ICommand OpenPublicFolder { get; private set; }

		public ICommand OpenEditorFolder { get; private set; }

		public ICommand OpenProjectFolder { get; private set; }

		private void openBackupFolder()
		{
			Log.Here().Activity("Opening backup folder!");
			DOS2DECommands.OpenBackupFolder(this);
		}

		public bool DataIsNewer(ModProjectData OtherData)
		{
			//if (OtherData.ModuleInfo.Timestamp > this.ModuleInfo.Timestamp || OtherData.ProjectInfo.Timestamp > this.ProjectInfo.Timestamp) return true;
			if (OtherData.ModuleInfo.ModifiedDate.Ticks > this.ModuleInfo.ModifiedDate.Ticks || OtherData.ProjectInfo.CreationDate.Ticks > this.ProjectInfo.CreationDate.Ticks) return true;
			return false;
		}

		public void UpdateData(ModProjectData OtherData)
		{
			this.ModuleInfo.Set(OtherData.ModuleInfo);
			this.ProjectInfo.Set(OtherData.ProjectInfo);
			CreateTooltip();
			SetVersion();
		}

		private void Init()
		{
			this.ModuleInfo = new ModuleInfo();
			this.ProjectInfo = new ProjectInfo();
			this.Dependencies = new List<DependencyInfo>();

			//OpenBackupFolder = new CallbackCommand();
			//OpenModsFolder = new CallbackCommand();
			//OpenPublicFolder = new CallbackCommand();
			//OpenEditorFolder = new CallbackCommand();
			//OpenProjectFolder = new CallbackCommand();

			OpenBackupFolder = new ActionCommand(() => { DOS2DECommands.OpenBackupFolder(this); });
			OpenGitFolder = new ActionCommand(() => { DOS2DECommands.OpenGitFolder(this); });
			OpenModsFolder = new ActionCommand(() => { DOS2DECommands.OpenModsFolder(this); });
			OpenPublicFolder = new ActionCommand(() => { DOS2DECommands.OpenPublicFolder(this); });
			OpenEditorFolder = new ActionCommand(() => { DOS2DECommands.OpenEditorFolder(this); });
			OpenProjectFolder = new ActionCommand(() => { DOS2DECommands.OpenProjectFolder(this); });

			//RaisePropertyChanged("OpenBackupFolder");
			//RaisePropertyChanged("OpenModsFolder");
			//RaisePropertyChanged("OpenPublicFolder");
			//RaisePropertyChanged("OpenEditorFolder");
			//RaisePropertyChanged("OpenProjectFolder");
		}

		public ModProjectData()
		{
			Init();
		}

		public static ModProjectData Test(string name)
		{
			var data = new ModProjectData();
			data.moduleInfo.Name = name;

			return data;
		}

		public void CreateTooltip()
		{
			var tooltipText = "";
			if (!String.IsNullOrEmpty(ModuleInfo.Author))
			{
				tooltipText = "created by " + ModuleInfo.Author;
				if (ModuleInfo.TargetModes != null && ModuleInfo.TargetModes.Count > 0)
				{
					tooltipText = tooltipText + " [" + String.Join(", ", ModuleInfo.TargetModes.ToArray()) + "]";
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

		public void SetVersion()
		{
			//(major version << 28) | (minor version << 24) | (revision << 16) | (build << 0)
			var major = (ModuleInfo.Version >> 28);
			var minor = (ModuleInfo.Version >> 24) & 0x0F;
			var revision = (ModuleInfo.Version >> 16) & 0xFF;
			var build = (ModuleInfo.Version & 0xFFFF);
			//var version = ((ModuleInfo.Version << 28) | (ModuleInfo.Version << 24) | (ModuleInfo.Version << 16) | (ModuleInfo.Version << 0)).ToString("X");
			//Log.Here().Important("[{5}] Bitshift test: {6} = {4} = {0}.{1}.{2}.{3}", major, minor, revision, build, version, ModuleInfo.Name, ModuleInfo.Version);
			Version = String.Format("{0}.{1}.{2}.{3}", major, minor, revision, build);
		}

		public void ReloadData()
		{
			if(File.Exists(ModMetaFilePath))
			{
				var modMetaXml = XDocument.Load(ModMetaFilePath);
				this.ModuleInfo.LoadFromXml(modMetaXml);
				LoadDependencies(modMetaXml);
				ModuleInfo.RaisePropertyChanged(String.Empty);
			}

			if (File.Exists(ProjectMetaFilePath))
			{
				FileInfo projectMetaFile = new FileInfo(ProjectMetaFilePath);

				var stream = projectMetaFile.OpenRead();
				var projectMetaXml = XDocument.Load(stream);
				this.ProjectInfo.LoadFromXml(projectMetaXml);
				stream.Close();

				LoadThumbnail(projectMetaFile.Directory.FullName);
				ProjectInfo.RaisePropertyChanged(String.Empty);
				
			}
		}

		private void LoadDependencies(XDocument modMetaXml)
		{
			try
			{
				var dependencyInfoXml = modMetaXml.XPathSelectElement("save/region/node/children/node[@id='Dependencies']");
				if (dependencyInfoXml.HasElements)
				{
					foreach (var node in dependencyInfoXml.Element("children").Elements())
					{
						DependencyInfo dependencyInfo = new DependencyInfo()
						{
							Folder = XmlDataHelper.GetDOS2AttributeValue(node, "Folder"),
							MD5 = XmlDataHelper.GetDOS2AttributeValue(node, "MD5"),
							Name = XmlDataHelper.GetDOS2AttributeValue(node, "Name"),
							Version = XmlDataHelper.GetDOS2AttributeValue(node, "Version")
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
		}

		private void LoadThumbnail(string projectDirectory)
		{
			Log.Here().Activity($"Checking {projectDirectory} for thumnails.");

			var thumbnail = Directory.GetFiles(projectDirectory, "thumbnail.*", SearchOption.TopDirectoryOnly);
			if (thumbnail.Length > 0)
			{
				var thumbpath = thumbnail.FirstOrDefault();
				if (FileCommands.IsValidImage(thumbpath))
				{
					ThumbnailPath = thumbpath;

					/*
					if(ThumbnailSource == null)
					{
						ThumbnailSource = new CachedImageSource();
					}

					ThumbnailSource.Init(ThumbnailPath);
					*/

					ThumbnailExists = Visibility.Visible;

					Log.Here().Activity($"Set thumbnail path to {thumbpath}");
				}
				else
				{
					Log.Here().Error($"{thumbpath} is not a valid image file.");
				}
			}
		}

		public ModProjectData(FileInfo ModMetaFile, string ProjectsFolderPath)
		{
			Init();

			if (ModMetaFile != null)
			{
				ModMetaFilePath = ModMetaFile.FullName;

				XDocument modMetaXml = null;
				try
				{
					var stream = ModMetaFile.OpenRead();
					modMetaXml = XDocument.Load(stream);
					stream.Close();
				}
				catch (Exception ex)
				{
					Log.Here().Error("Error loading mod meta.lsx: {0}", ex.ToString());

				}

				if (modMetaXml != null)
				{
					this.ModuleInfo.LoadFromXml(modMetaXml);

					LoadDependencies(modMetaXml);

					Log.Here().Important("[{0}] All mod data loaded.", this.ModuleInfo.Name);

					ModuleInfo.ModifiedDate = ModMetaFile.LastWriteTime;
				}
				else
				{
					Log.Here().Error("Error loading mod meta.lsx: modMetaXml is null. Is this an xml file?");
				}


			}

			if(ModuleInfo.Folder.Contains(ModuleInfo.UUID))
			{
				ProjectName = ModuleInfo.Folder.Replace("_" + ModuleInfo.UUID, "");
				FolderUUID = ModuleInfo.UUID;
			}
			else
			{
				var UUIDStartIndex = ModuleInfo.Folder.LastIndexOf('_');
				if(UUIDStartIndex > 0)
				{

					ProjectName = ModuleInfo.Folder.Replace(ModuleInfo.Folder.Substring(UUIDStartIndex), "");
					FolderUUID = ModuleInfo.Folder.Substring(UUIDStartIndex + 1);
				}
			}

			string projectDirectory = Path.Combine(ProjectsFolderPath, ProjectName);

			if (!Directory.Exists(projectDirectory))
			{
				projectDirectory = Path.Combine(ProjectsFolderPath, ModuleInfo.Folder);

				if (Directory.Exists(projectDirectory))
				{
					ProjectFolder = ModuleInfo.Folder;
				}
				else
				{
					Log.Here().Error($"Project directory not found for {ModuleInfo.Name} at {Path.Combine(ProjectsFolderPath, ProjectName)} and {projectDirectory}.");
				}
			}
			else
			{
				ProjectFolder = ProjectName;
			}

			//Log.Here().Important($"Project name set to {ProjectName}");

			try
			{
				string projectMetaFilePath = Path.Combine(projectDirectory, "meta.lsx");

				Log.Here().Activity("Attempting to load project meta.lsx at {0}", projectMetaFilePath);

				FileInfo projectMetaFile = new FileInfo(projectMetaFilePath);

				ProjectMetaFilePath = projectMetaFile.FullName;

				if (projectMetaFile != null)
				{
					var projectMetaXml = XDocument.Load(projectMetaFile.OpenRead());
					this.ProjectInfo.LoadFromXml(projectMetaXml);

					LoadThumbnail(projectDirectory);

					ProjectInfo.CreationDate = projectMetaFile.CreationTime;
				}
			}
			catch (Exception ex)
			{
				Log.Here().Error("Error loading project meta.lsx for {0}: {1}", this.ModuleInfo.Name, ex.ToString());
			}

			if(ModuleInfo != null)
			{
				CreateTooltip();
				SetVersion();
			}
		}
	}
}
