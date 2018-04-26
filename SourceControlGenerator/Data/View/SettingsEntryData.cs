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
	public class SettingsEntryData : PropertyChangedBase
	{
		private string name;

		public string Name
		{
			get { return name; }
			set
			{
				name = value;
				RaisePropertyChanged("Name");
			}
		}

		private string filter = "Test";

		public string Filter
		{
			get { return filter; }
			set
			{
				filter = value;
				RaisePropertyChanged("Filter");
			}
		}


		private FileBrowseType fileBrowseType;

		public FileBrowseType BrowseType
		{
			get { return fileBrowseType; }
			set
			{
				fileBrowseType = value;
				RaisePropertyChanged("BrowseType");
			}
		}

		private SettingsViewPropertyType viewPropertyType;

		public SettingsViewPropertyType ViewType
		{
			get { return viewPropertyType; }
			set
			{
				viewPropertyType = value;
				RaisePropertyChanged("ViewType");
			}
		}

		private object source;

		public object Source
		{
			get { return source; }
			set
			{
				source = value;
				RaisePropertyChanged("Source");
			}
		}

		private PropertyInfo sourcePropertyInfo;

		public PropertyInfo SourceProperty
		{
			get { return sourcePropertyInfo; }
			set
			{
				sourcePropertyInfo = value;
				RaisePropertyChanged("SourceProperty");
			}
		}


		public string OpenFileText
		{
			get
			{
				if(BrowseType == FileBrowseType.File)
				{
					return "Select File...";
				}
				else if (BrowseType == FileBrowseType.Directory)
				{
					return "Select Folder...";
				}
				else
				{
					return "";
				}
			}
		}

		private ParameterCommand onOpenedCommand;

		public ParameterCommand OnOpened
		{
			get { return onOpenedCommand; }
			set
			{
				onOpenedCommand = value;
				RaisePropertyChanged("OnOpened");
			}
		}

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
			OnOpened = new ParameterCommand(OnFileOpened);
		}
	}
}
