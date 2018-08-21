using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using SCG.Data;
using SCG.Data.View;
using SCG.SCGEnum;

namespace SCG.Converters
{
	public class SettingsAttributeConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			List<SettingsEntryData> settingsList = new List<SettingsEntryData>();

			if (value != null)
			{
				try
				{
					Type valType = value.GetType();
					var propInfo = valType.GetProperties();

					foreach(PropertyInfo prop in propInfo)
					{
						var attributes = Attribute.GetCustomAttributes(prop, typeof(VisibleToViewAttribute), false);

						if (attributes != null)
						{
							foreach (var attribute in attributes)
							{
								if (attribute != null && attribute is VisibleToViewAttribute viewAttribute)
								{
									if(viewAttribute.Visible)
									{
										settingsList.Add(new SettingsEntryData()
										{
											Name = viewAttribute.Name,
											BrowseType = viewAttribute.FileBrowseType,
											ViewType = viewAttribute.ViewType,
											Source = value,
											SourceProperty = prop
										});
										//Log.Here().Important("Adding attribute: {0} {1} {2}", viewAttribute.Name, viewAttribute.FileBrowseType, viewAttribute.ViewType);
									}
								}
								else
								{
									Log.Here().Error("Problem adding attribute: Casting to VisibleToViewAttribute failed.");
								}

							}
						}
						else
						{
							Log.Here().Error($"Problem reading attributes from settings class: {value.GetType()} | {attributes.Count()}");
						}
					}
				}
				catch(Exception ex)
				{
					Log.Here().Error($"Error parsing attributes by reflection: {ex.ToString()}");
				}
			}
			else
			{
				Log.Here().Error("Converter value is null!");
			}

			if(settingsList.Count <= 0)
			{
				settingsList.Add(new SettingsEntryData()
				{
					Name = "Test",
					BrowseType = FileBrowseType.Disabled,
					ViewType = SettingsViewPropertyType.Text,
					Source = null,
					SourceProperty = null
				});
			}

			return settingsList;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
