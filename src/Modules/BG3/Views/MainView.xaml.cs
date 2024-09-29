using SCG.BG3.ViewModels;

using System.Windows.Controls;

namespace SCG.BG3.Views;

public partial class MainView : ReactiveUserControl<MainViewViewModel>
{
	public MainView()
	{
		InitializeComponent();

		this.WhenActivated(d =>
		{
			d(this.OneWayBind(ViewModel, vm => vm.Projects, x => x.ProjectsListView.ItemsSource));
		});
	}
}
