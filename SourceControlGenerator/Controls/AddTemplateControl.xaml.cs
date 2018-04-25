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
using LL.SCG.Data.View;
using LL.SCG.Windows;


namespace LL.SCG.Controls
{
	/// <summary>
	/// Interaction logic for AddTemplateControl.xaml
	/// </summary>
	public partial class AddTemplateControl : UserControl
	{
		private EditTextWindow editTextWindow;

		public TemplateEditorData TemplateData
		{
			get { return (TemplateEditorData)GetValue(TemplateDataProperty); }
			set
			{
				SetValue(TemplateDataProperty, value);
			}
		}

		// Using a DependencyProperty as the backing store for TemplateData.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty TemplateDataProperty =
			DependencyProperty.Register("TemplateData", typeof(TemplateEditorData), typeof(AddTemplateControl), new PropertyMetadata(null));

		public ICommand ConfirmCommand
		{
			get { return (ICommand)GetValue(ConfirmCommandProperty); }
			set { SetValue(ConfirmCommandProperty, value); }
		}

		// Using a DependencyProperty as the backing store for ConfirmCommand.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ConfirmCommandProperty =
			DependencyProperty.Register("ConfirmCommand", typeof(ICommand), typeof(AddTemplateControl), new PropertyMetadata(null));


		public ICommand CancelCommand
		{
			get { return (ICommand)GetValue(CancelCommandProperty); }
			set { SetValue(CancelCommandProperty, value); }
		}

		// Using a DependencyProperty as the backing store for CancelCommand.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty CancelCommandProperty =
			DependencyProperty.Register("CancelCommand", typeof(ICommand), typeof(AddTemplateControl), new PropertyMetadata(null));



		public AddTemplateControl()
		{
			InitializeComponent();

			//Visibility = Visibility.Collapsed;
		}

		private void OnEditorTextConfirmed(string newText)
		{
			TemplateData.DefaultEditorText = newText;

			Log.Here().Important("Editor text changed?");
		}

		private void OnEditorTextCanceled()
		{
			
		}

		private void EditorTextExpandButton_Click(object sender, RoutedEventArgs e)
		{
			if(TemplateData != null)
			{
				if (editTextWindow == null) editTextWindow = new EditTextWindow(OnEditorTextConfirmed, OnEditorTextCanceled, "Edit Default Editor Text");
				editTextWindow.Text = TemplateData.DefaultEditorText;

				Window mainWindow = Application.Current.MainWindow;
				editTextWindow.Left = mainWindow.Left + (mainWindow.Width - editTextWindow.ActualWidth) / 2;
				editTextWindow.Top = mainWindow.Top + (mainWindow.Height - editTextWindow.ActualHeight) / 2;

				editTextWindow.Show();
			}
		}

		private void ConfirmButton_Click(object sender, RoutedEventArgs e)
		{

		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{

		}

		private void AddTemplateMain_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if(TemplateData != null)
			{
				
			}
			else
			{
				Log.Here().Error("Something went wrong. TemplateData is null!");
			}
		}
	}
}
