using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using static SCG.Extensions.ControlExtensions;

namespace SCG.Controls
{
	public interface IUnfocusable
	{
		void Unfocus();
		void Escape();
	}

	public class UnfocusableTextBox : TextBox, IUnfocusable
	{
		public bool CanUndoTextOnEscape
		{
			get { return (bool)GetValue(CanUndoTextOnEscapeProperty); }
			set { SetValue(CanUndoTextOnEscapeProperty, value); }
		}

		public static readonly DependencyProperty CanUndoTextOnEscapeProperty =
		  DependencyProperty.Register("CanUndoTextOnEscape",
		  typeof(bool), typeof(UnfocusableTextBox),
		  new PropertyMetadata(false));

		public bool UpdateBindingOnFocusLost
		{
			get { return (bool)GetValue(UpdateBindingOnFocusLostProperty); }
			set { SetValue(UpdateBindingOnFocusLostProperty, value); }
		}

		public static readonly DependencyProperty UpdateBindingOnFocusLostProperty =
		  DependencyProperty.Register("UpdateBindingOnFocusLost",
		  typeof(bool), typeof(UnfocusableTextBox),
		  new PropertyMetadata(false));

		public UnfocusableTextBox()
		{
			
		}

		public void Unfocus()
		{
			Keyboard.ClearFocus();
			lastText = Text;
			if (UpdateBindingOnFocusLost)
			{
				var bindingExpression = BindingOperations.GetBindingExpression(this, TextBox.TextProperty);
				if (bindingExpression != null)
				{
					bindingExpression.UpdateSource();
				}
			}

			var window = this.FindParent<Window>();
			if(window != null)
			{
				Keyboard.Focus(window);
			}
		}

		public void Escape()
		{
			if (CanUndoTextOnEscape && Text != lastText) Text = lastText;
			Keyboard.ClearFocus();

			var window = this.FindParent<Window>();
			if (window != null)
			{
				Keyboard.Focus(window);
			}
		}

		private string lastText = "";

		protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
		{
			base.OnGotKeyboardFocus(e);
			lastText = Text;
		}

		protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
		{
			base.OnGotKeyboardFocus(e);
			lastText = Text;
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			if (e.Key == Key.Return)
			{
				Unfocus();
			}
			else if(e.Key == Key.Escape && CanUndoTextOnEscape)
			{
				Escape();
			}
		}
	}
}
