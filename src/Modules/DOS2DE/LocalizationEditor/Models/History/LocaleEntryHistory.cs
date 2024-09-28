using SCG.Modules.DOS2DE.Data.View.Locale;

namespace SCG.Modules.DOS2DE.LocalizationEditor.Models;

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
