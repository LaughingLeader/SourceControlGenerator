﻿using System;
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
using System.Xml;
using System.Xml.Serialization;
using System.Collections.ObjectModel;

namespace SCG.Modules.DOS2DE.Utilities
{
	public class DOS2DELocalizationEditor
	{
		public static async Task<DOS2DELocalizationViewData> LoadLocalizationDataAsync(string dataRootPath, ModProjectData modProjectData, CancellationToken? token = null)
		{
			DOS2DELocalizationViewData localizationData = new DOS2DELocalizationViewData();
			try
			{
				if (token == null) token = CancellationToken.None;

				if (token.Value.IsCancellationRequested)
				{
					Log.Here().Warning($"Localization loading cancellation requested.");
					return localizationData;
				}

				if (!dataRootPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
				{
					dataRootPath += Path.DirectorySeparatorChar;
				}

				string modsRoot = Path.GetFullPath(Path.Combine(dataRootPath, "Mods", modProjectData.FolderName));
				string publicRoot = Path.GetFullPath(Path.Combine(dataRootPath, "Public", modProjectData.FolderName));

				bool modsExists = Directory.Exists(modsRoot);
				bool publicExists = Directory.Exists(publicRoot);

				if (!modsExists && !publicExists)
				{
					Log.Here().Warning($"Failed to find root folders for mod {modProjectData.DisplayName} at path '{modsRoot}' and '{publicRoot}'.");
					return localizationData;
				}

				if (modsExists)
				{
					string modsLocalePath = Path.Combine(modsRoot, "Localization");
					string dialogLocalePath = Path.Combine(modsRoot, "Story", "Dialogs");

					if (Directory.Exists(modsLocalePath))
					{
						Log.Here().Activity($"Loading localization data from '{modsLocalePath}'.");
						var modsLocaleData = await LoadFilesAsync(modsLocalePath, token, ".lsb");
						localizationData.ModsGroup.DataFiles = new ObservableRangeCollection<IKeyFileData>(modsLocaleData);
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
						localizationData.DialogGroup.DataFiles = new ObservableRangeCollection<IKeyFileData>(dialogLocaleData);
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
						localizationData.PublicGroup.DataFiles = new ObservableRangeCollection<IKeyFileData>(publicLocaleData);
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

				localizationData.UpdateCombinedGroup();

				Log.Here().Activity($"Localization loaded.");

				return localizationData;
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
				return localizationData;
			}
		}

		private static async Task<List<DOS2DEStringKeyFileData>> LoadFilesAsync(string directoryPath, CancellationToken? token = null, params string[] fileExtensions)
		{
			List<DOS2DEStringKeyFileData> stringKeyData = new List<DOS2DEStringKeyFileData>();

			var filters = new DirectoryEnumerationFilters()
			{
				InclusionFilter = f =>
				{
					if(FileCommands.FileExtensionFound(f.FileName, fileExtensions))
					{
						//Log.Here().Activity($"Matched extension '{String.Join(",", fileExtensions)}' in file '{f.FileName}'|'{f.FullPath}'");
						return true;
					}
					else
					{
						//Log.Here().Activity($"Did not find extension '{String.Join(",", fileExtensions)}' in file '{f.FileName}'|'{f.FullPath}'");
						return false;
					}
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

		public static async Task<DOS2DEStringKeyFileData> LoadResourceAsync(string path, CancellationToken? token = null)
		{
			return await Task.Run(() =>
			{
				var resourceFormat = ResourceFormat.LSB;
				if(FileCommands.FileExtensionFound(path, ".lsj"))
				{
					resourceFormat = ResourceFormat.LSJ;
				}

				var resource = LSLib.LS.ResourceUtils.LoadResource(path, resourceFormat);

				var data = new DOS2DEStringKeyFileData(resourceFormat, resource, path, Path.GetFileName(path));
				LoadFromResource(data, resource, resourceFormat);
				
				//foreach (var entry in data.Entries)
				//{
				//	Log.Here().Activity($"Entry: Key[{entry.Key}] = '{entry.Content}'");
				//}

				return data;
			});
		}

		public static bool LoadFromResource(DOS2DEStringKeyFileData stringKeyFileData, Resource resource, ResourceFormat resourceFormat, bool sort = true)
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
							DOS2DEKeyEntry localeEntry = new DOS2DEKeyEntry(node);
							NodeAttribute keyAtt = null;
							NodeAttribute contentAtt = null;
							node.Attributes.TryGetValue("UUID", out keyAtt);
							node.Attributes.TryGetValue("Content", out contentAtt);

							localeEntry.KeyAttribute = keyAtt;
							localeEntry.LockKey = true;

							if (contentAtt != null)
							{
								localeEntry.TranslatedStringAttribute = contentAtt;
								localeEntry.TranslatedString = contentAtt.Value as TranslatedString;
							}

							stringKeyFileData.Entries.Add(localeEntry);
						}

					}
				}

				if (resourceFormat == ResourceFormat.LSJ)
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
						DOS2DEKeyEntry localeEntry = new DOS2DEKeyEntry(node);
						stringKeyFileData.Entries.Add(localeEntry);

						localeEntry.LockKey = false;

						NodeAttribute contentAtt = null;
						node.Attributes.TryGetValue("TagText", out contentAtt);
						if (contentAtt != null)
						{
							localeEntry.TranslatedStringAttribute = contentAtt;
							localeEntry.TranslatedString = contentAtt.Value as TranslatedString;
						}
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
					stringKeyFileData.Entries = new ObservableRangeCollection<DOS2DEKeyEntry>(stringKeyFileData.Entries.OrderBy(e => e.Key).ToList());
				}

				return true;
			}
			catch(Exception ex)
			{
				Log.Here().Error($"Error loading from resource: {ex.ToString()}");
				return false;
			}
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

		public static void Debug_CreateEntries(ObservableRangeCollection<DOS2DEKeyEntry> Entries)
		{
			for (int i = 0; i < 4; i++)
			{
				var node = new LSLib.LS.Node();
				var att = new LSLib.LS.NodeAttribute(LSLib.LS.NodeAttribute.DataType.DT_TranslatedString);
				att.Value = new LSLib.LS.TranslatedString();

				var handle = Guid.NewGuid().ToString().Replace('-', 'g').Insert(0, "h");
				if (att.Value is LSLib.LS.TranslatedString str)
				{
					str.Handle = handle;
					str.Value = "Test";
				}

				node.Attributes.Add("Content", att);
				var entry = new DOS2DEKeyEntry(node);
				Entries.Add(entry);
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

		public static string NewHandle()
		{
			return Guid.NewGuid().ToString().Replace('-', 'g').Insert(0, "h");
		}

		public static string ExportData(DOS2DELocalizationViewData data)
		{
			string output = "<contentList>\n{0}</contentList>";
			string entriesStr = "";
			foreach(var g in data.Groups)
			{
				if(g != data.CombinedGroup)
				{
					foreach(var f in g.DataFiles)
					{
						string sourcePath = "";
						if (f is DOS2DEStringKeyFileData fileData)
						{
							sourcePath = Path.GetFileName(fileData.SourcePath);
						}

						foreach (var e in f.Entries)
						{
							if (!String.IsNullOrWhiteSpace(e.Key) && e.Key != "None")
							{
								entriesStr += "\t" + $"<content contentuid=\"{e.Handle}\" Source=\"{sourcePath}\" Key=\"{e.Key}\">{e.Content}</content>" + Environment.NewLine;
							}
							else
							{
								entriesStr += "\t" + $"<content contentuid=\"{e.Handle}\" Source=\"{sourcePath}\">{e.Content}</content>" + Environment.NewLine;
							}
						}
					}
				}
			}

			return String.Format(output, entriesStr);
		}
	}
}
