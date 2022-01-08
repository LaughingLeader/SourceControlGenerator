using SCG.Modules.DOS2DE.Utilities;
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
using System.Windows.Controls.Primitives;
using Xceed.Wpf.Toolkit;
using SCG.Modules.DOS2DE.LocalizationEditor.ViewModels;

namespace SCG.Modules.DOS2DE.LocalizationEditor.Views
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

			LocationChanged += delegate (object sender, EventArgs args)
			{
				var offset = LanguagesPopup.HorizontalOffset;
				// "bump" the offset to cause the popup to reposition itself
				//   on its own
				LanguagesPopup.HorizontalOffset = offset + 1;
				LanguagesPopup.HorizontalOffset = offset;
			};
			// Also handle the window being resized (so the popup's position stays
			//  relative to its target element if the target element moves upon 
			//  window resize)
			SizeChanged += delegate (object sender, SizeChangedEventArgs e2)
			{
				var offset = LanguagesPopup.HorizontalOffset;
				LanguagesPopup.HorizontalOffset = offset + 1;
				LanguagesPopup.HorizontalOffset = offset;
			};

			Deactivated += delegate (object sender, EventArgs args)
			{
				LanguagesExpander.IsExpanded = false;
			};
		}

		private void CreateBinding(string vmProperty, FrameworkElement element, DependencyProperty prop, BindingMode mode = BindingMode.OneWay, object source = null)
		{
			Binding binding = new Binding(vmProperty);
			if (source == null)
			{
				binding.Source = ViewModel;
			}
			else
			{
				binding.Source = source;
			}
			binding.Mode = mode;
			element.SetBinding(prop, binding);
		}

		private void CreateButtonBinding(string vmProperty, Button button)
		{
			Binding binding = new Binding(vmProperty);
			binding.Source = ViewModel;
			binding.Mode = BindingMode.OneWay;
			button.SetBinding(Button.CommandProperty, binding);
		}

		public void ResetBindings()
		{
			if (ViewModel != null)
			{
				CreateBinding("Languages", LanguagesChecklistBox, ListBox.ItemsSourceProperty);
				CreateBinding("LanguageCheckedCommand", LanguagesChecklistBox, CheckListBox.CommandProperty);

				CreateButtonBinding("SaveXMLCommand", SaveButton);
				CreateButtonBinding("SaveXMLAsCommand", SaveAsButton);
				CreateButtonBinding("OpenXMLFolderCommand", OpenFolderButton);
				CreateButtonBinding("GenerateXMLCommand", ExportButton);

				if (ViewModel.ActiveProjectSettings != null)
				{
					CreateBinding("ExportKeys", ExportKeysCheckBox, ToggleButton.IsCheckedProperty, BindingMode.TwoWay, ViewModel.ActiveProjectSettings);
					CreateBinding("ExportSource", ExportSourceCheckBox, ToggleButton.IsCheckedProperty, BindingMode.TwoWay, ViewModel.ActiveProjectSettings);
					CreateBinding("TargetLanguages", LanguagesChecklistBox, CheckListBox.SelectedValueProperty, BindingMode.TwoWay, ViewModel.ActiveProjectSettings);
				}
			}
		}

		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			this.Hide();
		}

		private void CopyButton_Click(object sender, RoutedEventArgs e)
		{
			Log.Here().Activity($"Command check: {this.SaveButton.Command}");
			if (FindName("OutputTextbox") is TextBox outputTextbox)
			{
				if (!String.IsNullOrWhiteSpace(outputTextbox.Text))
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
	}
}
