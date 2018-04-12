using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LL.SCG.Commands;
using LL.SCG.Enum;

namespace LL.SCG.Data.View
{
	public class SettingsEntryData
	{
		public string Name { get; set; }
		public FileBrowseType FileBrowseType { get; set; }
		public SettingsViewPropertyType ViewType { get; set; }

		public object Source { get; set; }
		public PropertyInfo SourceProperty { get; set; }

		public string OpenFileText
		{
			get
			{
				if(FileBrowseType == FileBrowseType.File)
				{
					return "Select File...";
				}
				else if (FileBrowseType == FileBrowseType.Directory)
				{
					return "Select Folder...";
				}
				else
				{
					return "";
				}
			}
		}

		public ActionCommand OnOpened { get; set; }

		public object Value
		{
			get
			{
				if(Source != null)
				{
					return SourceProperty.GetValue(Source);
				}
				return null;
			}

			set
			{
				if (Source != null)
				{
					SourceProperty.SetValue(Source, value);
				}
			}
		}

		private void OnFileOpened(object path)
		{
			if(path is string filePath)
			{
				Value = filePath;
			}
		}

		public SettingsEntryData()
		{
			OnOpened = new ActionCommand(OnFileOpened);
		}
	}
}
