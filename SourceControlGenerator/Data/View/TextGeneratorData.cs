using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SCG.Commands;
using SCG.Converters.Json;
using SCG.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Alphaleonis.Win32.Filesystem;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SCG.Data.View
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum TextGeneratorInputType
	{
		[Description("Replace keywords with text.")]
		Text,
		[Description("Incremental a keyword per text generation.")]
		Incremental,
		[Description("Decrement a keyword per text generation.")]
		Decremental
	}

	[DataContract]
	public class TextGeneratorData : PropertyChangedBase
	{
		public string InputText
		{
			get { return inputData != null ? inputData.Content : ""; }
			set
			{
				if(!Equals(inputData.Content, value))
				{
					inputData.Content = value;
					Notify("InputText");
				}
			}
		}

		public string OutputText
		{
			get { return outputData != null ? outputData.Content : ""; }
			set
			{
				if (!Equals(outputData.Content, value))
				{
					outputData.Content = value;
					Notify("OutputText");
				}
			}
		}

		private int generationAmount = 1;

		[DataMember]
		public int GenerationAmount
		{
			get { return generationAmount; }
			set
			{
				bool bSaveData = generationAmount != value;
				Update(ref generationAmount, value);

				if (bSaveData) SaveDataEvent?.Invoke(this, new EventArgs());
			}
		}

		private TextGeneratorInputType nextKeywordType = TextGeneratorInputType.Incremental;

		[DataMember]
		public TextGeneratorInputType NextKeywordType
		{
			get { return nextKeywordType; }
			set
			{
				bool bSaveData = nextKeywordType != value;
				Update(ref nextKeywordType, value);

				if(bSaveData) SaveDataEvent?.Invoke(this, new EventArgs());
			}
		}

		private TextGeneratorSaveFileData inputData;

		[DataMember]
		public TextGeneratorSaveFileData InputData
		{
			get { return inputData; }
			set
			{
				Update(ref inputData, value);
			}
		}

		private TextGeneratorSaveFileData outputData;

		[DataMember]
		public TextGeneratorSaveFileData OutputData
		{
			get { return outputData; }
			set
			{
				Update(ref outputData, value);
			}
		}

		public ICommand AddCommand { get; set; }

		public ICommand RemoveCommand { get; set; }

		public ICommand GenerateCommand { get; set; }

		[DataMember]
		public ObservableCollection<ITextGeneratorInputData> Keywords { get; set; }

		public Action OnFileLoaded { get; set; }

		public event EventHandler SaveDataEvent;

		private void OnInputTextChanged()
		{
			Notify("InputText");
		}

		private void OnOutputTextChanged()
		{
			Notify("OutputText");
		}

		private void OnDataFileLoaded()
		{
			OnFileLoaded.Invoke();
		}

		public void Init()
		{
			if(InputData == null)
			{
				InputData = new TextGeneratorSaveFileData();
			}

			if (OutputData == null)
			{
				OutputData = new TextGeneratorSaveFileData();
			}

			InputData.SaveAsText = "Save Input As...";
			InputData.OpenText = "Open Input...";
			OutputData.SaveAsText = "Save Output As...";
			OutputData.OpenText = "Open Output...";

			InputData.Init(OnInputTextChanged, OnDataFileLoaded);
			OutputData.Init(OnOutputTextChanged, OnDataFileLoaded);

			string initialDirectory = AppController.Main.CurrentModule != null ? DefaultPaths.ModuleTextGeneratorFolder(AppController.Main.CurrentModule.ModuleData) : DefaultPaths.RootFolder + @"Default\TextGenerator\";

			if (!InputData.Open() || String.IsNullOrWhiteSpace(InputData.DefaultFileName))
			{
				InputData.InitialDirectory = initialDirectory;
				InputData.DefaultFileName = "TextGenerator_Input1.txt";

				if (!FileCommands.IsValidFilePath(InputData.FilePath))
				{
					InputData.FilePath = Path.Combine(InputData.InitialDirectory, InputData.DefaultFileName);
				}
			}
			if (!OutputData.Open() || String.IsNullOrWhiteSpace(OutputData.DefaultFileName))
			{
				OutputData.InitialDirectory = initialDirectory;
				OutputData.DefaultFileName = "TextGenerator_Output1.txt";
				if (!FileCommands.IsValidFilePath(OutputData.FilePath))
				{
					OutputData.FilePath = Path.Combine(OutputData.InitialDirectory, OutputData.DefaultFileName);
				}
			}
		}

		public TextGeneratorData()
		{
			
		}
	}

	[DataContract]
	public class TextGeneratorSaveFileData : PropertyChangedBase, ISaveCommandData
	{
		private string defaultFileName;

		public string DefaultFileName
		{
			get { return defaultFileName; }
			set
			{
				Update(ref defaultFileName, value);
			}
		}

		private string filepath;

		[DataMember]
		public string FilePath
		{
			get { return filepath; }
			set
			{
				Update(ref filepath, value);
			}
		}

		private string defaultFilePath;

		public string InitialDirectory
		{
			get { return defaultFilePath; }
			set
			{
				Update(ref defaultFilePath, value);
			}
		}

		private string content = "";

		public string Content
		{
			get { return content; }
			set
			{
				Update(ref content, value);
				OnContentChanged?.Invoke();
			}
		}

		private string saveAsText;

		public string SaveAsText
		{
			get { return saveAsText; }
			set
			{
				Update(ref saveAsText, value);
			}
		}

		private string openText;

		public string OpenText
		{
			get { return openText; }
			set
			{
				Update(ref openText, value);
			}
		}

		public FileBrowserFilter FileTypes { get; set; } = CommonFileFilters.All;

		public SaveFileCommand SaveCommand { get; set; }

		public SaveFileAsCommand SaveAsCommand { get; set; }

		public ParameterCommand OpenCommand { get; set; }

		public ISaveCommandData SaveParameters => this;

		public Window TargetWindow { get; set; }

		public Action OnContentChanged;
		public Action OnFileLoaded;

		public bool Open()
		{
			if (FileCommands.IsValidFilePath(FilePath))
			{
				Content = FileCommands.ReadFile(FilePath);
				DefaultFileName = Path.GetFileName(FilePath);
				InitialDirectory = Directory.GetParent(FilePath).FullName;
				return true;
			}
			return false;
		}

		private void OnSave(bool success)
		{
			
		}

		private void OnSaveAs(bool success, string path)
		{
			if (success)
			{
				FilePath = path;
				DefaultFileName = Path.GetFileName(FilePath);
			}
			else
			{
				
			}
		}

		public void Init(Action OnContentChangedAction, Action OnFileLoadedAction = null)
		{
			OnContentChanged = OnContentChangedAction;

			if (OnFileLoadedAction != null) OnFileLoaded = OnFileLoadedAction;

			OpenCommand = new ParameterCommand((object param) =>
			{
				if (param is string FileLocationText)
				{
					FilePath = FileLocationText;
					Content = FileCommands.ReadFile(FilePath);
					OnFileLoaded?.Invoke();
				}
			});

			SaveCommand = new SaveFileCommand(OnSave, OnSaveAs);
			SaveAsCommand = new SaveFileAsCommand(OnSaveAs);
		}

		public TextGeneratorSaveFileData()
		{
			
		}
	}

	[JsonConverter(typeof(TextGeneratorJsonKeywordConverter))]
	public interface ITextGeneratorInputData
	{
		TextGeneratorInputType InputType { get; set; }

		string Keyword { get; set; }

		//object InputValue { get; set; }

		void Reset();

		string GetOutput(string input);
	}

	public class TextGeneratorInputTextData : PropertyChangedBase, ITextGeneratorInputData
	{
		private TextGeneratorInputType inputType = TextGeneratorInputType.Text;

		public TextGeneratorInputType InputType
		{
			get { return inputType; }
			set
			{
				Update(ref inputType, value);
			}
		}

		private string keyword = "";

		public string Keyword
		{
			get { return keyword; }
			set
			{
				Update(ref keyword, value);
			}
		}

		private string inputValue = "";

		public string InputValue
		{
			get { return inputValue; }
			set
			{
				Update(ref inputValue, value);
			}
		}

		public string GetOutput(string input)
		{
			return input.Replace(Keyword, InputValue);
		}

		public void Reset() { }

		public TextGeneratorInputTextData() { Keyword = "Keyword"; }

		public TextGeneratorInputTextData(string keywordString)
		{
			Keyword = keywordString;
		}
	}

	public class TextGeneratorInputNumberData : PropertyChangedBase, ITextGeneratorInputData
	{
		private TextGeneratorInputType inputType;

		public TextGeneratorInputType InputType
		{
			get { return inputType; }
			set
			{
				Update(ref inputType, value);
			}
		}

		private string keyword = "";

		public string Keyword
		{
			get { return keyword; }
			set
			{
				Update(ref keyword, value);
			}
		}

		private int startValue = 1;

		public int StartValue
		{
			get { return startValue; }
			set
			{
				Update(ref startValue, value);
			}
		}

		private int incrementBy = 1;

		public int IncrementBy
		{
			get { return incrementBy; }
			set
			{
				Update(ref incrementBy, value);
			}
		}


		[JsonIgnore] public int InputCount = 0;
		[JsonIgnore] public int LastValue = 0;

		public string GetOutput(string input)
		{
			if (String.IsNullOrWhiteSpace(keyword)) return input;

			int result = StartValue;
			if(InputCount > 0)
			{
				result = IncrementBy + LastValue;
			}

			LastValue = result;

			return input.Replace(Keyword, result.ToString());
		}

		public void Reset()
		{
			InputCount = 0;
			LastValue = 0;
		}

		public TextGeneratorInputNumberData() { }

		public TextGeneratorInputNumberData(TextGeneratorInputType batchTextCreatorInputType, string keywordString)
		{
			InputType = batchTextCreatorInputType;
			Keyword = keywordString;
		}
	}
}
