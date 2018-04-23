using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LL.SCG.Data
{
	public class UserKeywordData : PropertyChangedBase
	{
		private string dateCustom;

		public string DateCustom
		{
			get { return dateCustom; }
			set
			{
				dateCustom = value;
				RaisePropertyChanged("DateCustom");
			}
		}

		private ObservableCollection<KeywordData> keywords;

		public ObservableCollection<KeywordData> Keywords
		{
			get { return keywords; }
			set
			{
				keywords = value;
				RaisePropertyChanged("Keywords");
			}
		}

		public void AddKeyword()
		{
			Keywords.Add(new KeywordData());
			RaisePropertyChanged("Keywords");
		}

		public void RemoveLast()
		{
			if(Keywords.Count > 0)
			{
				Keywords.RemoveAt(keywords.Count - 1);
				RaisePropertyChanged("Keywords");
			}
		}

		public void ResetToDefault()
		{
			if(Keywords == null) Keywords = new ObservableCollection<KeywordData>();
			if (Keywords.Count > 0) Keywords.Clear();
			Keywords.Add(new KeywordData());
			Keywords.Add(new KeywordData());
			Keywords.Add(new KeywordData());

			DateCustom = "MMMM dd, yyyy";
		}

		public void RemoveEmpty()
		{
			if(Keywords != null && Keywords.Count > 0)
			{
				Keywords.RemoveAll(k => k.KeywordValue == "");
			}
		}
	}
}
