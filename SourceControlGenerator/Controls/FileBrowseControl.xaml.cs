using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

using LL.SCG.Enum;
using System.Windows.Input;
using LL.SCG.Core;
using Microsoft.WindowsAPICodePack.Dialogs;
using LL.SCG.Data.View;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Threading;

namespace LL.SCG.Controls
{
	public enum FileBrowserMode
	{
		Open,
		Save
	}

	public class FileBrowserFilter
	{
		public string Name { get; set; }
		public string Values { get; set; }
	}

	public static class CommonFileFilters
	{
		public static string CombineFilters(params FileBrowserFilter[] filters)
		{
			return String.Join(";", filters.Select(f => f.Values));
		}

		public static FileBrowserFilter All { get; private set; } = new FileBrowserFilter()
		{
			Name = "All types",
			Values = "*.*"
		};

		public static FileBrowserFilter NormalTextFile { get; private set; } = new FileBrowserFilter()
		{
			Name = "Normal text file",
			Values = "*.txt"
		};

		public static FileBrowserFilter Json { get; private set; } = new FileBrowserFilter()
		{
			Name = "JSON file",
			Values = "*.json"
		};

		public static FileBrowserFilter GitFiles { get; private set; } = new FileBrowserFilter()
		{
			Name = "Git text files",
			Values = "*.md;.gitignore;.gitattributes"
		};

		public static FileBrowserFilter SourceControlGeneratorFiles { get; private set; } = new FileBrowserFilter()
		{
			Name = "Source Control Generator files",
			Values = CombineFilters(NormalTextFile, Json, GitFiles)
		};

		public static FileBrowserFilter MarkdownFile { get; private set; } = new FileBrowserFilter()
		{
			Name = "Markdown file",
			Values = "*.md"
		};

		public static FileBrowserFilter HTMLFile { get; private set; } = new FileBrowserFilter()
		{
			Name = "HTML file",
			Values = "*.html"
		};

		public static FileBrowserFilter MarkdownConverterFiles { get; private set; } = new FileBrowserFilter()
		{
			Name = "Markdown Converter files",
			Values = CombineFilters(NormalTextFile, MarkdownFile, HTMLFile)
		};

		public static List<FileBrowserFilter> MarkdownConverterFilesList { get; private set; } = new List<FileBrowserFilter>()
		{
			MarkdownConverterFiles,
			All
		};

		public static List<FileBrowserFilter> DefaultFilters { get; set; } = new List<FileBrowserFilter>()
		{
			SourceControlGeneratorFiles,
			NormalTextFile,
			Json,
			All
		};
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


		public FileBrowseControl()
		{
			InitializeComponent();

			Loaded += FileBrowseControl_Loaded;

			//this.DataContext = this;
		}

		private void FileBrowseControl_Loaded(object sender, RoutedEventArgs e)
		{
			if (Filters == null) Filters = CommonFileFilters.DefaultFilters;

			if (!String.IsNullOrEmpty(FileLocationText))
			{
				TextBox textBox = (TextBox)this.FindName("FilePathDisplay");
				//Scroll to the end
				textBox.CaretIndex = textBox.Text.Length;
				var rect = textBox.GetRectFromCharacterIndex(textBox.CaretIndex);
				textBox.ScrollToHorizontalOffset(rect.Right);
			}
		}

		private void FileBrowseButton_Click(object sender, RoutedEventArgs e)
		{
			Window parentWindow = Window.GetWindow(this);

			Log.Here().Activity($"LastFileLocation is {LastFileLocation} FileLocationText: {FileLocationText}");

			if (String.IsNullOrEmpty(LastFileLocation))
			{
				if (!String.IsNullOrEmpty(FileLocationText) && FileCommands.IsValidPath(FileLocationText))
				{
					if(BrowseType == FileBrowseType.File)
					{
						if(FileCommands.IsValidFilePath(FileLocationText))
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
			}

			if(!FileCommands.IsValidPath(LastFileLocation))
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
					//if (!String.IsNullOrWhiteSpace(DefaultExt)) fileDialog.DefaultExtension = DefaultExt;

					Log.Here().Activity(DefaultExt);
					Log.Here().Activity(DefaultFileName);

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

						FileLocationText = path;
						LastFileLocation = FileLocationText;
						OnOpen?.Execute(FileLocationText);
					}
				}
			}

			if (!String.IsNullOrEmpty(FileLocationText))
			{
				TextBox textBox = (TextBox)this.FindName("FilePathDisplay");
				//Scroll to the end
				textBox.CaretIndex = textBox.Text.Length;
				var rect = textBox.GetRectFromCharacterIndex(textBox.CaretIndex);
				textBox.ScrollToHorizontalOffset(rect.Right);
			}
		}

		public void OnFileLocationChanged()
		{
			TextBox textBox = (TextBox)this.FindName("FilePathDisplay");

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
	}
}
