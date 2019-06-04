using SCG.Data;
using SCG.Modules.DOS2DE.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCG.Modules.DOS2DE.Data.View
{
	public class LocaleKeyEntry : PropertyChangedBase
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
				keyIsEditable = value;
				RaisePropertyChanged("KeyIsEditable");
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
				}
				else
				{
					key = value;
				}

				RaisePropertyChanged("Key");
			}
		}

		public string Content
		{
			get { return TranslatedString != null ? TranslatedString.Value : "Content"; }
			set
			{
				if (TranslatedString != null)
				{
					TranslatedString.Value = value;
					RaisePropertyChanged("Content");
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
					TranslatedString.Handle = value;
					RaisePropertyChanged("Handle");
				}
			}
		}

		private bool selected = false;

		public bool Selected
		{
			get { return selected; }
			set
			{
				selected = value;
				RaisePropertyChanged("Selected");
				LocaleEditorWindow.instance?.KeyEntrySelected(this, selected);
			}
		}

		public LocaleKeyEntry(LSLib.LS.Node resNode)
		{
			Node = resNode;

			if (resNode != null)
			{

			}
		}
	}
}
