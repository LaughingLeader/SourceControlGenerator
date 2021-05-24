using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Alphaleonis.Win32.Filesystem;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LSLib.LS;
using LSLib.LS.Enums;
using SCG.Core;
using SCG.Data.App;

namespace SCG.Modules.DOS2DE.Utilities
{
	public static class DOS2DEPackageCreator
	{
		private static bool IgnoreFile(string targetFilePath, string ignoredFileName)
		{
			if (Path.GetFileName(targetFilePath).Equals(ignoredFileName, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			if (Path.GetExtension(targetFilePath).Equals(ignoredFileName, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			return false;
		}

		public static async Task<bool> CreatePackage(string dataRootPath, List<string> inputPaths, string outputPath, List<string> ignoredFiles, CancellationToken? token = null)
		{
			try
			{
				if (token == null) token = CancellationToken.None;

				if (token.Value.IsCancellationRequested)
				{
					Log.Here().Warning($"Package cancellation requested.");
					//return Task.FromCanceled(token.Value);
					return false;
				}

				if (!dataRootPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
				{
					dataRootPath += Path.DirectorySeparatorChar;
				}

				var package = new Package();

				AppController.Main.UpdateProgressLog("Parsing project folders...");

				foreach (var f in inputPaths)
				{
					if (token.Value.IsCancellationRequested) throw new TaskCanceledException("Cancelled package creation.");
					AppController.Main.UpdateProgressLog($"Searching folder \"{f}\" for files to add to package.");
					await AddFilesToPackage(package, f, dataRootPath, outputPath, ignoredFiles, token.Value);
				}

				AppController.Main.UpdateProgressLog($"Writing package to \"{outputPath}\"");

				using (var writer = new PackageWriter(package, outputPath))
				{
					await WritePackage(writer, package, outputPath, token.Value);
				}

				Log.Here().Activity($"Package successfully created at {outputPath}");
				return true;
			}
			catch(Exception ex)
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

		private static Task AddFilesToPackage(Package package, string path, string dataRootPath, string outputPath, List<string>ignoredFiles, CancellationToken token)
		{
			Task task = null;

			task = Task.Run(() =>
			{
				if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()))
				{
					path += Path.DirectorySeparatorChar;
				}

				AppController.Main.UpdateProgressLog("Enumerating files...");

				var files = Directory.EnumerateFiles(path, DirectoryEnumerationOptions.Recursive | DirectoryEnumerationOptions.LargeCache, new DirectoryEnumerationFilters()
				{
					InclusionFilter = (f) =>
					{
						return !ignoredFiles.Any(x => IgnoreFile(f.FullPath, x));
					}
				}).ToDictionary(k => k.Replace(dataRootPath, String.Empty), v => v);

				foreach (KeyValuePair<string, string> file in files)
				{
					if (token.IsCancellationRequested)
					{
						throw new TaskCanceledException(task);
					}

					FilesystemFileInfo fileInfo = FilesystemFileInfo.CreateFromEntry(file.Value, file.Key);
					package.Files.Add(fileInfo);
				}
			}, token);

			return task;
		}

		private static Task WritePackage(PackageWriter writer, Package package, string outputPath, CancellationToken token)
		{
			var task = Task.Run(async () =>
			{
				// execute actual operation in child task
				var childTask = Task.Factory.StartNew(() =>
				{
					try
					{
						//writer.WriteProgress += WriteProgressUpdate;
						writer.Version = PackageVersion.V13;
						writer.Compression = CompressionMethod.LZ4;
						writer.CompressionLevel = CompressionLevel.MaxCompression;
						writer.Write();
					}
					catch (Exception)
					{
						// ignored because an exception on a cancellation request 
						// cannot be avoided if the stream gets disposed afterwards 
					}
				}, TaskCreationOptions.AttachedToParent);

				var awaiter = childTask.GetAwaiter();
				while (!awaiter.IsCompleted)
				{
					await Task.Delay(0, token);
				}
			}, token);

			return task;
		}

		public static async Task<bool> ExtractPackageAsync(string pakPath, string outputDirectory, CancellationToken token)
		{
			var task = await Task.Run(async () =>
			{
				// execute actual operation in child task
				var childTask = Task.Factory.StartNew(() =>
				{
					try
					{
						if (token.IsCancellationRequested)
						{
							return false;
						}

						var packager = new Packager();
						packager.UncompressPackage(pakPath, outputDirectory, null);
						return true;
					}
					catch (Exception) { return false; }
				}, TaskCreationOptions.AttachedToParent);

				var awaiter = childTask.GetAwaiter();
				while (!awaiter.IsCompleted)
				{
					await Task.Delay(0, token);
				}
				return childTask.Result;
			}, token);

			return task;
		}
	}
}
