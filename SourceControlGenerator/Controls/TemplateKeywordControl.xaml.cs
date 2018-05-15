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
	/// Interaction logic for TemplateKeywordControl.xaml
	/// </summary>
	public partial class TemplateKeywordControl : UserControl
	{
		public string KeywordName
		{
			get { return (string)GetValue(KeywordNameProperty); }
			set { SetValue(KeywordNameProperty, value); }
		}

		// Using a DependencyProperty as the backing store for KeywordName.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty KeywordNameProperty =
			DependencyProperty.Register("KeywordName", typeof(string), typeof(TemplateKeywordControl), new PropertyMetadata(""));

		public string KeywordValue
		{
			get { return (string)GetValue(KeywordValueProperty); }
			set { SetValue(KeywordValueProperty, value); }
		}

		// Using a DependencyProperty as the backing store for KeywordValue.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty KeywordValueProperty =
			DependencyProperty.Register("KeywordValue", typeof(string), typeof(TemplateKeywordControl), new PropertyMetadata(""));


		public bool KeywordNameReadOnly
		{
			get { return (bool)GetValue(KeywordNameReadOnlyProperty); }
			set { SetValue(KeywordNameReadOnlyProperty, value); }
		}

		// Using a DependencyProperty as the backing store for KeywordNameReadOnly.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty KeywordNameReadOnlyProperty =
			DependencyProperty.Register("KeywordNameReadOnly", typeof(bool), typeof(TemplateKeywordControl), new PropertyMetadata(false));


		public bool KeywordValueReadOnly
		{
			get { return (bool)GetValue(KeywordValueReadOnlyProperty); }
			set { SetValue(KeywordValueReadOnlyProperty, value); }
		}

		// Using a DependencyProperty as the backing store for KeywordValueReadOnly.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty KeywordValueReadOnlyProperty =
			DependencyProperty.Register("KeywordValueReadOnly", typeof(bool), typeof(TemplateKeywordControl), new PropertyMetadata(false));




		public TemplateKeywordControl()
		{
			InitializeComponent();
		}
	}
}
