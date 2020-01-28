using System;
using System.Collections.Generic;
using Alphaleonis.Win32.Filesystem;
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
using SCG.Modules.DOS2DE.Views;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Text.RegularExpressions;
using SCG.Util;

namespace SCG.Modules.DOS2DE.Data.View
{
	public class ModProjectData : ReactiveObject, IProjectData, IDisposable
	{
		private ProjectAppData projectAppData;

		public ProjectAppData ProjectAppData
		{
			get { return projectAppData; }
			set
			{
				this.RaiseAndSetIfChanged(ref projectAppData, value);
			}
		}

		private ProjectInfo projectInfo;

		public ProjectInfo ProjectInfo
		{
			get { return projectInfo; }
			set
			{
				this.RaiseAndSetIfChanged(ref projectInfo, value);
			}
		}

		private ModuleInfo moduleInfo;

		public ModuleInfo ModuleInfo
		{
			get { return moduleInfo; }
			set
			{
				this.RaiseAndSetIfChanged(ref moduleInfo, value);
				this.RaisePropertyChanged("Dependencies");
				this.RaisePropertyChanged("Name");
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
					this.RaisePropertyChanged("ShortDescription");
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
				this.RaiseAndSetIfChanged(ref tooltip, value);
			}
		}

		private bool isManaged;

		public bool IsManaged
		{
			get => isManaged;
			set { this.RaiseAndSetIfChanged(ref isManaged, value); }
		}


		private bool gitGenerated = false;

		public bool GitGenerated
		{
			get { return gitGenerated; }
			set
			{
				this.RaiseAndSetIfChanged(ref gitGenerated, value);
			}
		}

		private ProjectVersionData projectVersionData;

		public ProjectVersionData VersionData
		{
			get { return projectVersionData; }
			set
			{
				this.RaiseAndSetIfChanged(ref projectVersionData, value);
				this.RaisePropertyChanged("Version");
			}
		}

		public string Version => VersionData != null ? VersionData.Version : "0.0.0.0";

		private DateTime? lastBackup = null;

		public DateTime? LastBackup
		{
			get
			{
				return lastBackup;
			}
			set
			{
				this.RaiseAndSetIfChanged(ref lastBackup, value);
				this.RaisePropertyChanged("LastBackupText");
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
				this.RaiseAndSetIfChanged(ref selected, value);
			}

		}

		private string thumbnailPath;

		public string ThumbnailPath
		{
			get { return thumbnailPath; }
			set
			{
				this.RaiseAndSetIfChanged(ref thumbnailPath, value);
			}
		}


		private bool thumbnailExists = false;

		public bool ThumbnailExists
		{
			get { return thumbnailExists; }
			set
			{
				this.RaiseAndSetIfChanged(ref thumbnailExists, value);
			}
		}

		/*
		private CachedImageSource cachedImageSource;

		public CachedImageSource ThumbnailSource
		{
			get { return cachedImageSource; }
			set
			{
				this.RaiseAndSetIfChanged(ref cachedImageSource, value);
			}
		}
		*/

		private string modMetaFilePath;

		public string ModMetaFilePath
		{
			get { return modMetaFilePath; }
			set
			{
				this.RaiseAndSetIfChanged(ref modMetaFilePath, value);
			}
		}

		private string projectMetaFilePath;

		public string ProjectMetaFilePath
		{
			get { return projectMetaFilePath; }
			set
			{
				this.RaiseAndSetIfChanged(ref projectMetaFilePath, value);
			}
		}

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
			/*
			var major = (ModuleInfo.Version >> 28);
			var minor = (ModuleInfo.Version >> 24) & 0x0F;
			var revision = (ModuleInfo.Version >> 16) & 0xFF;
			var build = (ModuleInfo.Version & 0xFFFF);
			Version = String.Format("{0}.{1}.{2}.{3}", major, minor, revision, build);
			*/
			if(VersionData == null) VersionData = new ProjectVersionData();
			VersionData.ParseInt(ModuleInfo.Version);
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
			//Log.Here().Activity($"Checking {projectDirectory} for a thumbnails.");
			bool foundThumbnail = false;

			DirectoryEnumerationFilters filter = new DirectoryEnumerationFilters
			{
				InclusionFilter = (f) =>
				{
					if(!foundThumbnail && f.FileName.IndexOf("thumbnail", StringComparison.OrdinalIgnoreCase) > -1 && FileCommands.IsValidImage(f.FullPath))
					{
						//Log.Here().Activity($"Thumbnail! {f?.FullPath} | {f?.FileName}");
						foundThumbnail = true;
						return true;
					}
					return false;
				}
			};

			string thumbnail = null;

			if(Directory.Exists(projectDirectory))
			{
				thumbnail = Directory.EnumerateFiles(projectDirectory, DirectoryEnumerationOptions.Files, filter, PathFormat.FullPath).FirstOrDefault();
			}

			if (!String.IsNullOrWhiteSpace(thumbnail))
			{
				ThumbnailPath = Path.GetFullPath(thumbnail);
				ThumbnailExists = true;
			}
		}

		#region Async
		public async Task LoadModMetaAsync(string metaFilePath)
		{
			if (File.Exists(metaFilePath))
			{
				ModMetaFilePath = metaFilePath;

				Log.Here().Activity("Meta file found for project {0}. Reading file.", metaFilePath);
				string contents = XMLHelper.EscapeXmlAttributes(await FileCommands.ReadFileAsync(metaFilePath));
				if (contents != String.Empty)
				{
					XDocument modMetaXml = null;
					try
					{
						modMetaXml = XDocument.Parse(contents);
					}
					catch (Exception ex)
					{
						Log.Here().Error("Error loading mod meta.lsx: {0}", ex.ToString());

					}

					if (modMetaXml != null)
					{
						this.ModuleInfo.LoadFromXml(modMetaXml, true);

						LoadDependencies(modMetaXml);

						Log.Here().Important("[{0}] All mod data loaded.", this.ModuleInfo.Name);

						ModuleInfo.ModifiedDate = File.GetLastWriteTime(metaFilePath);
					}
					else
					{
						Log.Here().Error("Error loading mod meta.lsx: modMetaXml is null. Is this an xml file?");
					}
				}
			}

			if (ModuleInfo.Folder.Contains(ModuleInfo.UUID))
			{
				ProjectName = ModuleInfo.Folder.Replace("_" + ModuleInfo.UUID, "");
				FolderUUID = ModuleInfo.UUID;
			}
			else
			{
				var UUIDStartIndex = ModuleInfo.Folder.LastIndexOf('_');
				if (UUIDStartIndex > 0)
				{

					ProjectName = ModuleInfo.Folder.Replace(ModuleInfo.Folder.Substring(UUIDStartIndex), "");
					FolderUUID = ModuleInfo.Folder.Substring(UUIDStartIndex + 1);
				}
			}
			//Log.Here().Important($"Project name set to {ProjectName}");

			if (ModuleInfo != null)
			{
				CreateTooltip();
				SetVersion();
			}
		}

		public async Task LoadProjectMetaAsync(string projectsFolderPath)
		{
			try
			{
				string projectDirectory = Path.Combine(projectsFolderPath, ProjectName);

				if (!Directory.Exists(projectDirectory))
				{
					projectDirectory = Path.Combine(projectsFolderPath, ModuleInfo.Folder);

					if (Directory.Exists(projectDirectory))
					{
						ProjectFolder = ModuleInfo.Folder;
					}
					else
					{
						Log.Here().Error($"Project directory not found for {ModuleInfo.Name} at {Path.Combine(projectsFolderPath, ProjectName)} and {projectDirectory}.");
						Log.Here().Important($"Checking for meta.lsx files in '{projectsFolderPath}'.");
						var projectMetaFiles = Directory.EnumerateFiles(projectsFolderPath, DirectoryEnumerationOptions.Recursive | DirectoryEnumerationOptions.Files, new DirectoryEnumerationFilters
						{
							InclusionFilter = (f) =>
							{
								return f.FileName.Equals("meta.lsx", StringComparison.OrdinalIgnoreCase);
							}
						});

						if(projectMetaFiles.Count() > 0)
						{
							Regex regex = new Regex("^.*Module.*value=\"([^\"]+)\".* $", RegexOptions.IgnoreCase | RegexOptions.Multiline);

							foreach (var f in projectMetaFiles)
							{
								string contents = await FileCommands.ReadFileAsync(f);
								var match = regex.Match(contents);
								if(match.Success)
								{
									string modUUID = match.Groups[1].Value;
									if(modUUID.Equals(this.UUID, StringComparison.OrdinalIgnoreCase))
									{
										projectDirectory = Path.GetDirectoryName(f);
										Log.Here().Important($"Found project folder by UUID {this.UUID} at {projectDirectory}.");
										break;
									}
								}
							}
						}
					}
				}
				else
				{
					ProjectFolder = ProjectName;
				}

				if(Directory.Exists(projectDirectory))
				{
					string projectMetaFilePath = Path.ChangeExtension(Path.Combine(projectDirectory, "meta"), "lsx");
					ProjectMetaFilePath = Path.GetFullPath(projectMetaFilePath);

					Log.Here().Activity("Attempting to load project meta.lsx at {0}", projectMetaFilePath);
					string projectMetaFileContents = XMLHelper.EscapeXmlAttributes(await FileCommands.ReadFileAsync(projectMetaFilePath));

					if (!String.IsNullOrWhiteSpace(projectMetaFileContents))
					{
						var projectMetaXml = XDocument.Parse(projectMetaFileContents);
						this.ProjectInfo.LoadFromXml(projectMetaXml);
						ProjectInfo.CreationDate = File.GetCreationTime(projectMetaFilePath);
					}

					LoadThumbnail(projectDirectory);
				}
				else
				{
					Log.Here().Error($"Project directory not found for {ModuleInfo.Name} at '{projectDirectory}'.");
				}
			}
			catch (Exception ex)
			{
				Log.Here().Error("Error loading project meta.lsx for {0}: {1}", this.ModuleInfo.Name, ex.ToString());
			}
		}

		public async Task LoadAllDataAsync(string metaFilePath, string projectsFolderPath)
		{
			await LoadModMetaAsync(metaFilePath);
			await LoadProjectMetaAsync(projectsFolderPath);
		}

		public async Task ReloadDataAsync()
		{
			Log.Here().Activity("Reloading mod data.");

			if (File.Exists(ModMetaFilePath))
			{
				string contents = await FileCommands.ReadFileAsync(ModMetaFilePath);
				if (!String.IsNullOrWhiteSpace(contents))
				{
					var modMetaXml = XDocument.Parse(contents);
					if (modMetaXml != null)
					{
						this.ModuleInfo.LoadFromXml(modMetaXml);
						LoadDependencies(modMetaXml);
						ModuleInfo.RaisePropertyChanged(String.Empty);
					}
				}
			}

			if (File.Exists(ProjectMetaFilePath))
			{
				string contents = XMLHelper.EscapeXmlAttributes(await FileCommands.ReadFileAsync(ProjectMetaFilePath));
				if (!String.IsNullOrWhiteSpace(contents))
				{
					var projectMetaXml = XDocument.Parse(contents);
					if (projectMetaXml != null)
					{
						this.ProjectInfo.LoadFromXml(projectMetaXml);
						LoadThumbnail(Path.GetDirectoryName(ProjectMetaFilePath));
						ProjectInfo.RaisePropertyChanged(String.Empty);
					}
				}
			}
		}
		#endregion

		#region Synchronous
		public void LoadModMeta(string metaFilePath)
		{
			if (File.Exists(metaFilePath))
			{
				ModMetaFilePath = metaFilePath;

				Log.Here().Activity("Meta file found for project {0}. Reading file.", metaFilePath);
				string contents = XMLHelper.EscapeXmlAttributes(FileCommands.ReadFile(metaFilePath));
				if (contents != String.Empty)
				{
					XDocument modMetaXml = null;
					try
					{
						modMetaXml = XDocument.Parse(contents);
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

						ModuleInfo.ModifiedDate = File.GetLastWriteTime(metaFilePath);
					}
					else
					{
						Log.Here().Error("Error loading mod meta.lsx: modMetaXml is null. Is this an xml file?");
					}
				}
			}

			if (ModuleInfo.Folder.Contains(ModuleInfo.UUID))
			{
				ProjectName = ModuleInfo.Folder.Replace("_" + ModuleInfo.UUID, "");
				FolderUUID = ModuleInfo.UUID;
			}
			else
			{
				var UUIDStartIndex = ModuleInfo.Folder.LastIndexOf('_');
				if (UUIDStartIndex > 0)
				{

					ProjectName = ModuleInfo.Folder.Replace(ModuleInfo.Folder.Substring(UUIDStartIndex), "");
					FolderUUID = ModuleInfo.Folder.Substring(UUIDStartIndex + 1);
				}
			}
			//Log.Here().Important($"Project name set to {ProjectName}");

			if (ModuleInfo != null)
			{
				CreateTooltip();
				SetVersion();
			}
		}

		public void LoadProjectMeta(string projectsFolderPath)
		{
			try
			{
				string projectDirectory = Path.Combine(projectsFolderPath, ProjectName);

				if (!Directory.Exists(projectDirectory))
				{
					projectDirectory = Path.Combine(projectsFolderPath, ModuleInfo.Folder);

					if (Directory.Exists(projectDirectory))
					{
						ProjectFolder = ModuleInfo.Folder;
					}
					else
					{
						Log.Here().Error($"Project directory not found for {ModuleInfo.Name} at {Path.Combine(projectsFolderPath, ProjectName)} and {projectDirectory}.");
						Log.Here().Important($"Checking for meta.lsx files in '{projectsFolderPath}'.");
						var projectMetaFiles = Directory.EnumerateFiles(projectsFolderPath, DirectoryEnumerationOptions.Recursive | DirectoryEnumerationOptions.Files, new DirectoryEnumerationFilters
						{
							InclusionFilter = (f) =>
							{
								return f.FileName.Equals("meta.lsx", StringComparison.OrdinalIgnoreCase);
							}
						});

						if (projectMetaFiles.Count() > 0)
						{
							Regex regex = new Regex("^.*Module.*value=\"([^\"]+)\".* $", RegexOptions.IgnoreCase | RegexOptions.Multiline);

							foreach (var f in projectMetaFiles)
							{
								string contents = FileCommands.ReadFile(f);
								var match = regex.Match(contents);
								if (match.Success)
								{
									string modUUID = match.Groups[1].Value;
									if (modUUID.Equals(this.UUID, StringComparison.OrdinalIgnoreCase))
									{
										projectDirectory = Path.GetDirectoryName(f);
										Log.Here().Important($"Found project folder by UUID {this.UUID} at {projectDirectory}.");
										break;
									}
								}
							}
						}
					}
				}
				else
				{
					ProjectFolder = ProjectName;
				}

				if (Directory.Exists(projectDirectory))
				{
					string projectMetaFilePath = Path.ChangeExtension(Path.Combine(projectDirectory, "meta"), "lsx");
					ProjectMetaFilePath = Path.GetFullPath(projectMetaFilePath);

					Log.Here().Activity("Attempting to load project meta.lsx at {0}", projectMetaFilePath);
					string projectMetaFileContents = FileCommands.ReadFile(projectMetaFilePath);

					if (!String.IsNullOrWhiteSpace(projectMetaFileContents))
					{
						var projectMetaXml = XDocument.Parse(projectMetaFileContents);
						this.ProjectInfo.LoadFromXml(projectMetaXml);
						ProjectInfo.CreationDate = File.GetCreationTime(projectMetaFilePath);
					}

					RxApp.MainThreadScheduler.Schedule(() =>
					{
						LoadThumbnail(projectDirectory);
					});
				}
				else
				{
					Log.Here().Error($"Project directory not found for {ModuleInfo.Name} at '{projectDirectory}'.");
				}
			}
			catch (Exception ex)
			{
				Log.Here().Error("Error loading project meta.lsx for {0}: {1}", this.ModuleInfo.Name, ex.ToString());
			}
		}

		public void LoadAllData(string metaFilePath, string projectsFolderPath)
		{
			LoadModMeta(metaFilePath);
			LoadProjectMeta(projectsFolderPath);
		}

		public void ReloadData()
		{
			Log.Here().Activity("Reloading mod data.");

			if (File.Exists(ModMetaFilePath))
			{
				string contents = XMLHelper.EscapeXmlAttributes(FileCommands.ReadFile(ModMetaFilePath));
				if (!String.IsNullOrWhiteSpace(contents))
				{
					var modMetaXml = XDocument.Parse(contents);
					if (modMetaXml != null)
					{
						this.ModuleInfo.LoadFromXml(modMetaXml);
						LoadDependencies(modMetaXml);
						ModuleInfo.RaisePropertyChanged(String.Empty);
					}
				}
			}

			if (File.Exists(ProjectMetaFilePath))
			{
				string contents = XMLHelper.EscapeXmlAttributes(FileCommands.ReadFile(ProjectMetaFilePath));
				if (!String.IsNullOrWhiteSpace(contents))
				{
					var projectMetaXml = XDocument.Parse(contents);
					if (projectMetaXml != null)
					{
						this.ProjectInfo.LoadFromXml(projectMetaXml);
						LoadThumbnail(Path.GetDirectoryName(ProjectMetaFilePath));
						ProjectInfo.RaisePropertyChanged(String.Empty);
					}
				}
			}
		}
		#endregion

		public ModProjectData()
		{
			Init();

			if(DebugMode)
			{
				ModuleInfo = new ModuleInfo()
				{
					Name = "Test",
					Author = "LaughingLeader",
					ModifiedDate = DateTime.Now,
					Description = "Hello!",
					Version = 268435456
				};
				SetVersion();
				ProjectInfo = new ProjectInfo()
				{
					CreationDate = DateTime.Now,
					Name = "Test"
				};

				IsManaged = true;
				//ThumbnailPath = @"G:\Divinity Original Sin 2\DefEd\Data\Projects\LeaderLib\thumbnail.png";
				ThumbnailExists = false;
			}
			
		}

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					Log.Here().Activity("Disposing");
				}

				// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
				// TODO: set large fields to null.
				this.ModuleInfo = null;
				this.ProjectInfo = null;
				this.ThumbnailExists = false;
				this.ThumbnailPath = "";

				disposedValue = true;
			}
		}

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
			// TODO: uncomment the following line if the finalizer is overridden above.
			// GC.SuppressFinalize(this);
		}
		#endregion

		/// <summary>
		/// Used to made design-time test data.
		/// </summary>
		public bool DebugMode { get; set; }
	}
}
