﻿using SCG.Data;
using SCG.Data.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SCG.Modules.DOS2DE.Data.View.Locale
{
	public class LocaleMenuData : PropertyChangedBase
	{
		private MenuData fileMenu;

		public MenuData File
		{
			get { return fileMenu; }
			set
			{
				Update(ref fileMenu, value);
			}
		}

		private MenuData editMenu;

		public MenuData Edit
		{
			get { return editMenu; }
			set
			{
				Update(ref editMenu, value);
			}
		}

		private MenuData settingsMenu;

		public MenuData Settings
		{
			get { return settingsMenu; }
			set
			{
				Update(ref settingsMenu, value);
			}
		}

		private MenuData helpMenu;

		public MenuData Help
		{
			get { return helpMenu; }
			set
			{
				Update(ref helpMenu, value);
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

		public void RegisterShortcuts(Window window)
		{
			foreach (var menu in Menus)
			{
				menu.RegisterInputBinding(window);
			}
		}

		public void UnregisterShortcuts(Window window)
		{
			foreach (var menu in Menus)
			{
				menu.UnregisterInputBinding(window);
			}
		}

		public LocaleMenuData()
		{
			File = new MenuData("Base.File", "File");
			Edit = new MenuData("Base.Edit", "Edit");
			Settings = new MenuData("Base.Settings", "Settings");
			Help = new MenuData("Base.Help", "Help");

			Menus = new ObservableCollection<MenuData>()
			{
				File,
				Edit,
				Settings,
				Help
			};
		}
	}
}
