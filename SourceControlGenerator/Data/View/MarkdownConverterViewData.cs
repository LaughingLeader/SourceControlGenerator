using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using Alphaleonis.Win32.Filesystem;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using SCG.Commands;
using SCG.Core;
using SCG.Interfaces;
using SCG.Markdown;
using SCG.Windows;
using Newtonsoft.Json;
using ReactiveUI;

namespace SCG.Data.View
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

	[DataContract]
	public class MarkdownConverterViewData : ReactiveObject
	{
		private Window parentWindow;

		private string fileContents = "";

		private string input = "";

		public string Input
		{
			get { return input; }
			set
			{
				CanSaveInput = input != value && value != fileContents;
				this.RaiseAndSetIfChanged(ref input, value);
				this.RaisePropertyChanged("CanPreview");
			}
		}

		private bool canSaveInput = false;

		public bool CanSaveInput
		{
			get { return canSaveInput; }
			set
			{
				this.RaiseAndSetIfChanged(ref canSaveInput, value);
			}
		}

		private string output = "";

		public string Output
		{
			get { return output; }
			set
			{
				this.RaiseAndSetIfChanged(ref output, value);
			}
		}

		private TextWrapping textWrapMode = TextWrapping.Wrap;

		[DataMember]
		public TextWrapping TextWrapMode
		{
			get { return textWrapMode; }
			set
			{
				this.RaiseAndSetIfChanged(ref textWrapMode, value);
			}
		}

		private MarkdownInputType inputType = MarkdownInputType.Markdown;

		[DataMember]
		public MarkdownInputType InputType
		{
			get { return inputType; }
			set
			{
				this.RaiseAndSetIfChanged(ref inputType, value);
			}
		}

		private MarkdownConverterMode mode = MarkdownConverterMode.Single;

		[DataMember]
		public MarkdownConverterMode Mode
		{
			get { return mode; }
			set
			{
				this.RaiseAndSetIfChanged(ref mode, value);
				this.RaisePropertyChanged("SingleMode");
				this.RaisePropertyChanged("BatchMode");
			}
		}

		public Visibility SingleMode
		{
			get => mode == MarkdownConverterMode.Single ? Visibility.Visible : Visibility.Collapsed;
		}

		public Visibility BatchMode
		{
			get => mode == MarkdownConverterMode.Batch ? Visibility.Visible : Visibility.Collapsed;
		}

		public ObservableCollection<IMarkdownFormatter> Formatters { get; set; }

		public ObservableCollection<MenuData> TopMenus { get; set; }

		public bool CanPreview
		{
			get
			{
				return !String.IsNullOrEmpty(Input);
			}
		}

		public ICommand NextModeCommand { get; set; }

		public ICommand OpenInputFileCommand { get; set; }

		public ICommand SaveInputCommand { get; set; }

		public ICommand SaveInputAsCommand { get; set; }

		public ICommand SaveInputCopyAsCommand { get; set; }

		#region Single Mode
		private IMarkdownFormatter selectedFormatter;

		public IMarkdownFormatter SelectedFormatter
		{
			get { return selectedFormatter; }
			set
			{
				this.RaiseAndSetIfChanged(ref selectedFormatter, value);
				SelectedFormatterName = selectedFormatter.Name;
				SingleModeDefaultFileName = SelectedFormatterName.TrimWhitespace() + ".txt";
			}
		}

		private string selectedFormatterName = "";

		[DataMember]
		public string SelectedFormatterName
		{
			get { return selectedFormatterName; }
			set
			{
				this.RaiseAndSetIfChanged(ref selectedFormatterName, value);
			}
		}

		private string singleModeLastFileOutputPath = "";

		[DataMember]
		public string SingleModeLastFileExportPath
		{
			get { return singleModeLastFileOutputPath; }
			set
			{
				this.RaiseAndSetIfChanged(ref singleModeLastFileOutputPath, value);
				this.RaisePropertyChanged("CanSave");
			}
		}

		private string singleModeLastFileInputPath = "";

		[DataMember]
		public string SingleModeLastFileInputPath
		{
			get { return singleModeLastFileInputPath; }
			set
			{
				this.RaiseAndSetIfChanged(ref singleModeLastFileInputPath, value);
			}
		}


		private string singleModeDefaultFileName = "";

		public string SingleModeDefaultFileName
		{
			get { return singleModeDefaultFileName; }
			set
			{
				this.RaiseAndSetIfChanged(ref singleModeDefaultFileName, value);
			}
		}


		public bool CanExport
		{
			get
			{
				return !CanPreview && FileCommands.IsValidFilePath(SingleModeLastFileExportPath);
			}
		}

		public ICommand PreviewSingleCommand { get; set; }

		public ICommand ExportSingleCommand { get; set; }

		public ICommand ExportSingleAsCommand { get; set; }

		public void ExportSingle()
		{
			PreviewSelected();

			if(!String.IsNullOrEmpty(Output))
			{
				if (FileCommands.IsValidFilePath(SingleModeLastFileExportPath))
				{
					if(FileCommands.WriteToFile(SingleModeLastFileExportPath, Output))
					{
						Log.Here().Activity($"Converted text to {SelectedFormatter.Name} and saved to {SingleModeLastFileExportPath}.");
						StartSavingAsync();
					}
				}
			}
		}

		public void ExportSingleAs()
		{
			PreviewSelected();

			FileCommands.Save.OpenDialogAndSave(parentWindow, "Export Output As...", SingleModeLastFileExportPath, Output, (bool success, string filePath) =>
			{
				if (success)
				{
					SingleModeLastFileExportPath = Path.GetFullPath(filePath);
					Log.Here().Activity($"Converted text to {SelectedFormatter.Name} and saved to {SingleModeLastFileExportPath}.");
					StartSavingAsync();
				}
			}, Path.GetFileName(SingleModeLastFileExportPath), "", CommonFileFilters.MarkdownConverterFilesList.ToArray());
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

		public List<string> SelectedBatchFormatters
		{
			get
			{
				if (BatchFormatterData != null) return BatchFormatterData.Where(f => f.Enabled).Select(f => f.Name).ToList();
				return null;
			}
		}

		public bool CanBatchExport
		{
			get
			{
				return BatchFormatterData != null && BatchFormatterData.Any(f => f.Enabled == true);
			}
		}


		public ICommand BatchExportCommand { get; set; }

		public ICommand PreviewCommand { get; set; }

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
				await Task.Delay(250).ConfigureAwait(false);
				await Save().ConfigureAwait(false);
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
			await Save().ConfigureAwait(false);
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
						//Log.Here().Activity($"Saved markdown converter settings file for current module {AppController.Main.CurrentModule.ModuleData.ModuleName}.");
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

		public void LoadInputFile(object value)
		{
			if(value is string path)
			{
				if(!String.IsNullOrEmpty(path))
				{
					fileContents = FileCommands.ReadFile(path);
					Input = fileContents;

					this.SingleModeLastFileInputPath = path;
					CanSaveInput = false;
					StartSavingAsync();

					Output = String.Empty;
				}
			}
		}

		public void SetBatchExportRoot(string path)
		{
			if (!Directory.Exists(path)) Directory.CreateDirectory(path);

			if(Directory.Exists(path))
			{
				foreach(var batchData in BatchFormatterData)
				{
					batchData.FilePath = Path.Combine(path, batchData.Formatter.Name.TrimWhitespace() + batchData.DefaultFileExtension);
				}
			}
		}

		public void SaveInputFile()
		{
			if(CanSaveInput && FileCommands.IsValidFilePath(SingleModeLastFileInputPath))
			{
				FileCommands.WriteToFile(SingleModeLastFileInputPath, Input, false);
				fileContents = Input;
				CanSaveInput = false;
			}
		}

		public void SaveInputAsFile()
		{
			FileCommands.Save.OpenDialogAndSave(parentWindow, "Save Input As...", SingleModeLastFileInputPath, Input, (bool success, string filePath) =>
			{
				if (success)
				{
					SingleModeLastFileInputPath = Path.GetFullPath(filePath);
					fileContents = Input;
					CanSaveInput = false;
				}
			}, Path.GetFileName(SingleModeLastFileInputPath), "", CommonFileFilters.MarkdownConverterFilesList.ToArray());
		}

		public void SaveInputAsCopy()
		{
			FileCommands.Save.OpenDialogAndSave(parentWindow, "Save Input Copy As...", SingleModeLastFileInputPath, Input, null, Path.GetFileName(SingleModeLastFileInputPath), "", CommonFileFilters.MarkdownConverterFilesList.ToArray());
		}

		public MarkdownConverterViewData()
		{
			Formatters = new ObservableCollection<IMarkdownFormatter>(MarkdownConverter.InitFormatters());
			OpenInputFileCommand = new ParameterCommand(LoadInputFile);
			PreviewSingleCommand = new ActionCommand(PreviewSelected);
			PreviewCommand = new ParameterCommand(ConvertInputForData);
			NextModeCommand = new ActionCommand(NextMode);
			ExportSingleCommand = new ActionCommand(ExportSingle);
			ExportSingleAsCommand = new ActionCommand(ExportSingleAs);
			BatchExportCommand = new ActionCommand(ExportBatch);
			SaveInputCommand = new ActionCommand(SaveInputFile);
			SaveInputAsCommand = new ActionCommand(SaveInputAsFile);
			SaveInputCopyAsCommand = new ActionCommand(SaveInputAsCopy);
		}

		public void InitSettings(Window parentWindow, ICommand fileBrowserOpenFileCommand)
		{
			this.parentWindow = parentWindow;

			TopMenus = new ObservableCollection<MenuData>()
			{
				new MenuData("MD.File", "File").Add(
					new MenuData("MD.File.Load", "Open Input File...", fileBrowserOpenFileCommand, Key.O, ModifierKeys.Control),
					new MenuData("MD.File.Save", "Save Input File", SaveInputCommand, Key.S, ModifierKeys.Control),
					new MenuData("MD.File.SaveAs", "Save Input File As...", SaveInputAsCommand, Key.S, ModifierKeys.Control | ModifierKeys.Alt),
					new MenuData("MD.File.SaveAs", "Save a Copy of Input As...", SaveInputCopyAsCommand)
				),
				new MenuData("MD.Mode", "Mode").Add(
					new MenuData("MD.Mode.NextMode", "Next Mode", NextModeCommand, Key.Tab, ModifierKeys.Shift),
					new SeparatorData(),
					new MenuData("MD.Mode.Single", "Single Mode", new ActionCommand(() => { this.Mode = MarkdownConverterMode.Single; }), Key.D1, ModifierKeys.Control),
					new MenuData("MD.Mode.Batch", "Batch Mode", new ActionCommand(() => { this.Mode = MarkdownConverterMode.Batch; }), Key.D2, ModifierKeys.Control)
				),
				new MenuData("MD.Export", "Export").Add(
					new MenuData("MD.Export.SingleSeparatorHeader", "Single").Add(
						new MenuData("MD.Export.PreviewSingle", "Preview Output", PreviewSingleCommand),
						new MenuData("MD.Export.ExportSingle", "Export", ExportSingleCommand),
						new MenuData("MD.Export.ExportSingleAs", "Export As...", ExportSingleAsCommand)
					),
					new SeparatorData(),
					new MenuData("MD.Export.BatchSeparatorHeader", "Batch").Add(new MenuData("MD.Export.ExportBatch", "Export Selected", BatchExportCommand))
				)
			};

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
			this.RaisePropertyChanged("BatchFormatterData");
		}
	}
}
