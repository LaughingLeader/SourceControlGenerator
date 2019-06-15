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
	public class LocaleKeyEntry : PropertyChangedBase, ICloneable
	{
		public LSLib.LS.Node Node { get; set; }

		public LSLib.LS.NodeAttribute KeyAttribute { get; set; }

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
			get { return KeyAttribute != null ? KeyAttribute.Value.ToString() : key; }
			set
			{
				if (KeyAttribute != null)
				{
					KeyAttribute.Value = value;
					Notify("Key");
				}
				else
				{
					Update(ref key, value);
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
					Update(ref TranslatedString.Value, value);
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
					Update(ref TranslatedString.Handle, value);
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

		public LocaleKeyEntry(LSLib.LS.Node resNode)
		{
			Node = resNode;
		}

		public object Clone()
		{
			return new LocaleKeyEntry(this.Node)
			{
				Key = this.Key,
				Handle = this.Handle,
				Content = this.Content,
				Selected = this.Selected
			};
		}
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
