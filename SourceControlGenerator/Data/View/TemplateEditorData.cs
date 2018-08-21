using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SCG.Controls;
using SCG.Core;
using SCG.Commands;
using SCG.Util;
using SCG.Windows;
using System.Xml.Linq;
using SCG.Data.Xml;
using System.ComponentModel;
using SCG.Converters;
using SCG.SCGEnum;
using SCG.Interfaces;

namespace SCG.Data.View
{
	public class TemplateEditorData : PropertyChangedBase, ISaveCommandData
	{
		private IModuleData parentData;

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

		private EditorTextPropertyType editorTextProperty = EditorTextPropertyType.String;

		public EditorTextPropertyType EditorTextProperty
		{
			get { return editorTextProperty; }
			set
			{
				editorTextProperty = value;
				RaisePropertyChanged("EditorTextProperty");
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

		public string ToolTipText
		{
			get { return tooltipText; }
			set
			{
				tooltipText = value;
				RaisePropertyChanged("TooltipText");
			}
		}

		/*
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
		*/

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


		private string filename;

		public string Filename
		{
			get { return filename; }
			set
			{
				filename = value;
				RaisePropertyChanged("FileName");
			}
		}

		private string exportPath;

		public string ExportPath
		{
			get { return exportPath; }
			set
			{
				exportPath = value;
				RaisePropertyChanged("ExportPath");
			}
		}

		public FileBrowserFilter FileTypes { get; set; } = CommonFileFilters.All;

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

		private ParameterCommand openCommand;

		public ParameterCommand OpenCommand
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

		public ISaveCommandData SaveCommandParameters => this;

		//ISaveCommandData
		public string Content => EditorText;

		public bool IsValid
		{
			get
			{
				if(!String.IsNullOrWhiteSpace(ID) && !String.IsNullOrWhiteSpace(Name) && !String.IsNullOrWhiteSpace(Filename))
				{
					return true;
				}
				return false;
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
				//if (Path.GetFileName(path) == Path.GetFileName(DefaultFilePath)) SaveCommand.OpenSaveAsOnDefault = false;

				if (FileCommands.PathIsRelative(path))
				{
					path = Common.Functions.GetRelativePath.RelativePathGetter.Relative(Directory.GetCurrentDirectory(), path);
				}

				bool saveAppSettings = false;

				if(FilePath != path)
				{
					saveAppSettings = true;
					FilePath = path;
				}

				MainWindow.FooterLog("Saved {0} to {1}", Name, FilePath);

				if (saveAppSettings) FileCommands.Save.SaveModuleSettings(parentData);
			}
			else
			{
				MainWindow.FooterLog("Error saving {0} to {1}", Name, path);
			}
		}

		private static string GetPropertyValueFromXml(IModuleData moduleData, XElement xmlData, string propertyName, string defaultValue = "")
		{
			XElement element = XmlDataHelper.GetDescendantByAttributeValue(xmlData, "Property", "Name", propertyName);
			string value = "";
			if(element != null)
			{
				string type = element.Attribute("Type")?.Value;
				if (type == null) type = "String";

				string contents = element.Value;
				if(!String.IsNullOrWhiteSpace(contents))
				{
					if(type == "Resource")
					{
						var resourceVal = moduleData.LoadStringResource(contents);
						if (resourceVal != null) return resourceVal;
					}
					else if(type == "File")
					{
						if(File.Exists(contents))
						{
							try
							{
								var fileContents = File.ReadAllText(contents);
								return fileContents;
							}
							catch(Exception ex)
							{
								Log.Here().Error("Error loading file(\"{0}\") specified in templates.xml: {1}", contents, ex.ToString());
							}
						}
					}
					else
					{
						return contents;
					}
				}
			}

			return value;
		}

		private static string ReplaceNewlineSymbols(string str)
		{
			return str.Replace("\\n", Environment.NewLine).Replace("\\r", Environment.NewLine);
		}

		public static TemplateEditorData LoadFromXml(IModuleData moduleData, XElement xmlData)
		{
			string ID = XmlDataHelper.GetAttributeAsString(xmlData, "ID", "");
			if(!String.IsNullOrWhiteSpace(ID))
			{
				TemplateEditorData data = new TemplateEditorData()
				{
					ID = ID,
					Name = GetPropertyValueFromXml(moduleData, xmlData, "TabName"),
					LabelText = GetPropertyValueFromXml(moduleData, xmlData, "LabelText"),
					Filename = GetPropertyValueFromXml(moduleData, xmlData, "DefaultTemplateFilename"),
					ExportPath = GetPropertyValueFromXml(moduleData, xmlData, "ExportPath"),
					DefaultEditorText = GetPropertyValueFromXml(moduleData, xmlData, "DefaultEditorText"),
					ToolTipText = ReplaceNewlineSymbols(GetPropertyValueFromXml(moduleData, xmlData, "ToolTip"))
				};
				return data;
			}

			return null;
		}

		public void Init(IModuleData moduleData)
		{
			parentData = moduleData;
			/*
			if (File.Exists(DefaultFilePath))
			{
				DefaultEditorText = File.ReadAllText(DefaultFilePath);
			}
			else if(FileCommands.IsValidPath(DefaultFilePath) && !String.IsNullOrEmpty(DefaultEditorText))
			{
				File.WriteAllText(DefaultFilePath, DefaultEditorText);
			}
			*/

			if (!String.IsNullOrWhiteSpace(Filename)) DefaultFilePath = Path.Combine(DefaultPaths.ModuleTemplatesFolder(parentData), Filename);

			if (String.IsNullOrWhiteSpace(FilePath) && !String.IsNullOrWhiteSpace(DefaultFilePath))
			{
				FilePath = DefaultFilePath;
			}

			if (DefaultEditorText == null) DefaultEditorText = "";

			if (String.IsNullOrEmpty(Name) && !String.IsNullOrEmpty(Filename))
			{
				Name = Filename;
			}

			if (File.Exists(FilePath))
			{
				EditorText = File.ReadAllText(FilePath);
				Log.Here().Important("Loaded {0} template file at {1}", Name, FilePath);
			}
			else
			{
				EditorText = DefaultEditorText;
				Log.Here().Warning("Template file {0} not found at {1}. Using default template.", Name, FilePath);
			}


			if (String.IsNullOrEmpty(OpenFileText))
			{
				OpenFileText = "Select " + LabelText;
			}

			SaveAsText = "Save " + Name + " As...";

			OpenCommand = new ParameterCommand((object param) =>
			{
				if(param is string FileLocationText)
				{
					FilePath = FileLocationText;
					FileCommands.Save.SaveModuleSettings(parentData);
					EditorText = FileCommands.ReadFile(FilePath);
				}
			});

			SaveCommand = new SaveFileCommand(OnSave, OnSaveAs);
			SaveAsCommand = new SaveFileAsCommand(OnSaveAs);

			if (!File.Exists(FilePath) && FileCommands.IsValidPath(FilePath) && !String.IsNullOrWhiteSpace(EditorText))
			{
				FileCommands.WriteToFile(filePath, EditorText);
			}
		}
	}
}
