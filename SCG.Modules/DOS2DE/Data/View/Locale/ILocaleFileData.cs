using DynamicData.Binding;
using SCG.Modules.DOS2DE.Data.Savable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCG.Modules.DOS2DE.Data.View.Locale
{
	public interface ILocaleFileData
	{
		LocaleTabGroup Parent { get; }
		ObservableCollectionExtended<ILocaleKeyEntry> Entries { get; set; }
		ReadOnlyObservableCollection<ILocaleKeyEntry> VisibleEntries { get; }

		LocaleFileLinkData FileLinkData { get; set; }
		string SourcePath { get; set; }
		string Name { get; set; }
		string RenameText { get; set; }
		bool Active { get; set; }
		bool AllSelected { get; set; }
		bool Locked { get; set; }
		bool ChangesUnsaved { get; set; }
		bool CanClose { get; set; }
		bool CanRename { get; set; }
		bool IsRenaming { get; set; }
		bool HasFileLink { get; set; }
		bool CanCreateFileLink { get; set; }

		void SelectAll();
		void SelectNone();
		void OnSelectedKeyChanged(ILocaleKeyEntry key, bool selected);

		void AddUnsavedChange(ILocaleKeyEntry key, LocaleUnsavedChangesData unsavedChange);
	}
}
