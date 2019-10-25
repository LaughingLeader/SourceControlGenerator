using System;
using System.Collections.Generic;
using System.ComponentModel;
using Alphaleonis.Win32.Filesystem;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

using SCG.SCGEnum;
using System.Windows.Input;
using SCG.Core;
using Microsoft.WindowsAPICodePack.Dialogs;
using SCG.Data.View;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Threading;

namespace SCG.Controls
{
	public enum FileBrowserMode
	{
		Open,
		Save
	}

	/// <summary>
	/// Interaction logic for FileBrowseControl.xaml
	/// </summary>
	public partial class FileBrowseControl : UserControl
	{
		public string OpenFileText
		{
			get { return (string)GetValue(BrowseTextProperty); }
			set
			{
				SetValue(BrowseTextProperty, value);
			}
		}

		public string FileLocationText
		{
			get { return (string)GetValue(FileLocationTextProperty); }
			set
			{
				SetValue(FileLocationTextProperty, value);
			}
		}

		public string LastFileLocation
		{
			get { return (string)GetValue(LastFileLocationProperty); }
			set
			{
				SetValue(LastFileLocationProperty, value);
			}
		}

		public string DefaultExt
		{
			get { return (string)GetValue(DefaultExtProperty); }
			set
			{
				SetValue(DefaultExtProperty, value);
			}
		}

		public FileBrowseType BrowseType
		{
			get { return (FileBrowseType)GetValue(FileBrowseTypeProperty); }
			set
			{
				SetValue(FileBrowseTypeProperty, value);
			}
		}

		public List<FileBrowserFilter> Filters
		{
			get { return (List<FileBrowserFilter>)GetValue(FiltersProperty); }
			set { SetValue(FiltersProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Filters.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty FiltersProperty =
			DependencyProperty.Register("Filters", typeof(List<FileBrowserFilter>), typeof(FileBrowseControl), new PropertyMetadata(null));

		public static readonly DependencyProperty BrowseTextProperty =
			DependencyProperty.Register("OpenFileText", typeof(string),
			typeof(FileBrowseControl), new PropertyMetadata(""));

		public static readonly DependencyProperty FileLocationTextProperty =
			DependencyProperty.Register("FileLocationText", typeof(string),
			typeof(FileBrowseControl), new PropertyMetadata(OnFileLocationChangedCallback));

		private static void OnFileLocationChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			FileBrowseControl fileBrowseControl = sender as FileBrowseControl;
			if (fileBrowseControl != null)
			{
				fileBrowseControl.OnFileLocationChanged();
			}
		}

		public static readonly DependencyProperty LastFileLocationProperty =
			DependencyProperty.Register("LastFileLocation", typeof(string),
			typeof(FileBrowseControl), new PropertyMetadata(""));

		public static readonly DependencyProperty DefaultExtProperty =
			DependencyProperty.Register("DefaultExt", typeof(string),
			typeof(FileBrowseControl), new PropertyMetadata("*"));

		public static readonly DependencyProperty FileBrowseTypeProperty =
			DependencyProperty.Register("BrowseType", typeof(FileBrowseType),
			typeof(FileBrowseControl), new FrameworkPropertyMetadata(FileBrowseType.File, FrameworkPropertyMetadataOptions.AffectsRender, OnBrowseTypeChangedCallback));

		public static void OnBrowseTypeChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			FileBrowseControl fileBrowseControl = sender as FileBrowseControl;
			if (fileBrowseControl != null)
			{
				fileBrowseControl.BrowseType = (FileBrowseType)e.NewValue;
				fileBrowseControl.OnFileLocationChanged();
			}
		}

		public FileValidation FileValidation
		{
			get { return (FileValidation)GetValue(FileValidationProperty); }
			set { SetValue(FileValidationProperty, value); }
		}

		public static readonly DependencyProperty FileValidationProperty =
			DependencyProperty.Register("FileValidation", typeof(FileValidation), typeof(FileBrowseControl), new PropertyMetadata(FileValidation.None));

		public ICommand OnOpen
		{
			get { return (ICommand)GetValue(OnOpenProperty); }
			set { SetValue(OnOpenProperty, value); }
		}

		public static readonly DependencyProperty OnOpenProperty =
			DependencyProperty.Register("OnOpen", typeof(ICommand), typeof(FileBrowseControl), new PropertyMetadata(null));



		public FileBrowserMode BrowseMode
		{
			get { return (FileBrowserMode)GetValue(BrowseModeProperty); }
			set { SetValue(BrowseModeProperty, value); }
		}

		// Using a DependencyProperty as the backing store for BrowseMode.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty BrowseModeProperty =
			DependencyProperty.Register("BrowseMode", typeof(FileBrowserMode), typeof(FileBrowseControl), new PropertyMetadata(FileBrowserMode.Open));

		public string DefaultFileName
		{
			get { return (string)GetValue(DefaultFileNameProperty); }
			set { SetValue(DefaultFileNameProperty, value); }
		}

		// Using a DependencyProperty as the backing store for DefaultFileName.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty DefaultFileNameProperty =
			DependencyProperty.Register("DefaultFileName", typeof(string), typeof(FileBrowseControl), new PropertyMetadata(""));

		public string BrowseToolTip
		{
			get { return (string)GetValue(BrowseToolTipProperty); }
			set { SetValue(BrowseToolTipProperty, value); }
		}

		// Using a DependencyProperty as the backing store for BrowseToolTip.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty BrowseToolTipProperty =
			DependencyProperty.Register("BrowseToolTip", typeof(string), typeof(FileBrowseControl), new PropertyMetadata("Browse..."));

		public int CircleSize
		{
			get { return (int)GetValue(CircleSizeProperty); }
			set { SetValue(CircleSizeProperty, value); }
		}

		public static readonly DependencyProperty CircleSizeProperty =
			DependencyProperty.Register("CircleSize", typeof(int), typeof(FileBrowseControl), new PropertyMetadata(4));

		public FileBrowseControl()
		{
			InitializeComponent();

			Loaded += FileBrowseControl_Loaded;
			SizeChanged += FileBrowseControl_Loaded;
			//GotFocus += FileBrowseControl_Loaded;
			//this.DataContext = this;
		}

		private void FileBrowseControl_Loaded(object sender, RoutedEventArgs e)
		{
			if (Filters == null) Filters = CommonFileFilters.DefaultFilters;

			AutoScrollTextbox();
		}

		private void FileBrowseButton_Click(object sender, RoutedEventArgs e)
		{
			StartBrowse();
		}
		
		public void StartBrowse()
		{
			Window parentWindow = Window.GetWindow(this);

			//Log.Here().Activity($"LastFileLocation is {LastFileLocation} FileLocationText: {FileLocationText}");

			if (!String.IsNullOrEmpty(FileLocationText) && FileCommands.IsValidPath(FileLocationText))
			{
				if (BrowseType == FileBrowseType.File)
				{
					if (FileCommands.IsValidFilePath(FileLocationText))
					{
						var parentDirectory = new DirectoryInfo(FileLocationText);
						if (parentDirectory != null)
						{
							LastFileLocation = parentDirectory.Parent.FullName;
						}
					}
					else
					{
						LastFileLocation = FileLocationText;
					}
				}
				else
				{
					var directory = new DirectoryInfo(FileLocationText);
					if (directory != null)
					{
						LastFileLocation = directory.FullName;
					}
				}
			}

			if (!FileCommands.IsValidPath(LastFileLocation))
			{
				if (AppController.Main.CurrentModule != null && AppController.Main.CurrentModule.ModuleData != null)
				{
					LastFileLocation = DefaultPaths.ModuleRootFolder(AppController.Main.CurrentModule.ModuleData);
				}
				else
				{
					LastFileLocation = DefaultPaths.RootFolder;
				}
				// Path.GetFullPath(Assembly.GetExecutingAssembly().Location);
			}

			if (BrowseType == FileBrowseType.File)
			{
				CommonFileDialog fileDialog = null;

				if (BrowseMode == FileBrowserMode.Open)
				{
					var openFileDialog = new CommonOpenFileDialog();
					openFileDialog.Multiselect = false;
					fileDialog = openFileDialog;
				}
				else if(BrowseMode == FileBrowserMode.Save)
				{
					var saveFileDialog = new CommonSaveFileDialog();
					saveFileDialog.AlwaysAppendDefaultExtension = false;
					saveFileDialog.OverwritePrompt = true;
					fileDialog = saveFileDialog;
				}

				if(fileDialog != null)
				{
					fileDialog.Title = OpenFileText;
					fileDialog.InitialDirectory = LastFileLocation;
					string fileName = Path.GetFileName(LastFileLocation);
					
					//if (!String.IsNullOrWhiteSpace(DefaultExt)) fileDialog.DefaultExtension = DefaultExt;

					if (Filters != null)
					{
						if(Filters.Count <= 0)
						{
							Filters.Add(CommonFileFilters.All);
						}

						foreach (var filter in Filters)
						{
							fileDialog.Filters.Add(new CommonFileDialogFilter(filter.Name, filter.Values));
						}
					}
					else
					{
						fileDialog.Filters.Add(new CommonFileDialogFilter("All Files", "*.*"));
					}

					if (FileCommands.IsValidFilePath(FileLocationText))
					{
						fileDialog.DefaultFileName = Path.GetFileName(FileLocationText);
					}
					else if (!String.IsNullOrWhiteSpace(DefaultFileName))
					{
						if (!String.IsNullOrWhiteSpace(DefaultExt))
						{
							fileDialog.DefaultFileName = DefaultFileName + DefaultExt;
						}
						else
						{
							fileDialog.DefaultFileName = DefaultFileName;
						}
					}
					//Log.Here().Activity($"DefaultFileName: {fileDialog.DefaultFileName}");

					var result = fileDialog.ShowDialog(parentWindow);

					if (result == CommonFileDialogResult.Ok)
					{
						string filename = fileDialog.FileName;

						if(fileDialog is CommonOpenFileDialog openFileDialog)
						{
							filename = String.Join(";", openFileDialog.FileNames);
						}
						else if(fileDialog is CommonSaveFileDialog saveFileDialog)
						{
							filename = saveFileDialog.FileName;
						}

						if (FileCommands.PathIsRelative(filename))
						{
							filename = Common.Functions.GetRelativePath.RelativePathGetter.Relative(Directory.GetCurrentDirectory(), fileDialog.FileName);
						}
						else
						{
							if(!FileCommands.IsValidFilePath(filename))
							{
								filename = filename.CleanFileName();
								filename = Path.GetFullPath(filename);
							}							
						}

						FileLocationText = filename;
						LastFileLocation = Path.GetDirectoryName(FileLocationText);
						OnOpen?.Execute(FileLocationText);
					}
				}
			}
			else if (BrowseType == FileBrowseType.Directory)
			{
				/*
				VistaFolderBrowserDialog folderDialog = new VistaFolderBrowserDialog();
				folderDialog.SelectedPath = LastFileLocation;
				folderDialog.Description = OpenFileText;
				folderDialog.UseDescriptionForTitle = true;
				folderDialog.ShowNewFolderButton = true;		

				Nullable<bool> result = folderDialog.ShowDialog(parentWindow);
				*/

				CommonFileDialog fileDialog = null;

				if (BrowseMode == FileBrowserMode.Open)
				{
					var openFileDialog = new CommonOpenFileDialog();
					openFileDialog.Multiselect = false;
					openFileDialog.AllowNonFileSystemItems = true;
					openFileDialog.IsFolderPicker = true;

					fileDialog = openFileDialog;
				}
				else if (BrowseMode == FileBrowserMode.Save)
				{
					fileDialog = new CommonSaveFileDialog();
				}

				if (fileDialog != null)
				{
					fileDialog.Title = OpenFileText;
					fileDialog.InitialDirectory = LastFileLocation;
					fileDialog.DefaultDirectory = Path.GetFullPath(DefaultPaths.RootFolder);

					var result = fileDialog.ShowDialog(parentWindow);

					if (String.IsNullOrWhiteSpace(DefaultFileName))
					{
						if (FileCommands.IsValidFilePath(FileLocationText))
						{
							fileDialog.DefaultFileName = Path.GetDirectoryName(FileLocationText);
						}
					}
					else
					{
						fileDialog.DefaultFileName = DefaultFileName;
					}

					if (result == CommonFileDialogResult.Ok)
					{
						string path = "";

						if (BrowseMode == FileBrowserMode.Open && fileDialog is CommonOpenFileDialog openFileDialog)
						{
							path = openFileDialog.FileNames.First();
						}
						else
						{
							path = fileDialog.FileName;
						}

						if (FileCommands.PathIsRelative(path))
						{
							path = path.Replace(Directory.GetCurrentDirectory(), "");
						}

						path = Uri.UnescapeDataString(path); // Get rid of %20

						FileLocationText = path;
						LastFileLocation = FileLocationText;
						OnOpen?.Execute(FileLocationText);
					}
				}
			}

			AutoScrollTextbox();
		}

		private void AutoScrollTextbox()
		{
			if (!String.IsNullOrEmpty(FileLocationText))
			{
				//await Task.Delay(500);

				FilePathDisplay.CaretIndex = FilePathDisplay.Text.Length;
				var rect = FilePathDisplay.GetRectFromCharacterIndex(FilePathDisplay.CaretIndex);
				FilePathDisplay.ScrollToHorizontalOffset(rect.Right * 2);

				/*
				await Dispatcher.BeginInvoke((Action)(() =>
				{
					//Scroll to the end
					FilePathDisplay.CaretIndex = FilePathDisplay.Text.Length;
					var rect = FilePathDisplay.GetRectFromCharacterIndex(FilePathDisplay.CaretIndex);
					FilePathDisplay.ScrollToHorizontalOffset(rect.Right * 2);
				}), DispatcherPriority.ApplicationIdle);
				*/
			}
		}

		public void OnFileLocationChanged()
		{

			FileValidation = FileValidation.None;

			if (!String.IsNullOrWhiteSpace(FileLocationText))
			{
				if (!FileCommands.IsValidPath(FileLocationText))
				{
					FileValidation = FileValidation.Error;
				}
				else
				{

					if (BrowseType == FileBrowseType.Directory)
					{
						if (!Directory.Exists(FileLocationText))
						{
							FileValidation = FileValidation.Warning;
						}
					}
					else if (BrowseType == FileBrowseType.File)
					{
						if (!File.Exists(FileLocationText))
						{
							FileValidation = FileValidation.Warning;
						}
					}
				}
			}

			if(FileValidation == FileValidation.None)
			{
				if(ToolTip != null)
				{
					ClearValue(FileBrowseControl.ToolTipProperty);
				}
			}
			else if (FileValidation == FileValidation.Warning)
			{
				if (BrowseType == FileBrowseType.Directory)
				{
					ToolTip = "Folder not found.";
				}
				else if (BrowseType == FileBrowseType.File)
				{
					ToolTip = "File not found.";
				}
			}
			else if (FileValidation == FileValidation.Error)
			{
				ToolTip = "Error: Path is not valid.";
			}
		}

		private void ClearFileLocationText(object sender, RoutedEventArgs e)
		{
			FileLocationText = "";
		}

		private void CutSelectedToClipboard(object sender, RoutedEventArgs e)
		{			
			if(!String.IsNullOrEmpty(FilePathDisplay.SelectedText))
			{
				var index = FilePathDisplay.CaretIndex;
				Clipboard.SetText(FilePathDisplay.SelectedText);
				FileLocationText = FileLocationText.Replace(FilePathDisplay.SelectedText, "");
				FilePathDisplay.SelectedText = "";

				FilePathDisplay.CaretIndex = index;
			}
		}

		private void CopySelectedToClipboard(object sender, RoutedEventArgs e)
		{
			if (!String.IsNullOrEmpty(FilePathDisplay.SelectedText))
			{
				Clipboard.SetText(FilePathDisplay.SelectedText);
			}
		}

		private void CopyAllToClipboard(object sender, RoutedEventArgs e)
		{
			Clipboard.SetText(FileLocationText);
		}

		private void PasteFromClipboard(object sender, RoutedEventArgs e)
		{
			if (Clipboard.ContainsText())
			{
				if(String.IsNullOrEmpty(FilePathDisplay.SelectedText))
				{
					FileLocationText = FilePathDisplay.Text.Insert(FilePathDisplay.CaretIndex, Clipboard.GetText());
				}
				else
				{
					FilePathDisplay.SelectedText = Clipboard.GetText();
				}
			}
		}

		private void ReplaceFromClipboard(object sender, RoutedEventArgs e)
		{
			if (Clipboard.ContainsText()) FileLocationText = Clipboard.GetText();
		}
	}
}
