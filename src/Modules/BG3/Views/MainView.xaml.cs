using SCG.Modules.BG3.ViewModels;

using System.Windows.Controls;

namespace SCG.Modules.BG3.Views;

public partial class MainView : ReactiveUserControl<MainViewViewModel>
{
	public MainView()
	{
		InitializeComponent();
	}
}
