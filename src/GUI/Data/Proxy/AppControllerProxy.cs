using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using SCG.Core;

namespace SCG.Data.Proxy
{
	public class AppControllerProxy : Freezable
	{
		#region Overrides of Freezable

		protected override Freezable CreateInstanceCore()
		{
			return new AppControllerProxy();
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
										typeof(AppController),
										typeof(AppControllerProxy),
										new UIPropertyMetadata(null));
	}
}
