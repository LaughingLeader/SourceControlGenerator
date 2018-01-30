using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LL.DOS2.SourceControl.Data
{
	public class KeywordData : PropertyChangedBase
	{
		private string keywordName;

		public string KeywordName
		{
			get { return keywordName; }
			set
			{
				keywordName = value;
				RaisePropertyChanged("KeywordName");
			}
		}

		private string keywordValue;

		public string KeywordValue
		{
			get { return keywordValue; }
			set
			{
				keywordValue = value;
				RaisePropertyChanged("KeywordValue");
			}
		}

		public KeywordData()
		{
			keywordName = "";
			KeywordValue = "";
		}
	}
}
