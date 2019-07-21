using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCG.Modules.DOS2DE.Data.View.Locale
{
	public class CustomLocaleTabGroup : LocaleTabGroup
	{
		public CustomLocaleTabGroup(string name) : base(name)
		{
			CanAddFiles = true;
		}
	}
}
