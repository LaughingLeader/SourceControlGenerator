using ReactiveHistory;

using System.Windows.Input;

namespace SCG.Data;

public abstract class HistoryBaseViewModel : ReactiveObject, IHistoryKeeper, IDisposable
{
	private CompositeDisposable Disposable { get; set; }

	public ICommand UndoCommand { get; set; }
	public ICommand RedoCommand { get; set; }
	public ICommand ClearCommand { get; set; }

	public IHistory History { get; set; }

	public void CreateSnapshot(Action undo, Action redo)
	{
		History.Snapshot(undo, redo);
	}

	public void UpdateList<T>(IList<T> source, IList<T> oldValue, IList<T> newValue)
	{
		void undo() => source = oldValue;
		void redo() => source = newValue;
		History.Snapshot(undo, redo);
		source = newValue;
	}

	public void AddWithHistory<T>(IList<T> source, T item)
	{
		var index = source.Count;
		void redo() => source.Insert(index, item);
		void undo() => source.RemoveAt(index);
		History.Snapshot(undo, redo);
		redo();
	}

	public void RemoveWithHistory<T>(IList<T> source, T item)
	{
		var index = source.IndexOf(item);
		void redo() => source.RemoveAt(index);
		void undo() => source.Insert(index, item);
		History.Snapshot(undo, redo);
		redo();
	}
	public void RemoveWithHistory<T>(IList<T> source, IEnumerable<T> items)
	{
		var last = source.ToList();
		var next = source.Where(x => !items.Contains(x));
		void redo() => source = next.ToList();
		void undo() => source = last;
		History.Snapshot(undo, redo);
		redo();
	}

	public void Undo()
	{
		History.Undo();
		//var canRedo = History.CanRedo.ToReadOnlyReactiveProperty().Value;
		//Log.Here().Activity($"Undo command called. canRedo: {canRedo}");
	}

	public void Redo()
	{
		History.Redo();
		//var canUndo = History.CanUndo.ToReadOnlyReactiveProperty().Value;
		//Log.Here().Activity($"Redo command called. canUndo: {canUndo}");
	}

	public virtual void Clear()
	{
		History.Clear();
	}

	public HistoryBaseViewModel()
	{
		Disposable = [];

		var history = new StackHistory();
		Disposable.Add(history);
		History = history;


		UndoCommand = ReactiveCommand.Create(Undo, History.CanUndo).DisposeWith(Disposable);
		RedoCommand = ReactiveCommand.Create(Redo, History.CanRedo).DisposeWith(Disposable);
		ClearCommand = ReactiveCommand.Create(Clear, History.CanClear).DisposeWith(Disposable);
	}

	public void Dispose()
	{
		this.Disposable.Dispose();
	}
}
