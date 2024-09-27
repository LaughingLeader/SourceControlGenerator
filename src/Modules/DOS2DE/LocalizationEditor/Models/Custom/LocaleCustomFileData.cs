using Newtonsoft.Json;

using ReactiveUI.Fody.Helpers;

using SCG.Modules.DOS2DE.Data.View;
using SCG.Modules.DOS2DE.Data.View.Locale;
using SCG.Modules.DOS2DE.LocalizationEditor.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCG.Modules.DOS2DE.LocalizationEditor.Models
{
	[JsonObject(MemberSerialization.OptIn)]
	public class LocaleCustomFileData : BaseLocaleFileData, ILocaleFileData
	{
		public ModProjectData Project { get; set; }

		[Reactive] public EnumLocaleLanguages Language { get; set; } = EnumLocaleLanguages.All;

		public bool IsCustom => true;

		public void LoadFromTextualStream(System.IO.StreamReader stream, char delimiter = '\t')
		{
			int lineNum = 0;
			string line;
			Dictionary<string, int> fileParameters = null;
			while ((line = stream.ReadLine()) != null)
			{
				lineNum += 1;
				// Skip top line, as it typically describes the columns
				if (lineNum == 1)
				{
					fileParameters = LocaleEditorCommands.GetSheetParamOrder(line, delimiter);
				}
				else if (!string.IsNullOrWhiteSpace(line) && fileParameters.Count > 0)
				{
					var lineEntries = line.Split(delimiter);

					string key = LocaleEditorCommands.GetSheetValue("key", fileParameters, lineEntries);
					string content = LocaleEditorCommands.GetSheetValue("content", fileParameters, lineEntries);
					string handle = LocaleEditorCommands.GetSheetValue("handle", fileParameters, lineEntries);

					var entry = new LocaleCustomKeyEntry(this)
					{
						Key = key,
						Content = content
					};
					if (!string.IsNullOrEmpty(handle)) entry.Handle = handle;
					entry.ChangesUnsaved = false;
					Entries.Add(entry);
				}
			}
		}

		public LocaleCustomFileData(LocaleTabGroup parent, string name = "") : base(parent, name)
		{
			CanRename = CanClose = true;
			CanCreateFileLink = false;
		}
	}
}
