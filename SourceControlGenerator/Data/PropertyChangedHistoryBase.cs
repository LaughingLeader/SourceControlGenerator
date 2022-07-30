using ReactiveHistory;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SCG.Data
{
	public interface IHistoryKeeper
	{
		IHistory History { get; set; }
	}

	public interface IPropertyChangedHistoryBase : IHistoryKeeper
	{
		bool ChangesUnsaved { get; set; }
		void SetHistoryFromObject(IHistoryKeeper obj);
	}

	public abstract class PropertyChangedHistoryBase : ReactiveObject, IPropertyChangedHistoryBase
	{
		[IgnoreDataMember]
		public IHistory History { get; set; }

		private bool changedUnsaved = false;

		[IgnoreDataMember]
		public bool ChangesUnsaved
		{
			get => changedUnsaved;
			set { this.RaiseAndSetIfChanged(ref changedUnsaved, value); }
		}

		public void SetHistoryFromObject(IHistoryKeeper obj)
		{
			History = obj.History;
		}

		//public void this.RaisePropertyChanged([CallerMemberName] string propertyName = null)
		//{
		//	this.RaisePropertyChanged(propertyName);
		//}

		//public void Update<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
		//{
		//	this.RaiseAndSetIfChanged(ref field, value, propertyName);
		//}

		private bool SetProperty<T>(object targetObject, string propertyName, T value, bool notify = true)
		{
			var prop = this.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.SetProperty | BindingFlags.Instance);
			if (prop != null && prop.CanWrite)
			{
				if (notify)
				{
					this.RaisePropertyChanging(propertyName);
				}
				prop.SetValue(this, value);
				if (notify)
				{
					this.RaisePropertyChanged(propertyName);
				}
				return true;
			}
			return false;
		}

		private bool SetField<T>(string fieldName, T value, string propertyName = null, bool notify = true)
		{
			var field = this.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
			if (field != null)
			{
				if (notify && propertyName != null)
				{
					this.RaisePropertyChanging(propertyName);
				}
				field.SetValue(this, value);
				if (notify && propertyName != null)
				{
					this.RaisePropertyChanged(propertyName);
				}
				return true;
			}
			return false;
		}

		public bool UpdateWithHistory<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
		{
			if (!Equals(field, value))
			{
				if (History != null)
				{
					var undoValue = field;
					var redoValue = value;
					bool lastChangesUnsaved = ChangesUnsaved;

					History.Snapshot(() =>
					{
						this.SetProperty(this, propertyName, undoValue, true);
						ChangesUnsaved = lastChangesUnsaved;
					}, () =>
					{
						this.SetProperty(this, propertyName, redoValue, true);
						ChangesUnsaved = true;
					});
				}

				this.RaiseAndSetIfChanged(ref field, value);
				return true;
			}
			return false;
		}

		public bool UpdateWithHistoryForObject<T>(ref T field, T value, object targetObject, string propertyName, bool notify = true, [CallerMemberName] string thisPropertyName = null)
		{
			if (!Equals(field, value))
			{
				if (History != null)
				{
					var undoValue = field;
					var redoValue = value;

					History.Snapshot(() =>
					{
						this.SetProperty(targetObject, propertyName, undoValue, notify);
					}, () =>
					{
						this.SetProperty(targetObject, propertyName, redoValue, notify);
					});
				}

				this.RaiseAndSetIfChanged(ref field, value);
				return true;
			}
			return false;
		}

		public bool UpdateWithHistoryWithField<T>(ref T field, T value, string fieldName, [CallerMemberName] string propertyName = null)
		{
			if (!Equals(field, value))
			{
				if (History != null)
				{
					var undoValue = field;
					var redoValue = value;
					bool lastChangesUnsaved = ChangesUnsaved;

					History.Snapshot(() =>
					{
						this.SetField(fieldName, undoValue, propertyName, true);
						ChangesUnsaved = lastChangesUnsaved;
					}, () =>
					{
						this.SetField(fieldName, redoValue, propertyName, true);
						ChangesUnsaved = true;
					});
				}

				this.RaiseAndSetIfChanged(ref field, value);
				return true;
			}
			return false;
		}
	}
}
