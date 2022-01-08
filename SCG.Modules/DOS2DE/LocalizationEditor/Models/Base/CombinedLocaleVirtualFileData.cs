using SCG.Modules.DOS2DE.Data.View.Locale;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCG.Modules.DOS2DE.LocalizationEditor.Models
{
	public class CombinedLocaleVirtualFileData : BaseLocaleFileData, ILocaleFileData
	{
		public CombinedLocaleVirtualFileData(LocaleTabGroup parent, string name = "") : base(parent, name)
		{
		}

		public bool IsCustom => false;
	}
}
