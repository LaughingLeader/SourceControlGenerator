using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Runtime.CompilerServices;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using ReactiveHistory;
using System.Reflection;
using System.Windows.Input;

namespace SCG.Data
{
	public abstract class HistoryBaseViewModel : IDisposable, INotifyPropertyChanged
	{
		private CompositeDisposable Disposable { get; set; }

		public ICommand UndoCommand { get; set; }
		public ICommand RedoCommand { get; set; }
		public ICommand ClearCommand { get; set; }

		private IHistory History { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;

		public virtual void OnPropertyNotify(string propertyName)
		{

		}

		public void Notify([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			OnPropertyNotify(propertyName);
		}

		private void Snapshot<T>(T field, T value, string propertyName = null)
		{
			if (History != null && propertyName != null)
			{
				var prop = this.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.SetProperty | BindingFlags.Instance);
				if (prop != null && prop.CanWrite)
				{
					History.Snapshot(() =>
					{
						prop.SetValue(this, field);
					}, () =>
					{
						prop.SetValue(this, value);
					});
				}
			}
		}

		public bool Update<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
		{
			if (!Equals(field, value))
			{
				Snapshot(field, value, propertyName);
				field = value;
				Notify(propertyName);
				return true;
			}
			return false;
		}

		public HistoryBaseViewModel()
		{
			Disposable = new CompositeDisposable();

			History = new StackHistory().AddTo(Disposable);

			var undo = new ReactiveCommand(History.CanUndo, false);
			undo.Subscribe(_ => History.Undo()).AddTo(this.Disposable);
			UndoCommand = undo;

			var redo = new ReactiveCommand(History.CanRedo, false);
			redo.Subscribe(_ => History.Redo()).AddTo(this.Disposable);
			RedoCommand = redo;

			var clear = new ReactiveCommand(History.CanClear, false);
			clear.Subscribe(_ => History.Clear()).AddTo(this.Disposable);
			ClearCommand = clear;
		}

		public void Dispose()
		{
			this.Disposable.Dispose();
		}
	}
}
