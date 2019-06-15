using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using SCG.Commands;
using SCG.Markdown;
using Newtonsoft.Json;

namespace SCG.Data.View
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
				Update(ref formatter, value);
				Name = formatter.Name;
				DefaultFileExtension = formatter.DefaultFileExtension;
				Notify("Name");
				Notify("OpenFileText");
			}
		}

		private bool enabled = false;

		public bool Enabled
		{
			get { return enabled; }
			set
			{
				Update(ref enabled, value);
				parentViewData?.Notify("CanBatchExport");
			}
		}

		private string filePath;

		public string FilePath
		{
			get { return filePath; }
			set
			{
				Update(ref filePath, value);
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
				Update(ref lastPath, value);
			}
		}

		private string defaultFileName;

		[JsonIgnore]
		public string DefaultFileName
		{
			get { return defaultFileName; }
			set
			{
				Update(ref defaultFileName, value);
			}
		}

		private string defaultFileExtension = ".txt";

		[JsonIgnore]
		public string DefaultFileExtension
		{
			get { return defaultFileExtension; }
			set
			{
				Update(ref defaultFileExtension, value);
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
