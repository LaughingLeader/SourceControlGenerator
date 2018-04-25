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

namespace LL.SCG.Controls
{
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

		public string Filter
		{
			get { return (string)GetValue(FilterProperty); }
			set
			{
				SetValue(FilterProperty, value);
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

		public static readonly DependencyProperty FilterProperty =
			DependencyProperty.Register("Filter", typeof(string),
			typeof(FileBrowseControl), new PropertyMetadata(""));

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


		public FileBrowseControl()
		{
			InitializeComponent();

			//this.DataContext = this;
		}

		private void FileBrowseButton_Click(object sender, RoutedEventArgs e)
		{
			Window parentWindow = Window.GetWindow(this);

			if (String.IsNullOrEmpty(LastFileLocation))
			{
				if (!String.IsNullOrEmpty(FileLocationText))
				{
					if(BrowseType == FileBrowseType.File)
					{
						var parentDirectory = new DirectoryInfo(FileLocationText);
						if(parentDirectory != null)
						{
							LastFileLocation = parentDirectory.Parent.FullName;
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
			else
			{
				//Confirm to the browse type
				if (BrowseType == FileBrowseType.File)
				{
					var parentDirectory = new DirectoryInfo(LastFileLocation);
					if (parentDirectory != null)
					{
						LastFileLocation = parentDirectory.Parent.FullName;
					}
				}
				else
				{
					var directory = new DirectoryInfo(LastFileLocation);
					if (directory != null)
					{
						LastFileLocation = directory.FullName;
					}
				}
			}

			if(!FileCommands.IsValidPath(LastFileLocation))
			{
				if (BrowseType == FileBrowseType.Directory)
				{
					LastFileLocation = Path.GetFullPath(Assembly.GetExecutingAssembly().Location);
				}
				else
				{
					LastFileLocation = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;
				}
			}

			Log.Here().Activity($"LastFileLocation is {LastFileLocation} FileLocationText: {FileLocationText}");

			if (BrowseType == FileBrowseType.File)
			{
				OpenFileDialog fileDialog = new OpenFileDialog();
				fileDialog.Title = OpenFileText;
				fileDialog.InitialDirectory = LastFileLocation;
				fileDialog.DefaultExt = DefaultExt;
				fileDialog.Filter = Filter;
				fileDialog.Multiselect = false;

				if(FileCommands.IsValidPath(FileLocationText))
				{
					fileDialog.FileName = Path.GetFileName(FileLocationText);
				}

				Nullable<bool> result = fileDialog.ShowDialog(parentWindow);
				if (result == true)
				{
					string filename = fileDialog.FileName;

					if(FileCommands.PathIsRelative(filename))
					{
						filename = Common.Functions.GetRelativePath.RelativePathGetter.Relative(Directory.GetCurrentDirectory(), fileDialog.FileName);
					}

					FileLocationText = filename;
					LastFileLocation = Path.GetDirectoryName(FileLocationText);
					OnOpen?.Execute(this);

					if(OnOpen == null)
					{
						System.Diagnostics.Debug.WriteLine("OnOpen is null!");
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

				var openFolder = new CommonOpenFileDialog();
				openFolder.AllowNonFileSystemItems = true;
				openFolder.Multiselect = false;
				openFolder.IsFolderPicker = true;
				openFolder.Title = OpenFileText;
				openFolder.DefaultFileName = "";
				openFolder.InitialDirectory = LastFileLocation;
				openFolder.DefaultDirectory = Path.GetFullPath(Assembly.GetExecutingAssembly().Location);

				var result = openFolder.ShowDialog(parentWindow);

				if (result == CommonFileDialogResult.Ok)
				{
					string path = openFolder.FileNames.First();
					if (FileCommands.PathIsRelative(path))
					{
						path = path.Replace(Directory.GetCurrentDirectory(), "");
					}

					FileLocationText = path;
					LastFileLocation = FileLocationText;
					OnOpen?.Execute(this);
				}
			}
		}

		public void OnFileLocationChanged()
		{
			TextBox textBox = (TextBox)this.FindName("FilePathDisplay");

			FileValidation = FileValidation.None;

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

			if(FileValidation == FileValidation.None)
			{
				if(textBox.ToolTip != null)
				{
					textBox.ClearValue(TextBox.ToolTipProperty);
				}
			}
			else if (FileValidation == FileValidation.Warning)
			{
				if (BrowseType == FileBrowseType.Directory)
				{
					textBox.ToolTip = "Folder not found.";
				}
				else if (BrowseType == FileBrowseType.File)
				{
					textBox.ToolTip = "File not found.";
				}
			}
			else if (FileValidation == FileValidation.Error)
			{
				textBox.ToolTip = "Error: Path is not valid.";
			}
		}
	}
}
