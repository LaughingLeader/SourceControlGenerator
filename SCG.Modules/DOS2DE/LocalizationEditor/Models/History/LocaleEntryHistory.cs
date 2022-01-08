using SCG.Modules.DOS2DE.Data.View.Locale;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCG.Modules.DOS2DE.LocalizationEditor.Models
{
	public struct LocaleEntryHistory
	{
		public ILocaleFileData ParentFile;
		public ILocaleKeyEntry Entry;
		public int Index;
		public bool ChangesUnsaved;
		public bool ParentChangesUnsaved;

		public string LastKey;
		public string LastContent;
		public string LastHandle;
	}
}
