using SCG.Modules.DOS2DE.LocalizationEditor.Models;
using SCG.Windows;

namespace SCG.Modules.DOS2DE.LocalizationEditor.Views;

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
