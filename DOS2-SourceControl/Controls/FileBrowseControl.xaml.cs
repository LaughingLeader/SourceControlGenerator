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
using Ookii.Dialogs.Wpf;
using LL.DOS2.SourceControl.Enum;
using System.Windows.Input;
using LL.DOS2.SourceControl.Core;

namespace LL.DOS2.SourceControl.Controls
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

		public FileBrowseType FileBrowseType
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
			typeof(FileBrowseControl), new PropertyMetadata(""));

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
			DependencyProperty.Register("FileBrowseType", typeof(FileBrowseType),
			typeof(FileBrowseControl), new PropertyMetadata(FileBrowseType.File));


		public ICommand OnOpen
		{
			get { return (ICommand)GetValue(OnOpenProperty); }
			set { SetValue(OnOpenProperty, value); }
		}

		// Using a DependencyProperty as the backing store for OnOpen.  This enables animation, styling, binding, etc...
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
				if (String.IsNullOrEmpty(FileLocationText))
				{
					if (FileBrowseType == FileBrowseType.Directory)
					{
						LastFileLocation = Path.GetFullPath(Assembly.GetExecutingAssembly().Location);
					}
					else
					{
						LastFileLocation = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;
					}
				}
				else
				{
					if(FileBrowseType == FileBrowseType.File)
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

			if (FileBrowseType == FileBrowseType.File)
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
			else if (FileBrowseType == FileBrowseType.Directory)
			{
				VistaFolderBrowserDialog folderDialog = new VistaFolderBrowserDialog();
				folderDialog.SelectedPath = LastFileLocation;
				folderDialog.Description = OpenFileText;
				folderDialog.UseDescriptionForTitle = true;
				folderDialog.ShowNewFolderButton = true;		

				Nullable<bool> result = folderDialog.ShowDialog(parentWindow);

				if(result == true)
				{
					string path = folderDialog.SelectedPath;
					if (FileCommands.PathIsRelative(path))
					{
						path = folderDialog.SelectedPath.Replace(Directory.GetCurrentDirectory(), "");
					}

					FileLocationText = path;
					LastFileLocation = FileLocationText;
					OnOpen?.Execute(this);
				}
			}
		}
	}
}
