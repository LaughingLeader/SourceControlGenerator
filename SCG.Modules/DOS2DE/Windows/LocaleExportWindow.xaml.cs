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
using ReactiveUI;

namespace SCG.Modules.DOS2DE.Windows
{
	/// <summary>
	/// Interaction logic for LocaleExportWindow.xaml
	/// </summary>
	public partial class LocaleExportWindow : HideWindowBase, IViewFor<LocaleViewModel>
	{
		public bool ExportAll { get; set; } = false;

		public LocaleExportWindow()
		{
			InitializeComponent();

			DataContextChanged += LocaleExportWindow_DataContextChanged;
		}

		private void LocaleExportWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if(ViewModel != null && ViewModel.ActiveProjectSettings != null)
			{
				this.Bind(ViewModel, vm => vm.ActiveProjectSettings.ExportKeys, view => view.ExportKeysCheckBox.IsChecked);
				this.Bind(ViewModel, vm => vm.ActiveProjectSettings.ExportSource, view => view.ExportSourceCheckBox.IsChecked);
			}
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

		public LocaleViewModel ViewModel
		{
			get { return localeViewData; }
			set
			{
				localeViewData = value;
				DataContext = localeViewData;
			}
		}
		object IViewFor.ViewModel
		{
			get => ViewModel;
			set
			{
				ViewModel = (LocaleViewModel)value;
			}
		}

		private void ExportButton_Click(object sender, RoutedEventArgs e)
		{
			if (ViewModel != null && FindName("OutputTextbox") is TextBox outputTextbox)
			{
				Log.Here().Activity("Exporting data to xml format.");
				outputTextbox.Text = "";
				outputTextbox.Text = LocaleEditorCommands.ExportDataAsXML(ViewModel, ExportAll);
			}
		}
	}
}
