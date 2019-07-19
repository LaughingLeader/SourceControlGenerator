using DynamicData.Binding;
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
		ObservableCollectionExtended<LocaleKeyEntry> Entries { get; set; }

		string SourcePath { get; set; }
		string Name { get; set; }
		bool Active { get; set; }
		bool AllSelected { get; set; }
		bool Locked { get; set; }
		bool ChangesUnsaved { get; set; }

		void SelectAll();
		void SelectNone();
	}
}
