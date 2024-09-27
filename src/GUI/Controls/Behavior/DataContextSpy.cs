using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace SCG.Controls.Behavior
{
	/// Source: https://stackoverflow.com/q/1658397
	/// <summary>
	/// Workaround to enable <see cref="DataContext"/> bindings in situations where the DataContext is not redily available. 
	/// </summary>
	/// <remarks>http://blogs.infragistics.com/blogs/josh_smith/archive/2008/06/26/data-binding-the-isvisible-property-of-contextualtabgroup.aspx</remarks>
	public class DataContextSpy : Freezable
	{
		public DataContextSpy()
		{
			// This binding allows the spy to inherit a DataContext.
			BindingOperations.SetBinding(this, DataContextProperty, new Binding());
		}

		public object DataContext
		{
			get { return GetValue(DataContextProperty); }
			set { SetValue(DataContextProperty, value); }
		}

		// Borrow the DataContext dependency property from FrameworkElement.
		public static readonly DependencyProperty DataContextProperty = FrameworkElement
			.DataContextProperty.AddOwner(typeof(DataContextSpy));

		protected override Freezable CreateInstanceCore()
		{
			// We are required to override this abstract method.
			throw new NotImplementedException();
		}
	}
}
