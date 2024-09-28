using Newtonsoft.Json;

using SCG.Collections;

using System.Windows.Data;

namespace SCG.Data;

public class UserKeywordData : ReactiveObject
{
	private string dateCustom;

	public string DateCustom
	{
		get { return dateCustom; }
		set
		{
			this.RaiseAndSetIfChanged(ref dateCustom, value);
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
		if (Keywords.Count > 0)
		{
			Keywords.RemoveAt(Keywords.Count - 1);
		}
	}

	public void ResetToDefault()
	{
		if (Keywords == null) Keywords = [];
		if (Keywords.Count > 0) Keywords.Clear();
		Keywords.Add(new KeywordData());
		Keywords.Add(new KeywordData());
		Keywords.Add(new KeywordData());

		DateCustom = "MMMM dd, yyyy";

		BindingOperations.EnableCollectionSynchronization(Keywords, KeywordsLock);
	}

	public void RemoveEmpty()
	{
		if (Keywords != null && Keywords.Count > 0)
		{
			Keywords.RemoveAll(k => k.KeywordValue == "");
		}
	}
}
