using SCG.Modules.DOS2DE.Data.View.Locale;
using SCG.Modules.DOS2DE.LocalizationEditor.ViewModels;

namespace SCG.Modules.DOS2DE.LocalizationEditor.Models;

public class CustomLocaleTabGroup : LocaleTabGroup
{
	public CustomLocaleTabGroup(LocaleViewModel parent, string name) : base(parent, name)
	{
		CanAddFiles = true;
	}
}
