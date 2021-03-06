﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SCG.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ReactiveUI;

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
				this.RaiseAndSetIfChanged(ref keywordName, value);
			}
		}

		private string keywordValue = "";

		public string KeywordValue
		{
			get { return keywordValue; }
			set
			{
				this.RaiseAndSetIfChanged(ref keywordValue, value);
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
				this.RaiseAndSetIfChanged(ref replaceAction, value);
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
