using SCG.Data;
using SCG.Modules.DOS2DE.Windows;
using SCG.Modules.DOS2DE.Data.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCG.Modules.DOS2DE.Data.View.Locale
{
	public class LocaleKeyEntry : PropertyChangedHistoryBase
	{
		public LSLib.LS.Node Node { get; set; }

		private LSLib.LS.NodeAttribute keyAttribute;

		public LSLib.LS.NodeAttribute KeyAttribute
		{
			get { return keyAttribute; }
			set
			{
				keyAttribute = KeyAttribute;
				if (keyAttribute != null)
				{
					Key = (string)keyAttribute.Value;
				}
			}
		}

		public LSLib.LS.NodeAttribute TranslatedStringAttribute { get; set; }

		public LSLib.LS.TranslatedString TranslatedString { get; set; }

		private bool keyIsEditable = false;

		public bool KeyIsEditable
		{
			get { return keyIsEditable; }
			set
			{
				Update(ref keyIsEditable, value);
			}
		}

		private string key = "None";

		public string Key
		{
			get { return key; }
			set
			{
				if(Update(ref key, value))
				{
					Notify("EntryKey");
				}

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
					if(Update(ref TranslatedString.Value, value))
					{
						Notify("EntryContent");
					}
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
					if(Update(ref TranslatedString.Handle, value))
					{
						Notify("EntryHandle");
					}
				}
			}
		}

		private bool selected = false;

		public bool Selected
		{
			get { return selected; }
			set
			{
				Update(ref selected, value);
				LocaleEditorWindow.instance?.KeyEntrySelected(this, selected);
			}
		}

		#region UI Properties
		/* 
		Properties used by the UI interface so we can track user changes with history. 
		*/

		public string EntryKey
		{
			get { return Key; }
			set
			{
				if(UpdateWithHistory(ref key, value, "Key"))
				{
					Notify("EntryKey");
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
					if(UpdateWithHistory(ref TranslatedString.Value, value, "Content"))
					{
						Notify("EntryContent");
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
					if(UpdateWithHistory(ref TranslatedString.Handle, value, "Handle"))
					{
						Notify("EntryHandle");
					}
				}
			}
		}

		#endregion

		public LocaleKeyEntry(LSLib.LS.Node resNode)
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

	public struct HandleHistory
	{
		public LocaleKeyEntry Key { get; set; }
		public string Handle { get; set; }

		public HandleHistory(LocaleKeyEntry key, string handle)
		{
			Key = key;
			Handle = handle;
		}
	}
}
