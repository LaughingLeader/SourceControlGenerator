using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SCG.Commands;
using SCG.Converters.Json;
using SCG.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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
			get { return inputData.Content; }
			set
			{
				inputData.Content = value;
				RaisePropertyChanged("InputText");
			}
		}

		public string OutputText
		{
			get { return outputData.Content; }
			set
			{
				outputData.Content = value;
				RaisePropertyChanged("OutputText");
			}
		}

		private int generationAmount = 1;

		[DataMember]
		public int GenerationAmount
		{
			get { return generationAmount; }
			set
			{
				generationAmount = value;
				RaisePropertyChanged("GenerationAmount");
			}
		}

		private TextGeneratorInputType nextKeywordType = TextGeneratorInputType.Incremental;

		[DataMember]
		public TextGeneratorInputType NextKeywordType
		{
			get { return nextKeywordType; }
			set
			{
				nextKeywordType = value;
				RaisePropertyChanged("NextKeywordType");
			}
		}

		private TextGeneratorSaveFileData inputData;

		[DataMember]
		public TextGeneratorSaveFileData InputData
		{
			get { return inputData; }
			set
			{
				inputData = value;
				RaisePropertyChanged("InputData");
			}
		}

		private TextGeneratorSaveFileData outputData;

		[DataMember]
		public TextGeneratorSaveFileData OutputData
		{
			get { return outputData; }
			set
			{
				outputData = value;
				RaisePropertyChanged("OutputData");
			}
		}

		public ICommand AddCommand { get; set; }

		public ICommand RemoveCommand { get; set; }

		public ICommand GenerateCommand { get; set; }

		[DataMember]
		public ObservableCollection<ITextGeneratorInputData> Keywords { get; set; }

		public void Init()
		{
			if(InputData != null)
			{
				InputData.Init();
			}
			else
			{
				InputData = new TextGeneratorSaveFileData();
			}

			if (OutputData != null)
			{
				OutputData.Init();
			}
			else
			{
				OutputData = new TextGeneratorSaveFileData();
			}

			if (String.IsNullOrWhiteSpace(InputData.Filename))
			{
				InputData.Filename = "TextGenerator_Input1.txt";

			}
			if (String.IsNullOrWhiteSpace(OutputData.Filename))
			{
				OutputData.Filename = "TextGenerator_Output1.txt";
			}

			InputData.SaveAsText = "Save Input As...";
			InputData.OpenText = "Open Input...";
			OutputData.SaveAsText = "Save Output As...";
			OutputData.OpenText = "Open Output...";
		}

		public TextGeneratorData()
		{
			
		}
	}

	[DataContract]
	public class TextGeneratorSaveFileData : PropertyChangedBase, ISaveCommandData
	{
		private string filename;

		public string Filename
		{
			get { return filename; }
			set
			{
				filename = value;
				RaisePropertyChanged("Filename");
			}
		}

		private string filepath;

		[DataMember]
		public string FilePath
		{
			get { return filepath; }
			set
			{
				filepath = value;
				RaisePropertyChanged("FilePath");
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

		private string content = "";

		public string Content
		{
			get { return content; }
			set
			{
				content = value;
				RaisePropertyChanged("Content");
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

		private string openText;

		public string OpenText
		{
			get { return openText; }
			set
			{
				openText = value;
				RaisePropertyChanged("OpenText");
			}
		}

		public FileBrowserFilter FileTypes { get; set; } = CommonFileFilters.All;

		public SaveFileCommand SaveCommand { get; set; }

		public SaveFileAsCommand SaveAsCommand { get; set; }

		public ParameterCommand OpenCommand { get; set; }

		public ISaveCommandData SaveParameters => this;

		public void Open()
		{
			if (FileCommands.IsValidFilePath(FilePath))
			{
				Content = FileCommands.ReadFile(FilePath);
				Filename = Path.GetFileName(FilePath);
			}
		}

		private void OnSave(bool success)
		{
			
		}

		private void OnSaveAs(bool success, string path)
		{
			if (success)
			{
				
			}
			else
			{
				
			}
		}

		public void Init()
		{
			Open();

			if(String.IsNullOrWhiteSpace(DefaultFilePath))
			{
				DefaultFilePath = DefaultPaths.RootFolder;
			}
		}

		public TextGeneratorSaveFileData()
		{
			OpenCommand = new ParameterCommand((object param) =>
			{
				if (param is string FileLocationText)
				{
					FilePath = FileLocationText;
					Content = FileCommands.ReadFile(FilePath);
				}
			});

			SaveCommand = new SaveFileCommand(OnSave, OnSaveAs);
			SaveAsCommand = new SaveFileAsCommand(OnSaveAs);
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
				inputType = value;
				RaisePropertyChanged("InputType");
			}
		}

		private string keyword = "";

		public string Keyword
		{
			get { return keyword; }
			set
			{
				keyword = value;
				RaisePropertyChanged("Keyword");
			}
		}

		private string inputValue = "";

		public string InputValue
		{
			get { return inputValue; }
			set
			{
				inputValue = value;
				RaisePropertyChanged("InputValue");
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
				inputType = value;
				RaisePropertyChanged("InputType");
			}
		}

		private string keyword = "";

		public string Keyword
		{
			get { return keyword; }
			set
			{
				keyword = value;
				RaisePropertyChanged("Keyword");
			}
		}

		private int startValue = 1;

		public int StartValue
		{
			get { return startValue; }
			set
			{
				startValue = value;
				RaisePropertyChanged("StartValue");
			}
		}

		private int incrementBy = 1;

		public int IncrementBy
		{
			get { return incrementBy; }
			set
			{
				incrementBy = value;
				RaisePropertyChanged("IncrementBy");
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
