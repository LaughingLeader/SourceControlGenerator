using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using LL.SCG.Data.View;

namespace LL.SCG.Data.Proxy
{
	public class ThemeColorProxy : Freezable
	{
		#region Overrides of Freezable

		protected override Freezable CreateInstanceCore()
		{
			return new IconProxy();
		}

		#endregion

		public object Data
		{
			get { return (object)GetValue(DataProperty); }
			set { SetValue(DataProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Data.
		// This enables animation, styling, binding, etc...
		public static readonly DependencyProperty DataProperty =
			DependencyProperty.Register("Data",
										typeof(ThemeColorData),
										typeof(ThemeColorProxy),
										new UIPropertyMetadata(null));
	}
}
