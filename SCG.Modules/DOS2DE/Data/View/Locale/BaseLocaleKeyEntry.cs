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

		void SetHistoryFromObject(IPropertyChangedHistoryBase obj);
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

		private bool keyIsEditable = true;

		public bool KeyIsEditable
		{
			get { return keyIsEditable; }
			set
			{
				this.RaiseAndSetIfChanged(ref keyIsEditable, value);
			}
		}

		private bool handleIsEditable = true;

		public bool HandleIsEditable
		{
			get => handleIsEditable;
			set { this.RaiseAndSetIfChanged(ref handleIsEditable, value); }
		}

		private bool contentIsEditable = true;

		public bool ContentIsEditable
		{
			get => contentIsEditable;
			set { this.RaiseAndSetIfChanged(ref contentIsEditable, value); }
		}


		private bool selected = false;

		public bool Selected
		{
			get { return selected; }
			set
			{
				if (Visible)
				{
					this.RaiseAndSetIfChanged(ref selected, value);
					OnSelected(selected);
				}
				else
				{
					if(Selected)
					{
						this.RaiseAndSetIfChanged(ref selected, false);
						OnSelected(false);
					}
				}
			}
		}

		private int index = -1;

		public int Index
		{
			get => index;
			set { this.RaiseAndSetIfChanged(ref index, value); }
		}

		private bool visibile = true;

		public bool Visible
		{
			get => visibile;
			set { this.RaiseAndSetIfChanged(ref visibile, value); }
		}

		public virtual void OnSelected(bool isSelected)
		{
			//Parent.OnSelectedKeyChanged(this, isSelected);
		}

		public BaseLocaleKeyEntry() { }

		public BaseLocaleKeyEntry(ILocaleFileData parent)
		{
			Parent = parent;
		}
	}
}
