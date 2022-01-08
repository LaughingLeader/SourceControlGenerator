using DynamicData.Binding;

using ReactiveUI;

using SCG.Modules.DOS2DE.Data.View.Locale;
using SCG.Modules.DOS2DE.LocalizationEditor.Models;

using System.Collections.Generic;

namespace SCG.Modules.DOS2DE.LocalizationEditor.ViewModels
{
	public class LocaleEditorDebugViewModel : ReactiveObject
	{
		private string test = "Count";

		public string Test
		{
			get => test;
			set { this.RaiseAndSetIfChanged(ref test, value); }
		}


		private List<ILocaleKeyEntry> getTestRemovedEntries()
		{
			var list = new List<ILocaleKeyEntry>();
			var parent = new LocaleCustomFileData(null, "Test.lsb");
			for (var i = 0; i < 20; i++)
			{
				list.Add(new LocaleCustomKeyEntry(parent)
				{
					Key = "TestKey" + i,
					Content = "TestContentBlahblahblahblahblahblahblahblahblah",
					Handle = "NoHandle"
				});
			}
			return list;
		}
		public ObservableCollectionExtended<ILocaleKeyEntry> MissingEntries { get; set; }

		public LocaleEditorDebugViewModel()
		{
			MissingEntries = new ObservableCollectionExtended<ILocaleKeyEntry>(getTestRemovedEntries());

			Test = "Count:" + MissingEntries.Count;
		}
	}
}
