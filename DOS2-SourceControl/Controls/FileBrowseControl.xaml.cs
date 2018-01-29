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

namespace LL.DOS2.SourceControl.Controls
{
	/// <summary>
	/// Interaction logic for FileBrowseControl.xaml
	/// </summary>
	public partial class FileBrowseControl : UserControl
	{
		public string FileLocationText
		{
			get { return (string)GetValue(FileLocationTextProperty); }
			set { SetValue(FileLocationTextProperty, value); }
		}

		public string LastFileLocation
		{
			get { return (string)GetValue(LastFileLocationProperty); }
			set { SetValue(LastFileLocationProperty, value); }
		}

		public static readonly DependencyProperty FileLocationTextProperty =
			DependencyProperty.Register("FileLocationText", typeof(string),
			typeof(FileBrowseControl), new PropertyMetadata(""));

		public static readonly DependencyProperty LastFileLocationProperty =
			DependencyProperty.Register("LastFileLocation", typeof(string),
			typeof(FileBrowseControl), new PropertyMetadata(""));

		public FileBrowseControl()
		{
			InitializeComponent();

			this.DataContext = this;
		}
	}
}
