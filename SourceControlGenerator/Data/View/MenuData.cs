using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

		private string shortcutText;

		public string ShortcutText
		{
			get { return shortcutText; }
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
