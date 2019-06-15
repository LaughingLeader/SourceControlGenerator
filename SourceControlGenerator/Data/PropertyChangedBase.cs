using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SCG.Data
{
	public class PropertyChangedBase : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		public void RaisePropertyChanged(string propertyName)
		{
			OnPropertyChanged(propertyName);
		}

		private void OnPropertyChanged(string property)
		{
			PropertyChangedEventHandler handler = this.PropertyChanged;

			if (handler != null)
			{
				var e = new PropertyChangedEventArgs(property);
				handler(this, e);
			}
		}
	}
}
