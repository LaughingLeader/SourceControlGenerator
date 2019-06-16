using ReactiveHistory;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SCG.Data
{
	public interface IPropertyChangedHistoryBase
	{
		IHistory History { get; set; }
		void SetHistoryFromObject(IPropertyChangedHistoryBase obj);
	}

	public abstract class PropertyChangedHistoryBase : INotifyPropertyChanged, IPropertyChangedBase, IPropertyChangedHistoryBase
	{
		public IHistory History { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;

		public void SetHistoryFromObject(IPropertyChangedHistoryBase obj)
		{
			History = obj.History;
		}

		public virtual void OnPropertyNotify(string propertyName)
		{

		}

		public void Notify([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			OnPropertyNotify(propertyName);
		}

		private bool SetProperty<T>(object targetObject, string propertyName, T value, bool notify = true)
		{
			var prop = this.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.SetProperty | BindingFlags.Instance);
			if (prop != null && prop.CanWrite)
			{
				prop.SetValue(this, value);
				if (notify) Notify(propertyName);
				return true;
			}
			return false;
		}

		private bool SetField<T>(string fieldName, T value, string propertyName = null, bool notify = true)
		{
			var field = this.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
			if (field != null)
			{
				field.SetValue(this, value);
				if (notify && propertyName != null) Notify(propertyName);
				return true;
			}
			return false;
		}

		public bool Update<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
		{
			if (!Equals(field, value))
			{
				field = value;
				Notify(propertyName);
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

					History.Snapshot(() =>
					{
						this.SetProperty(this, propertyName, undoValue, true);
					}, () =>
					{
						this.SetProperty(this, propertyName, redoValue, true);
					});
				}

				field = value;
				Notify(propertyName);
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

				field = value;
				Notify(thisPropertyName);
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

					History.Snapshot(() =>
					{
						this.SetField(fieldName, undoValue, propertyName, true);
					}, () =>
					{
						this.SetField(fieldName, redoValue, propertyName, true);
					});
				}

				field = value;
				Notify(propertyName);
				return true;
			}
			return false;
		}
	}
}
