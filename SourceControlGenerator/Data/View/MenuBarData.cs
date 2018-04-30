﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LL.SCG.Data.View
{
	public class MenuBarData : PropertyChangedBase
	{
		private MenuData fileMenu;

		public MenuData File
		{
			get { return fileMenu; }
			set
			{
				fileMenu = value;
				RaisePropertyChanged("File");
			}
		}

		private MenuData optionsMenu;

		public MenuData Options
		{
			get { return optionsMenu; }
			set
			{
				optionsMenu = value;
				RaisePropertyChanged("Options");
			}
		}

		private MenuData toolsMenu;

		public MenuData Tools
		{
			get { return toolsMenu; }
			set
			{
				toolsMenu = value;
				RaisePropertyChanged("Tools");
			}
		}


		private MenuData helpMenu;

		public MenuData Help
		{
			get { return helpMenu; }
			set
			{
				helpMenu = value;
				RaisePropertyChanged("Help");
			}
		}

		public ObservableCollection<MenuData> Menus { get; set; }

		public void RemoveAllModuleMenus(string ModuleName)
		{
			foreach(var menu in Menus)
			{
				menu.MenuItems.RemoveAll(m => m.Module == ModuleName);
			}
			Log.Here().Activity($"Removed menus for module {ModuleName}.");
		}

		public MenuData FindByID(string ID)
		{
			foreach(var menu in Menus)
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

			Menus = new ObservableCollection<MenuData>()
			{
				File,
				Options,
				Tools,
				Help
			};
		}
	}
}
