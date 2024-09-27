using ReactiveUI;

using SCG.Data;
using SCG.Data.View;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SCG.Modules.DOS2DE.LocalizationEditor.Models
{
	public class LocaleMenuData : ReactiveObject
	{
		private MenuData fileMenu;

		public MenuData File
		{
			get { return fileMenu; }
			set
			{
				this.RaiseAndSetIfChanged(ref fileMenu, value);
			}
		}

		private MenuData editMenu;

		public MenuData Edit
		{
			get { return editMenu; }
			set
			{
				this.RaiseAndSetIfChanged(ref editMenu, value);
			}
		}

		private MenuData viewMenu;

		public MenuData View
		{
			get { return viewMenu; }
			set
			{
				this.RaiseAndSetIfChanged(ref viewMenu, value);
			}
		}

		private MenuData toolsMenu;

		public MenuData Tools
		{
			get { return toolsMenu; }
			set
			{
				this.RaiseAndSetIfChanged(ref toolsMenu, value);
			}
		}

		private MenuData settingsMenu;

		public MenuData Settings
		{
			get { return settingsMenu; }
			set
			{
				this.RaiseAndSetIfChanged(ref settingsMenu, value);
			}
		}

		private MenuData helpMenu;

		public MenuData Help
		{
			get { return helpMenu; }
			set
			{
				this.RaiseAndSetIfChanged(ref helpMenu, value);
			}
		}

		public ObservableCollection<MenuData> Menus { get; set; }

		public MenuData FindByID(string ID)
		{
			foreach (var menu in Menus)
			{
				var match = menu.FindByID(ID);
				if (match != null) return match;
			}
			return null;
		}

		public void RegisterShortcuts(InputBindingCollection InputBindings)
		{
			foreach (var menu in Menus)
			{
				menu.RegisterInputBinding(InputBindings);
			}
		}

		public void UnregisterShortcuts(InputBindingCollection InputBindings)
		{
			foreach (var menu in Menus)
			{
				menu.UnregisterInputBinding(InputBindings);
			}
		}

		public LocaleMenuData()
		{
			File = new MenuData("Base.File", "File");
			Edit = new MenuData("Base.Edit", "Edit");
			View = new MenuData("Base.View", "View");
			Tools = new MenuData("Base.Tools", "Tools");
			Settings = new MenuData("Base.Settings", "Settings");
			Help = new MenuData("Base.Help", "Help");

			Menus = new ObservableCollection<MenuData>()
			{
				File,
				Edit,
				View,
				Tools,
				Settings,
				Help
			};
		}
	}
}
