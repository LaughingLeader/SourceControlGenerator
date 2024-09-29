using System.Collections.ObjectModel;

namespace SCG.Data.View;

public class MenuBarData : ReactiveObject
{
	[Reactive] public MenuData File { get; private set; }

	[Reactive] public MenuData Options { get; private set; }

	[Reactive] public MenuData Tools { get; private set; }

	[Reactive] public MenuData Help { get; private set; }

	public ObservableCollection<MenuData> Menus { get; private set; }

	public void RemoveAllModuleMenus(string ModuleName)
	{
		foreach (var menu in Menus)
		{
			menu.MenuItems.RemoveAll(m => m.Module == ModuleName);
		}
		Log.Here().Activity($"Removed menus for module {ModuleName}.");
	}

	public MenuData? FindByID(string ID)
	{
		foreach (var menu in Menus)
		{
			var match = menu.FindByID(ID);
			if (match != null) return match;
		}
		return null;
	}

	public MenuBarData()
	{
		File = new MenuData("Base.File", "File");
		Options = new MenuData("Base.Options", "Options");
		Tools = new MenuData("Base.Tools", "Tools");
		Help = new MenuData("Base.Help", "Help");

		Menus =
		[
			File,
			Options,
			Tools,
			Help
		];
	}
}
