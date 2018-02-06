using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LL.DOS2.SourceControl.Controls;
using LL.DOS2.SourceControl.Core;
using LL.DOS2.SourceControl.Commands;
using LL.DOS2.SourceControl.Util;
using LL.DOS2.SourceControl.Windows;

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

		private string editorText;

		public string EditorText
		{
			get { return editorText; }
			set
			{
				editorText = value;
				RaisePropertyChanged("EditorText");
			}
		}


		private string openFileText;

		public string OpenFileText
		{
			get { return openFileText; }
			set
			{
				openFileText = value;
				RaisePropertyChanged("OpenFileText");
			}
		}

		private string saveAsText;

		public string SaveAsText
		{
			get { return saveAsText; }
			set
			{
				saveAsText = value;
				RaisePropertyChanged("SaveAsText");
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

		private Func<string> GetFilePath;
		private Action<string> SetFilePath;

		public string FilePath
		{
			get { return GetFilePath != null ? GetFilePath.Invoke() : ""; }
			set
			{
				SetFilePath?.Invoke(value);
				RaisePropertyChanged("FilePath");
			}
		}

		private SaveFileCommand saveCommand;

		public SaveFileCommand SaveCommand
		{
			get { return saveCommand; }
			set
			{
				saveCommand = value;
				RaisePropertyChanged("SaveCommand");
			}
		}

		private SaveFileAsCommand saveAsCommand;

		public SaveFileAsCommand SaveAsCommand
		{
			get { return saveAsCommand; }
			set
			{
				saveAsCommand = value;
				RaisePropertyChanged("SaveAsCommand");
			}
		}

		private ActionCommand openCommand;

		public ActionCommand OpenCommand
		{
			get { return openCommand; }
			set
			{
				openCommand = value;
				RaisePropertyChanged("OpenCommand");
			}
		}

		public void SetToDefault()
		{
			EditorText = DefaultEditorText;
			RaisePropertyChanged("EditorText");
		}

		public TemplateEditorData() { }

		public TemplateEditorData(string TemplateName, string Label, string DefaultText, string Text, Func<string> getFilePath, Action<string> setFilePath, string Tooltip = "", string BrowseFileText = "")
		{
			Name = TemplateName;
			LabelText = Label;
			DefaultEditorText = DefaultText;
			EditorText = Text;

			GetFilePath = getFilePath;
			SetFilePath = setFilePath;

			TooltipText = Tooltip;
			if(String.IsNullOrEmpty(BrowseFileText))
			{
				OpenFileText = "Select " + LabelText;
			}
			else
			{
				OpenFileText = BrowseFileText;
			}

			SaveAsText = "Save " + Name + " As...";

			OpenCommand = new ActionCommand((object param) => 
			{
				FileBrowseControl fileBrowseControl = (FileBrowseControl)param;
				if (fileBrowseControl != null)
				{
					FilePath = fileBrowseControl.FileLocationText;
					EditorText = File.ReadAllText(FilePath);
				}
			});

			SaveCommand = new SaveFileCommand((bool success) =>
			{
				if (success)
				{
					MainWindow.FooterLog("Saved {0} to {1}", Name, FilePath);
				}
				else
				{
					MainWindow.FooterLog("Error saving {0} to {1}", Name, FilePath);
				}
			});

			SaveAsCommand = new SaveFileAsCommand((bool success, string path) =>
			{
				if (success)
				{
					if (FileCommands.PathIsRelative(path))
					{
						path = Common.Functions.GetRelativePath.RelativePathGetter.Relative(Directory.GetCurrentDirectory(), path);
					}
					FilePath = path;
					MainWindow.FooterLog("Saved {0} to {1}", Name, FilePath);
				}
				else
				{
					MainWindow.FooterLog("Error saving {0} to {1}", Name, path);
				}
			});
		}
	}
}
