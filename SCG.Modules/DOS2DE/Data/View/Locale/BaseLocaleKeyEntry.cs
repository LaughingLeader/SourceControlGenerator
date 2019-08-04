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
	public interface ILocaleKeyEntry
	{
		ILocaleFileData Parent { get; }
		bool KeyIsEditable { get; set; }
		bool Selected { get; set; }

		//Code accessible properties for changes without history
		string Key { get; set; }
		string Content { get; set; }
		string Handle { get; set; }

		//UI accessible properties for history tracking
		string EntryKey { get; set; }
		string EntryContent { get; set; }
		string EntryHandle { get; set; }

		void SetHistoryFromObject(IPropertyChangedHistoryBase obj);
	}

	public class BaseLocaleKeyEntry : PropertyChangedHistoryBase
	{
		public ILocaleFileData Parent { get; private set; }

		private bool keyIsEditable = true;

		public bool KeyIsEditable
		{
			get { return keyIsEditable; }
			set
			{
				this.RaiseAndSetIfChanged(ref keyIsEditable, value);
			}
		}

		private bool selected = false;

		public bool Selected
		{
			get { return selected; }
			set
			{
				this.RaiseAndSetIfChanged(ref selected, value);
				OnSelected(selected);
			}
		}

		public virtual void OnSelected(bool isSelected)
		{
			//Parent.OnSelectedKeyChanged(this, isSelected);
		}

		public BaseLocaleKeyEntry(ILocaleFileData parent)
		{
			Parent = parent;
		}
	}
}
