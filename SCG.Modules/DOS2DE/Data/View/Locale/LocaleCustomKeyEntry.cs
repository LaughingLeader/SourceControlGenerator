﻿using SCG.Data;
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
			set
			{
				this.RaiseAndSetIfChanged(ref key, value);
				this.RaisePropertyChanged("EntryKey");
			}
		}

		private string content;

		public string Content
		{
			get => content;
			set
			{
				this.RaiseAndSetIfChanged(ref content, value);
				this.RaisePropertyChanged("EntryContent");
			}
		}

		private string handle;

		public string Handle
		{
			get => handle;
			set
			{
				this.RaiseAndSetIfChanged(ref handle, value);
				this.RaisePropertyChanged("EntryHandle");
			}
		}

		public string EntryKey
		{
			get => key;
			set
			{
				if(this.UpdateWithHistory(ref key, value))
				{
					this.RaisePropertyChanged("Key");
				}
			}
		}

		public string EntryContent
		{
			get => content;
			set
			{
				if(this.UpdateWithHistory(ref content, value))
				{
					this.RaisePropertyChanged("Content");
				}
			}
		}

		private string entryHandle;

		public string EntryHandle
		{
			get => entryHandle;
			set
			{
				if(this.UpdateWithHistory(ref handle, value))
				{
					this.RaisePropertyChanged("Handle");
				}
			}
		}

		public override void OnSelected(bool isSelected)
		{
			LocaleEditorWindow.instance?.KeyEntrySelected(this, isSelected);
		}
	}
}
