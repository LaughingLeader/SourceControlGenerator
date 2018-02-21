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
using Ookii.Dialogs.Wpf;
using LL.SCG.Core;
using LL.SCG.Data.View;
using LL.SCG.Util;
using LL.SCG.Windows;

namespace LL.SCG.Controls
{
	/// <summary>
	/// Interaction logic for TemplateEditorControl.xaml
	/// </summary>
	public partial class TemplateEditorControl : UserControl
	{
		public TemplateEditorData TemplateData
		{
			get { return (TemplateEditorData)GetValue(DataProperty); }
			set { SetValue(DataProperty, value); }
		}

		// Using a DependencyProperty as the backing store for TemplateData.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty DataProperty =
			DependencyProperty.Register("TemplateData", typeof(TemplateEditorData), typeof(TemplateEditorControl), new PropertyMetadata(null));

		public string TemplateFilePath
		{
			get { return (string)GetValue(TemplateFilePathProperty); }
			set
			{
				SetValue(TemplateFilePathProperty, value);
				if (TemplateData != null) TemplateData.FilePath = TemplateFilePath;
			}
		}

		// Using a DependencyProperty as the backing store for TemplateFilePath.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty TemplateFilePathProperty =
			DependencyProperty.Register("TemplateFilePath", typeof(string), typeof(TemplateEditorControl), new PropertyMetadata(""));

		public TemplateEditorControl()
		{
			InitializeComponent();

			if (TemplateData != null) this.DataContext = TemplateData;
		}

		private string GetOutputText()
		{
			if (TemplateData != null)
			{
				string output = TemplateData.EditorText;
				if (!String.IsNullOrEmpty(output))
				{
					//Replace keynames with actual values

					return output;
				}
			}
			return "";
		}

		private void DefaultButton_Click(object sender, RoutedEventArgs e)
		{
			if(TemplateData != null)
			{
				if (TaskDialog.OSSupportsTaskDialogs)
				{
					using (TaskDialog dialog = new TaskDialog())
					{
						dialog.WindowTitle = "Reset " + TemplateData.Name +" to Default?";
						dialog.Content = "Current changes will be lost.";
						//dialog.ExpandedInformation = "";
						//dialog.Footer = "Task Dialogs support footers and can even include <a href=\"http://www.ookii.org\">hyperlinks</a>.";
						dialog.FooterIcon = TaskDialogIcon.Information;
						dialog.AllowDialogCancellation = true;
						//dialog.EnableHyperlinks = true;
						TaskDialogButton okButton = new TaskDialogButton(ButtonType.Ok);
						TaskDialogButton cancelButton = new TaskDialogButton(ButtonType.Cancel);
						dialog.Buttons.Add(okButton);
						dialog.Buttons.Add(cancelButton);
						dialog.ButtonStyle = TaskDialogButtonStyle.Standard;
						
						//dialog.HyperlinkClicked += new EventHandler<HyperlinkClickedEventArgs>(TaskDialog_HyperLinkClicked);
						TaskDialogButton button = dialog.ShowDialog(App.Current.MainWindow);
						if (button == okButton)
							TemplateData.SetToDefault();
					}
				}
				else
				{
					TemplateData.SetToDefault();
				}
			}
		}

		private void TestClick(object sender, RoutedEventArgs e)
		{
			Log.Here().Important("Clicked!");
		}
	}
}
