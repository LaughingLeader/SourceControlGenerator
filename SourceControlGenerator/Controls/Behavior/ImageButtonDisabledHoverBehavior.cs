using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interactivity;
using System.Windows.Media.Animation;

namespace SCG.Controls.Behavior
{
	public static class ImageButtonHoverOnDisabled
	{
		public static DependencyProperty EnableProperty =
			DependencyProperty.RegisterAttached("Enable", typeof(bool),
			typeof(ImageButton), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender, OnEnableChanged));

		public static void SetEnable(UIElement element, bool value)
		{
			element.SetValue(EnableProperty, value);
		}
		public static bool GetEnable(UIElement element)
		{
			return (bool)element.GetValue(EnableProperty);
		}

		private static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			UIElement uie = d as UIElement;

			if (uie != null)
			{
				var behColl = Interaction.GetBehaviors(uie);
				var existingBehavior = behColl.FirstOrDefault(b => b.GetType() ==
					  typeof(ImageButtonDisabledHoverBehavior)) as ImageButtonDisabledHoverBehavior;

				if ((bool)e.NewValue == false && existingBehavior != null)
				{
					behColl.Remove(existingBehavior);
				}

				else if ((bool)e.NewValue == true && existingBehavior == null)
				{
					behColl.Add(new ImageButtonDisabledHoverBehavior());
				}
			}
		}
	}

	public class ImageButtonDisabledHoverBehavior : Behavior<ImageButton>
	{
		private ContentControl buttonWrapper;
		private Binding binding;

		protected override void OnAttached()
		{
			base.OnAttached();
			
			if(buttonWrapper == null)
			{
				buttonWrapper = new ContentControl();

				var parent = this.AssociatedObject.Parent;
				if(parent is StackPanel stackPanel)
				{
					var index = stackPanel.Children.IndexOf(AssociatedObject);
					stackPanel.Children.RemoveAt(index);

					buttonWrapper.Content = this.AssociatedObject;
					binding = new Binding("IsMouseOver");
					binding.Source = buttonWrapper;
					AssociatedObject.SetBinding(ImageButton.IsHoveredProperty, binding);
					stackPanel.Children.Insert(index, buttonWrapper);
				}
			}
		}

		protected override void OnDetaching()
		{
			base.OnDetaching();

			if (buttonWrapper != null)
			{
				var parent = buttonWrapper.Parent;
				if (parent is StackPanel stackPanel)
				{
					var index = stackPanel.Children.IndexOf(buttonWrapper);
					stackPanel.Children.RemoveAt(index);
					if(binding != null)
					{
						BindingOperations.ClearBinding(AssociatedObject, ImageButton.IsHoveredProperty);
						binding = null;
					}
					stackPanel.Children.Insert(index, AssociatedObject);
				}

				buttonWrapper = null;
			}
		}
	}
}
