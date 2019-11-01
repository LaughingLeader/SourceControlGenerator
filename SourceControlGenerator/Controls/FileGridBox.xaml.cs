using SCG.SCGEnum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SCG.Controls
{
	/// <summary>
	/// Interaction logic for FileGridBox.xaml
	/// </summary>
	public partial class FileGridBox : UserControl
	{

		public string Header
		{
			get { return (string)GetValue(HeaderProperty); }
			set { SetValue(HeaderProperty, value); }
		}

		public static readonly DependencyProperty HeaderProperty =
			DependencyProperty.Register("Header", typeof(string), typeof(FileGridBox), new PropertyMetadata(""));

		public string FilePath
		{
			get { return (string)GetValue(FilePathProperty); }
			set { SetValue(FilePathProperty, value); }
		}

		public static readonly DependencyProperty FilePathProperty = 
			DependencyProperty.Register("FilePath", typeof(string), typeof(FileGridBox), new PropertyMetadata(""));

		public FileBrowseType BrowseType
		{
			get { return (FileBrowseType)GetValue(FileGridBoxBrowseTypeProperty); }
			set
			{
				SetValue(FileGridBoxBrowseTypeProperty, value);
			}
		}

		public static readonly DependencyProperty FileGridBoxBrowseTypeProperty =
			DependencyProperty.Register("BrowseType", typeof(FileBrowseType),
			typeof(FileGridBox), new PropertyMetadata(FileBrowseType.File));

		public FileGridBox()
		{
			InitializeComponent();
		}
	}
}
