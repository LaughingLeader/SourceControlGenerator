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
using LL.SCG.Enum;
using LL.SCG.Extensions;

namespace LL.SCG.Converters
{
	public class SettingsAttributeConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			List<SettingsEntryData> settingsList = new List<SettingsEntryData>();

			settingsList.Add(new SettingsEntryData()
			{
				Name = "Test",
				FileBrowseType = FileBrowseType.Directory,
				ViewType = SettingsViewPropertyType.Browser
			});

			if (value != null)
			{
				FieldInfo fi = value.GetType().GetField(value.ToString());
				if (fi != null)
				{
					var attributes = (VisibleToViewAttribute[])fi.GetCustomAttributes(typeof(VisibleToViewAttribute), false);
					if(attributes != null && attributes.Length > 0)
					{
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

							Log.Here().Important("Adding attribute: {0} {1} {2}", attribute.Name, attribute.FileBrowseType, attribute.ViewType);
						}
					}
					else
					{
						Log.Here().Error("Problem reading attributes from settings class");
					}
				}
				else
				{
					Log.Here().Error("Field info is null");
				}
			}
			else
			{
				Log.Here().Error("Settings value is null");
			}

			return settingsList;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
