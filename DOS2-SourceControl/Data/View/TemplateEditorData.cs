using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LL.DOS2.SourceControl.Data.View
{
    public class TemplateEditorData : PropertyChangedBase
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

		private string defaultEditorText;

		public string DefaultEditorText
		{
			get { return defaultEditorText; }
			set
			{
				defaultEditorText = value;
				RaisePropertyChanged("DefaultEditorText");
			}
		}

		private string browseText;

		public string BrowseText
		{
			get { return browseText; }
			set
			{
				browseText = value;
				RaisePropertyChanged("BrowseText");
			}
		}

		private string labelText;

		public string LabelText
		{
			get { return labelText; }
			set
			{
				labelText = value;
				RaisePropertyChanged("LabelText");
			}
		}

		private string tooltipText;

		public string TooltipText
		{
			get { return tooltipText; }
			set
			{
				tooltipText = value;
				RaisePropertyChanged("TooltipText");
			}
		}

		private Action saveCommand;

		public Action SaveCommand
		{
			get { return saveCommand; }
			set
			{
				saveCommand = value;
				RaisePropertyChanged("SaveCommand");
			}
		}

		private Action saveAsCommand;

		public Action SaveAsCommand
		{
			get { return saveAsCommand; }
			set
			{
				saveAsCommand = value;
				RaisePropertyChanged("SaveAsCommand");
			}
		}

		private Action openCommand;

		public Action OpenCommand
		{
			get { return openCommand; }
			set
			{
				openCommand = value;
				RaisePropertyChanged("OpenCommand");
			}
		}

	}
}
