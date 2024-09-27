using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using SCG.Controls;
using SCG.Data;
using SCG.Data.View;

namespace SCG.Converters
{
	class SettingsAttributeValueControlConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value != null && value is SettingsEntryData entry)
			{
				//Log.Here().Activity($"entry.ViewType {entry.ViewType} | entry.FileBrowseType {entry.FileBrowseType}");

				if (entry.ViewType == SettingsViewPropertyType.Browser)
				{
					//var browser = new FileBrowseControl();
					////browser.FileLocationText

					//Binding binding = new Binding("FileLocationText");
					//binding.Source = entry.Value;
					//binding.Mode = BindingMode.TwoWay;
					//binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
					//browser.SetBinding(FileBrowseControl.FileLocationTextProperty, binding);

					//return browser;

					return typeof(FileBrowseControl);
				}
			}

			return typeof(TextBox);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
