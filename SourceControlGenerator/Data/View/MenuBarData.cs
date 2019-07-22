using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCG.Data.View
{
	public class MenuBarData : ReactiveObject
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

		private MenuData optionsMenu;

		public MenuData Options
		{
			get { return optionsMenu; }
			set
			{
				Update(ref optionsMenu, value);
			}
		}

		private MenuData toolsMenu;

		public MenuData Tools
		{
			get { return toolsMenu; }
			set
			{
				Update(ref toolsMenu, value);
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
