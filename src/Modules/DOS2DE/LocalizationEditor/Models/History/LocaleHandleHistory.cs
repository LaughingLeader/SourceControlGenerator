using SCG.Modules.DOS2DE.Data.View.Locale;

namespace SCG.Modules.DOS2DE.LocalizationEditor.Models;

public struct LocaleHandleHistory
{
	public ILocaleKeyEntry Key { get; set; }
	public string Handle { get; set; }

	public LocaleHandleHistory(ILocaleKeyEntry key, string handle)
	{
		Key = key;
		Handle = handle;
	}
}
