using Newtonsoft.Json;

using ReactiveUI.Fody.Helpers;

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

		[Reactive] public EnumLocaleLanguages Language { get; set; }

		public LocaleCustomFileData(LocaleTabGroup parent, string name = "") : base(parent, name)
		{
			CanRename = CanClose = true;
			CanCreateFileLink = false;
		}
	}
}
