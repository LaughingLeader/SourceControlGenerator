﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using LL.SCG.Core;
using LL.SCG.Windows;

namespace LL.SCG.DOS2.Windows
{
	/// <summary>
	/// Interaction logic for SetupWindow.xaml
	/// </summary>
	public partial class SetupWindow : UnclosableWindow
	{
		private DOS2ProjectController controller;
		private Action onConfirmed;

		public SetupWindow(DOS2ProjectController projectController, Action OnConfirmed)
		{
			InitializeComponent();

			controller = projectController;
			onConfirmed = OnConfirmed;

			DataContext = controller.Data;
		}

		private void ConfirmButton_Click(object sender, RoutedEventArgs e)
		{
			onConfirmed?.Invoke();
			Close();
		}
	}
}
