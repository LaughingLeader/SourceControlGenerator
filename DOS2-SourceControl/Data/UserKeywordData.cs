using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LL.DOS2.SourceControl.Data
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
	}
}
