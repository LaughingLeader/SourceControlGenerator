using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SCG.Data;
using SCG.Data.View;
using SCG.Interfaces;
using SCG.Data.App;
using SCG.Util;
using System.Windows;
using SharpCompress.Archives.Zip;
using SharpCompress.Archives;
using SharpCompress.Writers;
using SharpCompress.Writers.Zip;
using SharpCompress.Common;
using SCG.Core;
using System.Collections.Concurrent;

namespace SCG.FileGen
{
	public enum BackupResult
	{
		Success,
		Skipped,
		Error
	}

	public static class BackupGenerator
	{
		public static async Task<BackupResult> CreateArchiveFromRoot(string rootPath, List<JunctionData> sourceFolders, string archiveFilePath, int incrementProgressAmount = -1)
		{
			if (sourceFolders != null && sourceFolders.Count > 0)
			{
				try
				{
					var foldersToParse = sourceFolders.Where(folderData => Directory.Exists(folderData.SourcePath));
					//ConcurrentBag<FileInfo> targetFiles = new ConcurrentBag<FileInfo>();
					var targetFiles = new ConcurrentBag<string>();

					if (incrementProgressAmount > 0) AppController.Main.UpdateProgressLog($"Searching source folders for files to save...");

					var tasks = foldersToParse.Select(f => Task.Run(() =>
					{
						foreach (var file in Directory.EnumerateFiles(f.SourcePath, "*", SearchOption.AllDirectories))
						{
							if (!String.IsNullOrEmpty(file) && File.Exists(file)) targetFiles.Add(file);
						}
					}));

					await Task.WhenAll(tasks).ConfigureAwait(false);

					if(targetFiles.Count > 0)
					{
						if (incrementProgressAmount > 0)
						{
							incrementProgressAmount = incrementProgressAmount / targetFiles.Count;
						}

						using (var zip = File.OpenWrite(archiveFilePath))
						using (var zipWriter = WriterFactory.Open(zip, ArchiveType.Zip, CompressionType.Deflate))
						{
							if (incrementProgressAmount > 0) AppController.Main.UpdateProgressLog("Saving archive...");
							foreach (var f in targetFiles)
							{
								//Disabled for now, since it seems to slow the process down.
								//if (incrementProgressAmount > 0) AppController.Main.UpdateProgressLog($"Adding \"{f.Replace(rootPath, "").Replace(@"\", "/").Substring(1)}\" to archive...");
								zipWriter.Write(f.Replace(rootPath, ""), f);
								if (incrementProgressAmount > 0) AppController.Main.UpdateProgress(incrementProgressAmount);
							}

							return BackupResult.Success;
						}
					}
					else
					{
						Log.Here().Important("No files found while attempting to back up project.");
						if (incrementProgressAmount > 0) AppController.Main.UpdateProgress(incrementProgressAmount, "No files found. Skipping.");
						return BackupResult.Skipped; // Gracefully skip
					}
				}
				catch(Exception ex)
				{
					Log.Here().Error($"Error writing archive {rootPath} to {archiveFilePath}: {ex.ToString()}");
				}
			}
			else
			{
				Log.Here().Error($"Source folders for project are empty.");
			}
			return BackupResult.Error;
		}

		public static async Task<BackupResult> CreateArchiveFromRepo(string repoPath, string rootPath, List<JunctionData> sourceFolders, string archiveFilePath, int incrementProgressAmount = -1)
		{
			if (sourceFolders != null && sourceFolders.Count > 0)
			{
				try
				{
					var foldersToParse = sourceFolders.Where(folderData => Directory.Exists(folderData.SourcePath)).ToList();
					var targetFiles = new ConcurrentBag<string>();

					var tasks = foldersToParse.Select(f => Task.Run(() =>
					{
						foreach (var file in Directory.EnumerateFiles(f.SourcePath, "*", SearchOption.AllDirectories))
						{
							if (!String.IsNullOrEmpty(file) && File.Exists(file)) targetFiles.Add(file);
						}
					}));

					tasks.Append(Task.Run(() =>
					{
						if (incrementProgressAmount > 0) AppController.Main.UpdateProgressLog($"Searching repository for files to save.");

						foreach (var file in Directory.EnumerateFiles(repoPath, "*", SearchOption.TopDirectoryOnly))
						{
							if (!String.IsNullOrEmpty(file) && File.Exists(file)) targetFiles.Add(file);
						}
					}));

					if (incrementProgressAmount > 0) AppController.Main.UpdateProgressLog($"Searching source folders for files to save.");

					await Task.WhenAll(tasks).ConfigureAwait(false);

					if (targetFiles.Count > 0)
					{
						if (incrementProgressAmount > 0)
						{
							incrementProgressAmount = incrementProgressAmount / targetFiles.Count;
						}

						using (var zip = File.OpenWrite(archiveFilePath))
						using (var zipWriter = WriterFactory.Open(zip, ArchiveType.Zip, CompressionType.Deflate))
						{
							if (incrementProgressAmount > 0) AppController.Main.UpdateProgressLog("Saving archive...");
							foreach (var f in targetFiles)
							{
								//if (incrementProgressAmount > 0) AppController.Main.UpdateProgressLog($"Adding \"{f.Replace(rootPath, "").Replace(repoPath, "").Replace(@"\", "/").Substring(1)}\" to archive...");
								zipWriter.Write(f.Replace(rootPath, "").Replace(repoPath, ""), f);
								if (incrementProgressAmount > 0) AppController.Main.UpdateProgress(incrementProgressAmount);
							}

							return BackupResult.Success;
						}
					}
					else
					{
						if (incrementProgressAmount > 0) AppController.Main.UpdateProgress(incrementProgressAmount, "No files found. Skipping.");
						return BackupResult.Skipped;
					}
				}
				catch (Exception ex)
				{
					Log.Here().Error($"Error writing archive {repoPath} to {archiveFilePath}: {ex.ToString()}");
				}
			}
			else
			{
				Log.Here().Error($"Source folders for project are null or empty.");
			}
			return BackupResult.Error;
		}

		public static async Task<BackupResult> CreateArchiveFromDirectory(string directoryPath, string archiveFilePath, int incrementProgressAmount = -1)
		{
			try
			{
				var targetFiles = new ConcurrentBag<string>();

				if (incrementProgressAmount > 0) AppController.Main.UpdateProgressLog($"Searching folder for files to save.");

				foreach (var file in Directory.EnumerateFiles(directoryPath, "*", SearchOption.AllDirectories))
				{
					if (!String.IsNullOrEmpty(file) && File.Exists(file)) targetFiles.Add(file);
				}

				if (targetFiles.Count > 0)
				{
					if (incrementProgressAmount > 0)
					{
						incrementProgressAmount = incrementProgressAmount / targetFiles.Count;
					}

					using (var zip = File.OpenWrite(archiveFilePath))
					using (var zipWriter = WriterFactory.Open(zip, ArchiveType.Zip, CompressionType.Deflate))
					{
						if (incrementProgressAmount > 0) AppController.Main.UpdateProgressLog("Saving archive...");
						foreach (var f in targetFiles)
						{
							//if (incrementProgressAmount > 0) AppController.Main.UpdateProgressLog($"Adding \"{f.Replace(directoryPath, "").Replace(@"\", "/").Substring(1)}\" to archive...");
							zipWriter.Write(f.Replace(directoryPath, ""), f);
							if (incrementProgressAmount > 0) AppController.Main.UpdateProgress(incrementProgressAmount);
						}

						return BackupResult.Success;
					}
				}
				else
				{
					if (incrementProgressAmount > 0) AppController.Main.UpdateProgress(incrementProgressAmount, "No files found. Skipping.");
					return BackupResult.Skipped;
				}
			}
			catch (Exception ex)
			{
				Log.Here().Error($"Error writing archive {directoryPath} to {archiveFilePath}: {ex.ToString()}");
			}
			return BackupResult.Error;
		}
	}
}
