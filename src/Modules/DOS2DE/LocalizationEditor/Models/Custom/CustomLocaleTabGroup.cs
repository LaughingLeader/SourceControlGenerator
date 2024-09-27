using SCG.Modules.DOS2DE.Data.View.Locale;
using SCG.Modules.DOS2DE.LocalizationEditor.ViewModels;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCG.Modules.DOS2DE.LocalizationEditor.Models
{
	public class CustomLocaleTabGroup : LocaleTabGroup
	{
		public CustomLocaleTabGroup(LocaleViewModel parent, string name) : base(parent, name)
		{
			CanAddFiles = true;
		}
	}
}
