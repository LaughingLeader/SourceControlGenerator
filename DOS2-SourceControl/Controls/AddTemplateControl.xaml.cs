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
using LL.DOS2.SourceControl.Data.View;
using LL.DOS2.SourceControl.Windows;
using Ookii.Dialogs.Wpf;

namespace LL.DOS2.SourceControl.Controls
{
	/// <summary>
	/// Interaction logic for AddTemplateControl.xaml
	/// </summary>
	public partial class AddTemplateControl : UserControl
	{
		private EditTextWindow editTextWindow;
		private TemplateEditorData data;

		public AddTemplateControl(TemplateEditorData templateEditorData)
		{
			InitializeComponent();

			data = templateEditorData;
			DataContext = data;
		}

		private void OnEditorTextConfirmed(string newText)
		{
			data.DefaultEditorText = newText;
		}

		private void OnEditorTextCanceled()
		{
			
		}

		private void EditorTextExpandButton_Click(object sender, RoutedEventArgs e)
		{
			if (editTextWindow == null) editTextWindow = new EditTextWindow(OnEditorTextConfirmed, OnEditorTextCanceled, "Edit Default Editor Text");
			editTextWindow.Text = data.DefaultEditorText;

			Window mainWindow = Application.Current.MainWindow;
			editTextWindow.Left = mainWindow.Left + (mainWindow.Width - editTextWindow.ActualWidth) / 2;
			editTextWindow.Top = mainWindow.Top + (mainWindow.Height - editTextWindow.ActualHeight) / 2;

			editTextWindow.Show();
		}
	}
}
