﻿using SCG.Data;
using SCG.Modules.DOS2DE.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SCG.Modules.DOS2DE.Data.View.Locale;

namespace SCG.Modules.DOS2DE.LocalizationEditor.Models
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
				if (UpdateWithHistory(ref key, value))
				{
					ChangesUnsaved = true;
					this.RaisePropertyChanged("Key");
				}
			}
		}
		public string EntryContent
		{
			get => content;
			set
			{
				if (UpdateWithHistory(ref content, value))
				{
					ChangesUnsaved = true;
					this.RaisePropertyChanged("Content");
				}
			}
		}
		public string EntryHandle
		{
			get => handle;
			set
			{
				if (UpdateWithHistory(ref handle, value))
				{
					ChangesUnsaved = true;
					this.RaisePropertyChanged("Handle");
				}
			}
		}

		public override void OnSelected(bool isSelected)
		{
			Parent?.OnSelectedKeyChanged(this, isSelected);
		}

		public LocaleCustomKeyEntry(ILocaleFileData parent) : base(parent)
		{

		}
	}
}
