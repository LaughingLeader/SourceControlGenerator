using SCG.Data;
using SCG.Modules.DOS2DE.Windows;
using SCG.Modules.DOS2DE.Data.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using LSLib.LS;
using SCG.Modules.DOS2DE.Utilities;

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
				if (KeyIsEditable && keyAttribute != null && keyAttribute.Value != null)
				{
					bool changed = false;
					if(keyAttribute.Value is TranslatedString translatedString)
					{
						changed = key != translatedString.Value;
						key = translatedString.Value;
					}
					else if (keyAttribute.Value is string str)
					{
						changed = key != str;
						key = str;
					}
					if (changed)
					{
						this.RaisePropertyChanged("Key");
						this.RaisePropertyChanged("EntryKey");
					}
				}
			}
		}

		public LSLib.LS.NodeAttribute TranslatedStringAttribute { get; set; }

		public LSLib.LS.TranslatedString TranslatedString { get; set; }

		public override void OnSelected(bool isSelected)
		{
			Parent.OnSelectedKeyChanged(this, isSelected);
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
			get { 
				if (TranslatedString != null)
				{
					return TranslatedString.Value;
				}
				else if(TranslatedStringAttribute.Value != null)
				{
					return (string)TranslatedStringAttribute.Value;
				}
				else
				{
					return "";
				}
			}
			set
			{
				if (TranslatedString != null)
				{
					this.RaiseAndSetIfChanged(ref TranslatedString.Value, value);
				}
				else if (TranslatedStringAttribute.Value != null)
				{
					TranslatedStringAttribute.Value = value;
					this.RaisePropertyChanged("Content");
				}
				this.RaisePropertyChanged("EntryContent");
			}
		}

		private string handle = "";

		public string Handle
		{

			get
			{
				if (TranslatedString != null)
				{
					return TranslatedString.Handle;
				}
				else if (!String.IsNullOrEmpty(handle))
				{
					return handle;
				}
				else
				{
					return LocaleEditorCommands.UnsetHandle;
				}
			}
			set
			{
				if (TranslatedString != null)
				{
					this.RaiseAndSetIfChanged(ref TranslatedString.Handle, value);
				}
				else
				{
					this.RaiseAndSetIfChanged(ref handle, value);
				}
				this.RaisePropertyChanged("EntryHandle");
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
				if (this.KeyIsEditable)
				{
					var last = key;
					if (this.UpdateWithHistory(ref key, value, "Key"))
					{
						this.RaisePropertyChanged("EntryKey");
						Parent.AddUnsavedChange(this, LocaleUnsavedChangesData.Create(this, LocaleChangedField.Key, last, value));
					}

					if (KeyAttribute != null)
					{
						KeyAttribute.Value = value;
					}
				}
			}
		}

		public string EntryContent
		{
			get { return Content; }
			set
			{
				if (this.ContentIsEditable && TranslatedString != null)
				{
					var last = TranslatedString.Value;
					if(this.UpdateWithHistory(ref TranslatedString.Value, value, "Content"))
					{
						//Log.Here().Activity($"Saving history for EntryContent| {last} => {value}");
						this.RaisePropertyChanged("EntryContent");
						Parent.AddUnsavedChange(this, LocaleUnsavedChangesData.Create(this, LocaleChangedField.Content, last, value));
					}
				}
			}
		}

		public string EntryHandle
		{
			get { return Handle; }
			set
			{
				if (this.HandleIsEditable && TranslatedString != null)
				{
					var last = TranslatedString.Handle;
					if (this.UpdateWithHistory(ref TranslatedString.Handle, value, "Handle"))
					{
						this.RaisePropertyChanged("EntryHandle");
						Parent.AddUnsavedChange(this, LocaleUnsavedChangesData.Create(this, LocaleChangedField.Handle, last, value));
					}
				}
			}
		}

		#endregion

		public LocaleNodeKeyEntry(LSLib.LS.Node resNode) : base()
		{
			Node = resNode;
		}
		public LocaleNodeKeyEntry(LSLib.LS.Node resNode, ILocaleFileData parent) : base(parent)
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
