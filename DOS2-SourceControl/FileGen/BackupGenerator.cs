using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LL.DOS2.SourceControl.Data;
using SharpCompress.Archives.Zip;
using SharpCompress.Archives;
using SharpCompress.Writers;
using SharpCompress.Writers.Zip;
using SharpCompress.Common;
using LL.DOS2.SourceControl.Data.View;

namespace LL.DOS2.SourceControl.FileGen
{
	public static class BackupGenerator
	{
		public static bool CreateArchiveFromData(string dataDirectory, List<string> directoryLayouts, ModProjectData project, string archiveFilePath)
		{
			if (directoryLayouts != null && directoryLayouts.Count > 0)
			{
				using (var archive = ZipArchive.Create())
				{
					foreach (var directoryBaseName in directoryLayouts)
					{
						var projectSubdirectoryName = directoryBaseName.Replace("ProjectName", project.Name).Replace("ProjectGUID", project.ModuleInfo.UUID);
						var projectDataFolder = Path.Combine(dataDirectory, projectSubdirectoryName);

						if (Directory.Exists(projectDataFolder))
						{
							foreach(var file in Directory.EnumerateFiles(projectDataFolder))
							{
								FileInfo fileInfo = new FileInfo(file);
								string fileKey = file.Replace(dataDirectory, "");
								archive.AddEntry(fileKey, fileInfo);
							}
						}
					}
					archive.SaveTo(archiveFilePath, CompressionType.Deflate);
					return true;
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
				Log.Here().Error("Error writing archive: {0}", ex.ToString());
			}
			return false;
		}
	}
}
