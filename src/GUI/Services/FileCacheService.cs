using LazyCache;

using System.Text;
using System.Text.Json;

namespace SCG.Services;
public class FileCacheService(IAppCache cache)
{
	private readonly IAppCache _cache = cache;

	private static async Task<byte[]?> GetBytesAsyncImpl(string path, CancellationToken token) => await File.ReadAllBytesAsync(path, token);

	public async Task<byte[]?> GetBytesAsync(string path, CancellationToken token)
	{
		return await _cache.GetOrAddAsync(path, () => GetBytesAsyncImpl(path, token));
	}

	private static async Task<string?> GetTextAsyncImpl(string path, CancellationToken token) => await File.ReadAllTextAsync(path, token);

	public async Task<string?> GetTextAsync(string path, CancellationToken token)
	{
		return await _cache.GetOrAddAsync(path, () => GetTextAsyncImpl(path, token));
	}

	private static async Task<T?> GetJsonDataAsyncImpl<T>(string path, JsonSerializerOptions opts, CancellationToken token)
	{
		var contents = await GetTextAsyncImpl(path, token);
		if (!string.IsNullOrEmpty(contents))
		{
			using var ms = new MemoryStream(Encoding.UTF8.GetBytes(contents));
			var data = await JsonSerializer.DeserializeAsync<T>(ms, opts, token);
			return data;
		}
		return default;
	}

	private static readonly JsonSerializerOptions _defaultSerializerOpts = new()
	{
		AllowTrailingCommas = true,
		IgnoreReadOnlyProperties = true,
		WriteIndented = true,
		PropertyNameCaseInsensitive = true,
	};

	public async Task<T?> GetJsonDataAsync<T>(string path, JsonSerializerOptions opts, CancellationToken token)
	{
		return await _cache.GetOrAddAsync(path, () => GetJsonDataAsyncImpl<T>(path, opts, token));
	}

	public async Task<T?> GetJsonDataAsync<T>(string path, CancellationToken token)
	{
		return await GetJsonDataAsync<T>(path, _defaultSerializerOpts, token);
	}

	public void ClearCached(string path) => _cache.Remove(path);

	//TODO clear cached image on filesystem change
}
