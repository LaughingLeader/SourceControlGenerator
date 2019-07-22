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
	public class KeywordData : ReactiveObject
	{
		private string keywordName = "";

		public string KeywordName
		{
			get { return keywordName; }
			set
			{
				Update(ref keywordName, value);
			}
		}

		private string keywordValue = "";

		public string KeywordValue
		{
			get { return keywordValue; }
			set
			{
				Update(ref keywordValue, value);
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
				Update(ref replaceAction, value);
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
