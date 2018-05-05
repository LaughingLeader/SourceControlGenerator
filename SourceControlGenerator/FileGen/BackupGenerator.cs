using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LL.SCG.Data;
using SharpCompress.Archives.Zip;
using SharpCompress.Archives;
using SharpCompress.Writers;
using SharpCompress.Writers.Zip;
using SharpCompress.Common;
using LL.SCG.Data.View;
using LL.SCG.Interfaces;
using LL.SCG.Data.App;

namespace LL.SCG.FileGen
{
	public static class BackupGenerator
	{
		public static bool CreateArchiveFromRoot(string rootPath, List<JunctionData> sourceFolders, string archiveFilePath)
		{
			if (sourceFolders != null && sourceFolders.Count > 0)
			{
				try
				{
					using (var archive = ZipArchive.Create())
					{
						foreach (var folderData in sourceFolders)
						{
							if (Directory.Exists(folderData.SourcePath))
							{
								foreach (var file in Directory.EnumerateFiles(folderData.SourcePath, "*", SearchOption.AllDirectories))
								{
									if(File.Exists(file))
									{
										FileInfo fileInfo = new FileInfo(file);
										string fileKey = file.Replace(rootPath, "");
										archive.AddEntry(fileKey, fileInfo);
									}
								}
							}
						}
						archive.SaveTo(archiveFilePath, CompressionType.Deflate);
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

		public static bool CreateArchiveFromRepo(string repoPath, string archiveFilePath)
		{
			try
			{
				using (var archive = ZipArchive.Create())
				{
					archive.AddAllFromDirectory(repoPath, "*", SearchOption.AllDirectories);
					archive.SaveTo(archiveFilePath, CompressionType.Deflate);
					return true;
				}
			}
			catch(Exception ex)
			{
				Log.Here().Error($"Error writing archive {repoPath} to {archiveFilePath}: {ex.ToString()}");
			}
			return false;
		}
	}
}
