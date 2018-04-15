﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LL.SCG.FileGen
{
	public static class JsonInterface
	{
		public static T DeserializeObject<T>(string path)
		{
			return JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
		}

		public static string SerializeObject(object o)
		{
			return JsonConvert.SerializeObject(o, Newtonsoft.Json.Formatting.Indented);
		}
	}
}