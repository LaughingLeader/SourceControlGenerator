using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using SCG.Collections;
using Newtonsoft.Json;

namespace SCG.Data
{
	public class UserKeywordData : ReactiveObject
	{
		private string dateCustom;

		public string DateCustom
		{
			get { return dateCustom; }
			set
			{
				Update(ref dateCustom, value);
			}
		}

		public ObservableImmutableList<KeywordData> Keywords { get; set; }

		[JsonIgnore]
		public object KeywordsLock { get; private set; } = new object();

		public void AddKeyword()
		{
			Keywords.Add(new KeywordData());
		}

		public void RemoveLast()
		{
			if(Keywords.Count > 0)
			{
				Keywords.RemoveAt(Keywords.Count - 1);
			}
		}

		public void ResetToDefault()
		{
			if(Keywords == null) Keywords = new ObservableImmutableList<KeywordData>();
			if (Keywords.Count > 0) Keywords.Clear();
			Keywords.Add(new KeywordData());
			Keywords.Add(new KeywordData());
			Keywords.Add(new KeywordData());

			DateCustom = "MMMM dd, yyyy";

			BindingOperations.EnableCollectionSynchronization(Keywords, KeywordsLock);
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
