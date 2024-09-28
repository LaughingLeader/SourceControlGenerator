namespace SCG.Data;

public abstract class HistoryBaseData : ReactiveObject
{
	private object _owner;

	public object Owner
	{
		get { return _owner; }
		set { this.RaiseAndSetIfChanged(ref _owner, value); }
	}

	private string _name;

	public string Name
	{
		get { return _name; }
		set { this.RaiseAndSetIfChanged(ref _name, value); }
	}

	public HistoryBaseData(object owner = null, string name = null)
	{
		this.Owner = owner;
		this.Name = name;
	}
}
