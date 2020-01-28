using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Alphaleonis.Win32;
using Alphaleonis.Win32.Filesystem;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LSLib.LS;
using LSLib.LS.Enums;
using SCG.Core;
using SCG.Data.App;
using SCG.Modules.DOS2DE.Data.View;
using SCG.Modules.DOS2DE.Data.View.Locale;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.ObjectModel;
using System.Globalization;
using SCG.FileGen;
using SCG.Modules.DOS2DE.Data;
using SCG.Modules.DOS2DE.Core;
using SCG.Modules.DOS2DE.Windows;
using System.Windows.Media;
using DynamicData.Binding;
using ReactiveUI;
using System.Reactive.Concurrency;
using System.Text.RegularExpressions;
using SCG.Modules.DOS2DE.Data.Savable;
using SCG.Extensions;

namespace SCG.Modules.DOS2DE.Utilities
{
	public struct TextualLocaleEntry
	{
		public string Key;
		public string Content;
		public string Handle;
	}

	public static class LocaleEditorCommands
	{
		public static readonly string UnsetHandle = "ls::TranslatedStringRepository::s_HandleUnknown";

		public static HashSet<string> IgnoredHandles { get; set; } = new HashSet<string>();

		#region Loading Localization Files
		public static async Task<LocaleViewModel> LoadLocalizationDataAsync(LocaleViewModel localizationData, DOS2DEModuleData vm, ModProjectData modProject, CancellationToken? token = null)
		{
			//await new SynchronizationContextRemover();
			localizationData.LinkedProjects.Add(modProject);
			var success = await LoadProjectLocalizationDataAsync(localizationData, vm, modProject, token);
			foreach (var g in localizationData.Groups)
			{
				foreach (var f in g.DataFiles)
				{
					foreach (var k in f.Entries)
					{
						k.SetHistoryFromObject(localizationData);
					}
				}
			}

			List<ILocaleKeyEntry> removedEntries = new List<ILocaleKeyEntry>();

			await LoadLinkedFilesAsync(vm, modProject, localizationData, removedEntries);
			localizationData.ShowMissingEntriesView(removedEntries);

			return localizationData;
		}

		public static async Task<LocaleViewModel> LoadLocalizationDataAsync(LocaleViewModel localizationData, DOS2DEModuleData vm, IEnumerable<ModProjectData> modProjects, CancellationToken? token = null)
		{
			await new SynchronizationContextRemover();

			localizationData.LinkedProjects.AddRange(modProjects);
			foreach (var project in modProjects)
			{
				var success = await LoadProjectLocalizationDataAsync(localizationData, vm, project, token);
			}
			foreach (var g in localizationData.Groups)
			{
				foreach (var f in g.DataFiles)
				{
					foreach (var k in f.Entries)
					{
						k.SetHistoryFromObject(localizationData);
					}
				}
			}

			List<ILocaleKeyEntry> removedEntries = new List<ILocaleKeyEntry>();

			foreach (var project in modProjects)
			{
				await LoadLinkedFilesAsync(vm, project, localizationData, removedEntries);
			}

			localizationData.ShowMissingEntriesView(removedEntries);

			return localizationData;
		}

		public static async Task<bool> LoadProjectLocalizationDataAsync(LocaleViewModel localizationData, DOS2DEModuleData vm, ModProjectData modProjectData, CancellationToken? token = null)
		{
			try
			{
				if (token == null) token = CancellationToken.None;

				if (token.Value.IsCancellationRequested)
				{
					Log.Here().Warning($"Localization loading cancellation requested.");
					return false;
				}

				string dataRootPath = vm.Settings.DOS2DEDataDirectory;

				if (!dataRootPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
				{
					dataRootPath += Path.DirectorySeparatorChar;
				}

				string modsRoot = Path.GetFullPath(Path.Combine(dataRootPath, "Mods", modProjectData.FolderName));
				string publicRoot = Path.GetFullPath(Path.Combine(dataRootPath, "Public", modProjectData.FolderName));
				string customLocaleDir = DOS2DEDefaultPaths.CustomLocaleDirectory(vm, modProjectData);

				bool modsExists = Directory.Exists(modsRoot);
				bool publicExists = Directory.Exists(publicRoot);
				bool customExists = Directory.Exists(customLocaleDir);

				if (!modsExists && !publicExists)
				{
					Log.Here().Warning($"Failed to find root folders for mod {modProjectData.DisplayName} at path '{modsRoot}' and '{publicRoot}'.");
					return false;
				}

				if (modsExists)
				{
					string modsLocalePath = Path.Combine(modsRoot, "Localization");
					string dialogLocalePath = Path.Combine(modsRoot, "Story", "Dialogs");
					string globalsPath = Path.Combine(modsRoot, "Globals");
					string levelsPath = Path.Combine(modsRoot, "Levels");

					localizationData.ModsGroup.SourceDirectories.Add(modsLocalePath);
					localizationData.DialogGroup.SourceDirectories.Add(dialogLocalePath);

					if (Directory.Exists(modsLocalePath))
					{
						Log.Here().Activity($"Loading localization data from '{modsLocalePath}'.");
						var modsLocaleData = await LoadFilesAsync(localizationData.ModsGroup, modsLocalePath, modProjectData, token, ".lsb");
						
						localizationData.ModsGroup.DataFiles.AddRange(modsLocaleData);
					}
					else
					{
						Log.Here().Warning($"Failed to find locale folder for {modProjectData.DisplayName} at path '{modsLocalePath}'.");
						//localizationData.ModsGroup.Visibility = false;
					}

					if (Directory.Exists(dialogLocalePath))
					{
						Log.Here().Activity($"Loading dialog localization data from '{dialogLocalePath}'.");
						var dialogLocaleData = await LoadFilesAsync(localizationData.DialogGroup, dialogLocalePath, modProjectData, token, ".lsj");
						//Lock dialog files, as adding a new entry is more complicated than simply adding a key.
						dialogLocaleData.ForEach(f => {
							f.Locked = true;
							f.CanCreateFileLink = false;
						});
						localizationData.DialogGroup.DataFiles.AddRange(dialogLocaleData);
					}
					else
					{
						Log.Here().Warning($"Failed to find dialog folder for {modProjectData.DisplayName} at path '{dialogLocalePath}'.");
						//localizationData.DialogGroup.Visibility = false;
					}

					if (localizationData.Settings.LoadGlobals && Directory.Exists(globalsPath))
					{
						Log.Here().Activity($"Loading global root template data from '{globalsPath}'.");
						var globalTemplatesData = await LoadFilesAsync(localizationData.RootTemplatesGroup, globalsPath, modProjectData, token, ".lsf");
						//Lock lsf files, as adding a new entry is more complicated than simply adding a key.
						globalTemplatesData.ForEach(f => {
							f.Locked = true;
							f.CanCreateFileLink = false;
						});
						localizationData.GlobalTemplatesGroup.DataFiles.AddRange(globalTemplatesData);
					}

					if (localizationData.Settings.LoadLevelData && Directory.Exists(levelsPath))
					{
						Log.Here().Activity($"Loading level data from '{levelsPath}'.");
						var levelData = await LoadFilesAsync(localizationData.RootTemplatesGroup, levelsPath, modProjectData, token, ".lsf");
						//Lock lsf files, as adding a new entry is more complicated than simply adding a key.
						levelData.ForEach(f => {
							f.Locked = true;
							f.CanCreateFileLink = false;
						});
						localizationData.LevelDataGroup.DataFiles.AddRange(levelData);
					}
				}
				else
				{
					//localizationData.ModsGroup.Visibility = false;
					//localizationData.DialogGroup.Visibility = false;
				}

				if (publicExists)
				{
					string publicLocalePath = Path.Combine(publicRoot, "Localization");
					localizationData.PublicGroup.SourceDirectories.Add(publicLocalePath);

					if (Directory.Exists(publicLocalePath))
					{
						Log.Here().Activity($"Loading localization data from '{publicLocalePath}'.");
						var publicLocaleData = await LoadFilesAsync(localizationData.PublicGroup, publicLocalePath, modProjectData, token, ".lsb");
						localizationData.PublicGroup.DataFiles.AddRange(publicLocaleData);
					}
					else
					{
						//localizationData.PublicGroup.Visibility = false;

						Log.Here().Warning($"Failed to find locale folder for {modProjectData.DisplayName} at path '{publicLocalePath}'.");
					}
					
					string publicRootTemplatePath = Path.Combine(publicRoot, "RootTemplates");
					localizationData.RootTemplatesGroup.SourceDirectories.Add(publicRootTemplatePath);

					if (localizationData.Settings.LoadRootTemplates && Directory.Exists(publicRootTemplatePath))
					{
						Log.Here().Activity($"Loading localization data from '{publicRootTemplatePath}'.");
						var publicRootTemplatesLocaleData = await LoadFilesAsync(localizationData.RootTemplatesGroup, publicRootTemplatePath, modProjectData, token, ".lsf");
						publicRootTemplatesLocaleData.ForEach(f => {
							f.Locked = true;
							f.CanCreateFileLink = false;
						});
						localizationData.RootTemplatesGroup.DataFiles.AddRange(publicRootTemplatesLocaleData);
					}
					else
					{
						Log.Here().Warning($"Failed to find RootTemplates folder for {modProjectData.DisplayName} at path '{publicRootTemplatePath}'.");
					}
				}

				if(customExists)
				{
					var customFiles = await LoadCustomFilesAsync(customLocaleDir, modProjectData);
					localizationData.CustomGroup.DataFiles.AddRange(customFiles);
				}

				/*
				RxApp.MainThreadScheduler.Schedule(() =>
				{
					localizationData.ModsGroup.UpdateCombinedData();
					localizationData.DialogGroup.UpdateCombinedData();
					localizationData.PublicGroup.UpdateCombinedData();
					localizationData.CustomGroup.UpdateCombinedData();
					localizationData.CombinedGroup.UpdateCombinedData();
					Log.Here().Activity($"Updated all combined data.");
				});
				*/

				//localizationData.UpdateCombinedGroup();

				Log.Here().Activity($"Localization loaded.");

				return true;
			}
			catch (Exception ex)
			{
				if (!token.Value.IsCancellationRequested)
				{
					Log.Here().Error($"Error loading locale data: {ex.ToString()}");
				}
				else
				{
					Log.Here().Important($"Cancelled loading locale data: {ex.ToString()}");
				}
				return false;
			}
		}

		private static async Task<List<LocaleNodeFileData>> LoadFilesAsync(LocaleTabGroup groupData, string directoryPath, ModProjectData modProjectData, CancellationToken? token = null, params string[] fileExtensions)
		{
			List<LocaleNodeFileData> stringKeyData = new List<LocaleNodeFileData>();

			var filters = new DirectoryEnumerationFilters()
			{
				InclusionFilter = f =>
				{
					return FileCommands.FileExtensionFound(f.FileName, fileExtensions);
				},
				ErrorFilter = delegate (int errorCode, string errorMessage, string pathProcessed)
				{
					var gotException = errorCode == 5;

					if (gotException)
					{
						Log.Here().Error($"Error reading file at '{pathProcessed}': [{errorCode}]({errorMessage})");
					}

					return gotException;
				},
				RecursionFilter = f =>
				{
					Log.Here().Activity("Recursing: " + f.FileName);
					return true;
				}
			};
			if (token != null) filters.CancellationToken = token.Value;
			var lsbFiles = Directory.EnumerateFiles(directoryPath, Alphaleonis.Win32.Filesystem.DirectoryEnumerationOptions.Recursive, filters);
			var targetFiles = new ConcurrentBag<string>(lsbFiles);
			foreach (var filePath in targetFiles)
			{
				var data = await LoadResourceAsync(groupData, filePath);
				if(data != null)
				{
					data.ModProject = modProjectData;
					stringKeyData.Add(data);
				}
			}
			if (stringKeyData.Count > 0) stringKeyData = stringKeyData.OrderBy(f => f.Name).ToList();
			return stringKeyData;
		}

		public static async Task<List<LocaleCustomFileData>> LoadCustomFilesAsync(string customLocaleDir, ModProjectData modProject)
		{
			List<LocaleCustomFileData> customFiles = new List<LocaleCustomFileData>();

			var filters = new DirectoryEnumerationFilters()
			{
				InclusionFilter = f =>
				{
					return FileCommands.FileExtensionFound(f.FileName, ".json");
				},
				ErrorFilter = delegate (int errorCode, string errorMessage, string pathProcessed)
				{
					var gotException = errorCode == 5;

					if (gotException)
					{
						Log.Here().Error($"Error reading file at '{pathProcessed}': [{errorCode}]({errorMessage})");
					}

					return gotException;
				},
				RecursionFilter = f =>
				{
					return true;
				}
			};
			var files = Directory.EnumerateFiles(customLocaleDir, Alphaleonis.Win32.Filesystem.DirectoryEnumerationOptions.Recursive, filters);
			var targetFiles = new ConcurrentBag<string>(files);
			foreach (var filePath in targetFiles)
			{
				var data = await LoadCustomFileAsync(filePath);
				if (data != null)
				{
					customFiles.Add(data);
				}
			}

			if (customFiles.Count > 0) customFiles = customFiles.OrderBy(f => f.Name).ToList();

			return customFiles;
		}

		private static async Task<LocaleCustomFileData> LoadCustomFileAsync(string path)
		{
			try
			{
				LocaleCustomFileData fileData = await JsonInterface.DeserializeObjectAsync<LocaleCustomFileData>(path);
				fileData.CanClose = true;
				return fileData;
			}
			catch (Exception ex)
			{
				Log.Here().Error($"Error deserializing '{path}': {ex.ToString()}");
				return null;
			}
		}

		public static async Task<LocaleNodeFileData> LoadResourceAsync(LocaleTabGroup groupData, string path, CancellationToken? token = null)
		{
			return await Task.Run(() =>
			{
				if (token != null && token.Value.IsCancellationRequested) return null;
				return LoadResource(groupData, path);
			});
		}

		public static LocaleNodeFileData LoadResource(LocaleTabGroup groupData, string path)
		{
			try
			{
				var resourceFormat = ResourceFormat.LSB;
				if (FileCommands.FileExtensionFound(path, ".lsj"))
				{
					resourceFormat = ResourceFormat.LSJ;
				}
				else if (FileCommands.FileExtensionFound(path, ".lsf"))
				{
					resourceFormat = ResourceFormat.LSF;
				}

				var resource = LSLib.LS.ResourceUtils.LoadResource(path, resourceFormat);

				var data = new LocaleNodeFileData(groupData, resourceFormat, resource, path, Path.GetFileNameWithoutExtension(path));
				var entries = LoadFromResource(resource, resourceFormat);
				if (entries.Count <= 0 && resourceFormat == ResourceFormat.LSF) // Root templates without any translated string nodes
				{
					return null;
				}
				else
				{
					foreach (var entry in entries)
					{
						data.Entries.Add(entry);
						entry.Parent = data;
					}
					return data;
				}
			}
			catch (Exception ex)
			{
				Log.Here().Error($"Error loading '{path}': {ex.ToString()}");
				return null;
			}
		}

		private static void LoadFromNode_Recursive(List<LocaleNodeKeyEntry> newEntries, Node node, ResourceFormat resourceFormat)
		{
			if(node.Attributes.TryGetValue("Type", out var nodeTypeAttribute))
			{
				if(nodeTypeAttribute.Value.GetType() == typeof(string))
				{
					string nodeType = (string)nodeTypeAttribute.Value;
					if (!nodeType.Equals("character", StringComparison.OrdinalIgnoreCase) && !nodeType.Equals("item", StringComparison.OrdinalIgnoreCase))
					{
						return;
					}
				}
			}
			newEntries.AddRange(LoadAllFromNodeAttributes(node, resourceFormat, false));
			if(node.Children.Count > 0)
			{
				foreach(var nList in node.Children.Values)
				{
					foreach(var n in nList)
					{
						LoadFromNode_Recursive(newEntries, n, resourceFormat);
					}
				}
			}
		}

		public static List<LocaleNodeKeyEntry> LoadFromResource(Resource resource, ResourceFormat resourceFormat, bool sort = false)
		{
			List<LocaleNodeKeyEntry> newEntries = new List<LocaleNodeKeyEntry>();

			try
			{
				if (resourceFormat == ResourceFormat.LSB)
				{
					var rootNode = resource.Regions.First().Value;
					foreach (var entry in rootNode.Children)
					{
						foreach (var node in entry.Value)
						{
							LocaleNodeKeyEntry localeEntry = LoadFromNode(node, resourceFormat);
							newEntries.Add(localeEntry);
						}

					}
				}
				else
				{
					var rootNode = resource.Regions.First().Value;

					if(rootNode != null)
					{
						foreach (var nList in rootNode.Children.Values)
						{
							foreach (var node in nList)
							{
								LoadFromNode_Recursive(newEntries, node, resourceFormat);
							}
						}
					}

					//var stringNodes = new List<Node>();

					//foreach (var nodeList in rootNode.Children)
					//{
					//	var nodes = FindTranslatedStringsInNodeList(nodeList);
					//	stringNodes.AddRange(nodes);
					//}

					//foreach(var node in stringNodes)
					//{
					//	LocaleNodeKeyEntry localeEntry = LoadFromNode(node, resourceFormat);
					//	newEntries.Add(localeEntry);
					//}

					/*
					foreach(var region in resource.Regions)
					{
						Debug_TraceRegion(region, 0);
					}
					*/
				}

				if (sort)
				{
					newEntries = newEntries.OrderBy(e => e.Key).ToList();
				}
			}
			catch(Exception ex)
			{
				Log.Here().Error($"Error loading from resource: {ex.ToString()}");
			}

			return newEntries;
		}

		public static List<LocaleNodeKeyEntry> LoadAllFromNodeAttributes(Node node, ResourceFormat resourceFormat, bool generateNewHandle = false)
		{
			List<LocaleNodeKeyEntry> newEntries = new List<LocaleNodeKeyEntry>();
			foreach (var kvp in node.Attributes)
			{
				if (kvp.Value.Type == NodeAttribute.DataType.DT_TranslatedString)
				{
					// || kvp.Value.Type == NodeAttribute.DataType.DT_TranslatedFSString
					LocaleNodeKeyEntry localeEntry = new LocaleNodeKeyEntry(node);
					localeEntry.KeyIsEditable = false;
					localeEntry.Key = kvp.Key;

					localeEntry.TranslatedStringAttribute = kvp.Value;
					localeEntry.TranslatedString = kvp.Value.Value as TranslatedString;

					if(generateNewHandle) localeEntry.Handle = CreateHandle();

					newEntries.Add(localeEntry);
				}
			}

			return newEntries;
		}
		public static LocaleNodeKeyEntry LoadFromNode(Node node, ResourceFormat resourceFormat, bool generateNewHandle = false)
		{
			if (resourceFormat == ResourceFormat.LSB)
			{
				LocaleNodeKeyEntry localeEntry = new LocaleNodeKeyEntry(node);
				NodeAttribute keyAtt = null;
				NodeAttribute contentAtt = null;
				if(node.Attributes.TryGetValue("UUID", out keyAtt))
				{
					localeEntry.KeyAttribute = keyAtt;
				}
				else
				{
					keyAtt = new NodeAttribute(NodeAttribute.DataType.DT_FixedString);
					localeEntry.KeyAttribute = keyAtt;
				}
				if(!node.Attributes.TryGetValue("Content", out contentAtt))
				{
					contentAtt = new NodeAttribute(NodeAttribute.DataType.DT_TranslatedString);
					if (contentAtt.Value is TranslatedString translatedString)
					{
						translatedString.Value = "";
						translatedString.Handle = CreateHandle();
						localeEntry.TranslatedString = translatedString;
					}
				}
				
				localeEntry.KeyIsEditable = true;

				localeEntry.TranslatedStringAttribute = contentAtt;
				localeEntry.TranslatedString = contentAtt.Value as TranslatedString;

				if (generateNewHandle)
				{
					localeEntry.TranslatedString.Handle = CreateHandle();
				}

				return localeEntry;
			}
			else if (resourceFormat == ResourceFormat.LSJ || resourceFormat == ResourceFormat.LSX)
			{
				LocaleNodeKeyEntry localeEntry = new LocaleNodeKeyEntry(node);
				
				localeEntry.KeyIsEditable = false;
				localeEntry.Key = resourceFormat == ResourceFormat.LSJ ? "TagText" : "";

				NodeAttribute contentAtt = null;
				
				if (!node.Attributes.TryGetValue("TagText", out contentAtt))
				{
					contentAtt = new NodeAttribute(NodeAttribute.DataType.DT_TranslatedString);
					if (contentAtt.Value is TranslatedString translatedString)
					{
						translatedString.Value = "";
						translatedString.Handle = CreateHandle();
						localeEntry.TranslatedString = translatedString;
					}
				}

				localeEntry.TranslatedStringAttribute = contentAtt;
				localeEntry.TranslatedString = contentAtt.Value as TranslatedString;

				return localeEntry;
			}
			return null;
		}

		public static List<Node> FindTranslatedStringsInNodeList(KeyValuePair<string, List<LSLib.LS.Node>> nodeList)
		{
			List<Node> nodes = new List<Node>();
			foreach (var node in nodeList.Value)
			{
				var stringNodes = FindTranslatedStringInNode(node);
				nodes.AddRange(stringNodes);
			}
			return nodes;
		}

		private static List<Node> FindTranslatedStringInNode(LSLib.LS.Node node)
		{
			List<Node> nodes = new List<Node>();
			foreach (var att in node.Attributes)
			{
				if(att.Value.Value is TranslatedString translatedString)
				{
					nodes.Add(node);
					break;
				}
			}

			if (node.ChildCount > 0)
			{
				foreach (var c in node.Children)
				{
					var extraNodes = FindTranslatedStringsInNodeList(c);
					nodes.AddRange(extraNodes);
				}
			}

			return nodes;
		}
		#endregion

		#region Saving

		public static async Task<bool> BackupDataFiles(LocaleViewModel data, string backupDirectory, CancellationToken? token = null)
		{
			try
			{
				if (!Directory.Exists(backupDirectory)) Directory.CreateDirectory(backupDirectory);

				List<string> sourceFiles = new List<string>();
				string sysFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.Replace("/", "-");
				string archivePath = Path.Combine(backupDirectory, "LocalizationEditorBackup") + "_" + DateTime.Now.ToString(sysFormat + "_HH-mm-ss") + ".zip";

				List<string> replacePaths = new List<string>();

				foreach (var f in data.SelectedGroup.DataFiles.OfType<LocaleNodeFileData>())
				{
					if (File.Exists(f.SourcePath))
					{
						Regex regex = new Regex(@"(.*Divinity Original Sin 2\\DefEd\\Data\\)", RegexOptions.IgnoreCase);
						var m = regex.Match(f.SourcePath);
						if(m.Success)
						{
							var val = m.Groups[1].Value;
							if(!replacePaths.Contains(val))
							{
								replacePaths.Add(val);
							}
						}
						sourceFiles.Add(f.SourcePath);
					}
				}

				if (sourceFiles.Count > 0)
				{
					var result = await BackupGenerator.CreateArchiveFromFiles(sourceFiles, archivePath, replacePaths, token);
					Log.Here().Activity($"Localization backup result to '{archivePath}': {result.ToString()}");
					return result != FileCreationTaskResult.Error;
				}
				else
				{
					Log.Here().Activity("Skipping localization backup, as no files were found.");
					return true;
				}
			}
			catch (Exception ex)
			{
				Log.Here().Error($"Error backing up localization files: {ex.ToString()}");
				return false;
			}
		}

		public static async Task<bool> BackupDataFile(string sourceFilePath, string backupDirectory, CancellationToken? token = null)
		{
			try
			{
				if (File.Exists(sourceFilePath))
				{
					if (!Directory.Exists(backupDirectory)) Directory.CreateDirectory(backupDirectory);
					string sysFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.Replace("/", "-");
					string archivePath = Path.Combine(backupDirectory, Path.GetFileName(sourceFilePath)) + "_" + DateTime.Now.ToString(sysFormat + "_HH-mm-ss") + ".zip";

					List<string> replacePaths = new List<string>();
					Regex regex = new Regex(@"(.*Divinity Original Sin 2\\DefEd\\Data\\)", RegexOptions.IgnoreCase);
					var m = regex.Match(sourceFilePath);
					if (m.Success)
					{
						replacePaths.Add(m.Groups[1].Value);
					}
					List<string> sourceFiles = new List<string>() { sourceFilePath };
					var result = await BackupGenerator.CreateArchiveFromFiles(sourceFiles, archivePath, replacePaths, token);
					Log.Here().Activity($"Localization backup result to '{archivePath}': {result.ToString()}");
					return result != FileCreationTaskResult.Error;
				}
				else
				{
					Log.Here().Activity("Skipping localization backup, as the file was not found.");
					return true;
				}
			}
			catch (Exception ex)
			{
				Log.Here().Error($"Error backing up localization files: {ex.ToString()}");
				return false;
			}
		}

		public static async Task<int> SaveDataFiles(LocaleViewModel data, CancellationToken? token = null)
		{
			if (data.SelectedGroup == null) return -1;

			foreach(var dir in data.SelectedGroup.SourceDirectories)
			{
				if (!Directory.Exists(dir))
				{
					Directory.CreateDirectory(dir);
				}
			}

			int success = 0;
			if (data.SelectedGroup != data.CustomGroup)
			{
				foreach (LocaleNodeFileData f in data.SelectedGroup.DataFiles.Where(f => f.ChangesUnsaved || f.Entries.Any(x => x.ChangesUnsaved)).Cast<LocaleNodeFileData>())
				{
					int result = await SaveDataFile(f, token);
					success += result;
					if (result > 0)
					{
						f.SetChangesUnsaved(false, true);
					}
				}
			}
			else
			{
				foreach (var f in data.CustomGroup.DataFiles.Where(f => f.ChangesUnsaved).Cast<LocaleCustomFileData>())
				{
					string targetDirectory = DOS2DEDefaultPaths.CustomLocaleDirectory(data.ModuleData, f.Project);
					int result = await SaveDataFile(f, targetDirectory, token);
					success += result;
					if (result > 0)
					{
						f.SetChangesUnsaved(false, true);
					}
				}
			}
			Log.Here().Activity($"Files saved: '{success}'.");
			return success;
		}

		public static async Task<int> SaveDataFiles(List<LocaleNodeFileData> dataFiles, CancellationToken? token = null)
		{
			int success = 0;
			foreach (var f in dataFiles)
			{
				success += await SaveDataFile(f, token);
			}
			Log.Here().Activity($"Files saved: '{success}'.");
			return success;
		}

		public static async Task<int> SaveDataFile(LocaleNodeFileData dataFile, CancellationToken? token = null)
		{
			try
			{
				if (dataFile.Source != null)
				{
					var parentDir = Directory.GetParent(dataFile.SourcePath)?.FullName;
					if (!String.IsNullOrEmpty(parentDir) && !Directory.Exists(parentDir))
					{
						Directory.CreateDirectory(parentDir);
					}
					var saveFormat = dataFile.Format;
					if (saveFormat == ResourceFormat.LSX && FileCommands.FileExtensionFound(dataFile.SourcePath, ".lsb"))
					{
						saveFormat = ResourceFormat.LSB;
					}
					Log.Here().Activity($"Saving '{dataFile.Name}' to '{dataFile.SourcePath}'.");
					await Task.Run(() => LSLib.LS.ResourceUtils.SaveResource(dataFile.Source, dataFile.SourcePath, saveFormat));
					Log.Here().Important($"Saved '{dataFile.SourcePath}'.");
					dataFile.UnsavedChanges.Clear();
					dataFile.ChangesUnsaved = false;
					return 1;
				}
				else
				{
					Log.Here().Error($"Localization file '{dataFile.Name}' has no source.");
				}
			}
			catch (Exception ex)
			{
				Log.Here().Error($"Error saving localizaton resource: {ex.ToString()}");
			}
			return 0;
		}

		public static async Task<int> SaveDataFile(LocaleCustomFileData dataFile, string targetDirectory, CancellationToken? token = null)
		{
			try
			{
				Log.Here().Activity($"Saving '{dataFile.Name}' to '{dataFile.SourcePath}'.");

				if (dataFile.SourcePath.EndsWith(".tsv"))
				{
					string output = "Key\tContent\tHandle\n";
					for(int i = 0; i < dataFile.Entries.Count; i++)
					{
						var entry = dataFile.Entries[i];
						output += $"{entry.Key}\t{entry.Content}{entry.Handle}";
						if(i < dataFile.Entries.Count - 1)
						{
							output += Environment.NewLine;
						}
						if (await FileCommands.WriteToFileAsync(dataFile.SourcePath, output))
						{
							Log.Here().Important($"Saved '{dataFile.SourcePath}'.");
							return 1;
						}
					}

				}
				else if (dataFile.SourcePath.EndsWith(".json") || String.IsNullOrEmpty(dataFile.SourcePath))
				{
					string outputFilename = String.IsNullOrEmpty(dataFile.SourcePath) ? Path.Combine(targetDirectory, dataFile.Name, ".json") : dataFile.SourcePath;
					string json = JsonInterface.SerializeObject(dataFile);
					if (await FileCommands.WriteToFileAsync(outputFilename, json))
					{
						Log.Here().Important($"Saved '{outputFilename}'.");
						return 1;
					}
				}
			}
			catch (Exception ex)
			{
				Log.Here().Error($"Error saving localizaton resource: {ex.ToString()}");
			}
			return 0;
		}

		private static string GetSourceFileName(ILocaleFileData fileData, bool findActualSource, LocaleViewModel data, ILocaleKeyEntry e, bool useDisplayName = false)
		{
			if (!findActualSource && fileData is LocaleNodeFileData keyFileData)
			{
				if (!useDisplayName)
				{
					useDisplayName = Path.GetExtension(keyFileData.SourcePath).Equals(".lsf", StringComparison.OrdinalIgnoreCase);
				}
				return EscapeXml(Path.GetFileName(!useDisplayName ? keyFileData.SourcePath : keyFileData.Name));
			}

			if (findActualSource)
			{
				var actualSource = data.SelectedGroup.DataFiles.Where(d => d.Entries.Contains(e)).FirstOrDefault();
				if (actualSource is LocaleNodeFileData sourceFileData)
				{
					if (!useDisplayName)
					{
						useDisplayName = Path.GetExtension(actualSource.SourcePath).Equals(".lsf", StringComparison.OrdinalIgnoreCase);
					}
					return EscapeXml(Path.GetFileName(!useDisplayName ? sourceFileData.SourcePath : sourceFileData.Name));
				}
			}

			return "";
		}

		public static bool IgnoreHandle(string handle, ILocaleFileData fileData)
		{
			//Larian handles for empty GMSpawnSubsection
			if (handle == UnsetHandle || handle == "heee99d71g1f41g4ba2g8adbg98fad94256ca" || handle == "hfeccb8bbgf99fg4028gb187g607c18c2cbaa" || handle.StartsWith("ResStr_"))
			{
				return true;
			}
			if (!fileData.CanOverride && IgnoredHandles.Contains(handle)) return true;
			return false;
		}

		public static string ExportDataAsXML(LocaleViewModel data)
		{
			string output = "<contentList>\n{0}</contentList>";
			string entriesStr = "";

			if (data.SelectedGroup != null)
			{
				var fileData = data.SelectedGroup.SelectedFile;
				if (fileData != null)
				{
					bool findActualSource = fileData == data.SelectedGroup.CombinedEntries;

					var exportedKeys = fileData.Entries.Where(fd => fd.Selected && !IgnoreHandle(fd.Handle, fileData)).DistinctBy(x => x.Handle);

					bool exportSource = false;
					bool exportKeys = false;

					var settings = data.Settings.GetProjectSettings(data.GetMainProject());

					if (settings != null)
					{
						exportSource = settings.ExportSource;
						exportKeys = settings.ExportKeys;
					}

					if (exportSource && !exportKeys)
					{
						exportedKeys = exportedKeys.OrderBy(x => GetSourceFileName(fileData, findActualSource, data, x));
					}
					else if (!exportSource && exportKeys)
					{
						exportedKeys = exportedKeys.OrderBy(x => x.EntryKey);
					}
					else if (exportSource && exportKeys)
					{
						exportedKeys = exportedKeys.OrderBy(x => 
							GetSourceFileName(fileData, findActualSource, data, x)).ThenBy(x => x.EntryKey);
					}
					else
					{
						exportedKeys = exportedKeys.OrderBy(x => x.Handle);
					}

					foreach (var e in exportedKeys)
					{
						string sourcePath = GetSourceFileName(fileData, findActualSource, data, e);
						var sourceStr = "";

						if (exportSource)
						{
							sourceStr = $" Source=\"{sourcePath}\"";
						}

						var keyStr = "";

						if (exportKeys && !String.IsNullOrWhiteSpace(e.Key) && e.KeyIsEditable)
						{
							keyStr = $" Key=\"{e.Key}\"";
						}

						string content = EscapeXml(e.Content);

						string addStr = "\t" + "<content contentuid=\"{0}\"{1}{2}>{3}</content>" + Environment.NewLine;
						entriesStr += String.Format(addStr, e.Handle, sourceStr, keyStr, content);
					}
				}
			}

			return String.Format(output, entriesStr);
		}

		public static Resource CreateLocalizationResource()
		{
			try
			{
				using (var stream = new System.IO.MemoryStream())
				{
					var writer = new System.IO.StreamWriter(stream);
					writer.Write(SCG.Modules.DOS2DE.Properties.Resources.DefaultLocaleResource);
					writer.Flush();
					stream.Position = 0;
					Log.Here().Activity("Creating default localization resource.");
					var resource = LSLib.LS.ResourceUtils.LoadResource(stream, ResourceFormat.LSX);

					return resource;
				}
			}
			catch (Exception ex)
			{
				Log.Here().Error($"Error creating new localization resource: {ex.ToString()}");
				return null;
			}
		}
		#endregion

		#region Utilities
		/// <summary>
		/// Handles are a GUID with the dashes replaces with g, and an h prepended to the front.
		/// </summary>
		/// <returns></returns>
		public static string CreateHandle()
		{
			return Guid.NewGuid().ToString().Replace('-', 'g').Insert(0, "h");
		}

		public static LocaleNodeFileData CreateFileData(LocaleTabGroup groupData, string destinationPath, string name)
		{
			var resource = CreateLocalizationResource();
			var fileData = new LocaleNodeFileData(groupData, ResourceFormat.LSX, resource, destinationPath, name);
			var entries = LoadFromResource(resource, ResourceFormat.LSB);
			foreach (var entry in entries)
			{
				entry.Parent = fileData;
				fileData.Entries.Add(entry);
			}
			fileData.ChangesUnsaved = true;
			return fileData;
		}

		public static async Task<bool> LoadLinkedFilesAsync(DOS2DEModuleData vm, ModProjectData modProject, LocaleViewModel localizationData, List<ILocaleKeyEntry> removedEntries)
		{
			string linkFolder = DOS2DEDefaultPaths.LocalizationEditorLinkFolder(vm);
			if (Directory.Exists(linkFolder))
			{
				Log.Here().Activity($"Searching for link files in '{linkFolder}'.");

				var linkFiles = Directory.EnumerateFiles(linkFolder, new DirectoryEnumerationFilters
				{
					InclusionFilter = (f) =>
					{
						//Log.Here().Activity($"File: {f.FileName} | {FileCommands.FileExtensionFound(f.FileName, ".json")}");
						return FileCommands.FileExtensionFound(f.FileName, ".json");
					}
				});

				bool sourceFileMatch(string sourcePath, string storedFileName)
				{
					return Path.GetFileName(sourcePath).Equals(Path.GetFileName(storedFileName), StringComparison.OrdinalIgnoreCase);
				}

				foreach (var filePath in linkFiles)
				{
					try
					{
						LocaleProjectLinkData data = await JsonInterface.DeserializeObjectAsync<LocaleProjectLinkData>(filePath);
						if (data != null && data.Links.Count > 0 && data.ProjectUUID.Equals(modProject.UUID))
						{
							Log.Here().Activity($"Line file loaded: '{filePath}' => {data.ProjectUUID}.");

							foreach (var link in data.Links)
							{
								Log.Here().Activity($"Searching for locale files for '{link.TargetFile}'.");
								var targetFiles = localizationData.Groups.SelectMany(g => g.DataFiles).Where(f => sourceFileMatch(f.SourcePath, link.TargetFile));
								foreach (var fileData in targetFiles)
								{
									fileData.FileLinkData = link;
									Log.Here().Activity($"Set linked data for '{fileData.SourcePath}' to '{link.ReadFrom}'. Loading entries from linked file.");
									removedEntries.AddRange(RefreshLinkedData(fileData));
								}
							}

							localizationData.LinkedLocaleData.Add(data);
						}
					}
					catch (Exception ex)
					{
						Log.Here().Error($"Error deserializing '{filePath}': {ex.ToString()}");
					}
				}

				return true;
			}

			return false;
		}

		public static void SaveLinkedDataForFile(DOS2DEModuleData moduleData, IEnumerable<LocaleProjectLinkData> linkedLocaleData, ILocaleFileData fileData)
		{
			if (fileData.HasFileLink && fileData is LocaleNodeFileData nodeFileData)
			{
				if (nodeFileData.ModProject != null && !String.IsNullOrEmpty(nodeFileData.FileLinkData.ReadFrom))
				{
					var linkedList = linkedLocaleData.FirstOrDefault(x => x.ProjectUUID.Equals(nodeFileData.ModProject.UUID, StringComparison.OrdinalIgnoreCase));
					if (linkedList == null)
					{
						linkedList = new LocaleProjectLinkData()
						{
							ProjectUUID = nodeFileData.ModProject.UUID,
						};
					}

					linkedList.Links.Add(fileData.FileLinkData);

					SaveLinkedData(moduleData, linkedList);
				}
			}
		}

		public static void SaveLinkedData(DOS2DEModuleData moduleData, LocaleProjectLinkData linkFile)
		{
			var dir = DOS2DEDefaultPaths.LocalizationEditorLinkFolder(moduleData);
			if(!Directory.Exists(dir))
			{
				Directory.CreateDirectory(dir);
			}

			var filePath = moduleData.ModProjects.Items.Where(x => x.UUID.Equals(linkFile.ProjectUUID, StringComparison.OrdinalIgnoreCase)).
					Select(x => x.ModuleInfo.Folder).FirstOrDefault();
			if (!String.IsNullOrEmpty(filePath))
			{
				filePath = Path.Combine(dir, filePath + ".json");
				Log.Here().Activity($"Saving linked data to '{filePath}'.");
				string json = JsonInterface.SerializeObject(linkFile);
				FileCommands.WriteToFile(filePath, json);
			}
		}

		public static void SaveAllLinkedData(DOS2DEModuleData moduleData, IEnumerable<LocaleProjectLinkData> linkFiles)
		{
			foreach (var data in linkFiles)
			{
				SaveLinkedData(moduleData, data);
			}
		}

		public static List<ILocaleKeyEntry> RefreshLinkedData(ILocaleFileData fileData)
		{
			List<ILocaleKeyEntry> allMissingEntries = new List<ILocaleKeyEntry>();
			if (File.Exists(fileData.FileLinkData.ReadFrom))
			{
				Log.Here().Activity($"Loading linked file data from {fileData.FileLinkData.ReadFrom}");
				string path = fileData.FileLinkData.ReadFrom;
				if (FileCommands.FileExtensionFound(path, ".txt", ".tsv", ".csv"))
				{
					char delimiter = '\t';
					if (FileCommands.FileExtensionFound(path, ".csv")) delimiter = ',';

					using (var stream = new System.IO.StreamReader(path))
					{
						int lineNum = 0;
						string line = String.Empty;

						List<TextualLocaleEntry> entries = new List<TextualLocaleEntry>();

						Regex regularModePattern = new Regex($"^(.*){delimiter}+(.*)$", RegexOptions.Singleline);
						Regex handleModePattern = new Regex($"^(.*?){delimiter}+(.*?){delimiter}+(.*?)$", RegexOptions.Singleline);

						Regex r = regularModePattern;

						bool handleMode = false;

						while ((line = stream.ReadLine()) != null)
						{
							lineNum += 1;
							// Skip top line, as it typically describes the columns
							if (lineNum == 1)
							{
								if (delimitStepSkipLine(line, true))
								{
									handleMode = true;
									r = handleModePattern;
									Log.Here().Activity($"Handle mode activated for '{path}'.");
									continue;
								}
								else if (delimitStepSkipLine(line))
								{
									r = regularModePattern;
									continue;
								}
							}

							var match = r.Match(line);
							if (match.Success)
							{
								string key = match.Groups.Count >= 1 ? match.Groups[1].Value : "NewKey";
								string content = match.Groups.Count >= 2 ? match.Groups[2].Value : "";
								string handle = (handleMode && match.Groups.Count >= 3) ? match.Groups[3].Value : "";

								//Log.Here().Activity($"New entry: {key} => {content}{(handleMode ? "|" + handle : "")}");
	
								if (!String.IsNullOrWhiteSpace(key))
								{
									entries.Add(new TextualLocaleEntry { Key = key, Content = content, Handle = handle });
								}
							}
						}

						if(entries.Count > 0)
						{
							bool changesUnsaved = false;
							foreach(var entry in entries)
							{
								var existingEntry = fileData.Entries.FirstOrDefault(x => x.Key.Equals(entry.Key, StringComparison.OrdinalIgnoreCase));
								if (existingEntry != null)
								{
									if (!entry.Content.Equals(existingEntry.Content) || !entry.Key.Equals(existingEntry.Key))
									{
										Log.Here().Activity($"Updated entry: {existingEntry.Key} => {entry.Key} | {existingEntry.Content} => {entry.Content}");
										existingEntry.Content = entry.Content;
										existingEntry.Key = entry.Key;
										existingEntry.ChangesUnsaved = true;
										changesUnsaved = true;
									}

									if(!String.IsNullOrWhiteSpace(entry.Handle) && entry.Handle != existingEntry.Handle)
									{
										Log.Here().Activity($"Updated entry handle: {existingEntry.Handle} => {entry.Handle}");
										existingEntry.Handle = entry.Handle;
										existingEntry.ChangesUnsaved = true;
										changesUnsaved = true;
									}
								}
								else
								{
									if(fileData is LocaleNodeFileData nodeFileData)
									{
										var newEntry = CreateNewLocaleEntry(nodeFileData, entry.Key, entry.Content);
										if (!String.IsNullOrWhiteSpace(entry.Handle))
										{
											newEntry.Handle = entry.Handle;
										}
										nodeFileData.Entries.Add(newEntry);
										newEntry.ChangesUnsaved = true;
										changesUnsaved = true;
										//newEntry.Index = nodeFileData.Entries.IndexOf(newEntry);
										Log.Here().Activity($"Added new entry: {newEntry.Key} | {newEntry.Content} | {newEntry.Handle}");
									}
								}
							}

							var missingEntries = fileData.Entries.Where(x => !entries.Any(e => e.Key.Equals(x.Key, StringComparison.OrdinalIgnoreCase)));
							//Log.Here().Important($"Missing entries:{String.Join(Environment.NewLine, missingEntries.Select(x => x.EntryKey))}");
							allMissingEntries.AddRange(missingEntries);

							if (!fileData.ChangesUnsaved) fileData.ChangesUnsaved = changesUnsaved;
						}
					}
				}
				
			}
			return allMissingEntries;
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
				toxml = toxml.Replace(Environment.NewLine, "&lt;br&gt;");
			}
			return toxml;
		}

		public static LocaleNodeKeyEntry CreateNewLocaleEntry(ILocaleFileData fileData, string key = "NewKey", string content = "")
		{
			Region parent = null;
			ResourceFormat format = ResourceFormat.LSB;

			if(fileData is LocaleNodeFileData nodeFileData)
			{
				parent = nodeFileData.RootRegion;
			}

			if (parent == null)
			{
				parent = new Region();
				parent.Name = "root";
				parent.RegionName = "TranslatedStringKeys";
			}

			var node = new Node();
			node.Parent = parent;
			node.Name = "TranslatedStringKey";
			parent.AppendChild(node);

			node.Attributes = new Dictionary<string, NodeAttribute>();
			var translatedStringAtt = new NodeAttribute(NodeAttribute.DataType.DT_TranslatedString);
			TranslatedString translatedString = new TranslatedString();
			translatedString.Handle = CreateHandle();
			translatedString.Value = content;
			translatedStringAtt.Value = translatedString;

			var keyAtt = new NodeAttribute(NodeAttribute.DataType.DT_FixedString);
			keyAtt.Value = key == "NewKey" ? key + (fileData.Entries.Count + 1) : key;

			node.Attributes.Add("Content", translatedStringAtt);
			node.Attributes.Add("ExtraData", new NodeAttribute(NodeAttribute.DataType.DT_LSString) { Value = "" });
			node.Attributes.Add("Speaker", new NodeAttribute(NodeAttribute.DataType.DT_FixedString) { Value = "" });
			node.Attributes.Add("Stub", new NodeAttribute(NodeAttribute.DataType.DT_Bool) { Value = false});
			node.Attributes.Add("UUID", keyAtt);

			LocaleNodeKeyEntry localeEntry = LoadFromNode(node, format);
			localeEntry.Parent = fileData;
			localeEntry.ChangesUnsaved = true;
			return localeEntry;
		}

		private static bool delimitStepSkipLine(string line, bool handleCheck = false)
		{
			Regex r;
			if (!handleCheck)
			{
				r = new Regex(@"^Key(\s*?|,)Content$", RegexOptions.IgnoreCase);
			}
			else
			{
				r = new Regex(@"^Key(\s*?|,)Content(\s*?|,)Handle$", RegexOptions.IgnoreCase);
			}
			return r.Match(line)?.Success == true;
		}

		private static LocaleNodeFileData CreateNodeFileDataFromTextual(LocaleTabGroup groupData, System.IO.StreamReader stream, string sourceDirectory, string filePath, char delimiter)
		{
			//For exporting to lsb later
			string name = Path.GetFileNameWithoutExtension(filePath);
			string futureSourcePath = FileCommands.EnsureExtension(Path.Combine(sourceDirectory, name), ".lsb");

			LocaleNodeFileData fileData = CreateFileData(groupData, futureSourcePath, name);
			//Remove the empty default new key
			fileData.Entries.Clear();
			//fileData.RootRegion.Children.Values.First().First().Children.Clear();
			fileData.RootRegion.Children.Values.First().Clear();

			int lineNum = 0;
			string line = String.Empty;

			Regex regularModePattern = new Regex($"^(.*){delimiter}+(.*)$", RegexOptions.Singleline);
			Regex handleModePattern = new Regex($"^(.*?){delimiter}+(.*?){delimiter}+(.*?)$", RegexOptions.Singleline);

			Regex r = regularModePattern;

			bool handleMode = false;

			while ((line = stream.ReadLine()) != null)
			{
				lineNum += 1;
				// Skip top line, as it typically describes the columns
				if (lineNum == 1)
				{
					if (delimitStepSkipLine(line, true))
					{
						handleMode = true;
						r = handleModePattern;
						Log.Here().Activity($"Handle mode activated for '{filePath}'.");
						continue;
					}
					else if (delimitStepSkipLine(line))
					{
						r = regularModePattern;
						continue;
					}
				}

				var match = r.Match(line);
				if(match.Success)
				{
					string key = match.Groups.Count >= 1 ? match.Groups[1].Value : "NewKey";
					string content = match.Groups.Count >= 2 ? match.Groups[2].Value : "";
					string handle = (handleMode && match.Groups.Count >= 3) ? match.Groups[3].Value : "";

					Log.Here().Activity($"New entry: {key} => {content}{(handleMode ? "|" + handle : "")}");

					var entry = CreateNewLocaleEntry(fileData, key, content);
					if (handleMode && !String.IsNullOrEmpty(handle)) entry.Handle = handle;
					entry.ChangesUnsaved = true;
					fileData.Entries.Add(entry);
				}
			}

			if (fileData.Entries.Count > 1)
			{
				fileData.ChangesUnsaved = true;
			}

			return fileData;
		}

		public static List<ILocaleFileData> ImportFilesAsData(IEnumerable<string> files, LocaleTabGroup groupData)
		{
			List<ILocaleFileData> newFileDataList = new List<ILocaleFileData>();
			foreach (var path in files)
			{
				if (FileCommands.FileExtensionFound(path, ".lsb", ".lsj"))
				{
					Task<ILocaleFileData> task = Task.Run<ILocaleFileData>(async () => await LoadResourceAsync(groupData, path));
					if (task != null && task.Result != null)
					{
						newFileDataList.Add(task.Result);
					}
				}
				else if (FileCommands.FileExtensionFound(path, ".txt", ".tsv", ".csv"))
				{
					char delimiter = '\t';
					if (FileCommands.FileExtensionFound(path, ".csv")) delimiter = ',';

					using (var stream = new System.IO.StreamReader(path))
					{
						foreach (var sourceDir in groupData.SourceDirectories)
						{
							ILocaleFileData fileData = null;
							if(!groupData.IsCustom)
							{
								fileData = CreateNodeFileDataFromTextual(groupData, stream, sourceDir, path, delimiter);
								// Automatically create linked data
								fileData.FileLinkData = new LocaleFileLinkData()
								{
									ReadFrom = path,
									TargetFile = fileData.SourcePath
								};
							}
							else
							{
								fileData = new LocaleCustomFileData(groupData, Path.GetFileNameWithoutExtension(path))
								{
									SourcePath = path
								};
							}
							
							
							newFileDataList.Add(fileData);
						}
					}
				}
			}
			return newFileDataList;
		}

		public static List<ILocaleKeyEntry> ImportFilesAsEntries(IEnumerable<string> files, ILocaleFileData fileData, bool addDuplicates = false)
		{
			List<ILocaleKeyEntry> newEntryList = new List<ILocaleKeyEntry>();
			try
			{
				foreach(var path in files)
				{
					Log.Here().Activity($"Checking file '{path}'");

					if (FileCommands.FileExtensionFound(path, ".lsb", ".lsj"))
					{
						Log.Here().Activity($"Creating entries from resource.");

						Task<ILocaleFileData> task = Task.Run<ILocaleFileData>(async () => await LoadResourceAsync(fileData.Parent, path).ConfigureAwait(false));
						if (task != null && task.Result != null)
						{
							newEntryList.AddRange(task.Result.Entries);
							continue;
						}
					}
					else if (FileCommands.FileExtensionFound(path, ".txt", ".tsv", ".csv"))
					{
						Log.Here().Activity($"Creating entries from delimited text file.");
						char delimiter = '\t';
						if (FileCommands.FileExtensionFound(path, ".csv")) delimiter = ',';

						using (var stream = new System.IO.StreamReader(path))
						{
							int lineNum = 0;
							string line = String.Empty;

							Regex regularModePattern = new Regex($"^(.*){delimiter}+(.*)$", RegexOptions.Singleline);
							Regex handleModePattern = new Regex($"^(.*?){delimiter}+(.*?){delimiter}+(.*?)$", RegexOptions.Singleline);

							Regex r = regularModePattern;

							bool handleMode = false;

							while ((line = stream.ReadLine()) != null)
							{
								lineNum += 1;
								// Skip top line, as it typically describes the columns
								if (lineNum == 1)
								{
									if (delimitStepSkipLine(line, true))
									{
										handleMode = true;
										r = handleModePattern;
										Log.Here().Activity($"Handle mode activated for '{path}'.");
										continue;
									}
									else if (delimitStepSkipLine(line))
									{
										handleMode = false;
										r = regularModePattern;
										continue;
									}
								}

								var match = r.Match(line);
								if (match.Success)
								{
									string key = match.Groups.Count >= 1 ? match.Groups[1].Value : "NewKey";
									string content = match.Groups.Count >= 2 ? match.Groups[2].Value : "";
									string handle = (handleMode && match.Groups.Count >= 3) ? match.Groups[3].Value : "";

									bool addNew = true;

									if (!String.IsNullOrWhiteSpace(key) || !String.IsNullOrWhiteSpace(handle))
									{
										ILocaleKeyEntry updateEntry = null;

										if (!String.IsNullOrWhiteSpace(handle))
										{
											updateEntry = fileData.Entries.FirstOrDefault(e => e.Handle == handle);
										}
										if(updateEntry == null) 
										{
											updateEntry = fileData.Entries.FirstOrDefault(e => e.Key == key);
										}

										if (updateEntry != null)
										{
											addNew = addDuplicates != false;
											Log.Here().Activity($"Updating content for existing key [{key}|{handle}].");
											if (!updateEntry.Content.Equals(content, StringComparison.Ordinal))
											{
												updateEntry.Content = content;
											}
											if (!String.IsNullOrWhiteSpace(handle))
											{
												updateEntry.Key = key;
											}
										}
									}

									if (addNew)
									{
										var entry = CreateNewLocaleEntry(fileData, key, content);
										if (handleMode && !String.IsNullOrEmpty(handle)) entry.Handle = handle;
										Log.Here().Activity($"Added new entry [{entry.Key}|{entry.Handle}].");
										newEntryList.Add(entry);
									}
								}
							}
							stream.Close();
						}
					}
				}
			}
			catch (Exception ex)
			{
				Log.Here().Error("Error importing files as entries to the localization editor: " + ex.ToString());
			}
			return newEntryList;
		}
		#endregion

		#region Settings

		public static void LoadSettings(DOS2DEModuleData moduleData, LocaleViewModel localeData)
		{
			string settingsPath = Path.GetFullPath(DOS2DEDefaultPaths.LocalizationEditorSettings(moduleData));

			if (File.Exists(settingsPath))
			{
				Log.Here().Activity($"Loading localization editor settings from '{settingsPath}'.");
				try
				{
					localeData.Settings = JsonInterface.DeserializeObject<LocaleEditorSettingsData>(settingsPath);
					localeData.OnSettingsLoaded();
				}
				catch (Exception ex)
				{
					Log.Here().Error($"Error deserializing settings: {ex.ToString()}");
					localeData.Settings = new LocaleEditorSettingsData();
				}
			}
			else
			{
				localeData.Settings = new LocaleEditorSettingsData();
			}
		}

		public static void LoadProjectSettings(DOS2DEModuleData moduleData, LocaleViewModel localeData)
		{
			if(localeData.Settings != null)
			{
				if(localeData.Settings.Projects.Count > 0)
				{
					foreach (var entry in localeData.Settings.Projects)
					{
						var project = moduleData.ModProjects.Items.FirstOrDefault(x => x.FolderName == entry.FolderName);
						if (project != null)
						{
							entry.Name = project.DisplayName;
						}
					}
				}

				foreach (var project in localeData.LinkedProjects)
				{
					var settings = localeData.Settings.GetProjectSettings(project);
					if (String.IsNullOrEmpty(settings.LastEntryImportPath))
					{
						settings.LastEntryImportPath = localeData.CurrentImportPath;
						settings.LastFileImportPath = localeData.CurrentImportPath;
					}
				}

				localeData.Settings.Projects.OrderBy(x => x.Name);
				localeData.Settings.SaveCommand = localeData.SaveSettingsCommand;
			}
		}

		public static void SaveSettings(DOS2DEModuleData moduleData, LocaleViewModel localeData)
		{
			string settingsPath = Path.GetFullPath(DOS2DEDefaultPaths.LocalizationEditorSettings(moduleData));

			if (localeData.Settings != null)
			{
				localeData.Settings.Projects.OrderBy(x => x.Name);
				Log.Here().Activity($"Saving localization editor settings to '{settingsPath}'.");
				string json = JsonInterface.SerializeObject(localeData.Settings);
				FileCommands.WriteToFile(settingsPath, json);
			}
		}
		#endregion

		#region Debug
		public static void Debug_CreateEntries(ILocaleFileData fileData, ObservableCollectionExtended<ILocaleKeyEntry> Entries)
		{
			Random rnd = new Random();

			for (int i = 0; i < 4; i++)
			{
				var node = new LSLib.LS.Node();
				var att = new LSLib.LS.NodeAttribute(LSLib.LS.NodeAttribute.DataType.DT_TranslatedString);
				att.Value = new LSLib.LS.TranslatedString();

				var handle = Guid.NewGuid().ToString().Replace('-', 'g').Insert(0, "h");
				if (att.Value is LSLib.LS.TranslatedString str)
				{
					str.Handle = handle;
					str.Value = $"<font color='{String.Format("#{0:X6}", rnd.Next(0x1000000))}'>Test</font>";
					str.Value += $" <font color='{String.Format("#{0:X6}", rnd.Next(0x1000000))}'>Test2</font>";
				}

				node.Attributes.Add("Content", att);
				var entry = LoadFromNode(node, ResourceFormat.LSB, false);
				entry.Parent = fileData;
				Entries.Add(entry);
			}
		}

		public static void Debug_CreateCustomEntries(ILocaleFileData fileData, ObservableCollectionExtended<ILocaleKeyEntry> Entries)
		{
			Random rnd = new Random();

			for (int i = 0; i < 4; i++)
			{
				LocaleCustomKeyEntry data = new LocaleCustomKeyEntry(fileData)
				{
					Handle = Guid.NewGuid().ToString().Replace('-', 'g').Insert(0, "h"),
					Key = "Test" + rnd.Next(99),
					Content = "Flabbababba"
				};
				Entries.Add(data);
			}
		}

		public static void Debug_TraceRegion(KeyValuePair<string, LSLib.LS.Region> keyValuePair, int indent = 0)
		{
			Log.Here().Activity($"{String.Concat(Enumerable.Repeat("\t", indent))}Key[{keyValuePair.Key}]|Value[{keyValuePair.Value}]");
			foreach (var att in keyValuePair.Value.Attributes)
			{
				Debug_TraceAtt(att, indent + 1);
			}
			foreach (var child in keyValuePair.Value.Children)
			{
				Debug_TraceNodes(child, indent + 1);
			}
		}

		public static void Debug_TraceNodes(KeyValuePair<string, List<LSLib.LS.Node>> keyValuePair, int indent = 0)
		{
			Log.Here().Activity($"{String.Concat(Enumerable.Repeat("\t", indent))}Key[{keyValuePair.Key}]|Value[{keyValuePair.Value}]");
			foreach (var next in keyValuePair.Value)
			{
				Debug_TraceNode(next, indent + 1);
			}
		}

		public static void Debug_TraceNode(LSLib.LS.Node v, int indent = 0)
		{
			Log.Here().Activity($"{String.Concat(Enumerable.Repeat("\t", indent))}Name[{v.Name}]|{v.GetType()}|Parent: [{v.Parent.Name}]");
			foreach (var att in v.Attributes)
			{
				Debug_TraceAtt(att, indent + 1);
			}
			foreach (var child in v.Children)
			{
				Debug_TraceNodes(child, indent + 1);
			}
		}

		public static void Debug_TraceAtt(KeyValuePair<string, LSLib.LS.NodeAttribute> attdict, int indent = 0)
		{
			if (attdict.Value.Value is TranslatedString translatedString)
			{
				Log.Here().Activity($"{String.Concat(Enumerable.Repeat("\t", indent))}Attribute: {attdict.Key} | {attdict.Value.Type} = {attdict.Value.Value} | handle '{translatedString.Handle}'");
			}
			else
			{
				Log.Here().Activity($"{String.Concat(Enumerable.Repeat("\t", indent))}Attribute: {attdict.Key} | {attdict.Value.Type} = {attdict.Value.Value}");
			}
		}
		#endregion
	
		public static async Task<HashSet<string>> LoadIgnoredHandlesAsync(string path)
		{
			HashSet<string> handles = new HashSet<string>();
			try
			{
				using (var stream = File.OpenText(path))
				{
					string line = "";

					while ((line = await stream.ReadLineAsync()) != null)
					{
						handles.Add(line);
					}
				}
			}
			catch (Exception ex)
			{
				Log.Here().Error($"Error reading '{path}': {ex.ToString()}");
			}
			return handles;
		}
	}
}
