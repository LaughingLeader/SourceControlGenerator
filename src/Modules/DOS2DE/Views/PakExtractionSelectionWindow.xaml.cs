using ReactiveUI;

using SCG.Modules.DOS2DE.ViewModels;
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

namespace SCG.Modules.DOS2DE.Views
{
	/// <summary>
	/// Interaction logic for PakExtractionSelectionWindow.xaml
	/// </summary>
	public partial class PakExtractionSelectionWindow : HideWindowBase, IViewFor<PakExtractionViewModel>
	{
		public PakExtractionViewModel ViewModel { get; set; }

		object IViewFor.ViewModel
		{
			get => ViewModel;
			set => ViewModel = (PakExtractionViewModel)value;
		}

		public PakExtractionSelectionWindow()
		{
			InitializeComponent();

			ViewModel = new PakExtractionViewModel();

			DataContext = ViewModel;

			this.OneWayBind(this.ViewModel, vm => vm.Paks, view => view.PakSelectionControl.ItemsSource);
		}
	}
}
