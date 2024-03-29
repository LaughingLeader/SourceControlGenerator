﻿using Microsoft.Xaml.Behaviors;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace SCG.Controls.Behavior
{
	/// <summary>
	/// Source: https://stackoverflow.com/a/43455605
	/// </summary>
	public class ProgressBarAnimationBehavior : Behavior<ProgressBar>
	{
		private bool _IsAnimating = false;

		protected override void OnAttached()
		{
			base.OnAttached();
			ProgressBar progressBar = this.AssociatedObject;
			progressBar.ValueChanged += ProgressBar_ValueChanged;
		}

		private void ProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			//Don't animate backwards
			if (_IsAnimating || e.NewValue < e.OldValue)
			{
				return;
			}

			_IsAnimating = true;

			DoubleAnimation doubleAnimation = new DoubleAnimation(e.OldValue, e.NewValue, new Duration(TimeSpan.FromSeconds(0.3)), FillBehavior.Stop);
			doubleAnimation.Completed += Db_Completed;

			((ProgressBar)sender).BeginAnimation(ProgressBar.ValueProperty, doubleAnimation);

			e.Handled = true;
		}

		private void Db_Completed(object sender, EventArgs e)
		{
			_IsAnimating = false;
		}

		protected override void OnDetaching()
		{
			base.OnDetaching();
			ProgressBar progressBar = this.AssociatedObject;
			progressBar.ValueChanged -= ProgressBar_ValueChanged;
		}
	}
}
