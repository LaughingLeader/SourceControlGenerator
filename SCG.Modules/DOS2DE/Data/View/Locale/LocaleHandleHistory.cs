using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCG.Modules.DOS2DE.Data.View.Locale
{
	public struct LocaleHandleHistory
	{
		public LocaleNodeKeyEntry Key { get; set; }
		public string Handle { get; set; }

		public LocaleHandleHistory(ILocaleKeyEntry key, string handle)
		{
			Key = key;
			Handle = handle;
		}
	}
}
