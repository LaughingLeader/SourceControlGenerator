using SCG.Modules.DOS2DE.ViewModels;
using SCG.Windows;

namespace SCG.Modules.DOS2DE.Views;

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
