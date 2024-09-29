using DynamicData.Binding;

using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace SCG.Data.View;

public interface IMenuData
{
	string? Module { get; set; }
}

public class MenuShortcutInputBinding
{
	public Key Key { get; set; }
	public ModifierKeys Modifiers { get; set; }

	public KeyBinding InputBinding { get; set; }

	public MenuShortcutInputBinding(Key key, ModifierKeys? modifiers = null)
	{
		Key = key;
		if (modifiers == null)
		{
			Modifiers = ModifierKeys.None;
		}
		else
		{
			Modifiers = modifiers.Value;
		}
	}
}

[DebuggerDisplay("{Header}, Children={MenuItems.Count}")]
public class MenuData : ReactiveObject, IMenuData
{
	[Reactive] public string ID { get; set; }
	[Reactive] public string Header { get; set; }
	[Reactive] public bool IsEnabled { get; set; }
	[Reactive] public bool IsChecked { get; set; }
	[Reactive] public string ShortcutText { get; set; }
	[Reactive] public ICommand? ClickCommand { get; set; }
	[Reactive] public string? Module { get; set; }
	[Reactive] public Binding? HeaderBinding { get; set; }

	public List<MenuShortcutInputBinding> Shortcuts { get; } = [];

	private readonly ObservableCollectionExtended<IMenuData> _menuItems = [];
	public ObservableCollectionExtended<IMenuData> MenuItems
	{
		get => _menuItems;
		set
		{
			if(value != null) _menuItems.AddRange(value);
		}
	}

	private readonly List<InputBindingCollection> registeredInputCollections = [];

	public MenuShortcutInputBinding AddShortcut(Key shortcutKey, ModifierKeys? shortcutModifiers = null)
	{
		var shortcut = new MenuShortcutInputBinding(shortcutKey, shortcutModifiers);
		Shortcuts.Add(shortcut);
		UpdateShortcutText();

		if (registeredInputCollections.Count > 0)
		{
			shortcut.InputBinding = new KeyBinding(ClickCommand, shortcut.Key, shortcut.Modifiers);

			foreach (var InputBindings in registeredInputCollections)
			{
				InputBindings.Add(shortcut.InputBinding);
			}
		}

		return shortcut;
	}

	public void UpdateShortcutText()
	{
		if (Shortcuts.Count > 0)
		{
			var text = "";
			for (var i = 0; i < Shortcuts.Count; i++)
			{
				var shortcut = Shortcuts[i];
				if (i > 0) text += " or ";
				if (shortcut.Modifiers != ModifierKeys.None)
				{
					text += SCG.App.ModifierKeysConverter.ConvertToString(shortcut.Modifiers) + "+" + SCG.App.KeyConverter.ConvertToString(shortcut.Key);
				}
				else
				{
					text += SCG.App.KeyConverter.ConvertToString(shortcut.Key);
				}
			}
			ShortcutText = text;
		}
		else
		{
			ShortcutText = "";
		}
	}

	public void SetHeaderBinding(object Source, string Path, BindingMode bindingMode = BindingMode.Default, UpdateSourceTrigger updateSourceTrigger = UpdateSourceTrigger.PropertyChanged)
	{
		var binding = new Binding
		{
			Source = Source,
			Path = new PropertyPath(Path),
			Mode = bindingMode,
			UpdateSourceTrigger = updateSourceTrigger
		};
		HeaderBinding = binding;
	}

	public void Register(string ModuleName, params IMenuData[] newMenuItems)
	{
		for (var i = 0; i < newMenuItems.Length; i++)
		{
			var menuItem = newMenuItems[i];
			menuItem.Module = ModuleName;
			MenuItems.Add(menuItem);
		}
	}

	public void RegisterInputBinding(InputBindingCollection InputBindings)
	{
		if (Shortcuts.Count > 0)
		{
			foreach (var shortcut in Shortcuts)
			{
				if (shortcut.InputBinding == null)
				{
					shortcut.InputBinding = new KeyBinding(ClickCommand, shortcut.Key, shortcut.Modifiers);
				}
				//Log.Here().Activity($"Registered binding: {shortcut.Key} + {shortcut.Modifiers}");
				InputBindings.Add(shortcut.InputBinding);
			}
		}

		if (MenuItems != null)
		{
			foreach (var menu in MenuItems)
			{
				if (menu is MenuData menuData)
				{
					menuData.RegisterInputBinding(InputBindings);
				}
			}
		}

		if (!registeredInputCollections.Contains(InputBindings))
		{
			registeredInputCollections.Add(InputBindings);
		}
	}

	public void UnregisterInputBinding(InputBindingCollection InputBindings)
	{
		if (Shortcuts.Count > 0)
		{
			foreach (var shortcut in Shortcuts)
			{
				if (shortcut.InputBinding != null)
				{
					InputBindings.Remove(shortcut.InputBinding);
				}
			}
		}

		if (MenuItems != null)
		{
			foreach (var menu in MenuItems)
			{
				if (menu is MenuData menuData)
				{
					menuData.UnregisterInputBinding(InputBindings);
				}
			}
		}

		if (registeredInputCollections.Contains(InputBindings))
		{
			registeredInputCollections.Remove(InputBindings);
		}
	}

	public MenuData? FindByID(string ID)
	{
		if (this.ID == ID) return this;

		if (MenuItems.Count > 0)
		{
			var match = MenuItems.Where(d => d is MenuData menu && menu.ID == ID).FirstOrDefault() as MenuData;
			if (match != null)
			{
				return match;
			}
			else
			{
				foreach (var data in this.MenuItems)
				{
					if (data is MenuData menu)
					{
						var nextMatch = menu.FindByID(ID);
						if (nextMatch != null) return nextMatch;
					}
				}
			}
		}

		return null;
	}

	public MenuData Add(params IMenuData[] menuItems)
	{
		foreach (var item in menuItems)
		{
			MenuItems.Add(item);
		}
		return this;
	}

	public MenuData()
	{
		ID = "";
		Header = "";
		ShortcutText = "";
		IsEnabled = true;
		IsChecked = true;

		this.WhenAnyValue(x => x.IsEnabled).Subscribe(b =>
		{
			if(ClickCommand != null)
			{
				ClickCommand.CanExecute(b);
			}
		});
	}

	public MenuData(string menuID) : this()
	{
		ID = menuID;
	}

	public MenuData(string menuID, string menuName) : this()
	{
		ID = menuID;
		Header = menuName;
	}

	public MenuData(string menuID, string menuName, ICommand command = null,
		Key? shortcutKey = null, ModifierKeys? shortcutModifiers = null) : this()
	{
		ID = menuID;
		Header = menuName;

		if (command != null) ClickCommand = command;

		if (shortcutKey != null)
		{
			Shortcuts.Add(new MenuShortcutInputBinding(shortcutKey.Value, shortcutModifiers));
			UpdateShortcutText();
		}
	}

	public MenuData(string menuID, string menuName, ICommand command, params MenuShortcutInputBinding[] shortcuts) : this()
	{
		ID = menuID;
		Header = menuName;
		ClickCommand = command;

		if (shortcuts != null)
		{
			foreach (var shortcut in shortcuts)
			{
				Shortcuts.Add(shortcut);
			}
			UpdateShortcutText();
		}
	}
}

[DebuggerDisplay("---")]
public class SeparatorData : IMenuData
{
	public string? Module { get; set; }
}
