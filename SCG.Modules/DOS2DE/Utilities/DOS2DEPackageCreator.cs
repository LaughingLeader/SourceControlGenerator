using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LSLib.LS;
using LSLib.LS.Enums;
using SCG.Data.App;

namespace SCG.Modules.DOS2DE.Utilities
{
	public static class DOS2DEPackageCreator
	{
		public static async Task<bool> CreatePackage(string dataRootPath, List<string> inputPaths, string outputPath, List<string> ignoredFiles)
		{
			try
			{
				if (!dataRootPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
				{
					dataRootPath += Path.DirectorySeparatorChar;
				}

				var package = new Package();

				var tasks = inputPaths.Select(f => Task.Run(() =>
				{
					string path = f;

					if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()))
					{
						path += Path.DirectorySeparatorChar;
					}

					Dictionary<string, string> files = Alphaleonis.Win32.Filesystem.Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories)
					.ToDictionary(k => k.Replace(dataRootPath, String.Empty), v => v);

					foreach (KeyValuePair<string, string> file in files)
					{
						if(!ignoredFiles.Contains(Path.GetFileName(file.Value)))
						{
							FilesystemFileInfo fileInfo = FilesystemFileInfo.CreateFromEntry(file.Value, file.Key);
							package.Files.Add(fileInfo);
						}
					}
				}));

				await Task.WhenAll(tasks).ConfigureAwait(false);

				using (var writer = new PackageWriter(package, outputPath))
				{
					//writer.WriteProgress += WriteProgressUpdate;
					writer.Version = PackageVersion.V13;
					writer.Compression = CompressionMethod.LZ4;
					writer.CompressionLevel = CompressionLevel.MaxCompression;
					writer.Write();
				}

				Log.Here().Activity($"Package successfully created at {outputPath}");
				return true;
			}
			catch(Exception ex)
			{
				Log.Here().Error($"Error creating package: {ex.ToString()}");
				return false;
			}
		}
	}
}
