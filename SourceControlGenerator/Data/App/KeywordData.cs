using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SCG.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SCG.Data
{
	public class KeywordData : PropertyChangedBase
	{
		private string keywordName = "";

		public string KeywordName
		{
			get { return keywordName; }
			set
			{
				keywordName = value;
				RaisePropertyChanged("KeywordName");
			}
		}

		private string keywordValue = "";

		public string KeywordValue
		{
			get { return keywordValue; }
			set
			{
				keywordValue = value;
				RaisePropertyChanged("KeywordValue");
			}
		}

		public delegate string SetKeywordText(IProjectData projectData);

		private SetKeywordText replaceAction;

		[JsonIgnore]
		public SetKeywordText Replace
		{
			get { return replaceAction; }
			set
			{
				replaceAction = value;
				RaisePropertyChanged("Replace");
			}
		}

		public string ReplaceText(string inputText, IProjectData projectData = null)
		{
			if (Replace != null)
			{
				return inputText.Replace(KeywordName, Replace(projectData));
			}
			else
			{
				return inputText.Replace(KeywordName, KeywordValue);
			}
		}

		public KeywordData()
		{
			keywordName = "";
			KeywordValue = "";
		}
	}
}
