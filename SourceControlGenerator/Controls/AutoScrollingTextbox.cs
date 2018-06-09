using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace SCG.Controls
{
	public class AutoScrollingTextbox : TextBox
	{
		public bool AutoScrollDisable { get; set; } = false;

		private DispatcherTimer resetTimer;

		public AutoScrollingTextbox() : base()
		{
			PreviewMouseWheel += AutoScrollingTextbox_PreviewMouseWheel;
		}

		private void AutoScrollingTextbox_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
		{
			AutoScrollDisable = true;

			if (resetTimer == null)
			{
				resetTimer = new DispatcherTimer(new TimeSpan(0, 0, 20), DispatcherPriority.Normal, delegate
				{
					AutoScrollDisable = false;
					AutoScroll();
				}, Application.Current.Dispatcher);
			}
			else
			{
				resetTimer.Stop();
			}

			resetTimer.Start();
		}

		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);
			VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
			HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
		}

		protected override void OnTextChanged(TextChangedEventArgs e)
		{
			base.OnTextChanged(e);
			AutoScroll();
		}

		public void AutoScroll()
		{
			if (!AutoScrollDisable)
			{
				CaretIndex = Text.Length;
				ScrollToEnd();
			}
		}
	}
}
