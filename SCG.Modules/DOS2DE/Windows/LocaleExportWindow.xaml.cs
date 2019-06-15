using SCG.Modules.DOS2DE.Utilities;
using SCG.Modules.DOS2DE.Data.View.Locale;
using SCG.Windows;
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

namespace SCG.Modules.DOS2DE.Windows
{
	/// <summary>
	/// Interaction logic for LocaleExportWindow.xaml
	/// </summary>
	public partial class LocaleExportWindow : HideWindowBase
	{
		public LocaleExportWindow()
		{
			InitializeComponent();
		}

		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			this.Hide();
		}

		private void CopyButton_Click(object sender, RoutedEventArgs e)
		{
			if(FindName("OutputTextbox") is TextBox outputTextbox)
			{
				if(!String.IsNullOrWhiteSpace(outputTextbox.Text))
				{
					Clipboard.SetText(outputTextbox.Text);
				}
			}
		}

		private LocaleViewModel localeViewData;

		public LocaleViewModel LocaleData
		{
			get { return localeViewData; }
			set
			{
				localeViewData = value;
				DataContext = localeViewData;
			}
		}


		private void ExportButton_Click(object sender, RoutedEventArgs e)
		{
			if (LocaleData != null && FindName("OutputTextbox") is TextBox outputTextbox)
			{
				Log.Here().Activity("Exporting data to xml format.");
				outputTextbox.Text = "";
				outputTextbox.Text = LocaleEditorCommands.ExportDataAsXML(LocaleData, LocaleData.Settings.ExportSource, LocaleData.Settings.ExportKeys);
			}
		}
	}
}
