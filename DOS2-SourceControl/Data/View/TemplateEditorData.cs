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
		public string ID { get; set; }

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

		public Func<string> GetFilePath { private get; set; }
		public Action<string> SetFilePath { private get; set; }

		public string FilePath
		{
			get { return GetFilePath != null ? GetFilePath.Invoke() : ""; }
			set
			{
				SetFilePath?.Invoke(value);
				RaisePropertyChanged("FilePath");
			}
		}

		private string filename;

		public string FileName
		{
			get { return filename; }
			set
			{
				filename = value;
				RaisePropertyChanged("FileName");
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

		private string defaultFilePath;

		public string DefaultFilePath
		{
			get { return defaultFilePath; }
			set
			{
				defaultFilePath = value;
				RaisePropertyChanged("DefaultFilePath");
			}
		}


		public void SetToDefault()
		{
			EditorText = DefaultEditorText;
			RaisePropertyChanged("EditorText");
		}

		private void OnSave(bool success)
		{
			if (success)
			{
				MainWindow.FooterLog("Saved {0} to {1}", Name, FilePath);
			}
			else
			{
				MainWindow.FooterLog("Error saving {0} to {1}", Name, FilePath);
			}
		}

		private void OnSaveAs(bool success, string path)
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
		}

		public void Init()
		{
			if (File.Exists(DefaultFilePath))
			{
				DefaultEditorText = File.ReadAllText(DefaultFilePath);
			}
			else if(FileCommands.IsValidPath(DefaultFilePath) && !String.IsNullOrEmpty(DefaultEditorText))
			{
				File.WriteAllText(DefaultFilePath, DefaultEditorText);
			}

			if (DefaultEditorText == null) DefaultEditorText = "";

			if (String.IsNullOrEmpty(Name) && !String.IsNullOrEmpty(FileName))
			{
				Name = FileName;
			}

			if (File.Exists(FilePath))
			{
				EditorText = File.ReadAllText(FilePath);
				Log.Here().Important("Loaded {0} template file at {1}", Name, FilePath);
			}
			else
			{
				EditorText = DefaultEditorText;
				Log.Here().Error("Template file {0} not found at {1}. Using default template.", Name, FilePath);
			}


			if (String.IsNullOrEmpty(OpenFileText))
			{
				OpenFileText = "Select " + LabelText;
			}

			SaveAsText = "Save " + Name + " As...";

			OpenCommand = new ActionCommand((object param) =>
			{
				FileBrowseControl fileBrowseControl = (FileBrowseControl)param;
				if (fileBrowseControl != null)
				{
					FilePath = fileBrowseControl.FileLocationText;
					EditorText = File.ReadAllText(FilePath);
					FileCommands.Save.SaveAppSettings();
				}
			});

			SaveCommand = new SaveFileCommand(OnSave, OnSaveAs);

			SaveAsCommand = new SaveFileAsCommand(OnSaveAs);
		}
	}
}
