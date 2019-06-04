using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCG.Modules.DOS2DE.Data.View
{
	public interface IKeyFileData
	{
		ObservableRangeCollection<LocaleKeyEntry> Entries { get; set; }

		string Name { get; set; }
		bool Active { get; set; }
		bool AllSelected { get; set; }
		bool Locked { get; set; }

		void SelectAll();
		void SelectNone();
	}
}
