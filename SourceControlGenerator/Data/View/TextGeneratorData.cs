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
using ReactiveUI;
using System.Reactive.Concurrency;
using SCG.Windows;
using DynamicData;
using DynamicData.Binding;

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
		public ObservableCollectionExtended<TextGeneratorData> GeneratorPresets { get; set; }

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
			var data = new TextGeneratorData();
			if (GeneratorPresets.Count == 0)
			{
				data.Name = "Default";
			}
			else
			{
				data.Name = "Preset" + (GeneratorPresets.Count + 1);
			}
			data.Init(this);
			GeneratorPresets.Add(data);
			ActivePresetIndex = GeneratorPresets.Count - 1;
		}

		public void RemoveCurrentPreset()
		{
			var lastIndex = ActivePresetIndex;
			var presets = GeneratorPresets.ToList();
			presets.Remove(ActiveData);
			GeneratorPresets.Clear();
			GeneratorPresets.AddRange(presets);
			this.RaisePropertyChanged("GeneratorPresets");

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

			if(GeneratorPresets.Count > 0)
			{
				foreach(var preset in GeneratorPresets)
				{
					preset.Init(this);
				}
			}
		}

		public TextGeneratorViewModel()
		{
			GeneratorPresets = new ObservableCollectionExtended<TextGeneratorData>();
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

		public void Init(TextGeneratorViewModel parent)
		{
			Parent = parent;
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

			InputData.Init(this, OnInputTextChanged, OnDataFileLoaded);
			OutputData.Init(this, OnOutputTextChanged, OnDataFileLoaded);

			string initialDirectory = AppController.Main.CurrentModule != null ? DefaultPaths.ModuleTextGeneratorFolder(AppController.Main.CurrentModule.ModuleData) : DefaultPaths.RootFolder + @"Default\TextGenerator\";

			RxApp.MainThreadScheduler.Schedule(TimeSpan.FromMilliseconds(50), _ =>
			{
				if (string.IsNullOrWhiteSpace(InputData.DefaultFileName) || !InputData.Open())
				{
					InputData.InitialDirectory = initialDirectory;
					InputData.DefaultFileName = "TextGenerator_Input1.txt";

					if (!FileCommands.IsValidFilePath(InputData.FilePath))
					{
						InputData.FilePath = Path.Combine(InputData.InitialDirectory, InputData.DefaultFileName);
					}

					if (string.IsNullOrEmpty(InputData.Content)) InputData.Open();
				}
				if (string.IsNullOrWhiteSpace(OutputData.DefaultFileName) || !OutputData.Open())
				{
					OutputData.InitialDirectory = initialDirectory;
					OutputData.DefaultFileName = "TextGenerator_Output1.txt";
					if (!FileCommands.IsValidFilePath(OutputData.FilePath))
					{
						OutputData.FilePath = Path.Combine(OutputData.InitialDirectory, OutputData.DefaultFileName);
					}
					if(string.IsNullOrEmpty(OutputData.Content)) OutputData.Open();
				}
			});


			if((int)this.NextKeywordType > 2)
			{
				this.NextKeywordType = TextGeneratorInputType.Incremental;
			}
		}

		public TextGeneratorData()
		{
			Keywords = new ObservableCollection<ITextGeneratorInputData>();
		}
	}

	[DataContract]
	public class TextGeneratorSaveFileData : ReactiveObject, ISaveCommandData
	{
		public TextGeneratorData Parent { get; private set; }

		private string defaultFileName;

		public string DefaultFileName
		{
			get { return defaultFileName; }
			set
			{
				this.RaiseAndSetIfChanged(ref defaultFileName, value);
				this.RaisePropertyChanged("SaveParameters");
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
				this.RaisePropertyChanged("SaveParameters");
			}
		}

		private string defaultFilePath;

		public string InitialDirectory
		{
			get { return defaultFilePath; }
			set
			{
				this.RaiseAndSetIfChanged(ref defaultFilePath, value);
				this.RaisePropertyChanged("SaveParameters");
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

		public Window TargetWindow { get; set; }

		public Action OnContentChanged;
		public Action OnFileLoaded;

		public bool Open()
		{
			if (File.Exists(FilePath))
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
			if (success)
			{
				Parent.Parent.FooterText = $"Saved preset text to '{FilePath}'.";
			}
			else;
			{
				Parent.Parent.FooterText = $"Failed to save preset text to '{FilePath}'.";
			}
		}

		private void OnSaveAs(bool success, string path)
		{
			if (success)
			{
				FilePath = path;
				DefaultFileName = Path.GetFileName(FilePath);
				InitialDirectory = Directory.GetParent(FilePath).FullName;
				Parent.Parent.FooterText = $"Saved preset text to '{FilePath}'.";
			}
			else
			{
				Parent.Parent.FooterText = $"Failed to save preset text to '{FilePath}'.";
			}
		}

		public void Init(TextGeneratorData parent, Action OnContentChangedAction, Action OnFileLoadedAction = null)
		{
			Parent = parent;
			OnContentChanged = OnContentChangedAction;

			if (OnFileLoadedAction != null) OnFileLoaded = OnFileLoadedAction;

			OpenCommand = new ParameterCommand((object param) =>
			{
				if (param is string path)
				{
					FilePath = path;
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
			get { return numberPadding; }
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

			int finalPadding = NumberPadding > 0 ? NumberPadding + 1 : 0;
			string finalResult = result.ToString($"D{finalPadding}");

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
