using ReactiveUI;

using SCG.Modules.DOS2DE.LocalizationEditor.Models;
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

namespace SCG.Modules.DOS2DE.LocalizationEditor.Views
{
	/// <summary>
	/// Interaction logic for LocaleOptionsWindow.xaml
	/// </summary>
	public partial class LocaleOptionsWindow : HideWindowBase, IViewFor<LocaleEditorSettingsData>
	{
		public LocaleOptionsWindow() : base()
		{
			InitializeComponent();
		}

		private LocaleEditorSettingsData viewData;

		public LocaleEditorSettingsData ViewModel
		{
			get { return viewData; }
			set
			{
				viewData = value;
				DataContext = viewData;
			}
		}

		object IViewFor.ViewModel
		{
			get => ViewModel;
			set
			{
				ViewModel = (LocaleEditorSettingsData)value;
			}
		}

		public void LoadData(LocaleEditorSettingsData data)
		{
			ViewModel = data;
			this.OneWayBind(this.ViewModel, vm => vm.SaveCommand, view => view.SaveButton.Command);
			ViewModel.SaveCommand.Execute(null);
		}
	}
}
