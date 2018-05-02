using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using LL.SCG.Commands;
using LL.SCG.Core;
using LL.SCG.Interfaces;
using LL.SCG.Markdown;
using Newtonsoft.Json;

namespace LL.SCG.Data.View
{
	public enum MarkdownConverterMode
	{
		[Description("Single Conversion")]
		Single,
		[Description("Batch Conversion")]
		Batch
	}

	public enum MarkdownInputType
	{
		Markdown = 0,
		HTML
	}

	public class MarkdownConverterModeToColumnWidthConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (parameter is MarkdownConverterMode targetMode)
			{
				if (value is MarkdownConverterMode mode && mode == targetMode)
				{
					return new GridLength(1, GridUnitType.Star);
					//return "*";
				}
			}
			return GridLength.Auto;
			//return "Auto";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}

	public class MarkdownConverterViewData : PropertyChangedBase
	{
		private string input = "";

		public string Input
		{
			get { return input; }
			set
			{
				input = value;
				RaisePropertyChanged("Input");
				RaisePropertyChanged("CanPreview");
			}
		}

		private string output = "";

		public string Output
		{
			get { return output; }
			set
			{
				output = value;
				RaisePropertyChanged("Output");
			}
		}

		private TextWrapping textWrapMode = TextWrapping.Wrap;

		public TextWrapping TextWrapMode
		{
			get { return textWrapMode; }
			set
			{
				textWrapMode = value;
				RaisePropertyChanged("TextWrapMode");
			}
		}

		private MarkdownInputType inputType = MarkdownInputType.Markdown;

		public MarkdownInputType InputType
		{
			get { return inputType; }
			set
			{
				inputType = value;
				RaisePropertyChanged("InputType");
			}
		}

		private MarkdownConverterMode mode = MarkdownConverterMode.Single;

		public MarkdownConverterMode Mode
		{
			get { return mode; }
			set
			{
				mode = value;
				RaisePropertyChanged("Mode");
				RaisePropertyChanged("SingleMode");
				RaisePropertyChanged("BatchMode");
			}
		}

		[JsonIgnore]
		public Visibility SingleMode
		{
			get => mode == MarkdownConverterMode.Single ? Visibility.Visible : Visibility.Collapsed;
		}

		[JsonIgnore]
		public Visibility BatchMode
		{
			get => mode == MarkdownConverterMode.Batch ? Visibility.Visible : Visibility.Collapsed;
		}

		[JsonIgnore]
		public ObservableCollection<IMarkdownFormatter> Formatters { get; set; }

		[JsonIgnore]
		public bool CanPreview
		{
			get
			{
				return !String.IsNullOrEmpty(Input);
			}
		}

		[JsonIgnore] public ICommand NextModeCommand { get; set; }

		[JsonIgnore] public ICommand OpenInputFileCommand { get; set; }

		#region Single Mode
		private IMarkdownFormatter selectedFormatter;

		[JsonIgnore]
		public IMarkdownFormatter SelectedFormatter
		{
			get { return selectedFormatter; }
			set
			{
				selectedFormatter = value;
				RaisePropertyChanged("SelectedFormatter");
				SelectedFormatterName = selectedFormatter.Name;
				SingleModeDefaultFileName = SelectedFormatterName.TrimWhitespace() + ".txt";
			}
		}

		private string selectedFormatterName = "";

		public string SelectedFormatterName
		{
			get { return selectedFormatterName; }
			set
			{
				selectedFormatterName = value;
				RaisePropertyChanged("SelectedFormatterName");
			}
		}

		private string singleModeLastFileOutputPath = "";

		public string SingleModeLastFileExportPath
		{
			get { return singleModeLastFileOutputPath; }
			set
			{
				singleModeLastFileOutputPath = value;
				RaisePropertyChanged("SingleModeLastFileExportPath");
				RaisePropertyChanged("CanSave");
			}
		}

		private string singleModeLastFileInputPath = "";

		public string SingleModeLastFileInputPath
		{
			get { return singleModeLastFileInputPath; }
			set
			{
				singleModeLastFileInputPath = value;
				RaisePropertyChanged("SingleModeLastFileInputPath");
			}
		}


		private string singleModeDefaultFileName = "";

		[JsonIgnore]
		public string SingleModeDefaultFileName
		{
			get { return singleModeDefaultFileName; }
			set
			{
				singleModeDefaultFileName = value;
				RaisePropertyChanged("SingleModeDefaultFileName");
			}
		}


		[JsonIgnore]
		public bool CanSave
		{
			get
			{
				return !CanPreview && FileCommands.IsValidFilePath(SingleModeLastFileExportPath);
			}
		}

		[JsonIgnore] public ICommand PreviewSingleCommand { get; set; }

		[JsonIgnore] public ICommand ExportSingleCommand { get; set; }

		public void ExportSingle()
		{
			PreviewSelected();

			if(!String.IsNullOrEmpty(Output))
			{
				if (FileCommands.IsValidFilePath(SingleModeLastFileExportPath))
				{
					FileCommands.WriteToFile(SingleModeLastFileExportPath, Output);
					Log.Here().Activity($"Converted text to {SelectedFormatter.Name} and saved to {SingleModeLastFileExportPath}.");
					StartSavingAsync();
				}
			}
		}

		public void PreviewSelected()
		{
			if (SelectedFormatter != null && !String.IsNullOrWhiteSpace(Input))
			{
				if (InputType == MarkdownInputType.Markdown)
				{
					string html = MarkdownConverter.ConvertMarkdownToHTML(Input);
					Output = SelectedFormatter.ConvertHTML(html);
				}
				else if (InputType == MarkdownInputType.HTML)
				{
					Output = SelectedFormatter.ConvertHTML(Input);
				}
			}
		}

		#endregion

		#region Batch Mode
		public List<MarkdownFormatterData> BatchFormatterData { get; set; }

		[JsonIgnore]
		public List<string> SelectedBatchFormatters
		{
			get
			{
				if (BatchFormatterData != null) return BatchFormatterData.Where(f => f.Enabled).Select(f => f.Name).ToList();
				return null;
			}
		}

		[JsonIgnore] public ICommand BatchExportCommand { get; set; }

		[JsonIgnore] public ICommand PreviewCommand { get; set; }

		public void ExportBatch()
		{
			if (!String.IsNullOrWhiteSpace(Input))
			{
				var formatters = BatchFormatterData.Where(f => f.Enabled);

				foreach (var formatterData in formatters)
				{
					if (FileCommands.IsValidFilePath(formatterData.FilePath))
					{
						string output = "";
						if (InputType == MarkdownInputType.Markdown)
						{
							string html = MarkdownConverter.ConvertMarkdownToHTML(Input);
							output = formatterData.Formatter.ConvertHTML(html);
						}
						else if (InputType == MarkdownInputType.HTML)
						{
							output = formatterData.Formatter.ConvertHTML(Input);
						}

						FileCommands.WriteToFile(formatterData.FilePath, output);
						Log.Here().Activity($"Converted text to {formatterData.Name} and saved to {formatterData.FilePath}.");
					}
				}
			}
		}

		#endregion

		private void ConvertInputForData(object value)
		{
			if (value is IMarkdownFormatter formatter)
			{
				if (formatter != null && !String.IsNullOrWhiteSpace(Input))
				{
					if (InputType == MarkdownInputType.Markdown)
					{
						string html = MarkdownConverter.ConvertMarkdownToHTML(Input);
						Output = formatter.ConvertHTML(html);
					}
					else if (InputType == MarkdownInputType.HTML)
					{
						Output = formatter.ConvertHTML(Input);
					}
				}
			}
		}

		public void NextMode()
		{
			Mode = Mode == MarkdownConverterMode.Single ? MarkdownConverterMode.Batch : MarkdownConverterMode.Single;
		}

		private bool savingSettings = false;
		//private Timer saveDelayTimer;

		public async void StartSavingAsync()
		{
			if (!savingSettings)
			{
				savingSettings = true;
				//if (saveDelayTimer == null) saveDelayTimer = new Timer(_ => OnSaveTimerComplete());
				//saveDelayTimer.Change(250, Timeout.Infinite);
				await Task.Delay(250);
				await Save();
				savingSettings = false;
			}
			else
			{
				/*
				if(saveDelayTimer != null)
				{
					//Reset
					//saveDelayTimer.Change(250, Timeout.Infinite);
				}
				*/
			}
		}

		private async void OnSaveTimerComplete()
		{
			//saveDelayTimer.Change(Timeout.Infinite, Timeout.Infinite);
			await Save();
			savingSettings = false;
		}

		public Task<bool> Save()
		{
			if(AppController.Main != null)
			{
				if(AppController.Main.CurrentModule != null && AppController.Main.CurrentModule.ModuleData != null)
				{
					var outputPath = DefaultPaths.ModuleMarkdownConverterSettingsFile(AppController.Main.CurrentModule.ModuleData);
					var outputText = JsonConvert.SerializeObject(this, Formatting.Indented);
					if(FileCommands.WriteToFile(outputPath, outputText, true))
					{
						Log.Here().Activity($"Saved markdown converter settings file for current module {AppController.Main.CurrentModule.ModuleData.ModuleName}.");
						return Task.FromResult<bool>(true);
					}
				}
				else
				{
					Log.Here().Error($"Could not save markdown converter settings file for current module: ModuleData is null.");
				}
			}
			return Task.FromResult<bool>(false);
		}

		private void LoadInputFile(object value)
		{
			if(value is string path)
			{
				Input = FileCommands.ReadFile(path);
				if(!String.IsNullOrEmpty(path))
				{
					StartSavingAsync();
				}
			}
		}

		public MarkdownConverterViewData()
		{
			Formatters = new ObservableCollection<IMarkdownFormatter>(MarkdownConverter.InitFormatters());
			OpenInputFileCommand = new ParameterCommand(LoadInputFile);
			PreviewSingleCommand = new ActionCommand(PreviewSelected);
			PreviewCommand = new ParameterCommand(ConvertInputForData);
			NextModeCommand = new ActionCommand(NextMode);
			ExportSingleCommand = new ActionCommand(ExportSingle);
			BatchExportCommand = new ActionCommand(ExportBatch);
		}

		public void InitSettings()
		{
			if (String.IsNullOrEmpty(SelectedFormatterName))
			{
				SelectedFormatter = Formatters.First();
			}
			else
			{
				var nextFormatter = Formatters.Where(f => f.Name == SelectedFormatterName).FirstOrDefault();
				if (nextFormatter != null)
				{
					SelectedFormatter = nextFormatter;
				}
				else
				{
					SelectedFormatter = Formatters.First();
				}
			}

			List<MarkdownFormatterData> previousBatchData = null;

			if (BatchFormatterData != null)
			{
				previousBatchData = new List<MarkdownFormatterData>(BatchFormatterData);
				/*
				foreach (var data in BatchFormatterData)
				{
					previousBatchData.Add(new MarkdownFormatterData(this)
					{
						Name = data.Name,
						Enabled = data.Enabled,
						FilePath = data.FilePath,
					});
				}
				*/
			}

			BatchFormatterData = new List<MarkdownFormatterData>();

			var startFilePath = "";

			if (AppController.Main != null)
			{
				if (AppController.Main.CurrentModule != null && AppController.Main.CurrentModule.ModuleData != null)
				{
					startFilePath = DefaultPaths.ModuleExportFolder(AppController.Main.CurrentModule.ModuleData);
					Directory.CreateDirectory(startFilePath);

					if (String.IsNullOrWhiteSpace(SingleModeLastFileExportPath))
					{
						SingleModeLastFileExportPath = startFilePath;
					}

					if (String.IsNullOrWhiteSpace(SingleModeLastFileInputPath))
					{
						SingleModeLastFileExportPath = DefaultPaths.ModuleRootFolder(AppController.Main.CurrentModule.ModuleData);
					}
				}
			}

			if (String.IsNullOrEmpty(startFilePath)) startFilePath = DefaultPaths.RootFolder;

			if (String.IsNullOrWhiteSpace(SingleModeLastFileExportPath))
			{
				SingleModeLastFileExportPath = startFilePath;
			}

			if (String.IsNullOrWhiteSpace(SingleModeLastFileInputPath))
			{
				SingleModeLastFileExportPath = startFilePath;
			}

			foreach (var formatter in Formatters)
			{
				var previousData = previousBatchData != null ? previousBatchData.Where(f => f.Name == formatter.Name).FirstOrDefault() : null;

				//if(previousData != null) Log.Here().Activity($"Previous data {previousData.Enabled} | {previousData.FilePath}");

				var enabled = previousData != null ? previousData.Enabled : false;
				var filePath = previousData != null && !String.IsNullOrEmpty(previousData.FilePath) ? previousData.FilePath : "";

				if(!FileCommands.IsValidFilePath(filePath) && FileCommands.IsValidDirectoryPath(filePath))
				{
					filePath = Path.Combine(filePath, formatter.Name.TrimWhitespace());
				}
				
				BatchFormatterData.Add(new MarkdownFormatterData(this)
				{
					Name = formatter.Name,
					Formatter = formatter,
					Enabled = enabled,
					FilePath = filePath,
					LastPath = !String.IsNullOrEmpty(filePath) ? filePath : startFilePath,
					DefaultFileName = formatter.Name.TrimWhitespace()
				});
			}
			RaisePropertyChanged("BatchFormatterData");
		}
	}
}
