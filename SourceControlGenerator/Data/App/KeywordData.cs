﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LL.SCG.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace LL.SCG.Data
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

		public delegate string SetKeywordText(IProjectData projectData);
		[JsonIgnore]
		public SetKeywordText Replace;

		public KeywordData()
		{
			keywordName = "";
			KeywordValue = "";
		}
	}
}