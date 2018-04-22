using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LL.SCG.Data.View
{
	public class MenuData : PropertyChangedBase
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

		public ObservableCollection<MenuData> MenuItems { get; set; }

		public void Add(params MenuData[] menuItems)
		{
			for(int i = 0; i < menuItems.Length; i++)
			{
				MenuItems.Add(menuItems[i]);
			}
		}

		public MenuData()
		{
			MenuItems = new ObservableCollection<MenuData>();
		}

		public MenuData(string menuName)
		{
			MenuItems = new ObservableCollection<MenuData>();

			Header = menuName;
		}
	}
}
