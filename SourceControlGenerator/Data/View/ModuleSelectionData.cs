using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SCG.Data.View
{
	public class ModuleSelectionData : ReactiveObject
	{
		private string moduleName;

		public string ModuleName
		{
			get { return moduleName; }
			set
			{
				this.RaiseAndSetIfChanged(ref moduleName, value);
			}
		}

		private string displayName;

		public string DisplayName
		{
			get { return displayName; }
			set
			{
				this.RaiseAndSetIfChanged(ref displayName, value);
			}
		}

		private string logo;

		public string Logo
		{
			get { return logo; }
			set
			{
				this.RaiseAndSetIfChanged(ref logo, value);
			}
		}

		private Visibility logoExists = Visibility.Collapsed;

		public Visibility LogoExists
		{
			get { return logoExists; }
			set
			{
				this.RaiseAndSetIfChanged(ref logoExists, value);
			}
		}



	}
}
