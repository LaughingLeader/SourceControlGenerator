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
	public class LocaleNodeKeyEntry : BaseLocaleKeyEntry, ILocaleKeyEntry
	{
		public LSLib.LS.Node Node { get; set; }

		private LSLib.LS.NodeAttribute keyAttribute;

		public LSLib.LS.NodeAttribute KeyAttribute
		{
			get { return keyAttribute; }
			set
			{
				keyAttribute = value;
				if (keyAttribute != null && keyAttribute.Value != null)
				{
					Key = (string)keyAttribute.Value;
				}
			}
		}

		public LSLib.LS.NodeAttribute TranslatedStringAttribute { get; set; }

		public LSLib.LS.TranslatedString TranslatedString { get; set; }

		public override void OnSelected(bool isSelected)
		{
			LocaleEditorWindow.instance?.KeyEntrySelected(this, isSelected);
		}

		private string key = "None";

		public string Key
		{
			get { return key; }
			set
			{
				this.RaiseAndSetIfChanged(ref key, value);
				this.RaisePropertyChanged("EntryKey");

				if (KeyAttribute != null)
				{
					KeyAttribute.Value = value;
				}
			}
		}

		public string Content
		{
			get { return TranslatedString != null ? TranslatedString.Value : "Content"; }
			set
			{
				if (TranslatedString != null)
				{
					Log.Here().Activity($"Content is changing| {TranslatedString.Value} => {value}");
					this.RaiseAndSetIfChanged(ref TranslatedString.Value, value);
					this.RaisePropertyChanged("EntryContent");
				}
			}
		}

		public string Handle
		{
			get { return TranslatedString != null ? TranslatedString.Handle : "ls::TranslatedStringRepository::s_HandleUnknown"; }
			set
			{
				if (TranslatedString != null)
				{
					this.RaiseAndSetIfChanged(ref TranslatedString.Handle, value);
					this.RaisePropertyChanged("EntryHandle");
				}
			}
		}

		#region UI Properties
		/* 
		Properties used by the UI interface so we can track user changes with history. 
		*/

		public string EntryKey
		{
			get { return key; }
			set
			{
				if (this.UpdateWithHistory(ref key, value, "Key"))
				{
					this.RaisePropertyChanged("EntryKey");
				}

				if (KeyAttribute != null)
				{
					KeyAttribute.Value = value;
				}
			}
		}

		public string EntryContent
		{
			get { return Content; }
			set
			{
				if (TranslatedString != null)
				{
					var last = TranslatedString.Value;
					if(this.UpdateWithHistory(ref TranslatedString.Value, value, "Content"))
					{
						Log.Here().Activity($"Saving history for EntryContent| {last} => {value}");
						this.RaisePropertyChanged("EntryContent");
					}
				}
			}
		}

		public string EntryHandle
		{
			get { return Handle; }
			set
			{
				if (TranslatedString != null)
				{
					if(this.UpdateWithHistory(ref TranslatedString.Handle, value, "Handle"))
					{
						this.RaisePropertyChanged("EntryHandle");
					}
				}
			}
		}

		#endregion

		public LocaleNodeKeyEntry(LSLib.LS.Node resNode)
		{
			Node = resNode;
		}

		//public object Clone()
		//{
		//	return new LocaleKeyEntry(this.Node)
		//	{
		//		Key = this.Key,
		//		Handle = this.Handle,
		//		Content = this.Content,
		//		Selected = this.Selected
		//	};
		//}
	}
}