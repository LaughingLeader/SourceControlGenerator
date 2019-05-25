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

				string modsLocalePath = Path.Combine(Path.GetFullPath(Path.Combine(dataRootPath, "Mods")), modProjectData.FolderName, "Localization");
				string publicLocalePath = Path.Combine(Path.GetFullPath(Path.Combine(dataRootPath, "Public")), modProjectData.FolderName, "Localization");

				bool modsExists = Directory.Exists(modsLocalePath);
				bool publicExists = Directory.Exists(publicLocalePath);

				if (!modsExists && !publicExists)
				{
					Log.Here().Warning($"Failed to find localization folders for mod {modProjectData.DisplayName} at path '{modsLocalePath}' and '{publicLocalePath}'.");
					return localizationData;
				}

				if(modsExists)
				{
					Log.Here().Activity($"Loading localization data from '{modsLocalePath}'.");
					var modsLocaleData = await LoadLSBFilesAsync(modsLocalePath, token);
					localizationData.Groups[1].Data = new ObservableCollection<DOS2DEStringKeyFileData>(modsLocaleData);
				}

				if (publicExists)
				{
					Log.Here().Activity($"Loading localization data from '{publicLocalePath}'.");
					var publicLocaleData = await LoadLSBFilesAsync(publicLocalePath, token);
					localizationData.Groups[2].Data = new ObservableCollection<DOS2DEStringKeyFileData>(publicLocaleData);
				}
				else
				{
					localizationData.Groups[2].Visibility = false;
				}

				localizationData.UpdateAll();

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

		private static async Task<List<DOS2DEStringKeyFileData>> LoadLSBFilesAsync(string directoryPath, CancellationToken? token = null)
		{
			List<DOS2DEStringKeyFileData> stringKeyData = new List<DOS2DEStringKeyFileData>();

			var filters = new DirectoryEnumerationFilters()
			{
				InclusionFilter = f =>
				{
					return Path.GetExtension(f.FileName).Equals(".lsb", StringComparison.OrdinalIgnoreCase);
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
				var data = await LoadLSBAsync(filePath);
				stringKeyData.Add(data);
			}
			stringKeyData = stringKeyData.OrderBy(f => f.Name).ToList();
			return stringKeyData;
		}

		public static async Task<DOS2DEStringKeyFileData> LoadLSBAsync(string path, CancellationToken? token = null)
		{
			return await Task.Run(() =>
			{
				var resource = LSLib.LS.ResourceUtils.LoadResource(path, ResourceFormat.LSB);

				var data = new DOS2DEStringKeyFileData(resource, Path.GetFileName(path));
				data.Entries = data.Entries.OrderBy(e => e.Key).ToList();
				//foreach (var entry in data.Entries)
				//{
				//	Log.Here().Activity($"Entry: Key[{entry.Key}] = '{entry.Content}'");
				//}

				return data;
			});
		}

		public static string NewHandle()
		{
			return Guid.NewGuid().ToString().Replace('-', 'g').Insert(0, "h");
		}
	}
}
