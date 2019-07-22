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
using Newtonsoft.Json;
using SCG.Modules.DOS2DE.Data.App;
using SCG.Modules.DOS2DE.Core;
using SCG.Modules.DOS2DE.Windows;
using System.Windows.Media;
using DynamicData.Binding;

namespace SCG.Modules.DOS2DE.Utilities
{
	public class LocaleEditorCommands
	{
		#region Loading Localization Files
		public static async Task<LocaleViewModel> LoadLocalizationDataAsync(DOS2DEModuleData vm, ModProjectData modProject, CancellationToken? token = null)
		{
			await new SynchronizationContextRemover();

			LocaleViewModel localizationData = new LocaleViewModel();
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
			return localizationData;
		}

		public static async Task<LocaleViewModel> LoadLocalizationDataAsync(DOS2DEModuleData vm, IEnumerable<ModProjectData> modProjects, CancellationToken? token = null)
		{
			await new SynchronizationContextRemover();

			LocaleViewModel localizationData = new LocaleViewModel();
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

					if (Directory.Exists(modsLocalePath))
					{
						Log.Here().Activity($"Loading localization data from '{modsLocalePath}'.");
						var modsLocaleData = await LoadFilesAsync(modsLocalePath, token, ".lsb");
						localizationData.ModsGroup.SourceDirectories.Add(modsLocalePath);
						localizationData.ModsGroup.DataFiles.AddRange(modsLocaleData);
						localizationData.ModsGroup.UpdateCombinedData();
					}
					else
					{
						Log.Here().Warning($"Failed to find locale folder for {modProjectData.DisplayName} at path '{modsLocalePath}'.");
						localizationData.ModsGroup.Visibility = false;
					}

					if (Directory.Exists(dialogLocalePath))
					{
						Log.Here().Activity($"Loading dialog localization data from '{dialogLocalePath}'.");
						var dialogLocaleData = await LoadFilesAsync(dialogLocalePath, token, ".lsj");
						//Lock dialog files, as adding a new entry is more complicated than simply adding a key.
						dialogLocaleData.ForEach(f => f.Locked = true);
						localizationData.DialogGroup.SourceDirectories.Add(dialogLocalePath);
						localizationData.DialogGroup.DataFiles.AddRange(dialogLocaleData);
						localizationData.DialogGroup.UpdateCombinedData();
					}
					else
					{
						Log.Here().Warning($"Failed to find dialog folder for {modProjectData.DisplayName} at path '{dialogLocalePath}'.");
						localizationData.DialogGroup.Visibility = false;
					}
				}
				else
				{
					localizationData.ModsGroup.Visibility = false;
					localizationData.DialogGroup.Visibility = false;
				}

				if (publicExists)
				{
					string publicLocalePath = Path.Combine(publicRoot, "Localization");

					if (Directory.Exists(publicLocalePath))
					{
						Log.Here().Activity($"Loading localization data from '{publicLocalePath}'.");
						var publicLocaleData = await LoadFilesAsync(publicLocalePath, token, ".lsb");
						localizationData.PublicGroup.SourceDirectories.Add(publicLocalePath);
						localizationData.PublicGroup.DataFiles.AddRange(publicLocaleData);
						localizationData.PublicGroup.UpdateCombinedData();
					}
					else
					{
						localizationData.PublicGroup.Visibility = false;

						Log.Here().Warning($"Failed to find locale folder for {modProjectData.DisplayName} at path '{publicLocalePath}'.");
					}
				}
				else
				{
					localizationData.PublicGroup.Visibility = false;
				}

				if(customExists)
				{
					var customFiles = await LoadCustomFilesAsync(customLocaleDir, modProjectData);
					localizationData.CustomGroup.DataFiles.AddRange(customFiles);
					localizationData.CustomGroup.UpdateCombinedData();
				}

				localizationData.CombinedGroup.UpdateCombinedData();
				//localizationData.UpdateCombinedGroup();

				Log.Here().Activity($"Localization loaded.");

				return true;
			}
			catch (Exception ex)
			{
				if (!token.Value.IsCancellationRequested)
				{
					Log.Here().Error($"Error creating package: {ex.ToString()}");
				}
				else
				{
					Log.Here().Important($"Cancelled creating package: {ex.ToString()}");
				}
				return false;
			}
		}

		private static async Task<List<LocaleNodeFileData>> LoadFilesAsync(string directoryPath, CancellationToken? token = null, params string[] fileExtensions)
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
					return true;
				}
			};
			if (token != null) filters.CancellationToken = token.Value;
			var lsbFiles = Directory.EnumerateFiles(directoryPath, Alphaleonis.Win32.Filesystem.DirectoryEnumerationOptions.Recursive, filters);
			var targetFiles = new ConcurrentBag<string>(lsbFiles);
			foreach (var filePath in targetFiles)
			{
				var data = await LoadResourceAsync(filePath);
				stringKeyData.Add(data);
			}
			stringKeyData = stringKeyData.OrderBy(f => f.Name).ToList();
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
			using (var reader = File.OpenText(path))
			{
				var fileText = await reader.ReadToEndAsync();
				try
				{
					LocaleCustomFileData fileData = await Task.Run(() => JsonConvert.DeserializeObject<LocaleCustomFileData>(fileText));
					fileData.CanClose = true;
					return fileData;
				}
				catch(Exception ex)
				{
					Log.Here().Error($"Error deserializing '{path}': {ex.ToString()}");
					return null;
				}
			}
		}

		public static async Task<LocaleNodeFileData> LoadResourceAsync(string path, CancellationToken? token = null)
		{
			return await Task.Run(() =>
			{
				var resourceFormat = ResourceFormat.LSB;
				if(FileCommands.FileExtensionFound(path, ".lsj"))
				{
					resourceFormat = ResourceFormat.LSJ;
				}

				var resource = LSLib.LS.ResourceUtils.LoadResource(path, resourceFormat);

				var data = new LocaleNodeFileData(resourceFormat, resource, path, Path.GetFileName(path));
				LoadFromResource(data, resource, resourceFormat);
				
				//foreach (var entry in data.Entries)
				//{
				//	Log.Here().Activity($"Entry: Key[{entry.Key}] = '{entry.Content}'");
				//}

				return data;
			});
		}

		public static bool LoadFromResource(LocaleNodeFileData stringKeyFileData, Resource resource, ResourceFormat resourceFormat, bool sort = true)
		{
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
							stringKeyFileData.Entries.Add(localeEntry);
						}

					}
				}

				if (resourceFormat == ResourceFormat.LSJ || resourceFormat == ResourceFormat.LSX)
				{
					var rootNode = resource.Regions.First().Value;

					var stringNodes = new List<Node>();

					foreach (var nodeList in rootNode.Children)
					{
						var nodes = FindTranslatedStringsInNodeList(nodeList);
						stringNodes.AddRange(nodes);
					}

					foreach(var node in stringNodes)
					{
						LocaleNodeKeyEntry localeEntry = LoadFromNode(node, resourceFormat);
						stringKeyFileData.Entries.Add(localeEntry);
					}

					/*
					foreach(var region in resource.Regions)
					{
						Debug_TraceRegion(region, 0);
					}
					*/
				}

				if (sort)
				{
					stringKeyFileData.Entries = new ObservableCollectionExtended<ILocaleKeyEntry>(stringKeyFileData.Entries.OrderBy(e => e.Key).ToList());
				}

				return true;
			}
			catch(Exception ex)
			{
				Log.Here().Error($"Error loading from resource: {ex.ToString()}");
				return false;
			}
		}

		public static LocaleNodeKeyEntry LoadFromNode(Node node, ResourceFormat resourceFormat, bool generateNewHandle = false)
		{
			if (resourceFormat == ResourceFormat.LSB)
			{
				LocaleNodeKeyEntry localeEntry = new LocaleNodeKeyEntry(node);
				NodeAttribute keyAtt = null;
				NodeAttribute contentAtt = null;
				node.Attributes.TryGetValue("UUID", out keyAtt);
				node.Attributes.TryGetValue("Content", out contentAtt);

				localeEntry.KeyAttribute = keyAtt;
				localeEntry.KeyIsEditable = true;

				if (contentAtt == null)
				{
					contentAtt = new NodeAttribute(NodeAttribute.DataType.DT_TranslatedString);
					if(contentAtt.Value is TranslatedString translatedString)
					{
						translatedString.Value = "";
						translatedString.Handle = CreateHandle();
						localeEntry.TranslatedString = translatedString;
					}
					localeEntry.TranslatedStringAttribute = contentAtt;
				}
				else
				{
					localeEntry.TranslatedStringAttribute = contentAtt;
					localeEntry.TranslatedString = contentAtt.Value as TranslatedString;
				}

				if(generateNewHandle)
				{
					localeEntry.TranslatedString.Handle = CreateHandle();
				}

				return localeEntry;
			}
			else if (resourceFormat == ResourceFormat.LSJ || resourceFormat == ResourceFormat.LSX)
			{
				LocaleNodeKeyEntry localeEntry = new LocaleNodeKeyEntry(node);
				
				localeEntry.KeyIsEditable = false;
				localeEntry.Key = "Dialog Text";

				NodeAttribute contentAtt = null;
				node.Attributes.TryGetValue("TagText", out contentAtt);
				if (contentAtt != null)
				{
					localeEntry.TranslatedStringAttribute = contentAtt;
					localeEntry.TranslatedString = contentAtt.Value as TranslatedString;
				}
				return localeEntry;
			}
			return null;
		}

		private static List<Node> FindTranslatedStringsInNodeList(KeyValuePair<string, List<LSLib.LS.Node>> nodeList)
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

				foreach (var f in data.SelectedGroup.DataFiles.OfType<LocaleNodeFileData>())
				{
					if (File.Exists(f.SourcePath))
					{
						sourceFiles.Add(f.SourcePath);
					}
				}

				if (sourceFiles.Count > 0)
				{
					var result = await BackupGenerator.CreateArchiveFromFiles(sourceFiles, archivePath, token);
					Log.Here().Activity($"Localization backup result to '{archivePath}': {result.ToString()}");
					return result != BackupResult.Error;
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

		public static async Task<int> SaveDataFiles(LocaleViewModel data, CancellationToken? token = null)
		{
			if (data.SelectedGroup == null) return -1;

			int success = 0;
			if (data.SelectedGroup != data.CustomGroup)
			{
				foreach (LocaleNodeFileData f in data.SelectedGroup.DataFiles.Cast<LocaleNodeFileData>())
				{
					success += await SaveDataFile(f, token);
					f.ChangesUnsaved = false;
				}
			}
			else
			{
				foreach (var f in data.CustomGroup.DataFiles.Cast<LocaleCustomFileData>())
				{
					string targetDirectory = DOS2DEDefaultPaths.CustomLocaleDirectory(data.ModuleData, f.Project);
					success += await SaveDataFile(f, targetDirectory, token);
					f.ChangesUnsaved = false;
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
					var saveFormat = dataFile.Format;
					if (saveFormat == ResourceFormat.LSX && FileCommands.FileExtensionFound(dataFile.SourcePath, ".lsb"))
					{
						saveFormat = ResourceFormat.LSB;
					}
					Log.Here().Activity($"Saving '{dataFile.Name}' to '{dataFile.SourcePath}'.");
					await Task.Run(() => LSLib.LS.ResourceUtils.SaveResource(dataFile.Source, dataFile.SourcePath, saveFormat));
					Log.Here().Important($"Saved '{dataFile.SourcePath}'.");
					return 1;
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

				string outputFilename = Path.Combine(targetDirectory, dataFile.Name, ".json");
				string json = JsonConvert.SerializeObject(dataFile, Newtonsoft.Json.Formatting.Indented);
				if(await FileCommands.WriteToFileAsync(outputFilename, json))
				{
					Log.Here().Important($"Saved '{dataFile.SourcePath}'.");
					return 1;
				}
			}
			catch (Exception ex)
			{
				Log.Here().Error($"Error saving localizaton resource: {ex.ToString()}");
			}
			return 0;
		}

		public static string ExportDataAsXML(LocaleViewModel data, bool exportSourceName = true, bool exportKeyName = true)
		{
			string output = "<contentList>\n{0}</contentList>";
			string entriesStr = "";

			if (data.SelectedGroup != null)
			{
				var fileData = data.SelectedGroup.SelectedFile;
				if (fileData != null)
				{
					string sourcePath = "";
					bool findActualSource = fileData == data.SelectedGroup.CombinedEntries;

					if (!findActualSource && fileData is LocaleNodeFileData keyFileData)
					{
						sourcePath = EscapeXml(Path.GetFileName(keyFileData.SourcePath));
					}

					foreach (var e in fileData.Entries.Where(fd => fd.Selected))
					{
						if (findActualSource)
						{
							var actualSource = data.SelectedGroup.DataFiles.Where(d => d.Entries.Contains(e)).FirstOrDefault();
							if (actualSource is LocaleNodeFileData sourceFileData)
							{
								sourcePath = EscapeXml(Path.GetFileName(sourceFileData.SourcePath));
							}
						}

						var sourceStr = "";

						if (exportSourceName)
						{
							sourceStr = $" Source =\"{sourcePath}\"";
						}

						var keyStr = "";

						if (exportKeyName && !String.IsNullOrWhiteSpace(e.Key) && e.KeyIsEditable)
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

		public static LocaleNodeFileData CreateFileData(string destinationPath, string name)
		{
			var resource = CreateLocalizationResource();
			var fileData = new LocaleNodeFileData(ResourceFormat.LSX, resource, destinationPath, name);
			LoadFromResource(fileData, resource, ResourceFormat.LSB, true);
			fileData.ChangesUnsaved = true;
			return fileData;
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
			}
			return toxml;
		}

		public static LocaleNodeKeyEntry CreateNewLocaleEntry(LocaleNodeFileData fileData, string key = "NewKey", string content = "")
		{
			var rootNode = fileData.Source.Regions.First().Value;

			var refNode = fileData.Entries.Cast<LocaleNodeKeyEntry>().FirstOrDefault(f => f.Node != null)?.Node;
			if (refNode != null)
			{
				var node = new Node();
				node.Parent = rootNode;
				node.Name = refNode.Name;
				//Log.Here().Activity($"Node name: {node.Name}");
				foreach (var kp in refNode.Attributes)
				{
					var att = new NodeAttribute(kp.Value.Type);
					att.Value = new TranslatedString()
					{
						Value = "",
						Handle = CreateHandle()
					};
					node.Attributes.Add(kp.Key, att);
				}

				LocaleNodeKeyEntry localeEntry = LoadFromNode(node, fileData.Format);
				localeEntry.Key = key == "NewKey" ? key + (fileData.Entries.Count + 1) : key;
				localeEntry.Content = content;
				return localeEntry;
			}
			return null;
		}

		private static LocaleNodeFileData CreateNodeFileDataFromTextual(System.IO.StreamReader stream, string sourceDirectory, string filePath, char delimiter)
		{
			//For exporting to lsb later
			string futureSourcePath = Path.Combine(sourceDirectory, Path.GetFileNameWithoutExtension(filePath), ".lsb");

			string name = Path.GetFileName(filePath);
			LocaleNodeFileData fileData = CreateFileData(futureSourcePath, name);

			int lineNum = 0;
			string line = String.Empty;

			while ((line = stream.ReadLine()) != null)
			{
				lineNum += 1;
				// Skip top line, as it typically describes the columns
				Log.Here().Activity(line);
				if (lineNum == 1 && line.Contains("Key\tContent")) continue;
				var parts = line.Split(delimiter);

				var key = parts.ElementAtOrDefault(0);
				var content = parts.ElementAtOrDefault(1);

				if (key == null) key = "NewKey";
				if (content == null) content = "";

				var entry = CreateNewLocaleEntry(fileData, key, content);
				fileData.Entries.Add(entry);
			}

			//Remove the empty default new key
			if (fileData.Entries.Count > 1)
			{
				fileData.Entries.Remove(fileData.Entries.First());
			}

			return fileData;
		}

		public static List<ILocaleFileData> ImportFilesAsData(IEnumerable<string> files, LocaleTabGroup groupData)
		{
			List<ILocaleFileData> newFileDataList = new List<ILocaleFileData>();
			try
			{
				foreach (var path in files)
				{
					if (FileCommands.FileExtensionFound(path, ".lsb", ".lsj"))
					{
						Task<ILocaleFileData> task = Task.Run<ILocaleFileData>(async () => await LoadResourceAsync(path).ConfigureAwait(false));
						if(task?.Result != null)
						{
							newFileDataList.Add(task.Result);
							continue;
						}
					}
					else if (FileCommands.FileExtensionFound(path, ".txt", ".tsv", ".csv"))
					{
						char delimiter = '\t';
						if (FileCommands.FileExtensionFound(path, ".csv")) delimiter = ',';
						
						using (var stream = new System.IO.StreamReader(path))
						{
							foreach(var sourceDir in groupData.SourceDirectories)
							{
								var fileData = CreateNodeFileDataFromTextual(stream, sourceDir, path, delimiter);
								newFileDataList.Add(fileData);
							}
						}
					}
				}
			}
			catch(Exception ex)
			{
				Log.Here().Error("Error importing files to the localization editor: " + ex.ToString());
			}
			LocaleEditorWindow.instance.SaveSettings();
			return newFileDataList;
		}

		public static List<ILocaleKeyEntry> ImportFilesAsEntries(IEnumerable<string> files, LocaleNodeFileData fileData)
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

						Task<ILocaleFileData> task = Task.Run<ILocaleFileData>(async () => await LoadResourceAsync(path).ConfigureAwait(false));
						if (task?.Result != null)
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

						string line = String.Empty;
						using (var stream = new System.IO.StreamReader(path))
						{
							int lineNum = 0;
							while ((line = stream.ReadLine()) != null)
							{
								lineNum += 1;
								// Skip top line, as it typically describes the columns
								if (lineNum == 1 && line.Contains("Key\tContent")) continue;
								var parts = line.Split(delimiter);

								var key = parts.ElementAtOrDefault(0);
								var content = parts.ElementAtOrDefault(1);

								if (key == null) key = "NewKey";
								if (content == null) content = "";

								var entry = CreateNewLocaleEntry(fileData, key, content);
								newEntryList.Add(entry);
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
			LocaleEditorWindow.instance.SaveSettings();
			return newEntryList;
		}
		#endregion

		#region Settings
		public static void LoadSettings(DOS2DEModuleData moduleData, LocaleViewModel localeData)
		{
			string settingsPath = Path.GetFullPath(DOS2DEDefaultPaths.LocalizationEditorSettings(moduleData));

			if(File.Exists(settingsPath))
			{
				Log.Here().Activity($"Loading localization editor settings from '{settingsPath}'.");
				localeData.Settings = JsonConvert.DeserializeObject<LocaleEditorSettingsData>(File.ReadAllText(settingsPath));
			}
			else
			{
				localeData.Settings = new LocaleEditorSettingsData();
			}
		}

		public static void SaveSettings(DOS2DEModuleData moduleData, LocaleViewModel localeData)
		{
			string settingsPath = Path.GetFullPath(DOS2DEDefaultPaths.LocalizationEditorSettings(moduleData));

			if (localeData.Settings != null)
			{
				Log.Here().Activity($"Saving localization editor settings to '{settingsPath}'.");
				string json = JsonConvert.SerializeObject(localeData.Settings, Newtonsoft.Json.Formatting.Indented);
				FileCommands.WriteToFile(settingsPath, json);
			}
		}
		#endregion

		#region Debug
		public static void Debug_CreateEntries(ObservableCollectionExtended<ILocaleKeyEntry> Entries)
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
				Entries.Add(entry);
			}
		}

		public static void Debug_CreateCustomEntries(ObservableCollectionExtended<ILocaleKeyEntry> Entries)
		{
			Random rnd = new Random();

			for (int i = 0; i < 4; i++)
			{
				LocaleCustomKeyEntry data = new LocaleCustomKeyEntry
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
	}
}
