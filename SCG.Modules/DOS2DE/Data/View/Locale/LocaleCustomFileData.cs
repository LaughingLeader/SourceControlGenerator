using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCG.Modules.DOS2DE.Data.View.Locale
{
	[JsonObject(MemberSerialization.OptIn)]
	public class LocaleCustomFileData : BaseLocaleFileData
	{
		public ModProjectData Project { get; set; }

		public LocaleCustomFileData(string name = "") : base(name)
		{
			CanRename = CanClose = true;
		}
	}
}
