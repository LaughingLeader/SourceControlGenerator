using SCG.Data;
using SCG.Modules.DOS2DE.Windows;
using SCG.Modules.DOS2DE.Data.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;

namespace SCG.Modules.DOS2DE.Data.View.Locale
{
	public class LocaleCustomKeyEntry : BaseLocaleKeyEntry, ILocaleKeyEntry
	{
		private string key;

		public string Key
		{
			get => key;
			set { this.RaiseAndSetIfChanged(ref key, value); }
		}

		private string content;

		public string Content
		{
			get => content;
			set { this.RaiseAndSetIfChanged(ref content, value); }
		}

		private string handle;

		public string Handle
		{
			get => handle;
			set { this.RaiseAndSetIfChanged(ref handle, value); }
		}

		private string entryKey;

		public string EntryKey
		{
			get => entryKey;
			set { this.UpdateWithHistory(ref entryKey, value); }
		}

		private string entryContent;

		public string EntryContent
		{
			get => entryContent;
			set { this.UpdateWithHistory(ref entryContent, value); }
		}

		private string entryHandle;

		public string EntryHandle
		{
			get => entryHandle;
			set { this.UpdateWithHistory(ref entryHandle, value); }
		}

		public override void OnSelected(bool isSelected)
		{
			LocaleEditorWindow.instance?.KeyEntrySelected(this, isSelected);
		}

		private readonly ModProjectData project;
		public ModProjectData Project => project;

		public LocaleCustomKeyEntry(ModProjectData parentProject)
		{
			project = parentProject;
		}
	}
}
