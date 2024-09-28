using SCG.BG3.Models;

using System.Collections.Concurrent;

namespace SCG.BG3.Services;
public class ProjectLoaderService(IEnvironmentService environmentService)
{
	private readonly IEnvironmentService _environment = environmentService;

	public async Task<IDictionary<string, ProjectData>> LoadProjectsAsync(string dataDirectoryPath, CancellationToken token)
	{
		ConcurrentDictionary<string, ProjectData> projects = [];

		try
		{
			var rootDirectory = Path.Join(dataDirectoryPath, "Projects");
			if (Directory.Exists(rootDirectory))
			{
				var opts = new ParallelOptions()
				{
					CancellationToken = token,
					MaxDegreeOfParallelism = _environment.ProcessorCount
				};

				var projectMetaFiles = Directory.EnumerateFiles(rootDirectory, "meta.lsx", DefaultResources.RecursiveOptions).ToList();

				await Parallel.ForEachAsync(projectMetaFiles, opts, async (metaPath, t) =>
				{
					var projectData = await ProjectData.FromMetaAsync(metaPath, t);
					if (projectData != null && projectData.GameProject != "Gustav" && !string.IsNullOrEmpty(projectData.UUID))
					{
						projects[projectData.UUID] = projectData;
					}
				});
			}
		}
		catch(Exception ex)
		{
			Log.Here().Error($"Error loading projects:\n{ex}");
		}

		return projects.ToDictionary();
	}

	public async Task<IDictionary<string, ModuleData>> LoadModulesAsync(string dataDirectoryPath, CancellationToken token)
	{
		ConcurrentDictionary<string, ModuleData> mods = [];

		try
		{
			var rootDirectory = Path.Join(dataDirectoryPath, "Mods");
			if (Directory.Exists(rootDirectory))
			{
				var opts = new ParallelOptions()
				{
					CancellationToken = token,
					MaxDegreeOfParallelism = _environment.ProcessorCount
				};

				var modFolders = Directory.EnumerateDirectories(rootDirectory, "*", DefaultResources.FlatSearchOptions).ToList();

				await Parallel.ForEachAsync(modFolders, opts, async (modFolderPath, t) =>
				{
					var metaFile = Path.Join(modFolderPath, "meta.lsx");
					if (File.Exists(metaFile))
					{
						var data = await ModuleData.FromMetaAsync(metaFile, t);
						if (data != null && !string.IsNullOrEmpty(data.UUID))
						{
							mods[data.UUID] = data;
						}
					}
				});
			}
		}
		catch(Exception ex)
		{
			Log.Here().Error($"Error loading mods:\n{ex}");
		}

		return mods.ToDictionary();
	}
}
