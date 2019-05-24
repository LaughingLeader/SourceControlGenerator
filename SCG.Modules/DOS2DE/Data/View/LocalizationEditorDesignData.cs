using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Alphaleonis.Win32.Filesystem;
using LSLib.LS.Enums;
using SCG.Modules.DOS2DE.Utilities;

namespace SCG.Modules.DOS2DE.Data.View
{
	public class LocalizationEditorDesignData
	{
		public DOS2DELocalizationViewData Data { get; set; }

		public string Name { get; set; } = "Test";

		public Visibility ModsVisible { get; set; } = Visibility.Hidden;

		public Visibility PublicVisible { get; set; } = Visibility.Hidden;

		public LocalizationEditorDesignData()
		{
			Data = new DOS2DELocalizationViewData();

			var metaFile = new FileInfo(@"G:\Divinity Original Sin 2\DefEd\Data\Mods\Nemesis_627c8d3a-7e6b-4fd2-8ce5-610d553fdbe9\meta.lsx");
			ModProjectData testData = new ModProjectData(metaFile, @"G:\Divinity Original Sin 2\DefEd\Data\Projects");

			//var result = LoadStringKeyData(@"G:\Divinity Original Sin 2\DefEd\Data\", testData);
			//Data = result.Data;
			//Name = result.Error;

			Data = new DOS2DELocalizationViewData();
			Name = testData.DisplayName;
			Data.ModsLocalization = new ObservableCollection<DOS2DEStringKeyFileData>();
			Data.ModsLocalization.Add(new DOS2DEStringKeyFileData(null, "Skills"));
			Data.ModsLocalization.Add(new DOS2DEStringKeyFileData(null, "Statuses"));
			Data.ModsLocalization.Add(new DOS2DEStringKeyFileData(null, "Potions"));

			Data.PublicLocalization = new ObservableCollection<DOS2DEStringKeyFileData>();
			Data.PublicLocalization.Add(new DOS2DEStringKeyFileData(null, "Skills"));
			Data.PublicLocalization.Add(new DOS2DEStringKeyFileData(null, "Statuses"));
			Data.PublicLocalization.Add(new DOS2DEStringKeyFileData(null, "Potions"));

			foreach(var d in Data.ModsLocalization)
			{
				d.Debug_TestEntries();
			}

			foreach (var d in Data.PublicLocalization)
			{
				d.Debug_TestEntries();
			}

			ModsVisible = Data.ModsLocalization.Count > 0 ? Visibility.Visible : Visibility.Hidden;
			PublicVisible = Data.ModsLocalization.Count > 0 ? Visibility.Visible : Visibility.Hidden;
		}

		public struct DesignResult<T>
		{
			public T Data;
			public string Error;
		}

		public DesignResult<T> NewResult<T>(T data, string error = "")
		{
			return new DesignResult<T>()
			{
				Data = data,
				Error = error
			};
		}

		public DesignResult<DOS2DELocalizationViewData> LoadStringKeyData(string dataRootPath, ModProjectData modProjectData)
		{
			DOS2DELocalizationViewData localizationData = new DOS2DELocalizationViewData();

			string error = "";

			try
			{
				
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
					return NewResult(localizationData, $"Failed to find localization folders for mod {modProjectData.DisplayName} at path '{modsLocalePath}' and '{publicLocalePath}'.");
				}

				if (modsExists)
				{
					Log.Here().Activity($"Loading localization data from '{modsLocalePath}'.");
					var modsLocaleData = LoadLSBFiles(modsLocalePath);
					error += modsLocaleData.Error;
					localizationData.ModsLocalization = new ObservableCollection<DOS2DEStringKeyFileData>(modsLocaleData.Data);
				}

				if (publicExists)
				{
					Log.Here().Activity($"Loading localization data from '{publicLocalePath}'.");
					var publicLocaleData = LoadLSBFiles(publicLocalePath);
					error += publicLocaleData.Error;
					localizationData.PublicLocalization = new ObservableCollection<DOS2DEStringKeyFileData>(publicLocaleData.Data);
				}

				return NewResult(localizationData, error);
			}
			catch (Exception ex)
			{
				error += $"Error creating package: {ex.ToString()}";
				return NewResult(localizationData, error);
			}
		}

		private DesignResult<List<DOS2DEStringKeyFileData>> LoadLSBFiles(string directoryPath)
		{
			List<DOS2DEStringKeyFileData> stringKeyData = new List<DOS2DEStringKeyFileData>();

			string error = "";

			var filters = new DirectoryEnumerationFilters()
			{
				InclusionFilter = f =>
				{
					return Path.GetExtension(f.FileName).Equals("lsb", StringComparison.OrdinalIgnoreCase);
				},
				ErrorFilter = delegate (int errorCode, string errorMessage, string pathProcessed)
				{
					var gotException = errorCode == 5;

					if (gotException)
					{
						error += $"Error reading file at '{pathProcessed}': [{errorCode}]({errorMessage})";
					}

					return gotException;
				},
				RecursionFilter = f =>
				{
					return true;
				}
			};
			var lsbFiles = Directory.EnumerateFiles(directoryPath, Alphaleonis.Win32.Filesystem.DirectoryEnumerationOptions.Recursive, filters);

			foreach (var filePath in lsbFiles)
			{
				var data = LoadLSB(filePath);
				stringKeyData.Add(data);
			}
			return NewResult(stringKeyData, error);
		}

		public DOS2DEStringKeyFileData LoadLSB(string path)
		{
			var resource = LSLib.LS.ResourceUtils.LoadResource(path, ResourceFormat.LSB);

			var data = new DOS2DEStringKeyFileData(resource);

			foreach (var entry in data.Entries)
			{
				Log.Here().Activity($"Entry: Key[{entry.Key}] = '{entry.Content}'");
			}

			return data;
		}
	}
}
