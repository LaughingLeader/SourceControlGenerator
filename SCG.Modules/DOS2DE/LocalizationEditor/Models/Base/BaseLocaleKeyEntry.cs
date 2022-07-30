using SCG.Data;
using SCG.Modules.DOS2DE.Windows;
using SCG.Modules.DOS2DE.Data.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using System.ComponentModel;
using SCG.Interfaces;
using ReactiveUI.Fody.Helpers;
using SCG.Modules.DOS2DE.LocalizationEditor.Models;

namespace SCG.Modules.DOS2DE.Data.View.Locale
{
	public interface ILocaleKeyEntry : INotifyPropertyChanging, IIndexable, IReactiveObject
	{
		ILocaleFileData Parent { get; set; }
		bool KeyIsEditable { get; set; }
		bool ContentIsEditable { get; set; }
		bool HandleIsEditable { get; set; }
		bool Selected { get; set; }
		bool ChangesUnsaved { get; set; }

		//Code accessible properties for changes without history
		string Key { get; set; }
		string Content { get; set; }
		string Handle { get; set; }

		//UI accessible properties for history tracking
		string EntryKey { get; set; }
		string EntryContent { get; set; }
		string EntryHandle { get; set; }
		bool Visible { get; set; }

		void SetHistoryFromObject(IHistoryKeeper obj);
	}

	public static class LocaleKeyEntryExtensions
	{
		public static bool ValuesMatch(this ILocaleKeyEntry keyEntry1, ILocaleKeyEntry keyEntry2)
		{
			return keyEntry1.Key.Equals(keyEntry2.Key) && keyEntry1.Content.Equals(keyEntry2.Content) &&
				keyEntry1.Handle.Equals(keyEntry2.Handle);
		}
	}

	public class BaseLocaleKeyEntry : PropertyChangedHistoryBase, IIndexable
	{
		public ILocaleFileData Parent { get; set; }

		[Reactive] public bool KeyIsEditable { get; set; } = true;

		[Reactive] public bool HandleIsEditable { get; set; } = true;

		[Reactive] public bool ContentIsEditable { get; set; } = true;

		[Reactive] public bool Selected { get; set; } = false;

		[Reactive] public int Index { get; set; } = -1;

		[Reactive] public bool Visible { get; set; } = true;

		public virtual void OnSelected(bool isSelected)
		{
			//Parent.OnSelectedKeyChanged(this, isSelected);
		}

		private void Init()
		{
			this.WhenAnyValue(x => x.Selected).Subscribe(b =>
			{
				if (Visible)
				{
					OnSelected(b);
				}
				else if (Selected)
				{
					OnSelected(false);
				}
			});
		}

		public BaseLocaleKeyEntry()
		{
			Init();
		}

		public BaseLocaleKeyEntry(ILocaleFileData parent)
		{
			Parent = parent;
			Init();
		}
	}
}
