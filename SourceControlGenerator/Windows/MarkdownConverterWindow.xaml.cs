﻿using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Xaml;
using LL.SCG.Commands;
using LL.SCG.Core;
using LL.SCG.Data.View;
using Markdig;
using XamlReader = System.Windows.Markup.XamlReader;

namespace LL.SCG.Windows
{
	/// <summary>
	/// Interaction logic for MarkdownConverterWindow.xaml
	/// </summary>
	public partial class MarkdownConverterWindow : HideWindowBase, IToolWindow
	{
		public MarkdownConverterViewData ViewData
		{
			get { return (MarkdownConverterViewData)GetValue(MyPropertyProperty); }
			set { SetValue(MyPropertyProperty, value); }
		}

		public static readonly DependencyProperty MyPropertyProperty =
			DependencyProperty.Register("ViewData", typeof(MarkdownConverterViewData), typeof(MarkdownConverterWindow), new PropertyMetadata(null));

		public MarkdownConverterWindow()
		{
			InitializeComponent();
		}

		public void Init(AppController controller)
		{
			var markdownMenu = controller.Data.MenuBarData.FindByID(MenuID.Markdown);
			if (markdownMenu != null && markdownMenu.InputBinding != null)
			{
				InputBindings.Add(markdownMenu.InputBinding);
			}
		}

		public void SetData(MarkdownConverterViewData viewData = null)
		{
			if(viewData == null)
			{
				viewData = new MarkdownConverterViewData();
			}
			ViewData = viewData;
			ViewData.InitSettings(this, new ActionCommand(InputOpenFileBrowser.StartBrowse));
			DataContext = ViewData;

			foreach (var menu in ViewData.TopMenus)
			{
				menu.RegisterInputBinding(this);
			}
		}

		private void Control_InvokeDataSaving(object sender, EventArgs e)
		{
			ViewData.StartSavingAsync();
		}

		private void InputPreviewLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			
		}
	}
}
