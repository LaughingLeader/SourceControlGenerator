using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace LL.SCG.Controls
{
	public class AutoScrollingTextbox : TextBox
	{
		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);
			VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
			HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
		}

		protected override void OnTextChanged(TextChangedEventArgs e)
		{
			base.OnTextChanged(e);
			CaretIndex = Text.Length;
			ScrollToEnd();
		}
	}
}
