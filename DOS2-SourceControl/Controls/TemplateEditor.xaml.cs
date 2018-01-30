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
	/// Interaction logic for TemplateEditor.xaml
	/// </summary>
	public partial class TemplateEditor : UserControl
	{
		public string TooltipText
		{
			get { return (string)GetValue(TooltipTextProperty); }
			set { SetValue(TooltipTextProperty, value); }
		}

		// Using a DependencyProperty as the backing store for TooltipText.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty TooltipTextProperty =
			DependencyProperty.Register("TooltipText", typeof(string), typeof(TemplateEditor), new PropertyMetadata(""));

		public string LabelText
		{
			get { return (string)GetValue(LabelTextProperty); }
			set { SetValue(LabelTextProperty, value); }
		}

		// Using a DependencyProperty as the backing store for LabelText.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty LabelTextProperty =
			DependencyProperty.Register("LabelText", typeof(string), typeof(TemplateEditor), new PropertyMetadata(""));

		public string EditorText
		{
			get { return (string)GetValue(EditorTextProperty); }
			set { SetValue(EditorTextProperty, value); }
		}

		// Using a DependencyProperty as the backing store for EditorText.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty EditorTextProperty =
			DependencyProperty.Register("EditorText", typeof(string), typeof(TemplateEditor), new PropertyMetadata(""));


		public string BrowseText
		{
			get { return (string)GetValue(BrowseTextProperty); }
			set { SetValue(BrowseTextProperty, value); }
		}

		// Using a DependencyProperty as the backing store for BrowseText.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty BrowseTextProperty =
			DependencyProperty.Register("BrowseText", typeof(string), typeof(TemplateEditor), new PropertyMetadata(""));


		public string TemplateFileLocationText
		{
			get { return (string)GetValue(TemplateFileLocationTextProperty); }
			set { SetValue(TemplateFileLocationTextProperty, value); }
		}

		// Using a DependencyProperty as the backing store for TemplateFileLocationText.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty TemplateFileLocationTextProperty =
			DependencyProperty.Register("TemplateFileLocationText", typeof(string), typeof(TemplateEditor), new PropertyMetadata(""));

		public Action OnFileOpened
		{
			get { return (Action)GetValue(OnFileOpenedProperty); }
			set { SetValue(OnFileOpenedProperty, value); }
		}

		// Using a DependencyProperty as the backing store for OnFileOpened.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty OnFileOpenedProperty =
			DependencyProperty.Register("OnFileOpened", typeof(Action), typeof(TemplateEditor), new PropertyMetadata(null));


		public TemplateEditor()
		{
			InitializeComponent();
		}
	}
}
