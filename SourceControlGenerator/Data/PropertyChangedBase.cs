using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SCG.Data
{
	/// <summary>
	/// Base class for notifying property changes.
	/// Use Update(ref field, value); when setting a property.
	/// Use Notify("PropertyName") when manually forcing a notification.
	/// Based on: https://github.com/wieslawsoltes/ReactiveHistory/blob/master/samples/ReactiveHistorySample.Models/ObservableObject.cs
	/// (MIT License)
	/// </summary>
	public abstract class PropertyChangedBase : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		public virtual void OnPropertyNotify(string propertyName)
		{

		}

		public void Notify([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			OnPropertyNotify(propertyName);
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
	}
}
