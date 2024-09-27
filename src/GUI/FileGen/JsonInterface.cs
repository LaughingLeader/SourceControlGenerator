using System;
using System.Collections.Generic;
using Alphaleonis.Win32.Filesystem;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SCG.FileGen
{
	public static class JsonInterface
	{
		public static T DeserializeObject<T>(string path)
		{
			try
			{
				return JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
			}
			catch(Exception ex)
			{
				Log.Here().Error($"Error deserializing json ({path}):\n{ex.ToString()}");
			}
			return default(T);
		}

		public static async Task<T> DeserializeObjectAsync<T>(string path)
		{
			try
			{
				string contents = await FileCommands.ReadFileAsync(path);
				return JsonConvert.DeserializeObject<T>(contents);
			}
			catch (Exception ex)
			{
				Log.Here().Error($"Error deserializing json ({path}):\n{ex.ToString()}");
			}
			return default(T);
		}

		public static string SerializeObject(object o, bool indented = true)
		{
			try
			{
				return JsonConvert.SerializeObject(o, indented ? Formatting.Indented : Formatting.None);
			}
			catch (Exception ex)
			{
				Log.Here().Error($"Error serializing json:\n{ex.ToString()}");
			}
			return "";
		}
	}
}
