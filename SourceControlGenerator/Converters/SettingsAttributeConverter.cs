using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using LL.SCG.Data;
using LL.SCG.Data.View;

namespace LL.SCG.Converters
{
	public class SettingsAttributeConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value != null)
			{
				FieldInfo fi = value.GetType().GetField(value.ToString());
				if (fi != null)
				{
					var attributes = (VisibleToViewAttribute[])fi.GetCustomAttributes(typeof(VisibleToViewAttribute), false);
					if(attributes != null && attributes.Length > 0)
					{
						List<SettingsEntryData> settingsList = new List<SettingsEntryData>();
						for(var i = 0; i < attributes.Length; i++)
						{
							var attribute = attributes[i];
							if(attribute.Visible)
							{
								settingsList.Add(new SettingsEntryData()
								{
									Name = attribute.Name,
									FileBrowseType = attribute.FileBrowseType,
									ViewType = attribute.ViewType
								});
							}
						}
						return settingsList;
					}
				}
			}

			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
