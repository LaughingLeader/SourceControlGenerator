using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using LL.SCG.Commands;
using LL.SCG.Markdown;
using Newtonsoft.Json;

namespace LL.SCG.Data.View
{
	public class MarkdownFormatterData : PropertyChangedBase
	{
		private IMarkdownFormatter formatter;

		[JsonIgnore]
		public IMarkdownFormatter Formatter
		{
			get { return formatter; }
			set
			{
				formatter = value;
				Name = formatter.Name;
				DefaultFileExtension = formatter.DefaultFileExtension;
				RaisePropertyChanged("Formatter");
				RaisePropertyChanged("Name");
				RaisePropertyChanged("OpenFileText");
			}
		}

		private bool enabled = false;

		public bool Enabled
		{
			get { return enabled; }
			set
			{
				enabled = value;
				RaisePropertyChanged("Enabled");
				if (parentViewData != null) parentViewData.RaisePropertyChanged("CanBatchExport");
			}
		}

		private string filePath;

		public string FilePath
		{
			get { return filePath; }
			set
			{
				filePath = value;
				RaisePropertyChanged("FilePath");
			}
		}

		public string Name { get; set; }

		[JsonIgnore]
		public string OpenFileText
		{
			get
			{
				if(Formatter != null)
				{
					return $"Save {Formatter.Name} output as...";
				}
				return "";
			}
		}

		private string lastPath;

		[JsonIgnore]
		public string LastPath
		{
			get { return lastPath; }
			set
			{
				lastPath = value;
				RaisePropertyChanged("LastPath");
			}
		}

		private string defaultFileName;

		[JsonIgnore]
		public string DefaultFileName
		{
			get { return defaultFileName; }
			set
			{
				defaultFileName = value;
				RaisePropertyChanged("DefaultFileName");
			}
		}

		private string defaultFileExtension = ".txt";

		[JsonIgnore]
		public string DefaultFileExtension
		{
			get { return defaultFileExtension; }
			set
			{
				defaultFileExtension = value;
				RaisePropertyChanged("DefaultFileExtension");
			}
		}


		[JsonIgnore] public ICommand OnFilePathSet { get; set; }

		private MarkdownConverterViewData parentViewData;
		private string lastSavedPath = "";

		public MarkdownFormatterData(MarkdownConverterViewData parentData = null)
		{
			if (parentData != null)
			{
				parentViewData = parentData;
			}

			OnFilePathSet = new ParameterCommand((object param) =>
			{
				bool invokeSave = lastSavedPath != FilePath || (String.IsNullOrWhiteSpace(FilePath) && !String.IsNullOrWhiteSpace(FilePath));

				if (invokeSave && parentViewData != null)
				{
					parentViewData.StartSavingAsync();
				}

				lastSavedPath = FilePath;
			});
		}
	}
}
