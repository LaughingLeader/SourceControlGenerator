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
using System.Windows.Shapes;

namespace LL.DOS2.SourceControl.Windows
{
	/// <summary>
	/// Interaction logic for EditTextWindow.xaml
	/// </summary>
	public partial class EditTextWindow : Window
	{
		private Action<string> OnConfirm;
		private Action OnCancel;

		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty TextProperty =
			DependencyProperty.Register("Text", typeof(string), typeof(EditTextWindow), new PropertyMetadata(""));


		public EditTextWindow(Action<string> onConfirm, Action onCancel, string windowTitle = "")
		{
			InitializeComponent();

			Title = windowTitle;
			DataContext = this;
		}

		private void ConfirmButton_Click(object sender, RoutedEventArgs e)
		{
			OnConfirm?.Invoke(Text);
			this.Hide();
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			OnCancel?.Invoke();
			this.Hide();
		}

		private void EditWindow_Closed(object sender, EventArgs e)
		{
			OnCancel?.Invoke();
		}
	}
}
