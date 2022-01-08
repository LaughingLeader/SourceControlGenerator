using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCG.Modules.DOS2DE.Data.View.Locale
{
	public enum LocaleChangedField
	{
		None,
		Key,
		Content,
		Handle
	}
	public class LocaleUnsavedChangesData
	{
		public LocaleChangedField ChangeType { get; set; }
		public string LastValue { get; set; }
		public string NewValue { get; set; }

		public ILocaleKeyEntry KeyEntry { get; set; }

		/*
		public bool Equals(LocaleUnsavedChangesData other)
		{
			return other.KeyEntry == this.KeyEntry && other.ChangeType == this.ChangeType && other.NewValue == this.LastValue;
		}
		*/

		public static LocaleUnsavedChangesData Create(ILocaleKeyEntry entry, LocaleChangedField changedField, string lastValue, string newValue)
		{
			return new LocaleUnsavedChangesData
			{
				KeyEntry = entry,
				ChangeType = changedField,
				LastValue = lastValue,
				NewValue = newValue
			};
		}
	}
}
