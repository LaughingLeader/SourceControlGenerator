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
	public static class BackupGenerator
	{
		public static async Task<bool> CreateArchiveFromRoot(string rootPath, List<JunctionData> sourceFolders, string archiveFilePath, int incrementProgressAmount = -1)
		{
			if (sourceFolders != null && sourceFolders.Count > 0)
			{
				try
				{
					var foldersToParse = sourceFolders.Where(folderData => Directory.Exists(folderData.SourcePath));
					ConcurrentBag<FileInfo> targetFiles = new ConcurrentBag<FileInfo>();

					var tasks = foldersToParse.Select(f => Task.Run(() =>
					{
						var files = Directory.EnumerateFiles(f.SourcePath, "*", SearchOption.AllDirectories);
						if (files != null)
						{
							foreach (var filePath in files)
							{
								if (File.Exists(filePath))
								{
									var fileInfo = new FileInfo(filePath);
									if (fileInfo != null) targetFiles.Add(fileInfo);
								}
							}
						}

					}));

					if (incrementProgressAmount > 0) AppController.Main.UpdateProgressLog($"Searching source folders for files to save...");

					await Task.WhenAll(tasks).ConfigureAwait(false);

					if (incrementProgressAmount > 0)
					{
						incrementProgressAmount = incrementProgressAmount / targetFiles.Count;
					}

					using (var zip = ZipArchive.Create())
					{
						if (incrementProgressAmount > 0) AppController.Main.UpdateProgressLog("Adding files to archive...");

						foreach(var f in targetFiles) 
						{
							zip.AddEntry(f.FullName.Replace(rootPath, ""), f);
							if (incrementProgressAmount > 0) AppController.Main.UpdateProgress(incrementProgressAmount);
						}

						if (incrementProgressAmount > 0) AppController.Main.UpdateProgressLog("Saving archive...");

						zip.SaveTo(archiveFilePath, CompressionType.Deflate);

						return true;
					}
				}
				catch(Exception ex)
				{
					Log.Here().Error($"Error writing archive {rootPath} to {archiveFilePath}: {ex.ToString()}");
				}
			}
			return false;
		}

		public static async Task<bool> CreateArchiveFromRepo(string repoPath, string rootPath, List<JunctionData> sourceFolders, string archiveFilePath, int incrementProgressAmount = -1)
		{
			if (sourceFolders != null && sourceFolders.Count > 0)
			{
				try
				{
					var foldersToParse = sourceFolders.Where(folderData => Directory.Exists(folderData.SourcePath)).ToList();
					ConcurrentBag<FileInfo> targetFiles = new ConcurrentBag<FileInfo>();

					var tasks = foldersToParse.Select(f => Task.Run(() =>
					{
						var files = Directory.EnumerateFiles(f.SourcePath, "*", SearchOption.AllDirectories);
						if (files != null)
						{
							foreach (var filePath in files)
							{
								if (File.Exists(filePath))
								{
									var fileInfo = new FileInfo(filePath);
									if (fileInfo != null) targetFiles.Add(fileInfo);
								}
							}
						}

					}));

					tasks.Append(Task.Run(() =>
					{
						var files = Directory.EnumerateFiles(repoPath, "*", SearchOption.TopDirectoryOnly);
						if (files != null)
						{
							if (incrementProgressAmount > 0) AppController.Main.UpdateProgressLog($"Searching repository for files to save.");
							foreach (var filePath in files)
							{
								//Log.Here().Important($"Checking file: {filePath}");
								if (File.Exists(filePath))
								{
									var fileInfo = new FileInfo(filePath);
									if (fileInfo != null) targetFiles.Add(fileInfo);
								}
							}
						}
					}));

					if (incrementProgressAmount > 0) AppController.Main.UpdateProgressLog($"Searching source folders for files to save.");

					await Task.WhenAll(tasks).ConfigureAwait(false);

					if (incrementProgressAmount > 0)
					{
						incrementProgressAmount = incrementProgressAmount / targetFiles.Count;
					}

					using (var zip = ZipArchive.Create())
					{
						if (incrementProgressAmount > 0) AppController.Main.UpdateProgressLog("Adding files to archive...");

						foreach(var f in targetFiles)
						{
							zip.AddEntry(f.FullName.Replace(rootPath, "").Replace(repoPath, ""), f);
							if (incrementProgressAmount > 0) AppController.Main.UpdateProgress(incrementProgressAmount);
						}

						if (incrementProgressAmount > 0) AppController.Main.UpdateProgressLog("Saving archive...");

						zip.SaveTo(archiveFilePath, CompressionType.Deflate);

						return true;
					}
				}
				catch (Exception ex)
				{
					Log.Here().Error($"Error writing archive {repoPath} to {archiveFilePath}: {ex.ToString()}");
				}
			}
			return false;
		}

		public static async Task<bool> CreateArchiveFromDirectory(string directoryPath, string archiveFilePath, int incrementProgressAmount = -1)
		{
			try
			{
				if (incrementProgressAmount > 0) AppController.Main.UpdateProgressLog($"Searching source folders for files to save...");

				using (var zip = ZipArchive.Create())
				{
					if (incrementProgressAmount > 0) AppController.Main.UpdateProgressLog("Adding files to archive...");

					var files = Directory.EnumerateFiles(directoryPath, "*", SearchOption.AllDirectories);
					if (files != null)
					{
						if (incrementProgressAmount > 0)
						{
							incrementProgressAmount = incrementProgressAmount / files.Count();
						}

						foreach (var filePath in files)
						{
							if (File.Exists(filePath))
							{
								var fileInfo = new FileInfo(filePath);
								if (fileInfo != null)
								{
									zip.AddEntry(fileInfo.FullName.Replace(directoryPath, ""), fileInfo);
								}
								if (incrementProgressAmount > 0) AppController.Main.UpdateProgress(incrementProgressAmount);
							}
						}
					}

					if (incrementProgressAmount > 0) AppController.Main.UpdateProgressLog("Saving archive...");

					zip.SaveTo(archiveFilePath, CompressionType.Deflate);

					return true;
				}
			}
			catch (Exception ex)
			{
				Log.Here().Error($"Error writing archive {directoryPath} to {archiveFilePath}: {ex.ToString()}");
			}
			return false;
		}
	}
}
