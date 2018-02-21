using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace LL.SCG.Controls
{
	public class PaddedGridViewColumnHeader : GridViewColumnHeader
	{
		public int MinWidth
		{
			get { return (int)GetValue(MinWidthProperty); }
			set { SetValue(MinWidthProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Property1.  
		// This enables animation, styling, binding, etc...
		public static readonly DependencyProperty MinWidthProperty
			= DependencyProperty.Register(
				  "MinWidth",
				  typeof(int),
				  typeof(PaddedGridViewColumnHeader),
				  new PropertyMetadata(false)
			  );
	}
}
