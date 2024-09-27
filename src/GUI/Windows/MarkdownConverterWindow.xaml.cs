using Alphaleonis.Win32.Filesystem;
using SCG.Commands;
using SCG.Core;
using SCG.Data.View;

using System;
using System.Windows;
using System.Windows.Input;

namespace SCG.Windows
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
			if (markdownMenu != null)
			{
				markdownMenu.RegisterInputBinding(this.InputBindings);
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
				menu.RegisterInputBinding(this.InputBindings);
			}

			if(File.Exists(ViewData.SingleModeLastFileInputPath))
			{
				ViewData.LoadInputFile(ViewData.SingleModeLastFileInputPath);
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
