using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SCG.Data.View
{
	public class ModuleSelectionData : PropertyChangedBase
	{
		private string moduleName;

		public string ModuleName
		{
			get { return moduleName; }
			set
			{
				moduleName = value;
				RaisePropertyChanged("ModuleName");
			}
		}

		private string displayName;

		public string DisplayName
		{
			get { return displayName; }
			set
			{
				displayName = value;
				RaisePropertyChanged("DisplayName");
			}
		}

		private string logo;

		public string Logo
		{
			get { return logo; }
			set
			{
				logo = value;
				RaisePropertyChanged("Logo");
			}
		}

		private Visibility logoExists = Visibility.Collapsed;

		public Visibility LogoExists
		{
			get { return logoExists; }
			set
			{
				logoExists = value;
				RaisePropertyChanged("LogoExists");
			}
		}



	}
}
