using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using SCG.Commands;
using SCG.SCGEnum;

namespace SCG.Data.View
{
	public class SettingsEntryData : ReactiveObject
	{
		private string name;

		public string Name
		{
			get { return name; }
			set
			{
				this.RaiseAndSetIfChanged(ref name, value);
			}
		}

		private FileBrowseType fileBrowseType;

		public FileBrowseType BrowseType
		{
			get { return fileBrowseType; }
			set
			{
				this.RaiseAndSetIfChanged(ref fileBrowseType, value);
			}
		}

		private SettingsViewPropertyType viewPropertyType;

		public SettingsViewPropertyType ViewType
		{
			get { return viewPropertyType; }
			set
			{
				this.RaiseAndSetIfChanged(ref viewPropertyType, value);
			}
		}

		private object source;

		public object Source
		{
			get { return source; }
			set
			{
				this.RaiseAndSetIfChanged(ref source, value);
			}
		}

		private PropertyInfo sourcePropertyInfo;

		public PropertyInfo SourceProperty
		{
			get { return sourcePropertyInfo; }
			set
			{
				this.RaiseAndSetIfChanged(ref sourcePropertyInfo, value);
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
				this.RaiseAndSetIfChanged(ref onOpenedCommand, value);
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
