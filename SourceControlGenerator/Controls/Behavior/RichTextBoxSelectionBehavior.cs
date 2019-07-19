using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Xceed.Wpf.Toolkit;

namespace SCG.Controls.Behavior
{
    public class RichTextBoxSelectionBehavior : Behavior<RichTextBox>
	{
		protected override void OnAttached()
		{
			base.OnAttached();
			AssociatedObject.SelectionChanged += RichTextBoxSelectionChanged;
		}

		protected override void OnDetaching()
		{
			base.OnDetaching();
			AssociatedObject.SelectionChanged -= RichTextBoxSelectionChanged;
		}

		void RichTextBoxSelectionChanged(object sender, System.Windows.RoutedEventArgs e)
		{
			SelectedText = AssociatedObject.Selection.Text;
		}

		public string SelectedText
		{
			get { return (string)GetValue(SelectedTextProperty); }
			set { SetValue(SelectedTextProperty, value); }
		}

		public static readonly DependencyProperty SelectedTextProperty =
			DependencyProperty.Register(
				"SelectedText",
				typeof(string),
				typeof(RichTextBoxSelectionBehavior),
				new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedTextChanged));

		private static void OnSelectedTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			//var behavior = d as RichTextBoxSelectionBehavior;
			//if (behavior == null)
			//	return;
			//behavior.AssociatedObject.Selection.Text = behavior.SelectedText;
		}
	}
}
