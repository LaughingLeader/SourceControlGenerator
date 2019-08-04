using System;
using System.Collections.Generic;
using Alphaleonis.Win32.Filesystem;
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
using System.Threading;

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
		public static async Task<BackupResult> CreateArchiveFromRoot(string rootPath, List<JunctionData> sourceFolders, string archiveFilePath, bool updateProgress = false, CancellationToken? token = null, int updateValue = 1)
		{
			if (sourceFolders != null && sourceFolders.Count > 0)
			{
				try
				{
					if (token == null) token = CancellationToken.None;

					var foldersToParse = sourceFolders.Where(folderData => Directory.Exists(folderData.SourcePath));
					//ConcurrentBag<FileInfo> targetFiles = new ConcurrentBag<FileInfo>();
					var targetFiles = new ConcurrentBag<string>();

					if (updateProgress) AppController.Main.UpdateProgressLog($"Searching source folders for files to save...");

					var tasks = foldersToParse.Select(f => Task.Run(() =>
					{
						foreach (var file in Directory.EnumerateFiles(f.SourcePath, "*", System.IO.SearchOption.AllDirectories))
						{
							if (!String.IsNullOrEmpty(file) && File.Exists(file)) targetFiles.Add(file);
						}
					}));

					await Task.WhenAll(tasks).ConfigureAwait(false);

					if (updateProgress) AppController.Main.UpdateProgress(updateValue);

					if (targetFiles.Count > 0)
					{
						//if (updateProgress) AppController.Main.Data.ProgressValueMax += targetFiles.Count;

						using (var zip = File.OpenWrite(archiveFilePath))
						using (var zipWriter = WriterFactory.Open(zip, ArchiveType.Zip, CompressionType.Deflate))
						{
							if (updateProgress) AppController.Main.UpdateProgressLog("Saving archive...");
							foreach (var f in targetFiles)
							{
								//Disabled for now, since it seems to slow the process down.
								//if (updateProgress) AppController.Main.UpdateProgressLog($"Adding \"{f.Replace(rootPath, "").Replace(@"\", "/").Substring(1)}\" to archive...");
								await WriteZipAsync(zipWriter, f.Replace(rootPath, ""), f, token.Value).ConfigureAwait(false);
							}

							return BackupResult.Success;
						}
					}
					else
					{
						Log.Here().Important("No files found while attempting to back up project.");
						if (updateProgress) AppController.Main.UpdateProgressLog("No files found. Skipping.");
						return BackupResult.Skipped; // Gracefully skip
					}
				}
				catch(Exception ex)
				{
					if (!token.Value.IsCancellationRequested)
					{
						Log.Here().Error($"Error writing archive {rootPath} to {archiveFilePath}: {ex.ToString()}");
					}
					else
					{
						Log.Here().Warning($"Cancelled writing archive \"{archiveFilePath}\".");
					}
				}
			}
			else
			{
				Log.Here().Error($"Source folders for project are empty.");
			}
			return BackupResult.Error;
		}

		public static async Task<BackupResult> CreateArchiveFromRepo(string repoPath, string rootPath, List<JunctionData> sourceFolders, string archiveFilePath, bool updateProgress = false, CancellationToken? token = null, int updateValue = 1)
		{
			if (sourceFolders != null && sourceFolders.Count > 0)
			{
				try
				{
					if (token == null) token = CancellationToken.None;

					var foldersToParse = sourceFolders.Where(folderData => Directory.Exists(folderData.SourcePath)).ToList();
					var targetFiles = new ConcurrentBag<string>();

					foreach(var f in foldersToParse)
					{
						if (updateProgress) AppController.Main.UpdateProgressLog($"Searching source folder \"{Path.GetDirectoryName(f.SourcePath)}\" for files to save.");
						await CrawlDirectoryAsync(targetFiles, f.SourcePath, "*", System.IO.SearchOption.AllDirectories, token.Value).ConfigureAwait(false);
					}

					if (updateProgress) AppController.Main.UpdateProgressLog($"Searching repository for files to save.");
					await CrawlDirectoryAsync(targetFiles, repoPath, "*", System.IO.SearchOption.TopDirectoryOnly, token.Value).ConfigureAwait(false);

					if (updateProgress) AppController.Main.UpdateProgress(updateValue);

					if (targetFiles.Count > 0)
					{
						//if (updateProgress) AppController.Main.Data.ProgressValueMax += targetFiles.Count;

						using (var zip = File.OpenWrite(archiveFilePath))
						using (var zipWriter = WriterFactory.Open(zip, ArchiveType.Zip, CompressionType.Deflate))
						{
							if (updateProgress) AppController.Main.UpdateProgressLog("Saving archive...");
							foreach (var f in targetFiles)
							{
								if (token.Value.IsCancellationRequested)
								{
									if (updateProgress) AppController.Main.UpdateProgressLog("Canceling...");
									return BackupResult.Skipped;
								}
								//Log.Here().Important($"Adding file {f} to archive.");
								await WriteZipAsync(zipWriter, f.Replace(rootPath, "").Replace(repoPath, ""), f, token.Value).ConfigureAwait(false);
							}

							return BackupResult.Success;
						}
					}
					else
					{
						if (updateProgress) AppController.Main.UpdateProgressLog("No files found. Skipping.");
						return BackupResult.Skipped;
					}
				}
				catch (Exception ex)
				{
					if (!token.Value.IsCancellationRequested)
					{
						Log.Here().Error($"Error writing archive {repoPath} to {archiveFilePath}: {ex.ToString()}");
					}
					else
					{
						Log.Here().Warning($"Cancelled writing archive \"{archiveFilePath}\".");
					}
				}
			}
			else
			{
				Log.Here().Error($"Source folders for project are null or empty.");
			}
			return BackupResult.Error;
		}

		public static async Task<BackupResult> CreateArchiveFromDirectory(string directoryPath, string archiveFilePath, bool updateProgress = true, CancellationToken? token = null, int updateValue = 1)
		{
			try
			{
				if (token == null) token = CancellationToken.None;

				var targetFiles = new ConcurrentBag<string>();

				if (updateProgress) AppController.Main.UpdateProgressLog($"Searching folder for files to save.");

				var task = Task.Factory.StartNew(() =>
				{
					foreach (var file in Directory.EnumerateFiles(directoryPath, "*", System.IO.SearchOption.AllDirectories))
					{
						if (!String.IsNullOrEmpty(file) && File.Exists(file)) targetFiles.Add(file);
					}
				});

				await Task.WhenAll(task).ConfigureAwait(false);

				if (updateProgress) AppController.Main.UpdateProgress(updateValue);

				if (targetFiles.Count > 0)
				{
					if (updateProgress)
					{
						AppController.Main.Data.ProgressValueMax += targetFiles.Count;
					}

					using (var zip = File.OpenWrite(archiveFilePath))
					using (var zipWriter = WriterFactory.Open(zip, ArchiveType.Zip, CompressionType.Deflate))
					{
						if (updateProgress) AppController.Main.UpdateProgressLog("Saving archive...");
						foreach (var f in targetFiles)
						{
							//Log.Here().Important($"Adding file {f} to archive.");

							await WriteZipAsync(zipWriter, f.Replace(directoryPath, ""), f, token.Value).ConfigureAwait(false);
						}

						return BackupResult.Success;
					}
				}
				else
				{
					if (updateProgress) AppController.Main.UpdateProgressLog("No files found. Skipping.");
					return BackupResult.Skipped;
				}
			}
			catch (Exception ex)
			{
				if(!token.Value.IsCancellationRequested)
				{
					Log.Here().Error($"Error writing archive {directoryPath} to {archiveFilePath}: {ex.ToString()}");
				}
				else
				{
					Log.Here().Warning($"Cancelled writing archive \"{archiveFilePath}\".");
				}
			}
			return BackupResult.Error;
		}

		public static async Task<BackupResult> CreateArchiveFromFiles(List<string> files, string archiveFilePath, IEnumerable<string> trimPath = null, CancellationToken? token = null)
		{
			try
			{
				if (files.Count > 0)
				{
					if (token == null) token = CancellationToken.None;

					using (var zip = File.OpenWrite(archiveFilePath))
					using (var zipWriter = WriterFactory.Open(zip, ArchiveType.Zip, CompressionType.Deflate))
					{
						Log.Here().Activity($"Saving {files.Count} files to archive at '{archiveFilePath}'.");
						foreach (var f in files)
						{
							if(trimPath == null)
							{
								await WriteZipAsync(zipWriter, f, f, token.Value);
							}
							else
							{
								string entryPath = f;
								foreach(var str in trimPath)
								{
									entryPath = entryPath.Replace(str, "");
									//Log.Here().Activity($"Replacing '{str}' in '{entryPath}'");
								}
								await WriteZipAsync(zipWriter, entryPath, f, token.Value);
							}
						}

						return BackupResult.Success;
					}
				}
				else
				{
					Log.Here().Error($"Source folders for project are empty.");
				}
			}
			catch (Exception ex)
			{
				if (!token.Value.IsCancellationRequested)
				{
					Log.Here().Error($"Error writing archive to '{archiveFilePath}': {ex.ToString()}");
				}
				else
				{
					Log.Here().Warning($"Cancelled writing archive \"{archiveFilePath}\".");
				}
			}
			return BackupResult.Error;
		}

		private static Task CrawlDirectoryAsync(ConcurrentBag<string> outputBag, string baseDirectory, string searchPattern, System.IO.SearchOption searchOptions, CancellationToken token)
		{
			if (token.IsCancellationRequested)
			{
				Log.Here().Warning($"Directory \"{baseDirectory}\" crawling cancel requested.");
				return Task.FromCanceled(token);
			}

			var task = Task.Run(async () =>
			{
				// execute actual operation in child task
				var childTask = Task.Factory.StartNew(() =>
				{
					try
					{
						foreach (var file in Directory.EnumerateFiles(baseDirectory, searchPattern, searchOptions))
						{
							if (!String.IsNullOrEmpty(file) && File.Exists(file)) outputBag.Add(file);
							//Log.Here().Important($"Added file \"{file}\"");
						}
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
					await Task.Delay(0, token).ConfigureAwait(false);
				}
			}, token);

			return task;
		}

		private static Task WriteZipAsync(IWriter writer, string entryName, string source, CancellationToken token)
		{
			if (token.IsCancellationRequested)
			{
				Log.Here().Warning($"Archive cancellation requested.");
				return Task.FromCanceled(token);
			}

			var task = Task.Run(async () =>
			{
				// execute actual operation in child task
				var childTask = Task.Factory.StartNew(() =>
				{
					try
					{
						writer.Write(entryName, source);
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
					await Task.Delay(0, token).ConfigureAwait(false);
				}
			}, token);

			return task;
		}
	}
}
