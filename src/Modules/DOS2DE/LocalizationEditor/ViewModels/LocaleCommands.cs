using SCG.Modules.DOS2DE.LocalizationEditor.Models;

using System.Windows.Input;

namespace SCG.Modules.DOS2DE.LocalizationEditor.ViewModels;

public class LocaleCommands
{
	private LocaleViewModel _vm { get; set; }

	public ICommand AddCustomFileCommand { get; private set; }
	public ICommand RemoveCustomFileCommand { get; private set; }

	public void AddCustomFile()
	{

	}

	public void RemoveCustomFile(LocaleCustomFileData customFileData)
	{
		_vm.CustomGroup.DataFiles.Remove(customFileData);
	}

	public LocaleCommands(LocaleViewModel vm)
	{
		_vm = vm;

		AddCustomFileCommand = ReactiveCommand.Create(AddCustomFile);
		RemoveCustomFileCommand = ReactiveCommand.Create<LocaleCustomFileData>(RemoveCustomFile);
	}
}
