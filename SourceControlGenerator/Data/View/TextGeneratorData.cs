﻿using Newtonsoft.Json;
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
using ReactiveUI;
using SCG.Windows;

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
	public class TextGeneratorViewModel : ReactiveObject
	{
		[DataMember]
		public ObservableCollection<TextGeneratorData> GeneratorPresets { get; set; }

		private int textGeneratorActiveSettingsIndex = 0;

		[DataMember]
		public int ActivePresetIndex
		{
			get { return textGeneratorActiveSettingsIndex; }
			set
			{
				bool bSaveData = textGeneratorActiveSettingsIndex != value;
				this.RaiseAndSetIfChanged(ref textGeneratorActiveSettingsIndex, value);
				this.RaisePropertyChanged("ActiveData");
			}
		}

		public TextGeneratorData ActiveData
		{
			get
			{
				if(ActivePresetIndex < GeneratorPresets.Count)
				{
					return GeneratorPresets[ActivePresetIndex];
				}
				return null;
			}
		}

		private string footerText = "";

		public string FooterText
		{
			get => footerText;
			set { this.RaiseAndSetIfChanged(ref footerText, value); }
		}

		public ICommand AddCommand { get; set; }
		public ICommand RemoveCommand { get; set; }
		public ICommand GenerateCommand { get; set; }
		public ICommand AddSettingsCommand { get; set; }
		public ICommand SaveSettingsCommand { get; set; }
		public ICommand RemoveSettingsCommand { get; set; }

		public Window TargetWindow { get; set; }

		public Action OnFileLoaded { get; set; }

		public event EventHandler SaveDataEvent;

		public void SaveData(TextGeneratorData data)
		{
			SaveDataEvent.Invoke(data, new EventArgs());
		}

		public void AddPreset()
		{
			var data = new TextGeneratorData(this);
			if (GeneratorPresets.Count == 0)
			{
				data.Name = "Default";
			}
			else
			{
				data.Name = "Preset" + (GeneratorPresets.Count + 1);
			}
			data.Init();
			GeneratorPresets.Add(data);
			ActivePresetIndex = GeneratorPresets.Count - 1;
		}

		public void RemoveCurrentPreset()
		{
			var lastIndex = ActivePresetIndex;
			var presets = GeneratorPresets.ToList();
			presets.Remove(ActiveData);
			GeneratorPresets = new ObservableCollection<TextGeneratorData>(presets);

			if (lastIndex > 0)
			{
				ActivePresetIndex = lastIndex - 1;
			}
			else
			{
				ActivePresetIndex = 0;
			}
		}

		public void Init(Window parentWindow)
		{
			TargetWindow = parentWindow;

			AddSettingsCommand = ReactiveCommand.Create(AddPreset);

			RemoveSettingsCommand = ReactiveCommand.Create(() =>
			{
				FileCommands.OpenConfirmationDialog(TargetWindow, "Delete Preset", $"Delete {ActiveData.Name}?", "Changes will be lost.", new Action<bool>((b) =>
				{
					if(b)
					{
						RemoveCurrentPreset();
					}
				}));
			});

			SaveSettingsCommand = ReactiveCommand.Create(() =>
			{
				try
				{
					SaveData(ActiveData);
				}
				catch (Exception ex)
				{
					Log.Here().Error("Error saving text generator data: " + ex.ToString());
				}
			});
		}

		public TextGeneratorViewModel()
		{
			GeneratorPresets = new ObservableCollection<TextGeneratorData>();
		}
	}

	[DataContract]
	public class TextGeneratorData : ReactiveObject
	{
		public TextGeneratorViewModel Parent { get; private set; }

		public string InputText
		{
			get { return inputData != null ? inputData.Content : ""; }
			set
			{
				if(!Equals(inputData.Content, value))
				{
					inputData.Content = value;
					this.RaisePropertyChanged("InputText");
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
					this.RaisePropertyChanged("OutputText");
				}
			}
		}

		private string name;

		[DataMember]
		public string Name
		{
			get => name;
			set { this.RaiseAndSetIfChanged(ref name, value); }
		}


		private int generationAmount = 1;

		[DataMember]
		public int GenerationAmount
		{
			get { return generationAmount; }
			set
			{
				bool bSaveData = generationAmount != value;
				this.RaiseAndSetIfChanged(ref generationAmount, value);

				if (bSaveData) Parent?.SaveData(this);
			}
		}

		private TextGeneratorSaveFileData inputData;

		[DataMember]
		public TextGeneratorSaveFileData InputData
		{
			get { return inputData; }
			set
			{
				this.RaiseAndSetIfChanged(ref inputData, value);
			}
		}

		private TextGeneratorSaveFileData outputData;

		[DataMember]
		public TextGeneratorSaveFileData OutputData
		{
			get { return outputData; }
			set
			{
				this.RaiseAndSetIfChanged(ref outputData, value);
			}
		}

		[DataMember]
		public ObservableCollection<ITextGeneratorInputData> Keywords { get; set; }


		private TextGeneratorInputType nextKeywordType = TextGeneratorInputType.Incremental;

		[DataMember]
		public TextGeneratorInputType NextKeywordType
		{
			get { return nextKeywordType; }
			set
			{
				bool bSaveData = nextKeywordType != value;
				this.RaiseAndSetIfChanged(ref nextKeywordType, value);

				if (bSaveData) Parent?.SaveData(this);
			}
		}

		private void OnInputTextChanged()
		{
			this.RaisePropertyChanged("InputText");
		}

		private void OnOutputTextChanged()
		{
			this.RaisePropertyChanged("OutputText");
		}

		private void OnDataFileLoaded()
		{
			Parent?.OnFileLoaded?.Invoke();
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

			InputData.TargetWindow = Parent.TargetWindow;
			OutputData.TargetWindow = Parent.TargetWindow;

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

		public TextGeneratorData(TextGeneratorViewModel parent)
		{
			Keywords = new ObservableCollection<ITextGeneratorInputData>();
			Parent = parent;
		}
	}

	[DataContract]
	public class TextGeneratorSaveFileData : ReactiveObject, ISaveCommandData
	{
		private string defaultFileName;

		public string DefaultFileName
		{
			get { return defaultFileName; }
			set
			{
				this.RaiseAndSetIfChanged(ref defaultFileName, value);
			}
		}

		private string filepath;

		[DataMember]
		public string FilePath
		{
			get { return filepath; }
			set
			{
				this.RaiseAndSetIfChanged(ref filepath, value);
			}
		}

		private string defaultFilePath;

		public string InitialDirectory
		{
			get { return defaultFilePath; }
			set
			{
				this.RaiseAndSetIfChanged(ref defaultFilePath, value);
			}
		}

		private string content = "";

		public string Content
		{
			get { return content; }
			set
			{
				this.RaiseAndSetIfChanged(ref content, value);
				OnContentChanged?.Invoke();
			}
		}

		private string saveAsText;

		public string SaveAsText
		{
			get { return saveAsText; }
			set
			{
				this.RaiseAndSetIfChanged(ref saveAsText, value);
			}
		}

		private string openText;

		public string OpenText
		{
			get { return openText; }
			set
			{
				this.RaiseAndSetIfChanged(ref openText, value);
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

	[DataContract]
	public class TextGeneratorInputTextData : ReactiveObject, ITextGeneratorInputData
	{
		private TextGeneratorInputType inputType = TextGeneratorInputType.Text;

		[DataMember]
		public TextGeneratorInputType InputType
		{
			get { return inputType; }
			set
			{
				//this.RaiseAndSetIfChanged(ref inputType, value);
			}
		}

		private string keyword = "";

		[DataMember]
		public string Keyword
		{
			get { return keyword; }
			set
			{
				this.RaiseAndSetIfChanged(ref keyword, value);
			}
		}

		private string inputValue = "";

		[DataMember]
		public string InputValue
		{
			get { return inputValue; }
			set
			{
				this.RaiseAndSetIfChanged(ref inputValue, value);
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

	[DataContract]
	public class TextGeneratorInputNumberData : ReactiveObject, ITextGeneratorInputData
	{
		private TextGeneratorInputType inputType = TextGeneratorInputType.Incremental;

		[DataMember]
		public TextGeneratorInputType InputType
		{
			get { return inputType; }
			set
			{
				this.RaiseAndSetIfChanged(ref inputType, value);
			}
		}

		private string keyword = "";

		[DataMember]
		public string Keyword
		{
			get { return keyword; }
			set
			{
				this.RaiseAndSetIfChanged(ref keyword, value);
			}
		}

		private int startValue = 1;

		[DataMember]
		public int StartValue
		{
			get { return startValue; }
			set
			{
				this.RaiseAndSetIfChanged(ref startValue, value);
			}
		}

		private int incrementBy = 1;

		[DataMember]
		public int IncrementBy
		{
			get { return incrementBy; }
			set
			{
				this.RaiseAndSetIfChanged(ref incrementBy, value);
			}
		}

		private int numberPadding = 0;

		[DataMember]
		public int NumberPadding
		{
			get { return incrementBy; }
			set
			{
				this.RaiseAndSetIfChanged(ref numberPadding, value);
			}
		}

		public int InputCount = 0;
		public int LastValue = 0;

		public string GetOutput(string input)
		{
			if (String.IsNullOrWhiteSpace(keyword)) return input;

			int result = StartValue;
			if(InputCount > 0)
			{
				result = IncrementBy + LastValue;
			}

			LastValue = result;

			string finalResult = result.ToString().PadLeft(NumberPadding);

			return input.Replace(Keyword, finalResult);
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
