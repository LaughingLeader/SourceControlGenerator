using SCG.Modules.DOS2DE.Data.View.Locale;

namespace SCG.Modules.DOS2DE.LocalizationEditor.Models.Design;

public class LocaleTestFileData : BaseLocaleFileData, ILocaleFileData
{
	public LocaleTestFileData(LocaleTabGroup parent, string name = "") : base(parent, name)
	{

	}

	public bool IsCustom => false;
}
