using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LL.SCG.Data.View
{
	public interface IMenuData
	{
		string Module { get; set; }
	}

	[DebuggerDisplay("{Header}, Children={MenuItems.Count}")]
	public class MenuData : PropertyChangedBase, IMenuData
	{
		private string header;

		public string Header
		{
			get
			{
				if (GetHeader != null)
				{
					return GetHeader.Invoke();
				}
				return header;
			}
			set
			{
				header = value;
				RaisePropertyChanged("Header");
			}
		}

		private Key? shortcutKey;

		public Key? ShortcutKey
		{
			get { return shortcutKey; }
			set
			{
				shortcutKey = value;
				RaisePropertyChanged("ShortcutKey");
				RaisePropertyChanged("ShortcutText");
			}
		}

		private ModifierKeys? shortcutModifiers;

		public ModifierKeys? ShortcutModifiers
		{
			get { return shortcutModifiers; }
			set
			{
				shortcutModifiers = value;
				RaisePropertyChanged("ShortcutModifiers");
				RaisePropertyChanged("ShortcutText");
			}
		}

		public KeyBinding InputBinding { get; set; }

		private string shortcutText;

		public string ShortcutText
		{
			get
			{
				if(ShortcutKey != null)
				{
					if(ShortcutModifiers != null)
					{
						return ShortcutModifiers.Value.ToString() + "+" + ShortcutKey.Value.ToString();
					}
					else
					{
						return ShortcutKey.Value.ToString();
					}
				}
				return shortcutText;
			}
			set
			{
				shortcutText = value;
				RaisePropertyChanged("ShortcutText");
			}
		}

		public Func<string> GetHeader { get; set; }

		private ICommand clickCommand;

		public ICommand ClickCommand
		{
			get { return clickCommand; }
			set
			{
				clickCommand = value;
				RaisePropertyChanged("ClickCommand");
			}
		}

		private string module;

		public string Module
		{
			get { return module; }
			set
			{
				module = value;
				RaisePropertyChanged("Module");
			}
		}

		public ObservableCollection<IMenuData> MenuItems { get; set; }

		public void Register(string ModuleName, params IMenuData[] newMenuItems)
		{
			for(int i = 0; i < newMenuItems.Length; i++)
			{
				var menuItem = newMenuItems[i];
				menuItem.Module = ModuleName;
				MenuItems.Add(menuItem);
			}
		}

		public void RegisterInputBinding(Window window)
		{
			if (ShortcutKey != null && InputBinding == null)
			{
				ModifierKeys modifier = ShortcutModifiers == null ? ModifierKeys.None : ShortcutModifiers.Value;
				var binding = new KeyBinding(ClickCommand, ShortcutKey.Value, modifier);
				InputBinding = binding;
				window.InputBindings.Add(binding);
			}

			if(MenuItems != null && MenuItems.Count > 0)
			{
				foreach(var menu in MenuItems)
				{
					if(menu is MenuData menuData)
					{
						menuData.RegisterInputBinding(window);
					}
				}
			}
		}

		public void UnregisterInputBinding(Window window)
		{
			if (InputBinding != null)
			{
				window.InputBindings.Remove(InputBinding);
			}

			if (MenuItems != null && MenuItems.Count > 0)
			{
				foreach (var menu in MenuItems)
				{
					if (menu is MenuData menuData)
					{
						menuData.UnregisterInputBinding(window);
					}
				}
			}
		}

		public MenuData()
		{
			MenuItems = new ObservableCollection<IMenuData>();
		}

		public MenuData(string menuName, ICommand command = null)
		{
			MenuItems = new ObservableCollection<IMenuData>();

			Header = menuName;

			if (command != null) ClickCommand = command;
		}
	}

	[DebuggerDisplay("---")]
	public class SeparatorData : IMenuData
	{
		public string Module { get; set; }
	}
}
